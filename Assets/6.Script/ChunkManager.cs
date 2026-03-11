using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance;
    private World world;

    void Awake()
    {
        Instance = this;
        world = FindObjectOfType<World>();

        if (world == null)
            Debug.LogError("ChunkManager: World reference not found in scene!");
    }

    /// <summary>
    /// 폭발로 인해 반경 내 블록 제거
    /// 
    public void DestroyBlocks(Vector3 explosionPos, float radius)
    {
        Vector3Int worldPos = Vector3Int.FloorToInt(explosionPos);
        int r = Mathf.CeilToInt(radius);

        HashSet<Chunk> modifiedChunks = new HashSet<Chunk>();

        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                for (int z = -r; z <= r; z++)
                {
                    Vector3Int pos = worldPos + new Vector3Int(x, y, z);
                    if (Vector3.Distance(worldPos, pos) > radius) continue;

                    Chunk chunk = GetChunkFromWorldPos(pos);
                    if (chunk == null) continue;

                    Vector3Int local = WorldPosToLocalVoxel(pos);

                    // 범위 안 블록 Air로 변경
                    chunk.voxels[local.x, local.y, local.z].type = VoxelType.Air;
                    chunk.voxels[local.x, local.y, local.z].hp = 0;

                    modifiedChunks.Add(chunk);
                }
            }
        }

        // 변경된 청크만 메시 갱신 (UpdateMesh 안전 호출)
        foreach (var ch in modifiedChunks)
        {
            if (ch != null && ch.chunkrenderer != null)
            {
                var method = ch.chunkrenderer.GetType().GetMethod("UpdateMesh");
                if(method != null)
                {
                    method.Invoke(ch.chunkrenderer, null);
                }
                else
                {
                    Debug.LogWarning($"ChunkRenderer at {ch.origin} missing UpdateMesh()");
                }
            }
            else if(ch != null)
            {
                Debug.LogWarning("ChunkRenderer missing on chunk at origin: " + ch.origin);
            }
        }
    }

    private Chunk GetChunkFromWorldPos(Vector3Int worldPos)
    {
        if (world == null) return null;

        int cx = Mathf.FloorToInt((float)worldPos.x / Chunk.chunkSize);
        int cz = Mathf.FloorToInt((float)worldPos.z / Chunk.chunkSize);
        Vector2Int key = new Vector2Int(cx, cz);

        if (world.chunks.TryGetValue(key, out Chunk chunk))
            return chunk;

        return null;
    }

    private Vector3Int WorldPosToLocalVoxel(Vector3Int worldPos)
    {
        int cx = Mathf.FloorToInt((float)worldPos.x / Chunk.chunkSize);
        int cz = Mathf.FloorToInt((float)worldPos.z / Chunk.chunkSize);

        int lx = worldPos.x - cx * Chunk.chunkSize;
        int ly = worldPos.y;
        int lz = worldPos.z - cz * Chunk.chunkSize;

        // 안전 범위 클램프
        lx = Mathf.Clamp(lx, 0, Chunk.chunkSize - 1);
        ly = Mathf.Clamp(ly, 0, Chunk.chunkHeight - 1);
        lz = Mathf.Clamp(lz, 0, Chunk.chunkSize - 1);

        return new Vector3Int(lx, ly, lz);
    }
}
