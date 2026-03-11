using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//gpt한테 달아달라한 주석임 주석만 달아달라고 했음
public class World : MonoBehaviour
{
    public Material Wmaterial;   // 월드(블록)에 적용할 머티리얼 (텍스처/셰이더)
    public int worldSize = 20;    // 생성할 월드의 청크 갯수 (x 방향, z 방향)

    int worldSeed;
    bool seedReady = false;
    bool generatd = false;
    public int GenerationId { get; private set; } = 0;
    //저장용 딕셔너리
    public Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    public void BeginGenerate(int seed)
    {
        if (generatd && seed == worldSeed) return; // 같은 시드로 중복 생성 방지

        worldSeed = seed;
        seedReady = true;

        foreach (Transform child in transform) Destroy(child.gameObject);
        chunks.Clear();

        GenerateWorld();
        RenderAllChunks();
        GenerationId++;
        generatd = true;

        // 밑에 코드가 원래 열려 있었는데 주석 처리 하니깐 기존에 청크로 만들어진
        // 흙 블럭이 부숴지고 나서 남아 있는 버그 사라짐 ( 청크 중복 문제였다 )

        // 생성된 모든 청크를 실제 GameObject로 변환
        //foreach (var kvp in chunks)
        //{
        //    GameObject chunkObj = new GameObject("Chunk" + kvp.Key.x + "_" + kvp.Key.y);
        //    chunkObj.transform.parent = this.transform; // World 오브젝트의 자식으로 설정
        //    chunkObj.transform.position = new Vector3(kvp.Key.x * Chunk.chunkSize, 0, kvp.Key.y * Chunk.chunkSize);


        //    // 메시 필터 /렌더러 /콜라이더
        //    MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
        //    MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
        //    MeshCollider mc = chunkObj.AddComponent<MeshCollider>();
        //    mr.material = Wmaterial;

        //    // 렌더링 스크립트로 메시 생성
        //    ChunkRenderer cr = chunkObj.AddComponent<ChunkRenderer>();
        //    cr.RenderChunk(kvp.Value);
        //}
    }

    void RenderAllChunks()
    {
        foreach (var kvp in chunks)
        {
            var go = new GameObject($"Chunk{kvp.Key.x}_{kvp.Key.y}");
            go.transform.SetParent(transform);
            go.transform.position =
                new Vector3(kvp.Key.x * Chunk.chunkSize, 0, kvp.Key.y * Chunk.chunkSize); // ← z에도 *chunkSize

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            var mc = go.AddComponent<MeshCollider>();
            mr.material = Wmaterial;

            var cr = go.AddComponent<ChunkRenderer>();
            cr.RenderChunk(kvp.Value);
        }
    }


