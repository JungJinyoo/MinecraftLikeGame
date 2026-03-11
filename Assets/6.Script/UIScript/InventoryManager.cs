using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public InventoryUI inventoryUI;
    public InventoryData inventoryData;
    public HotbarUI hotbarUI;
    public CraftingtableUI craftingtableUI;

    public GameObject droppedItemPrefab;
    public Transform playerCamera;

    public ItemStack HeldStack = new ItemStack();
    public int heldCount;

    [Header("Drag Icon")]
    public Image dragIcon;
    public Canvas canvas;

    public int playerId = 1;
    private int fromSlot = -1;
    //추가
    public RectTransform dragIconGroup; // 드래그 아이콘 전체 그룹
    public Text dragCountText; // 추가: 드래그 중 수량 표시용
    public Text dragCountTextShadow;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Debug.Log($"[InventoryManager] Awake 호출됨. inventoryData.SlotCount={inventoryData.SlotCount}");

        // 슬롯 개수 잘못되면 초기화
    if (inventoryData.SlotCount != 46)
    {
        Debug.LogWarning($"[InventoryManager] ❌ 슬롯 개수 불일치! SlotCount={inventoryData.SlotCount}, 예상=46 → 강제 초기화");
        inventoryData.Init(46);
    }

        // 카메라 감시 코루틴 시작
        StartCoroutine(WaitForPlayerCamera());
    }

    private void Update()
    {
        // 드래그 중 아이콘 따라다니기
        if (dragIconGroup.gameObject.activeSelf)
        {
            dragIconGroup.position = Input.mousePosition;
        }
    }

    IEnumerator WaitForPlayerCamera()
    {
        // PlayerController 또는 카메라가 생길 때까지 기다림
        while (playerCamera == null)
        {
            var player = FindObjectOfType<FirstPersonController>();
            if (player != null)
            {
                var cam = player.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    playerCamera = cam.transform;
                    Debug.Log("[InventoryManager] 플레이어 카메라 자동 연결 완료 ✅");
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.5f); // 0.5초마다 재시도
        }
    }

    public void ShowDragIcon(bool show, ItemStack stack = null)
    {
        Debug.Log($"[ShowDragIcon] show={show} time={Time.time} caller={System.Environment.StackTrace}");

        // 강제 복구: 부모 Canvas 활성화
        if (canvas != null && !canvas.gameObject.activeSelf)
        {
            Debug.Log("[ShowDragIcon] canvas was inactive -> activating it");
            canvas.gameObject.SetActive(true);
        }

        // 강제 활성화 (임시)
        if (dragIconGroup != null)
            dragIconGroup.gameObject.SetActive(show);

        if (dragIcon != null)
        {
            if (show)
            {
                dragIcon.enabled = true;
                var c = dragIcon.color; c.a = 1f; dragIcon.color = c;
            }
            else
            {
                dragIcon.enabled = false;
            }
        }

        if (dragCountText != null)
        {
            dragCountText.enabled = show;
            var tc = dragCountText.color; tc.a = show ? 1f : 0f; dragCountText.color = tc;
        }

        if (dragCountTextShadow != null)
        {
            dragCountTextShadow.enabled = show;
            var tc2 = dragCountTextShadow.color; tc2.a = show ? 1f : 0f; dragCountTextShadow.color = tc2;
        }

        if (show && stack != null && stack.item != null)
        {
            dragIcon.sprite = stack.item.icon;
            dragCountText.text = stack.count > 1 ? stack.count.ToString() : "";
            dragCountTextShadow.text = dragCountText.text;
        }
        else
        {
            dragIcon.sprite = null;
            dragCountText.text = "";
            dragCountTextShadow.text = "";
        }
    }

    public void ClearHeldItemUI()
    {
        HeldStack = new ItemStack();
        dragIconGroup.gameObject.SetActive(false);
    }

    public void OnSlotClicked(int index, bool isRightClick)
    {
        if (index == 45)
    {
        // 아이템 있을 때만 cleanresult() 실행
        var resultSlot = inventoryData.GetSlot(45);
        if (!resultSlot.IsEmpty)
        {
            cleanresult();
        }
        return;
    }
        Debug.Log($"[OnSlotClicked] index={index}, isRightClick={isRightClick}, slots.Count={inventoryData.SlotCount}, HeldEmpty={HeldStack.IsEmpty}");
        var slot = inventoryData.GetSlot(index);

        // ----------------------------------------
        // fromSlot 기록
        // ----------------------------------------
        if (HeldStack.IsEmpty && !slot.IsEmpty)
            fromSlot = index;

        // ----------------------------------------
        // 1) 우클릭: 빈 슬롯에 1개 내려놓기
        // ----------------------------------------
        if (isRightClick && !HeldStack.IsEmpty && slot.IsEmpty)
        {
            int moveCount = 1;
            ItemDefinition itemDef = HeldStack.item;

            inventoryData.SetSlot(index, new ItemStack(itemDef, 1));
            HeldStack.count -= 1;

            int remainingCount = HeldStack.count;

            ShowDragIcon(true, HeldStack);
            inventoryUI.Refresh();
            hotbarUI?.Refresh();

            StartCoroutine(SendMoveItemToServer(
                playerId, fromSlot, index,
                itemDef.itemName, moveCount,
                remainingCount
            ));

            if (HeldStack.count <= 0)
                fromSlot = -1;

            return;
        }

        // ----------------------------------------
        // 2) 우클릭: 절반 나누기
        // ----------------------------------------
        if (isRightClick && HeldStack.IsEmpty && !slot.IsEmpty)
        {
            int half = Mathf.CeilToInt(slot.count / 2f);
            int remaining = slot.count - half;

            ItemDefinition itemDef = slot.item;

            HeldStack = new ItemStack(itemDef, half);

            inventoryData.SetSlot(index, new ItemStack(itemDef, remaining));

            ShowDragIcon(true, HeldStack);
            inventoryUI.Refresh();
            hotbarUI?.Refresh();

            StartCoroutine(SendMoveItemToServer(
                playerId, index, index,
                itemDef.itemName, half,
                remaining
            ));

            fromSlot = index;
            return;
        }

        // ----------------------------------------
        // 3) 좌클릭: 슬롯에서 전부 집기
        // ----------------------------------------
        if (HeldStack.IsEmpty && !slot.IsEmpty)
        {
            HeldStack = slot.Clone();

            int moveCount = HeldStack.count;

            ClearSlot(index);

            ShowDragIcon(true, HeldStack);

            StartCoroutine(SendMoveItemToServer(
                playerId, index, index,
                HeldStack.item.itemName, moveCount,
                0
            ));

            fromSlot = index;
            return;
        }

        // ----------------------------------------
        // 4) 좌클릭: 빈 슬롯에 내려놓기
        // ----------------------------------------
        if (!HeldStack.IsEmpty && slot.IsEmpty)
        {
            var cloneStack = HeldStack.Clone();
            inventoryData.SetSlot(index, cloneStack);
            int moveCount = cloneStack.count;

            // 서버에 보낼 remainingCount 계산
            var originalSlot = inventoryData.GetSlot(fromSlot); // fromSlot 상태 읽기
            int remaining = originalSlot != null && !originalSlot.IsEmpty ? originalSlot.count : 0;

            // 남은 수 계산 후 UI 삭제
            ClearHeldItemUI();

            StartCoroutine(SendMoveItemToServer(
                playerId, fromSlot, index,
                cloneStack.item.itemName, moveCount,
                remaining
            ));

            fromSlot = -1;
            inventoryUI.Refresh();
            hotbarUI?.Refresh();
            return;
        }

        // ----------------------------------------
        // 5) 같은 아이템 합치기
        // ----------------------------------------
        if (!HeldStack.IsEmpty && !slot.IsEmpty && slot.item == HeldStack.item)
        {
            int space = slot.item.maxStack - slot.count;
            int toMove = Mathf.Min(HeldStack.count, space);

            if (toMove <= 0) return;

            slot.count += toMove;
            HeldStack.count -= toMove;

            inventoryData.SetSlot(index, slot);

            int remainingCount = HeldStack.count;

            StartCoroutine(SendMoveItemToServer(
                playerId, fromSlot, index,
                slot.item.itemName, toMove,
                remainingCount
            ));

            if (HeldStack.count <= 0)
            {
                ClearHeldItemUI();
                fromSlot = -1;
            }

            inventoryUI.Refresh();
            hotbarUI?.Refresh();
            return;
        }

        // ----------------------------------------
        // 6) 다른 아이템 스왑
        // ----------------------------------------
        if (!HeldStack.IsEmpty && !slot.IsEmpty && slot.item != HeldStack.item)
        {
            var temp = slot.Clone();
            var moving = HeldStack.Clone();

            inventoryData.SetSlot(index, moving);
            HeldStack = temp.Clone();

            ShowDragIcon(true, HeldStack);

            int remainingCount = HeldStack.count;

            StartCoroutine(SendMoveItemToServer(
                playerId, fromSlot, index,
                moving.item.itemName, moving.count,
                remainingCount
            ));

            fromSlot = -1;

            inventoryUI.Refresh();
            hotbarUI?.Refresh();
            return;
        }
        if(index==45)
        {
            Debug.Log("45번슬롯");
        }

        inventoryUI.Refresh();
        hotbarUI?.Refresh();
    }


   public void cleanresult()
    { 
        StartCoroutine(CleanSlot());
    }

    IEnumerator CleanSlot()
    {
        var resultslot = inventoryData.GetSlot(45);
        if (resultslot.IsEmpty)
            yield break;

        yield return new WaitForSeconds(0.05f);

        for (int i = 41; i < 45; i++)
        {
            inventoryData.Minus(i);
        }

        inventoryData.SetSlot(45, new ItemStack());

        inventoryUI.Refresh();
        hotbarUI?.Refresh();
    }

    public void OnCSlotClicked(int index, bool isRightClick)
    {
        var slot = inventoryData.CGetSlot(index);
        var resultslot = inventoryData.CGetSlot(9);

        // 빈 슬롯 우클릭->하나 내려놓기
        if (isRightClick && !HeldStack.IsEmpty && slot.IsEmpty)
        {
            var oneItem = new ItemStack(HeldStack.item, 1);
            inventoryData.CSetSlot(index, oneItem);

            HeldStack.count -= 1;
            if (HeldStack.count <= 0)
                ClearHeldItemUI();
            else
                ShowDragIcon(true, HeldStack);

            inventoryUI.Refresh();
            hotbarUI?.Refresh();
            return;
        }

        // 우클릭 절반 잡기
        if (isRightClick && HeldStack.IsEmpty && !slot.IsEmpty)
        {
            int half = Mathf.CeilToInt(slot.count / 2f);
            HeldStack = new ItemStack(slot.item, half);
            slot.count -= half;

            if (slot.count <= 0)
                inventoryData.CSetSlot(index, new ItemStack());

            ShowDragIcon(true, HeldStack);
            craftingtableUI.Refresh();
            return;
        }

        // 좌클릭 기본 로직 동일
        if (HeldStack.IsEmpty && !slot.IsEmpty)
        {
            HeldStack = slot.Clone();
            inventoryData.CSetSlot(index, new ItemStack());
            ShowDragIcon(true, HeldStack);
        }
        else if (!HeldStack.IsEmpty && slot.IsEmpty && slot != resultslot)
        {
            inventoryData.CSetSlot(index, HeldStack.Clone());
            ClearHeldItemUI();
        }
        else if (!HeldStack.IsEmpty && !slot.IsEmpty && slot.item == HeldStack.item && slot != resultslot)
        {
            int space = slot.item.maxStack - slot.count;
            int toMove = Mathf.Min(space, HeldStack.count);
            slot.count += toMove;
            HeldStack.count -= toMove;

            if (HeldStack.count <= 0)
                ClearHeldItemUI();
        }
        else if (!HeldStack.IsEmpty && !slot.IsEmpty && slot.item != HeldStack.item && slot != resultslot)
        {
            var temp = slot.Clone();
            inventoryData.CSetSlot(index, HeldStack.Clone());
            HeldStack = temp.Clone();
            ShowDragIcon(true, HeldStack);
        }
        else if (!HeldStack.IsEmpty && !resultslot.IsEmpty && resultslot.item == HeldStack.item)
        {
            HeldStack.count += resultslot.count;
        }

        if (index == 9 && !resultslot.IsEmpty)
        {
            StartCoroutine(CleanCSlot());
        }

        inventoryUI.Refresh();
        hotbarUI?.Refresh();
    }

    IEnumerator CleanCSlot()
    {

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 9; i++)
        {
            inventoryData.CMinus(i);
        }

        inventoryData.CSetSlot(9, new ItemStack());

        inventoryUI.Refresh();
        craftingtableUI.Refresh();
        hotbarUI?.Refresh();
    }

    public void DropHeldItem()
    {
        if (HeldStack == null || HeldStack.IsEmpty)
        {
            Debug.Log("버릴게 없는데?");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning("[InventoryManager] playerCamera가 아직 연결되지 않았습니다 ❌");
            return;
        }

        // 서버로 전송 (slot_index 없이)
        StartCoroutine(SendDropItemToServer(HeldStack.item.itemName, HeldStack.count, HeldStack.durability));

        // 아이템 드롭 위치
        Vector3 dropPos = playerCamera.position + playerCamera.forward * 2f + Vector3.up * 0.5f;
        GameObject drop = Instantiate(droppedItemPrefab, dropPos, Quaternion.identity);

        // DroppedItem에 데이터 전달
        var dropped = drop.GetComponent<DroppedItem>();
        dropped.itemData = HeldStack.item;
        dropped.count = HeldStack.count;
        dropped.durability = HeldStack.durability;

        // Rigidbody로 던지기
        var rb = drop.GetComponent<Rigidbody>();
        rb?.AddForce(playerCamera.forward * 2f, ForceMode.Impulse);

        ClearHeldItemUI();
        inventoryUI.Refresh();
    }

    private IEnumerator SendDropItemToServer(string itemName, int count, int durability)
    {
        string url = "https://minehub.co.kr/inventory/drop";

        // 슬롯 인덱스 제거
        InventoryItemData data = new InventoryItemData
        {
            player_id = playerId,
            item_name = itemName,
            count = count,
            durability = durability,
            slot_index = -1, // 의미 없는 값으로
            use_count = 0
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log($"📦 전송 JSON (slot_index 제거): {json}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.redirectLimit = 0;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ DB 삭제 요청 실패: {request.error}");
            }
            else
            {
                Debug.Log($"✅ DB 삭제 요청 성공: {request.downloadHandler.text}");
            }
        }
    }



    public void AddItem(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty) return;

        // 🔹 1. 인벤토리(0~26)에서 같은 아이템 합치기
        for (int i = 0; i < 27 && stack.count > 0; i++)
        {
            var slot = inventoryData.GetSlot(i);
            if (!slot.IsEmpty && slot.item == stack.item)
            {
                int space = slot.item.maxStack - slot.count;
                if (space <= 0) continue;

                int toMove = Mathf.Min(space, stack.count);
                slot.count += toMove;
                stack.count -= toMove;

                slot.slot_index = i;
                StartCoroutine(UploadItemToDB(new ItemStack(stack.item, toMove, stack.durability, i)));
            }
        }

        // 🔹 3. 핫바(27~35)에서 같은 아이템 합치기
        for (int i = 27; i < 36 && stack.count > 0; i++)
        {
            var slot = inventoryData.GetSlot(i);
            if (!slot.IsEmpty && slot.item == stack.item)
            {
                int space = slot.item.maxStack - slot.count;
                if (space <= 0) continue;

                int toMove = Mathf.Min(space, stack.count);
                slot.count += toMove;
                stack.count -= toMove;

                slot.slot_index = i;
                StartCoroutine(UploadItemToDB(new ItemStack(stack.item, toMove, stack.durability, i)));
            }
        }

        // 🔹 4. 핫바 빈 슬롯 채우기
        for (int i = 27; i < 36 && stack.count > 0; i++)
        {
            var slot = inventoryData.GetSlot(i);
            if (slot.IsEmpty)
            {
                int toMove = Mathf.Min(stack.count, stack.item.maxStack);
                var newStack = new ItemStack(stack.item, toMove, stack.durability, i);
                inventoryData.SetSlot(i, newStack);
                stack.count -= toMove;

                StartCoroutine(UploadItemToDB(newStack));
            }
        }

        // 🔹 2. 인벤토리 빈 슬롯 채우기
        for (int i = 0; i < 27 && stack.count > 0; i++)
        {
            var slot = inventoryData.GetSlot(i);
            if (slot.IsEmpty)
            {
                int toMove = Mathf.Min(stack.count, stack.item.maxStack);
                var newStack = new ItemStack(stack.item, toMove, stack.durability, i);
                inventoryData.SetSlot(i, newStack);
                stack.count -= toMove;

                StartCoroutine(UploadItemToDB(newStack));
            }
        }
        hotbarUI?.Refresh();
        inventoryUI?.Refresh();
    }




    IEnumerator UploadItemToDB(ItemStack stack)
    {
        string url = "https://minehub.co.kr/inventory/add";

        InventoryItemData data = new InventoryItemData
        {
            player_id = playerId,
            item_name = stack.item.itemName,
            count = stack.count,
            durability = stack.durability,
            slot_index = stack.slot_index
        };

        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogError($"❌ DB 업로드 실패: {request.error}");
            else
                Debug.Log($"✅ DB 업로드 성공: {request.downloadHandler.text}");
        }
    }

    //테스트용 추가
    public void UseItem(int slotIndex)
    {
        var slot = inventoryData.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty)
        {
            Debug.Log("❌ 빈 슬롯입니다.");
            return;
        }

        ItemDefinition itemDef = slot.item;
        ItemType type = itemDef.itemType;

        if (!itemstat.itemstats.ContainsKey(type) || !itemstat.itemstats[type].eatable)
        {
            Debug.LogWarning($"⚠️ {type} 은(는) 먹을 수 없는 아이템입니다.");
            return;
        }

        float fullValue = itemstat.itemstats[type].full;

        // 포만감 적용
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            if (p.TryGetComponent(out PhotonView pv) && pv.IsMine)
            {
                pv.RPC("RPC_AddFull", RpcTarget.All, fullValue);
                break;
            }
        }

        // 아이템 서버 연동
        StartCoroutine(SendUseItemToServer(slot, slotIndex));
    }

    // 아이템 사용용
    public IEnumerator SendUseItemToServer(ItemStack slot, int slotIndex)
    {
        // null 체크
        if (slot == null || slot.item == null)
        {
            Debug.LogWarning($"❌ 서버로 보낼 아이템이 없습니다. slotIndex: {slotIndex}");
            yield break;
        }

        string url = "https://minehub.co.kr/inventory/use";

        InventoryItemData data = new InventoryItemData
        {
            player_id = playerId,
            item_name = slot.item.itemName,
            use_count = 1,
            durability = slot.durability, // 새로 추가된 내구도
            slot_index = slotIndex
        };

        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ DB 사용 요청 실패: {request.error}");
            }
            else
            {
                Debug.Log($"✅ DB 사용 요청 성공: {request.downloadHandler.text}");

                // ✅ 아이템 수량 감소
                slot.count -= 1;
                if (slot.count <= 0)
                {
                    slot.item = null;
                    slot.durability = -1;
                }

                // ✅ UI 갱신
                if (InventoryUI.Instance != null)
                    InventoryUI.Instance.Refresh();


                if (HotbarUI.Instance != null)
                    HotbarUI.Instance.Refresh();

            }
        }
    }
  
    IEnumerator SendMoveItemToServer(int playerId, int fromSlot, int toSlot, string itemName, int count, int remainingCount)
    {
        InventoryMoveData data = new InventoryMoveData();
        data.player_id = playerId;
        data.fromSlot = fromSlot;
        data.toSlot = toSlot;
        data.item_name = itemName;
        data.count = count;
        data.remainingCount = remainingCount;

        string json = JsonUtility.ToJson(data);
        Debug.Log("📦 인벤토리 이동 전송: " + json);

        using (UnityWebRequest www = new UnityWebRequest("https://minehub.co.kr/inventory/move", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ 인벤토리 이동 실패: {www.error}");
            }
            else
            {
                Debug.Log("✅ 인벤토리 이동 성공: " + www.downloadHandler.text);
            }
        }
    }


    private void ClearSlot(int index)
    {
        inventoryData.SetSlot(index, new ItemStack());
    }

    // ... 네 코드 유지 ...
    public void OnSSlotClicked(int index, bool isRightClick, InventoryData stoveData)
    {
        var slot = stoveData.SGetSlot(index);
        var resultslot = stoveData.SGetSlot(2); // 결과 슬롯
        int actualFromSlot = fromSlot;

        // -------------------------
        // 1️⃣ 우클릭: 절반 나누기
        // -------------------------
        if (isRightClick && HeldStack.IsEmpty && !slot.IsEmpty)
        {
            int half = Mathf.CeilToInt(slot.count / 2f);
            int remaining = slot.count - half;

            HeldStack = new ItemStack(slot.item, half);
            stoveData.SSetSlot(index, new ItemStack(slot.item, remaining));

            actualFromSlot = index;
            fromSlot = actualFromSlot;

            ShowDragIcon(true, HeldStack);
            StoveUI.Instance.Refresh();
            StoveUI.Instance?.RequestSyncToServer();

            StartCoroutine(SendMoveItemToServer(
                playerId, actualFromSlot, index,
                HeldStack.item.itemName, half, remaining
            ));
            return;
        }

        // -------------------------
        // 2️⃣ 좌클릭: 슬롯에서 집기
        // -------------------------
        if (HeldStack.IsEmpty && !slot.IsEmpty)
        {
            HeldStack = slot.Clone();
            int moveCount = slot.count;

            stoveData.SSetSlot(index, new ItemStack()); // 슬롯 비우기
            actualFromSlot = index;
            fromSlot = actualFromSlot;

            ShowDragIcon(true, HeldStack);

            StartCoroutine(SendMoveItemToServer(
                playerId, actualFromSlot, index,
                HeldStack.item.itemName, moveCount, 0
            ));
            return;
        }

        // -------------------------
        // 3️⃣ 빈 슬롯에 내려놓기
        // -------------------------
        if (!HeldStack.IsEmpty && slot.IsEmpty && slot != resultslot)
        {
            stoveData.SSetSlot(index, HeldStack.Clone());
            int moveCount = HeldStack.count;

            int remaining = 0; // 빈 슬롯이므로 remainingCount 0

            StartCoroutine(SendMoveItemToServer(
                playerId, actualFromSlot, index,
                HeldStack.item.itemName, moveCount, remaining
            ));

            ClearHeldItemUI();
            fromSlot = -1;

            StoveUI.Instance.Refresh();
            inventoryUI?.Refresh();
            hotbarUI?.Refresh();
            StoveUI.Instance?.RequestSyncToServer();
            return;
        }

        // -------------------------
        // 4️⃣ 같은 아이템 합치기
        // -------------------------
        if (!HeldStack.IsEmpty && !slot.IsEmpty && slot.item == HeldStack.item && slot != resultslot)
        {
            int space = slot.item.maxStack - slot.count;
            int toMove = Mathf.Min(space, HeldStack.count);

            slot.count += toMove;
            HeldStack.count -= toMove;

            stoveData.SSetSlot(index, slot);

            int remaining = HeldStack.count;

            StartCoroutine(SendMoveItemToServer(
                playerId, actualFromSlot, index,
                slot.item.itemName, toMove, remaining
            ));

            if (HeldStack.count <= 0)
                ClearHeldItemUI();

            fromSlot = HeldStack.IsEmpty ? -1 : fromSlot;

            inventoryUI?.Refresh();
            hotbarUI?.Refresh();
            StoveUI.Instance.Refresh();
            StoveUI.Instance?.RequestSyncToServer();
            return;
        }

        // -------------------------
        // 5️⃣ 스왑
        // -------------------------
        if (!HeldStack.IsEmpty && !slot.IsEmpty && slot.item != HeldStack.item && slot != resultslot)
        {
            var temp = slot.Clone();
            stoveData.SSetSlot(index, HeldStack.Clone());
            HeldStack = temp.Clone();

            ShowDragIcon(true, HeldStack);

            StartCoroutine(SendMoveItemToServer(
                playerId, actualFromSlot, index,
                temp.item.itemName, temp.count, HeldStack.count
            ));

            fromSlot = -1;

            inventoryUI?.Refresh();
            hotbarUI?.Refresh();
            StoveUI.Instance.Refresh();
            StoveUI.Instance?.RequestSyncToServer();
            return;
        }

        // -------------------------
        // 6️⃣ 결과 슬롯 합치기
        // -------------------------
        if (!HeldStack.IsEmpty && !resultslot.IsEmpty && resultslot.item == HeldStack.item)
        {
            int addCount = resultslot.count;
            HeldStack.count += addCount;

            stoveData.SSetSlot(2, new ItemStack()); // 결과 슬롯 비우기

            StartCoroutine(SendMoveItemToServer(
                playerId, actualFromSlot, 2,
                resultslot.item.itemName, addCount, HeldStack.count
            ));

            fromSlot = -1;

            inventoryUI?.Refresh();
            hotbarUI?.Refresh();
            StoveUI.Instance.Refresh();
            StoveUI.Instance?.RequestSyncToServer();
            return;
        }

        StoveUI.Instance.Refresh();
        inventoryUI?.Refresh();
        hotbarUI?.Refresh();
    }

}