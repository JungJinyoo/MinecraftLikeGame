using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public enum BurnableType
{
    NONE = 0,
    WOOD,
    PLANK,
    COAL,
    STICK
}

public static class BurnableMap
{
    public static readonly Dictionary<ItemType, BurnableType> table = new()
    {
        { ItemType.WOOD, BurnableType.WOOD },
        { ItemType.PLANK, BurnableType.PLANK },
        { ItemType.COAL_BLOCK, BurnableType.COAL },
        { ItemType.STICK, BurnableType.STICK },
    };

    public static readonly Dictionary<BurnableType, float> heatValue = new()
    {
        { BurnableType.NONE, 0f },
        { BurnableType.WOOD, 10f },
        { BurnableType.PLANK, 6f },
        { BurnableType.COAL, 80f },
        { BurnableType.STICK, 3f }
    };

    public static bool IsBurnable(ItemType t)
        => table.ContainsKey(t);

    public static float GetHeat(ItemType t)
    {
        if (!table.ContainsKey(t)) return 0;
        return heatValue[table[t]];
    }
}

public class Stove : MonoBehaviour
{
    public InventoryData inventoryData;
    public PhotonView pv;

    public bool isCooking;
    public float cookTimer;
    public float cookTimeTotal = 8f;

    public float heatRemaining;   // 현재 타고 있는 연료 남은 시간
    public float maxFuelTime;     // 현재 연료 1개 전체 시간

    private Vector3Int pos;

    public Stovemanager Smanager = new Stovemanager();

    //----------------------------------------------------------------------
    public void ForceStopIfInvalid()
    {
        var in0 = inventoryData.SGetSlot(0);
        var fuel = inventoryData.SGetSlot(1);

        bool noFuel = heatRemaining <= 0 && fuel.IsEmpty;
        bool noIngredient = in0.IsEmpty;

        // 재료 없으면 바로 중단
        if (noIngredient)
        {
            isCooking = false;
            cookTimer = 0f;
        }

        // 연료도 없으면 heatRemaining도 초기화
        if (noFuel)
        {
            heatRemaining = 0f;
            maxFuelTime = 0f;
        }
    }

    public void CreateLocalData()
    {
        inventoryData = ScriptableObject.CreateInstance<InventoryData>();
        inventoryData.SInit(3);

        Smanager = new Stovemanager();

        if (Smanager.recipes.Count == 0)
        {
            Smanager.addrecipe(new Stoverecipe
            {
                width = 1,
                height = 1,
                pattern = new ItemType[1, 1] { { ItemType.RAW_COWMEAT } },
                result = ItemType.BAKED_COWMEAT
            });

            Smanager.addrecipe(new Stoverecipe
            {
                width = 1,
                height = 1,
                pattern = new ItemType[1, 1] { { ItemType.RAW_PIGMEAT } },
                result = ItemType.BAKED_PIGMEAT
            });

            Smanager.addrecipe(new Stoverecipe
            {
                width = 1,
                height = 1,
                pattern = new ItemType[1, 1] { { ItemType.COAL } },
                result = ItemType.COAL_BLOCK
            });
        }

        heatRemaining = 0f;
        maxFuelTime = 0f;
    }

    public void SetPos(Vector3Int p) => pos = p;
    public Vector3Int GetPos() => pos;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (inventoryData == null) return; // 안전장치