    public void GenerateWorld()
    {
        if (!seedReady)
        {
            Debug.LogWarning("[World] 시드가 준비되지 않아 생성하지 않음. BeginGenerate(seed) 먼저 호출 필요");
            return;
        }

        Debug.Log("월드 스크립트 (시드: " + worldSeed + ")");
        for (int cx = 0; cx < worldSize; cx++)     // x방향으로 worldSize만큼 반복
        {
            for (int cz = 0; cz < worldSize; cz++) // z방향으로 worldSize만큼 반복
            {
                Vector3 chunkOrigin = new Vector3(cx * Chunk.chunkSize, 0, cz * Chunk.chunkSize);
                Vector2Int pos = new Vector2Int(cx, cz); // 청크 좌표 (cx, cz)
                Chunk chunk = new Chunk(chunkOrigin);                // 새 청크 데이터 생성
                GenerateChunkData(chunk, cx, cz);         // 청크 내부의 블록 데이터 채우기
                chunks.Add(pos, chunk);                   // 딕셔너리에 저장
            }
        }
    }
    // 청크 내부의 블록 데이터(지형) 생성
    void GenerateChunkData(Chunk chunk, int cx, int cz)
    {
        // 테스트 위해 잠깐 추가 민수
        // ★ 청크 고유 시드: 월드시드와 좌표를 섞어서 항상 같은 결과가 나오게 함
        //   (큰 소수 계수로 충돌을 줄임)

        int chunkSeed = worldSeed ^ (cx * 73856093) ^ (cz * 19349663);
        System.Random rng = new System.Random(chunkSeed); // 이 청크 전용 RNG

        for (int x = 0; x < Chunk.chunkSize; x++)   // 청크 내부 x축
        {
            for (int z = 0; z < Chunk.chunkSize; z++)   // 청크 내부 z축
            {
                // 월드 좌표 (청크 좌표 + 청크 내부 좌표)
                int worldX = cx * Chunk.chunkSize + x;
                int worldZ = cz * Chunk.chunkSize + z;

                int baseHeight = 20;

                float noise = Mathf.PerlinNoise(worldX * 0.01f, worldZ * 0.02f);
                float height = baseHeight + noise * 35f;
                int surfaceY = Mathf.FloorToInt(height);

                for (int y = 0; y < Chunk.chunkHeight; y++)
                {
                    float caveNoise = Perlin3D(worldX * 0.05f, y * 0.05f, worldZ * 0.05f);

                    bool isCave =
                        caveNoise > 0.58f
                        && y < surfaceY - 4;
                    if (isCave)
                    {
                        chunk.voxels[x, y, z] = new Voxel(VoxelType.Air, Tooltag.NULL, 0, 0);
                        continue;
                    }

                    if (y > surfaceY)
                        chunk.voxels[x, y, z] = new Voxel(VoxelType.Air, Tooltag.NULL, 0, 0);     // 높이보다 위는 공기
                    else if (y == surfaceY)
                        chunk.voxels[x, y, z] = new Voxel(VoxelType.GRASS, VoxelStats.stats[VoxelType.GRASS].currenttool,
                        VoxelStats.stats[VoxelType.GRASS].hp, VoxelStats.stats[VoxelType.GRASS].hardness);   // 표면은 풀 체력과 방어력은 차후 추가
                    else if (y > surfaceY - 5)
                        chunk.voxels[x, y, z] = new Voxel(VoxelType.DIRT, VoxelStats.stats[VoxelType.DIRT].currenttool,
                        VoxelStats.stats[VoxelType.DIRT].hp, VoxelStats.stats[VoxelType.DIRT].hardness);    // 표면에서 5칸 밑까지는 흙
                    else
                        chunk.voxels[x, y, z] = new Voxel(VoxelType.STONE, VoxelStats.stats[VoxelType.STONE].currenttool,
                        VoxelStats.stats[VoxelType.STONE].hp, VoxelStats.stats[VoxelType.STONE].hardness);   // 더 아래는 돌
                }
                //광물생성 로직
                for (int y = 0; y < Chunk.chunkHeight; y++)
                {
                    var v = chunk.voxels[x, y, z];

                    if (v.type != VoxelType.STONE) continue;

                    double coalChance = rng.NextDouble();
                    double ironChance = rng.NextDouble();

                    //석탄
                    if (y < surfaceY - 4 && coalChance < 0.01f)
                    {
                        chunk.voxels[x, y, z].type = VoxelType.COAL_BLOCK;
                    }
                    //철
                    else if (y < surfaceY - 7 && ironChance < 0.007f)
                    {
                        chunk.voxels[x, y, z].type = VoxelType.IRON_BLOCK;
                    }

                }
            }
        }

        //나무생성 임시
        for (int x = 0; x < Chunk.chunkSize; x++)
        {
            for (int z = 0; z < Chunk.chunkSize; z++)
            {
                int surfaceY = -1;
                for (int y = Chunk.chunkHeight - 1; y >= 0; y--)
                {
                    if (chunk.voxels[x, y, z].type == VoxelType.GRASS)
                    {
                        surfaceY = y;
                        break;
                    }
                }
                if (surfaceY == -1) continue; // 표면 없음

                //if (Random.value > 0.02f) continue;
                if (rng.NextDouble() > 0.02) continue;

                int trunkHeight = rng.Next(4, 6);
                bool space = true;
                for (int i = 1; i <= trunkHeight + 2; i++)
                {
                    if (chunk.voxels[x, surfaceY + i, z].type != VoxelType.Air)
                    {
                        space = false;
                        break;
                    }
                }
                if (!space) continue;

                for (int i = 1; i <= trunkHeight; i++)
                {
                    chunk.voxels[x, surfaceY + i, z] =
                        new Voxel(VoxelType.WOOD,
                            VoxelStats.stats[VoxelType.WOOD].currenttool,
                            VoxelStats.stats[VoxelType.WOOD].hp,
                            VoxelStats.stats[VoxelType.WOOD].hardness
                        );
                }

                int leafStart = surfaceY + trunkHeight - 1;
                for (int lx = -2; lx <= 2; lx++)
                {
                    for (int lz = -2; lz <= 2; lz++)
                    {
                        for (int ly = 0; ly <= 2; ly++)
                        {
                            if (Mathf.Abs(lx) + Mathf.Abs(lz) + ly < 5)
                            {
                                int tx = x + lx;
                                int ty = leafStart + ly;
                                int tz = z + lz;

                                if (tx >= 0 && tx < Chunk.chunkSize &&
                                    tz >= 0 && tz < Chunk.chunkSize &&
                                    chunk.voxels[tx, ty, tz].type == VoxelType.Air)
                                {
                                    chunk.voxels[tx, ty, tz] =
                                        new Voxel(VoxelType.LEAF,
                                            VoxelStats.stats[VoxelType.LEAF].currenttool,
                                            VoxelStats.stats[VoxelType.LEAF].hp,
                                            VoxelStats.stats[VoxelType.LEAF].hardness
                                        );
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public Chunk GetChunk(int cx, int cz)
    {
        Vector2Int pos = new Vector2Int(cx, cz);
        if (chunks.TryGetValue(pos, out Chunk chunk))
            return chunk;

        return null;
    }

    // Unity 시작 시 월드 생성 + 렌더링
    void Start()
    {
        // Debug.Log($"[World] Start called. IsMasterClient: {PhotonNetwork.IsMasterClient}");
        // StartCoroutine(WaitForSeedAndGenerate());

        //GenerateWorld(); // 월드 데이터(청크 데이터) 생성

        //// 생성된 모든 청크를 실제 GameObject로 변환
        //foreach (var kvp in chunks)
        //{
        //    // 청크 오브젝트 생성 (이름은 "Chunk_x_y")
        //    GameObject chunkObj = new GameObject("Chunk" + kvp.Key.x + "_" + kvp.Key.y);
        //    chunkObj.transform.parent = this.transform; // World 오브젝트의 자식으로 설정
        //    chunkObj.transform.position = new Vector3(kvp.Key.x * Chunk.chunkSize, 0, kvp.Key.y * Chunk.chunkSize);
        //    //생성한 키 벨류값에 청크사이즈 할당해주기.

        //    //추가로 레이어 배치(진유 추가)
        //    int worldLayer = LayerMask.NameToLayer("World");
        //    chunkObj.layer = worldLayer;
        //    // 메시 필터 : 청크 메시 데이터를 담는 컴포넌트
        //    MeshFilter mf = chunkObj.AddComponent<MeshFilter>();

        //    // 메시 렌더러 : 실제 화면에 머티리얼을 입혀 보여주는 컴포넌트
        //    MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();

        //    // 메시 콜라이더 : 플레이어/물리엔진 충돌 처리를 위한 컴포넌트
        //    MeshCollider mc = chunkObj.AddComponent<MeshCollider>();

        //    // 이 청크가 사용할 머티리얼 지정
        //    mr.material = Wmaterial;

        //    // 청크 렌더링 담당 스크립트 추가
        //    ChunkRenderer cr = chunkObj.AddComponent<ChunkRenderer>();

        //    // 이 청크의 블록 데이터를 바탕으로 메시 생성
        //    cr.RenderChunk(kvp.Value);
        //}
    }
    IEnumerator WaitForSeedAndGenerate()
    {
        Debug.Log("[World] Waiting for seed...");

        float waitTime = 0f;
        float maxWaitTime = 10f; // 최대 10초 대기

        while (!WorldSeedManager.HasSeed && waitTime < maxWaitTime)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;

            if (waitTime % 1f < 0.1f) // 1초마다 로그
            {
                Debug.Log($"[World] Still waiting for seed... ({waitTime:F1}s)");
            }
        }

        if (WorldSeedManager.HasSeed)
        {
            Debug.Log($"[World] Seed received: {WorldSeedManager.WorldSeed}");
            BeginGenerate(WorldSeedManager.WorldSeed);
        }
        else
        {
            Debug.LogError("[World] Timeout waiting for seed after 10 seconds!");
        }
    }




    public Dictionary<Vector3Int, Stove> furnaceMap = new();
    public static World Instance;

    void Awake()
    {
        Instance = this;
         if (GetComponent<PhotonView>() == null)
             gameObject.AddComponent<PhotonView>();
    }

    [PunRPC]
    void RPC_SyncStovePlace(int x, int y, int z, int viewID)
    {
        Vector3Int pos = new Vector3Int(x, y, z);
        if (furnaceMap.ContainsKey(pos)) return;

        GameObject stoveObj = new GameObject($"Stove_{x}_{y}_{z}");
        stoveObj.transform.position = pos;

        PhotonView stovePV = stoveObj.AddComponent<PhotonView>();
        stovePV.ViewID = viewID;

        Stove stove = stoveObj.AddComponent<Stove>();
        stove.pv = stovePV;
        stove.SetPos(pos);

        stove.CreateLocalData();

        furnaceMap[pos] = stove;

        Debug.Log($"클라이언트에서 화로 생성 {pos} (ViewID {viewID})");
    }

    [PunRPC]
    public void RPC_SyncStoveBreak(int x, int y, int z)
    {
        var pos = new Vector3Int(x, y, z);
        if (furnaceMap.TryGetValue(pos, out Stove stove))
        {
            Destroy(stove.gameObject);
            furnaceMap.Remove(pos);
        }
    }

    [PunRPC]
    public void RPC_StartStoveCook(int x, int y, int z)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var pos = new Vector3Int(x, y, z);
        if (!furnaceMap.TryGetValue(pos, out Stove stove)) return;

        stove.TryStartCookIfPossible();
    }

    [PunRPC]
    public void RPC_RequestSetStoveSlots(int x, int y, int z, int[] types, int[] counts)
    {

        Vector3Int key = new(x, y, z);
        if (!furnaceMap.TryGetValue(key, out Stove stove)) return;

        for (int i = 0; i < 3; i++)
        {
            if (types[i] == (int)ItemType.NONE || counts[i] <= 0)
                stove.inventoryData.SSetSlot(i, new ItemStack());
            else
            {
                var def = ItemDatabase.GetDefinition((ItemType)types[i]);
                stove.inventoryData.SSetSlot(i, new ItemStack(def, counts[i]));
            }
        }

        // // 다른 플레이어만 갱신
        // stove.pv.RPC(nameof(Stove.RPC_ApplyState), RpcTarget.Others,
        // x, y, z,
        // stove.isCooking, stove.cookTimer, stove.cookTimeTotal,
        // stove.heatRemaining, stove.maxFuelTime,
        // stove.ConvertSlots());
        //요리/연료 중단 검사
        stove.ForceStopIfInvalid();

        // --- 상태 전체 동기화 ---
        stove.BroadcastState();
    }

    float Perlin3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float zx = Mathf.PerlinNoise(z, x);

        float yx = Mathf.PerlinNoise(y, x);
        float zy = Mathf.PerlinNoise(z, y);
        float xz = Mathf.PerlinNoise(x, z);

        return (xy + yz + zx + yx + zy + xz) / 6f;
    }
}