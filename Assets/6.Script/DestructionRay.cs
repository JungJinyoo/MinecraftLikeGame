using Photon.Pun; // 포톤 추가(민수)
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ItemVoxelMap
{
    // 아이템 → 복셀 매핑
    public static readonly Dictionary<ItemType, VoxelType> itemToVoxel = new()
    {
        { ItemType.WOOD, VoxelType.WOOD },
        { ItemType.PLANK, VoxelType.PLANK },
        { ItemType.STONE, VoxelType.STONE },
        { ItemType.COBBLESTONE, VoxelType.COBBLESTONE },
        { ItemType.IRON_BLOCK, VoxelType.IRON_BLOCK },
        { ItemType.CRAFTTABLE, VoxelType.CRAFTTABLE },
        { ItemType.FURANCE, VoxelType.FURANCE },
        { ItemType.COAL_BLOCK, VoxelType.COAL_BLOCK },
        { ItemType.GRASS, VoxelType.GRASS },
        { ItemType.DIRT, VoxelType.DIRT },
        { ItemType.RAW_COWMEAT, VoxelType.RAW_COWMEAT },
        { ItemType.NONE, VoxelType.Air }
    };

    // 복셀 → 아이템 매핑
    public static readonly Dictionary<VoxelType, ItemType> voxelToItem = new();

    // static 생성자: 자동 역매핑 생성
    static ItemVoxelMap()
    {
        foreach (var pair in itemToVoxel)
        {
            if (!voxelToItem.ContainsKey(pair.Value))
                voxelToItem.Add(pair.Value, pair.Key);
        }
    }

    // 아이템 → 복셀 변환
    public static VoxelType GetVoxelType(ItemType itemType)
    {
        return itemToVoxel.TryGetValue(itemType, out var voxel)
            ? voxel
            : VoxelType.Air;
    }

    // 복셀 → 아이템 변환
    public static ItemType GetItemType(VoxelType voxelType)
    {
        return voxelToItem.TryGetValue(voxelType, out var item)
            ? item
            : ItemType.NONE;
    }
}

public class DestructionRay : MonoBehaviour
{
    public Camera playerCamera; // 추가(민수)
    public PhotonView pv; // 추가(민수)

    public float DrawRay = 4.5f;          // Ray 길이
    public GameObject DesEffect;        // 블록 파괴 이펙트
    public World world;                 // World 스크립트 참조
    Ray ray;
    RaycastHit hitInfo;
    private HotbarUI hotbarUI; // public에서 private로 바꿔주고 코드상에 직접 연결
    private GameObject crashImg;
    public LayerMask ignore; // 무시할 Ray 객체

    public VoxelType type;

    public Tool item;
    public ItemType itemtype;

    float inputtime = 0.3f; //입력타임을 조절할 변수
    float nextinputtime = 0;

    GameObject player;

    public Animator testHandanim;

    private Voxel currentarget;
    private Vector3Int currenttargetpos;
    private Chunk currentChunk;
    private float nextMiningTime = 0f;
    public float miningInterval = 0.3f;   // 데미지 간격
    public float toolDamage = 1;            // 도구 공격력 기본 손(1)

    //파괴 균열 애니매이션 재생
    private MeshRenderer crashanim;
    private GameObject LineBox;
    public Material[] materials;
    public GameObject[] grabobj;
    public GameObject uihand;
    private bool grabsomething = false;
    private ButtonCtrl btnctrl;
    private AudioSource desAudio;
    public AudioClip[] desclip;

    //테스트용추가
    public DropItemManager dropManager;

