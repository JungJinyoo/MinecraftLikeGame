using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� �߰�
using Photon.Pun;
using Photon.Realtime;


public class WorldAuthority : MonoBehaviourPunCallbacks     // �� ���� �ݹ��� �ޱ� ���� 
{

    public static WorldAuthority Instance;
    void Awake() => Instance = this; // ���� 1���� ����


    // Ŭ�� �θ��� ������ (���� ����)
    public void RequestPlace(int cx, int cz, int lx, int ly, int lz, int vType)
    {
        photonView.RPC(nameof(RPC_RequestPlace), RpcTarget.MasterClient, cx, cz, lx, ly, lz, vType);
    }

    public void RequestBreak(int cx, int cz, int lx, int ly, int lz)
    {
        photonView.RPC(nameof(RPC_RequestBreak), RpcTarget.MasterClient, cx, cz, lx, ly, lz);
    }

    bool TryGetChunkGO(int cx, int cz, out GameObject go)
    {
        go = GameObject.Find($"Chunk{cx}_{cz}");
        return go != null;
    }

    [PunRPC]
    void RPC_RequestPlace(int cx, int cz, int lx, int ly, int lz, int vType, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;   // ������ 
        var chunkObj = GameObject.Find($"Chunk{cx}_{cz}");
        var vt = (VoxelType)vType;
        if (vt == VoxelType.Air) return; // �� ������ ���� vType�� ���� (���� �ʵ� ����)

        if (!TryGetChunkGO(cx, cz, out var go)) return;
        var cr = go.GetComponent<ChunkRenderer>();
        var chunk = cr ? cr.raychunk : null;
        if (chunk == null) return;

        if (ly < 0 || ly >= Chunk.chunkHeight) return;
        if (lx < 0 || lx >= Chunk.chunkSize || lz < 0 || lz >= Chunk.chunkSize) return;
        if (chunk.voxels[lx, ly, lz].type != VoxelType.Air) return;

        var stats = VoxelStats.stats[vt];

       if (vt == VoxelType.FURANCE)
{
    Vector3Int pos = new Vector3Int(
        (int)chunkObj.transform.position.x + lx,
        ly,
        (int)chunkObj.transform.position.z + lz
    );

    if (!World.Instance.furnaceMap.ContainsKey(pos))
    {
        GameObject stoveObj = new GameObject($"Stove_{pos.x}_{pos.y}_{pos.z}");
        stoveObj.transform.position = pos;

        PhotonView stovePV = stoveObj.AddComponent<PhotonView>();
        PhotonNetwork.AllocateViewID(stovePV);
        Stove stove = stoveObj.AddComponent<Stove>();
        stove.CreateLocalData();
        stove.pv = stovePV;
        stove.SetPos(pos);
        

        //이거 추가해야 클라이언트쪽에서 데이터 생성됨
        //

        //stove.Init();

        World.Instance.furnaceMap[pos] = stove;

        PhotonView worldPV = World.Instance.GetComponent<PhotonView>();
        worldPV.RPC("RPC_SyncStovePlace",
            RpcTarget.OthersBuffered, pos.x, pos.y, pos.z, stovePV.ViewID);
    }
}

        chunk.voxels[lx, ly, lz] = new Voxel(vt, stats.currenttool, stats.hp, stats.hardness);

        // �� ���� ����� '�� PV'���� 'Buffered'�� �� ���� �����ص� ����
        photonView.RPC(nameof(RPC_ApplyPlace), RpcTarget.AllBufferedViaServer, cx, cz, lx, ly, lz, vType);
        // ���⼭ ���� �� World/Chunk�� ���� �� ����
        // ��: World.Instance.ApplyPlace(...); (Apply ���ο��� AllBufferedViaServer�� �ݿ�)

    }

    [PunRPC]
    void RPC_RequestBreak(int cx, int cz, int lx, int ly, int lz)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        // ���� �� �ݿ� �� ����

