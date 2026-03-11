using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    [HideInInspector] public Chunk raychunk;      // 청크 데이터
    [HideInInspector] public Transform player;    // 외부(World)에서 주입 or 자동탐색

    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;

    void Awake()
    {
        mf = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
        mc = GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
    }

    // 메쉬 렌더링 함수 (World.cs에서 호출)
    public void RenderChunk(Chunk chunk)
    {
        raychunk = chunk;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int x = 0; x < Chunk.chunkSize; x++)
        {
            for (int y = 0; y < Chunk.chunkHeight; y++)
            {
                for (int z = 0; z < Chunk.chunkSize; z++)
                {
                    Voxel voxel = chunk.voxels[x, y, z];
                    if (voxel.type == VoxelType.Air) continue;

                    Vector3 pos = new Vector3(x, y, z);
                    AddFaces(chunk, x, y, z, vertices, triangles, uvs, pos, voxel.type);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc == null) mc = gameObject.AddComponent<MeshCollider>();

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // 렌더러 적용
        mf.sharedMesh = null;
        mf.sharedMesh = mesh;

        //  핵심: 콜라이더 강제 리셋 후 새 메시 적용
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;
    }

    // 블록의 6면 중 공기와 맞닿은 면만 생성
    void AddFaces(Chunk chunk, int x, int y, int z,
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs,
        Vector3 pos, VoxelType type)
    {
        if (y + 1 >= Chunk.chunkHeight || chunk.voxels[x, y + 1, z].type == VoxelType.Air)
            AddFace(vertices, triangles, uvs, pos, type, "top");
        if (y - 1 < 0 || chunk.voxels[x, y - 1, z].type == VoxelType.Air)
            AddFace(vertices, triangles, uvs, pos, type, "bottom");
        if (z + 1 >= Chunk.chunkSize || chunk.voxels[x, y, z + 1].type == VoxelType.Air)
            AddFace(vertices, triangles, uvs, pos, type, "front");
        if (z - 1 < 0 || chunk.voxels[x, y, z - 1].type == VoxelType.Air)
            AddFace(vertices, triangles, uvs, pos, type, "back");
        if (x + 1 >= Chunk.chunkSize || chunk.voxels[x + 1, y, z].type == VoxelType.Air)
            AddFace(vertices, triangles, uvs, pos, type, "right");
        if (x - 1 < 0 || chunk.voxels[x - 1, y, z].type == VoxelType.Air)
            AddFace(vertices, triangles, uvs, pos, type, "left");
    }

    void AddFace(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs,
             Vector3 pos, VoxelType type, string face)
    {
        int vertIndex = vertices.Count;

        switch (face)
        {
            case "top":
                vertices.Add(pos + new Vector3(0, 1, 0));
                vertices.Add(pos + new Vector3(0, 1, 1));
                vertices.Add(pos + new Vector3(1, 1, 1));
                vertices.Add(pos + new Vector3(1, 1, 0));
                break;
            case "bottom":
                vertices.Add(pos + new Vector3(0, 0, 0));
                vertices.Add(pos + new Vector3(1, 0, 0));
                vertices.Add(pos + new Vector3(1, 0, 1));
                vertices.Add(pos + new Vector3(0, 0, 1));
                break;
            case "front":
                vertices.Add(pos + new Vector3(0, 0, 1));
                vertices.Add(pos + new Vector3(1, 0, 1));
                vertices.Add(pos + new Vector3(1, 1, 1));
                vertices.Add(pos + new Vector3(0, 1, 1));
                break;
            case "back":
                vertices.Add(pos + new Vector3(1, 0, 0));
                vertices.Add(pos + new Vector3(0, 0, 0));
                vertices.Add(pos + new Vector3(0, 1, 0));
                vertices.Add(pos + new Vector3(1, 1, 0));
                break;
            case "left":
                vertices.Add(pos + new Vector3(0, 0, 0));
                vertices.Add(pos + new Vector3(0, 0, 1));
                vertices.Add(pos + new Vector3(0, 1, 1));
                vertices.Add(pos + new Vector3(0, 1, 0));
                break;
            case "right":
                vertices.Add(pos + new Vector3(1, 0, 1));
                vertices.Add(pos + new Vector3(1, 0, 0));
                vertices.Add(pos + new Vector3(1, 1, 0));
                vertices.Add(pos + new Vector3(1, 1, 1));
                break;
        }

        triangles.Add(vertIndex + 0);
        triangles.Add(vertIndex + 1);
        triangles.Add(vertIndex + 2);
        triangles.Add(vertIndex + 2);
        triangles.Add(vertIndex + 3);
        triangles.Add(vertIndex + 0);

        Vector2Int tile = BlockUVs.uvs[type].side;
        if (face == "front") tile = BlockUVs.uvs[type].front;
        if (face == "top") tile = BlockUVs.uvs[type].top;
        if (face == "bottom") tile = BlockUVs.uvs[type].bottom;

        uvs.AddRange(BlockUVs.GetUVs(tile));
    }
}