    void Start()
    {
        crashImg.SetActive(false);
        LineBox.SetActive(false);
    }

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        crashImg = transform.GetChild(0).gameObject;
        crashanim = crashImg.GetComponent<MeshRenderer>();
        LineBox = transform.GetChild(1).gameObject;
        btnctrl = GameObject.FindGameObjectWithTag("btnCrtl").GetComponent<ButtonCtrl>();
        hotbarUI = GameObject.FindGameObjectWithTag("HotBarUI").GetComponent<HotbarUI>(); // 추가(민수)
        pv = transform.parent.GetComponent<PhotonView>(); // 추가(민수)
        playerCamera = transform.parent.transform.GetChild(0).GetComponent<Camera>();
        desAudio = transform.parent.GetComponent<AudioSource>();
        DesEffect = transform.GetChild(2).gameObject;
        if (pv.IsMine)
        {
            uihand = transform.parent.transform.GetChild(0).transform.GetChild(0).transform.GetChild(1).gameObject;
        }
    }

    void Update()
    {
        if (!pv.IsMine) return; // 내 것만 처리

        if (playerCamera == null)
        {
            return; // null 체크 추가  추가(민수)
        }

        ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));//(Input.mousePosition); // 수정
        Debug.DrawRay(ray.origin, ray.direction * DrawRay, Color.green);

        itemtype = hotbarUI.GetSelectedItemType();

        if (Physics.Raycast(ray, out hitInfo, DrawRay))
        {
            // Debug.Log("Hit: " + hitInfo.collider.gameObject.name); 잠깐 주석
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (grabsomething)
            { testHandanim.SetTrigger("Usetool"); }
            else
            { testHandanim.SetTrigger("Shaking"); }
        }

        if (Physics.Raycast(ray, out hitInfo, DrawRay)) //기존 레이케스트
        {
            // 맞은 청크 오브젝트
            GameObject chunkObj = hitInfo.collider.gameObject;
            ChunkRenderer cr = chunkObj.GetComponent<ChunkRenderer>();
            if (cr == null)
            {
                // 청크가 아니면 그냥 무시
                return;
            }
            string chunkname = chunkObj.name;
            // Chunk 객체 가져오기 (ChunkRenderer 안에 Chunk를 참조하도록 수정 필요)
            Chunk chunk = cr.raychunk; // cr.chunk는 World에서 생성 시 할당

            Vector3 hitBlockPos = hitInfo.point - (hitInfo.normal * 0.01f);
            int bx = Mathf.FloorToInt(hitBlockPos.x);
            int by = Mathf.FloorToInt(hitBlockPos.y);
            int bz = Mathf.FloorToInt(hitBlockPos.z);

            int lx = bx - (int)chunkObj.transform.position.x;
            int lz = bz - (int)chunkObj.transform.position.z;

            Vector3 targetblockPos = chunkObj.transform.position + new Vector3(lx + 0.5f, by + 0.5f, lz + 0.5f);
            Quaternion targetblockrot = chunkObj.transform.rotation;

            Voxel voxel = chunk.voxels[lx, by, lz];

            LineBox.SetActive(true);

            //자식으로 들어와있는 crash 블럭의 위치가 시야에 따라 변경 안되어지도록 조치.
            LineBox.GetComponent<NoFollowobj>().attach(targetblockPos, targetblockrot);

            if (Input.GetMouseButton(0))
            {
                crashImg.SetActive(true);
                crashImg.GetComponent<NoFollowobj>().attach(targetblockPos, targetblockrot);

                // Y 범위 체크
                if (by < 0 || by >= Chunk.chunkHeight) return;
                if (voxel.type == VoxelType.Air) return;

                if (currentarget != voxel)
                {
                    if (currentarget != null && currentarget.type != VoxelType.Air)
                    {
                        currentarget.hp = VoxelStats.stats[currentarget.type].hp;
                    }
                    currentarget = voxel;
                    currentChunk = chunk;
                    currenttargetpos = new Vector3Int(lx, by, lz);

                    crashanim.material = materials[0];
                }

                if (Time.time >= nextMiningTime)
                {
                    if (grabsomething)
                    { testHandanim.SetTrigger("Usetool"); }
                    else
                    { testHandanim.SetTrigger("Shaking"); }

                    Tooltag tooltag = Tooltag.NULL;
                    float Damage = 0.5f;
                    float durability = 0;
                    float maxhardness = voxel.hardness;

                    //파티클 추가//
                    var ps = DesEffect.GetComponent<ParticleSystem>();
                    var tsa = ps.textureSheetAnimation;

                    float tileIndex = ParticleTileSetter.ParticleSetNum(voxel.type);
                    float total = tsa.numTilesX * tsa.numTilesY;
                    tsa.frameOverTime = new ParticleSystem.MinMaxCurve(tileIndex / total);

                    ps.Emit(3);
                    DesEffect.GetComponent<ParticleSystem>().Emit(3);
                    /////////

                    if (ToolStat.toolstats.TryGetValue(itemtype, out var data))
                    {
                        tooltag = data.tag;
                        Damage = data.damage;
                        durability = data.durability;
                    }
                    else
                    {
                        tooltag = Tooltag.NULL;
                        toolDamage = 0.5f; // 기본데미지
                    }

                    if (voxel.currrenttool == tooltag)
                    { toolDamage = Damage; }
                    else
                    { toolDamage = 0.5f; }

                    float breaktime = maxhardness * 1.5f / toolDamage;
                    float desDamage = 10f / (breaktime / miningInterval);

                    voxel.hp -= desDamage;
                    float nowhp = Mathf.Clamp01(voxel.hp / 10);
                    int matindex = Mathf.FloorToInt(nowhp * (materials.Length - 1));
                    crashanim.material = materials[matindex];

                    if (DesEffect != null)
                    {
                        Vector3 effectPos = chunkObj.transform.position + new Vector3(lx + 0.5f, by + 0.5f, lz + 0.5f);
                        DesEffect.transform.position = effectPos;
                    }

                    // 사운드
                    if (desAudio != null)
                    {
                        int num = 0;
                        if (voxel.type == VoxelType.PLANK || voxel.type == VoxelType.WOOD)
                        {
                            num = Random.Range(0, 4);
                        }
                        if (voxel.type == VoxelType.STONE || voxel.type == VoxelType.COBBLESTONE ||
                            voxel.type == VoxelType.COAL_BLOCK || voxel.type == VoxelType.IRON_BLOCK || voxel.type == VoxelType.FURANCE)
                        {
                            num = Random.Range(4, 7);
                        }
                        if (voxel.type == VoxelType.DIRT || voxel.type == VoxelType.GRASS
                            || voxel.type == VoxelType.LEAF)
                        {
                            num = Random.Range(7, 11);
                            if (voxel.hp <= 0)
                            {
                                num = Random.Range(11, 15);
                            }
                        }
                        desAudio.clip = desclip[num];
                        PlaydesSound();
                    }

                    if (voxel.hp <= 0)
                    {

                        int cx, cz;
                        TryParseChunkIndices(chunkname, out cx, out cz);

                        if (WorldAuthority.Instance != null)
                        {
                            WorldAuthority.Instance.RequestBreak(cx, cz, lx, by, lz);
                        }

                        crashanim.material = materials[9];
                        crashImg.SetActive(false);
                        currentarget = null;

                        nextMiningTime = Time.time + miningInterval;
                    }

                    nextMiningTime = Time.time + miningInterval;
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    crashanim.material = materials[9];
                    crashImg.SetActive(false);
                    if (currentarget != null && currentarget.type != VoxelType.Air)
                    {
                        currentarget.hp = VoxelStats.stats[currentarget.type].hp;
                    }

                    crashImg.SetActive(false);
                    LineBox.SetActive(false);
                    currentarget = null;
                }
            }

            if (Input.GetMouseButton(1))
            {
                if (Time.time >= nextinputtime)
                {
                    Vector3 uiBlockPos = hitInfo.point - (hitInfo.normal * 0.01f);
                    int hx = Mathf.FloorToInt(uiBlockPos.x); //히트한 블럭
                    int hy = Mathf.FloorToInt(uiBlockPos.y);
                    int hz = Mathf.FloorToInt(uiBlockPos.z);
                    int tx = hx - (int)chunkObj.transform.position.x;
                    int tz = hz - (int)chunkObj.transform.position.z;
                    VoxelType hitype = chunk.voxels[tx, hy, tz].type;

                    Vector3 PlacePos = hitInfo.point + (hitInfo.normal * 0.01f);//외부면.

                    int pbx = Mathf.FloorToInt(PlacePos.x);
                    int pby = Mathf.FloorToInt(PlacePos.y);
                    int pbz = Mathf.FloorToInt(PlacePos.z);

                    int plx = pbx - Mathf.FloorToInt(chunkObj.transform.position.x);
                    int plz = pbz - Mathf.FloorToInt(chunkObj.transform.position.z);

                    int cx, cz;
                    TryParseChunkIndices(chunkname, out cx, out cz);

                    if (pby < 0 || pby >= Chunk.chunkHeight) return;

                    if (plx < 0)
                    {
                        chunkObj = GameObject.Find($"Chunk{cx - 1}_{cz}");
                        plx += Chunk.chunkSize;
                    }
                    else if (plx >= Chunk.chunkSize)
                    {
                        chunkObj = GameObject.Find($"Chunk{cx + 1}_{cz}");
                        plx -= Chunk.chunkSize;
                    }

                    if (plz < 0)
                    {
                        chunkObj = GameObject.Find($"Chunk{cx}_{cz - 1}");
                        plz += Chunk.chunkSize;
                    }
                    else if (plz >= Chunk.chunkSize)
                    {
                        chunkObj = GameObject.Find($"Chunk{cx}_{cz + 1}");
                        plz -= Chunk.chunkSize;
                    }

                    cr = chunkObj.GetComponent<ChunkRenderer>();
                    chunk = cr.raychunk;

                    //플레이어가 있는 위치 설치 방지용
                    Vector3 blockWorldPos = chunkObj.transform.position + new Vector3(plx + 0.1f, pby + 0.1f, plz + 0.1f);
                    Vector3 _player = player.transform.position;
                    float distance = Vector3.Distance(blockWorldPos, _player);

                    if (hitype == VoxelType.CRAFTTABLE)
                    {
                        btnctrl.isCraftingVisible = true;
                        return;
                    }
                    else if (hitype == VoxelType.FURANCE)
                    {
                        Vector3Int pos = new Vector3Int(hx, hy, hz);

                        if (World.Instance.furnaceMap.TryGetValue(pos, out Stove stove))
                        {
                            FindObjectOfType<ButtonCtrl>().OpenStove(stove);
                            return;
                        }
                        else
                        {
                            Debug.LogWarning($"화로 데이터 없음 (pos: {pos})");
                        }
                        return;
                    }

                    // 다시 최신 청크로 재파싱
                    cr = chunkObj.GetComponent<ChunkRenderer>();
                    chunk = cr.raychunk;
                    chunkname = chunkObj.name;
                    TryParseChunkIndices(chunkname, out cx, out cz);

                    if (chunk.voxels[plx, pby, plz].type == VoxelType.Air && distance >= 1f)
                    {
                        if (type == VoxelType.Air) return;

                        if (WorldAuthority.Instance != null)
                        {
                            WorldAuthority.Instance.RequestPlace(cx, cz, plx, pby, plz, (int)type);
                        }
                        else
                        {
                            var stats2 = VoxelStats.stats[type];
                            chunk.voxels[plx, pby, plz] = new Voxel(type, stats2.currenttool, stats2.hp, stats2.hardness);
                            cr.RenderChunk(chunk);
                        }

                        int currentSlot = HotbarUI.Instance.selectedIndex + 27;
                        ItemStack slot = InventoryManager.Instance.inventoryData.GetSlot(currentSlot);
                        if (slot != null && slot.item != null)
                        {
                            StartCoroutine(InventoryManager.Instance.SendUseItemToServer(slot, currentSlot));
                        }

                        nextinputtime = Time.time + inputtime;
                    }

                    nextinputtime = Time.time + inputtime;
                }
            }
        }
        else
        {
            crashImg.SetActive(false);
            LineBox.SetActive(false);

            // 현재 타겟 초기화
            if (currentarget != null)
            {
                currentarget.hp = VoxelStats.stats[currentarget.type].hp;
                currentarget = null;
            }
        }

        ////////////////////////////////////////////////////////////////
        foreach (var grab in grabobj)
        {
            if (grab.name == itemtype.ToString())
            {
                grab.SetActive(true);
                grabsomething = true;
                if (pv.IsMine && uihand != null)
                    uihand.SetActive(false);
            }
            else
            {
                grab.SetActive(false);
            }
            if (itemtype.ToString() == "HAND")
            {
                grabsomething = false;
            }
        }
        if (!grabsomething)
        {
            if (pv.IsMine && uihand != null)
            { uihand.SetActive(true); }
        }

        // 핫바 아이템에 해당하는 블록 타입 선택
        if (ItemVoxelMap.itemToVoxel.TryGetValue(itemtype, out var _voxel))
        {
            type = _voxel; // VoxelType 할당
        }
        else
        {
            type = VoxelType.Air; // 블록이 아닌 경우 공기/손
        }

    }

    void PlaydesSound()
    {
        desAudio.PlayOneShot(desAudio.clip);
    }

    // 청크 이름 "Chunk3_5" → cx=3, cz=5
    bool TryParseChunkIndices(string chunkName, out int cx, out int cz)
    {
        cx = 0; cz = 0;
        if (string.IsNullOrEmpty(chunkName) || !chunkName.StartsWith("Chunk")) return false;
        var parts = chunkName.Substring("Chunk".Length).Split('_');
        if (parts.Length < 2) return false;
        int.TryParse(parts[0], out cx);
        int.TryParse(parts[1], out cz);
        return true;
    }
}