        TickBurning();
        TickCooking();
    }

    // ---------------- 연료 처리 ----------------
    void TickBurning()
    {
        // 현재 연료가 타는 중이면 시간만 깎음
        if (heatRemaining > 0f)
        {
            heatRemaining -= Time.deltaTime;
            if (heatRemaining < 0f) heatRemaining = 0f;

            // 연료 게이지도 부드럽게 보이고 싶으면 켜두고,
            // 너무 자주 쏘기 부담이면 주석처리 해도 됨
            BroadcastState();
            return;
        }

        // 연료 없으니 슬롯에서 새 연료 꺼내기
        var fuelSlot = inventoryData.SGetSlot(1);
        if (fuelSlot.IsEmpty) return;

        ItemType t = (ItemType)fuelSlot.item.itemType;
        if (!BurnableMap.IsBurnable(t)) return;

        float dur = BurnableMap.GetHeat(t);
        if (dur <= 0f) return;

        // 새 연료 장전
        maxFuelTime   = dur;
        heatRemaining = dur;

        inventoryData.SMinus(1);

        BroadcastState();
    }

    // ---------------- 요리 처리 ----------------
    void TickCooking()
    {
        // 재료 없으면 즉시 중단
        if (inventoryData.SGetSlot(0).IsEmpty)
        {
            isCooking = false;
            cookTimer = 0f;
            BroadcastState();
            return;
        }

        if (!isCooking)
        {
            TryStartCookIfPossible();
            return;
        }

        // 연료 없으면 중단
        if (heatRemaining <= 0)
        {
            isCooking = false;
            BroadcastState();
            return;
        }

        cookTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(cookTimer / cookTimeTotal);
        pv.RPC(nameof(RPC_UpdateProgress), RpcTarget.All, pos.x, pos.y, pos.z, progress);

        if (cookTimer >= cookTimeTotal)
            FinishCooking();
    }

    public void TryStartCookIfPossible()
    {
        var in0 = inventoryData.SGetSlot(0);
        if (in0.IsEmpty) return;
        if (heatRemaining <= 0f) return;

        ItemType[,] input = new ItemType[1, 1];
        input[0, 0] = (ItemType)in0.item.itemType;

        var r = Smanager.trycraft(input);
        if (r == null || r.type == ItemType.NONE) return;

        isCooking = true;
        //cookTimer = 0f;  // 이전 진행 유지할 거면 주석 유지

        BroadcastState();
    }

    void FinishCooking()
    {
        isCooking = false;
        cookTimer = 0f;

        var in0 = inventoryData.SGetSlot(0);
        if (in0.IsEmpty) return;

        ItemType[,] input = new ItemType[1, 1];
        input[0, 0] = (ItemType)in0.item.itemType;

        var r = Smanager.trycraft(input);
        if (r == null || r.type == ItemType.NONE) return;

        var def = ItemDatabase.GetDefinition(r.type);

        // 재료 1개 소모
        inventoryData.SMinus(0);

        // 결과 넣기
        var output = inventoryData.SGetSlot(2);
        if (output.IsEmpty)
            inventoryData.SSetSlot(2, new ItemStack(def, 1));
        else
            output.count++;

        BroadcastState();

        // 연료 남아있으면 바로 다음 요리 시도
        TryStartCookIfPossible();
    }

    // ---------------- 상태 브로드캐스트 ----------------
    public void BroadcastState()
    {
        if (pv == null) return;

        pv.RPC(nameof(RPC_ApplyState), RpcTarget.All,
            pos.x, pos.y, pos.z,
            isCooking, cookTimer, cookTimeTotal,
            heatRemaining, maxFuelTime,
            ConvertSlots()
        );
    }

    public int[] ConvertSlots()
    {
        int[] arr = new int[6]; // 3slot * (type,count)

        for (int i = 0; i < 3; i++)
        {
            var s = inventoryData.SGetSlot(i);
            arr[i * 2 + 0] = s.IsEmpty ? (int)ItemType.NONE : (int)s.item.itemType;
            arr[i * 2 + 1] = s.IsEmpty ? 0 : s.count;
        }

        return arr;
    }

    // ---------------- RPCs ----------------
    [PunRPC]
    public void RPC_UpdateProgress(int x, int y, int z, float value)
    {
         if (StoveUI.Instance && StoveUI.Instance.IsOpenedAt(new Vector3Int(x, y, z)))
        {
            StoveUI.Instance.SetProgress(value);
            StoveUI.Instance.RefreshBars();
        }
    }

    [PunRPC]
    public void RPC_ApplyState(
        int x, int y, int z,
        bool _cooking,
        float _timer,
        float _total,
        float _heat,
        float _maxFuel,
        int[] slotData)
    {
        var key = new Vector3Int(x, y, z);

        if (!World.Instance.furnaceMap.TryGetValue(key, out Stove stove))
            return;

        stove.isCooking     = _cooking;
        stove.cookTimer     = _timer;
        stove.cookTimeTotal = _total;
        stove.heatRemaining = _heat;
        stove.maxFuelTime   = _maxFuel;

        for (int i = 0; i < 3; i++)
        {
            int type  = slotData[i * 2 + 0];
            int count = slotData[i * 2 + 1];

            if (type == (int)ItemType.NONE || count <= 0)
                stove.inventoryData.SSetSlot(i, new ItemStack());
            else
            {
                var def = ItemDatabase.GetDefinition((ItemType)type);
                stove.inventoryData.SSetSlot(i, new ItemStack(def, count));
            }
        }

        if (StoveUI.Instance && StoveUI.Instance.IsOpenedAt(key))
        {
            StoveUI.Instance.BindStove(stove);
            StoveUI.Instance.Refresh();
            StoveUI.Instance.RefreshBars();  // 연료/요리 게이지 같이 갱신
        }
    }
}

public class Stoverecipe
{
    public int width, height;
    public ItemType[,] pattern;
    public ItemType result;
}

public class SItemslot
{
    public ItemType type;

    public static SItemslot None    = new SItemslot { type = ItemType.NONE };
    public static SItemslot Waiting = new SItemslot { type = (ItemType)9999 }; // 임의값
}

public class Stovemanager
{
    public List<Stoverecipe> recipes = new List<Stoverecipe>();

    public void addrecipe(Stoverecipe recipe)
    {
        recipes.Add(recipe);
    }

    public SItemslot trycraft(ItemType[,] slot)
    {
        foreach (var recipe in recipes)
        {
            if (Match(slot, recipe))
                return new SItemslot { type = recipe.result };
        }
        return null;
    }

    private bool Match(ItemType[,] input, Stoverecipe recipe)
    {
        ItemType[,] pattern = recipe.pattern;

        int inputHeight   = input.GetLength(0);
        int inputWidth    = input.GetLength(1);
        int patternHeight = pattern.GetLength(0);
        int patternWidth  = pattern.GetLength(1);

        if (inputHeight < patternHeight || inputWidth < patternWidth)
            return false;

        for (int y = 0; y < patternHeight; y++)
        {
            for (int x = 0; x < patternWidth; x++)
            {
                ItemType inputType   = input[y, x];
                ItemType patternType = pattern[y, x];

                if (patternType == ItemType.NONE)
                    continue;

                if (inputType != patternType)
                    return false;
            }
        }
        return true;
    }
}