        if (!TryGetChunkGO(cx, cz, out var go)) return;
        var cr = go.GetComponent<ChunkRenderer>();
        var chunk = cr ? cr.raychunk : null;
        if (chunk == null) return;

        if (ly < 0 || ly >= Chunk.chunkHeight) return;
        if (lx < 0 || lx >= Chunk.chunkSize || lz < 0 || lz >= Chunk.chunkSize) return;
        if (chunk.voxels[lx, ly, lz].type == VoxelType.Air) return;

        // 🔥 파괴 전 타입 저장
        VoxelType originalType = chunk.voxels[lx, ly, lz].type;

        var chunkObj = GameObject.Find($"Chunk{cx}_{cz}");

        // 🔥 스토브 제거 로직 (타입을 Air로 바꾸기 전에 검사해야 함)
        if (originalType == VoxelType.FURANCE)
        {
            Vector3Int pos = new Vector3Int(
                (int)chunkObj.transform.position.x + lx,
                ly,
                (int)chunkObj.transform.position.z + lz
            );

            if (World.Instance.furnaceMap.TryGetValue(pos, out Stove stove))
            {
                Destroy(stove.gameObject);
                World.Instance.furnaceMap.Remove(pos);
                Debug.Log($"스토브파괴 {pos}");
            }
        }

        // 🔥 아이템 드랍 (VoxelType → ItemType 매핑 사용)
        ItemType dropItemType = ItemVoxelMap.GetItemType(originalType);
        if (dropItemType != ItemType.NONE && DropItemManager.Instance != null)
        {
            Vector3 dropPos = chunkObj.transform.position + new Vector3(lx + 0.5f, ly + 0.5f, lz + 0.5f);
            DropItemManager.Instance.SpawnDropItem(dropItemType, dropPos);
        }

        // 실제 블록 파괴
        chunk.voxels[lx, ly, lz].type = VoxelType.Air;

        photonView.RPC(nameof(RPC_ApplyBreak), RpcTarget.AllBufferedViaServer, cx, cz, lx, ly, lz);

    }


    // ---- ��� Ŭ�󿡼� ���� ----
    [PunRPC]
    void RPC_ApplyPlace(int cx, int cz, int lx, int ly, int lz, int vType)
    {
        if (!TryGetChunkGO(cx, cz, out var go)) return;
        var cr = go.GetComponent<ChunkRenderer>();
        var chunk = cr ? cr.raychunk : null;
        if (chunk == null) return;

        if (ly < 0 || ly >= Chunk.chunkHeight) return;
        if (lx < 0 || lx >= Chunk.chunkSize || lz < 0 || lz >= Chunk.chunkSize) return;

        var vt = (VoxelType)vType;

        var stats = VoxelStats.stats[vt];
        chunk.voxels[lx, ly, lz] = new Voxel(vt, stats.currenttool, stats.hp, stats.hardness);
        cr.RenderChunk(chunk);
    }

    [PunRPC]
    void RPC_ApplyBreak(int cx, int cz, int lx, int ly, int lz)
    {
        if (!TryGetChunkGO(cx, cz, out var go)) return;
        var cr = go.GetComponent<ChunkRenderer>();
        var chunk = cr ? cr.raychunk : null;
        if (chunk == null) return;

        if (ly < 0 || ly >= Chunk.chunkHeight) return;
        if (lx < 0 || lx >= Chunk.chunkSize || lz < 0 || lz >= Chunk.chunkSize) return;

        chunk.voxels[lx, ly, lz].type = VoxelType.Air;
        cr.RenderChunk(chunk);
    }



    public override void OnMasterClientSwitched(Player newMaster)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
            // �ʿ��ϴٸ� ���� �ʱ�ȭ(��/ť �ʱ�ȭ, ĳ�� ������ ��)
            // Room Custom Properties�� WORLD_SEED ��Ȯ�� �� World �غ� ���� ����
        }
    }
}
