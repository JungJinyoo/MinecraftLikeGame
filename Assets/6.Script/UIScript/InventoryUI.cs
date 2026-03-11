using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    public Crafting craftingScript; // Inspector에서 연결
    public craftingmanager craftingManager => craftingScript.manager;
    [Header("Inventory Setup")]
    public GameObject slotPrefab;
    public Transform gridInventory;
    public Transform gridBottomPanel;
    public Transform gridLeftEquipment;
    public Transform girdShield;
    public Transform gridProduction;
    public Transform gridProductionResult;
    public int slotCount = 46;

    [Header("Prefab Sprite Change")]
    public Sprite Helmet;
    public Sprite highlightedHelmet;
    public Sprite ArmorTop;
    public Sprite highlightedArmorTop;
    public Sprite ArmorPants;
    public Sprite highlightedArmorPants;
    public Sprite Shoes;
    public Sprite highlightedShoes;
    public Sprite Shield;
    public Sprite highlightedShield;

    public InventoryManager inventoryManager;

    // [Header("Drag Settings")]
    // public Canvas canvas;
    // public Image dragIcon;

    


    public InventoryData inventoryData;
    private List<Button> slotButtons = new List<Button>();
    public ItemStack HeldStack = new ItemStack();


    //private ItemStack heldStack = new ItemStack();
    //private int heldIndex = -1;
    //[SerializeField] private ItemDefinition testItem;
    public HotbarUI hotbarUI;
    //public GameObject droppedItemPrefab;

    //테스트용 추가
    public static InventoryUI Instance;

    private ItemStack heldStack = new ItemStack();
    private void Awake()
    {
        if (inventoryData != null)
        {
            inventoryData.Init(46);
        }
        
    }
    void Start()
    {
        slotButtons = new List<Button>();

        for (int i = 0; i < 27; i++)
            CreateSlot(i, gridInventory);

        for (int i = 27; i < 36; i++)
            CreateSlot(i, gridBottomPanel);

        for (int i = 36; i < 40; i++)
        {
            Sprite baseSprite = null;
            Sprite highlightedSprite = null;
            switch (i)
            {
                case 36:
                    baseSprite = Helmet;
                    highlightedSprite = highlightedHelmet;
                    break;
                case 37:
                    baseSprite = ArmorTop;
                    highlightedSprite = highlightedArmorTop;
                    break;
                case 38:
                    baseSprite = ArmorPants;
                    highlightedSprite = highlightedArmorPants;
                    break;
                case 39:
                    baseSprite = Shoes;
                    highlightedSprite = highlightedShoes;
                    break;
            }
            CreateSlot(i, gridLeftEquipment, baseSprite, highlightedSprite);
        }

        for (int i = 40; i < 41; i++)
        {
            Sprite baseSprite = null;
            Sprite highlightedSprite = null;

            baseSprite = Shield;
            highlightedSprite = highlightedShield;

            CreateSlot(i, girdShield, baseSprite, highlightedSprite);
        }

        for (int i = 41; i < 45; i++)
            CreateSlot(i, gridProduction);

        for (int i = 45; i < 46; i++)
            CreateSlot(i, gridProductionResult);

        //dragIcon.enabled = false;


        // if (testItem != null)
        // {
        //     AddTestItem(testItem, 1);
        // }
        Refresh();
    }

    void Update()
    {

        // 드래그 중 아이콘 따라다니기
        // if (dragIcon.enabled)
        // {
        //     dragIcon.rectTransform.position = Input.mousePosition;
        // }

        // 바깥 클릭 감지 (드래그 중일 때)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (InventoryManager.Instance.HeldStack != null && !InventoryManager.Instance.HeldStack.IsEmpty)
            {
                // UI 위에 없으면 떨어뜨리기
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    InventoryManager.Instance.DropHeldItem();
                }
            }
        }
        UpdateCraftingResult();
    }


    void CreateSlot(int index, Transform parent, Sprite baseSprite = null, Sprite highlightedSprite = null)
{
    var go = Instantiate(slotPrefab, parent);
    var btn = go.GetComponent<Button>();
    var image = go.GetComponent<Image>();

    if (baseSprite != null)
    {
        image.sprite = baseSprite;

        var spriteState = btn.spriteState;
        spriteState.highlightedSprite = highlightedSprite;
        spriteState.pressedSprite = highlightedSprite;
        spriteState.selectedSprite = highlightedSprite;
        spriteState.disabledSprite = highlightedSprite;
        btn.spriteState = spriteState;
    }

    slotButtons.Add(btn);

    if (index != 45)
    {
        btn.onClick.AddListener(() => InventoryManager.Instance.OnSlotClicked(index, false));
    }

    // 45번 슬롯 전용 로직
    if (index == 45)
    {
        btn.onClick.AddListener(() =>
        {
            var result = inventoryData.GetSlot(45);
            var held = InventoryManager.Instance.HeldStack;

            if (result.IsEmpty)
                return;

            // 들고 있지 않으면 pick
            if (held.IsEmpty)
            {
                InventoryManager.Instance.HeldStack = result.Clone();
                InventoryManager.Instance.ShowDragIcon(true, InventoryManager.Instance.HeldStack);
            }
            // 같은 아이템이면 합치기
            else if (held.item == result.item)
            {
                held.count += result.count;
                InventoryManager.Instance.ShowDragIcon(true, held);
            }
            else
            {
                return; // 다른 거 들고 있으면 취소
            }

            // 재료 소모 + 결과 제거
            InventoryManager.Instance.cleanresult();

            Refresh();
            hotbarUI?.Refresh();
        });
    }

    // 우클릭 그대로
    var trigger = go.AddComponent<EventTrigger>();
    var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
    entry.callback.AddListener((data) =>
    {
        var pointer = (PointerEventData)data;
        if (pointer.button == PointerEventData.InputButton.Right)
        {
            InventoryManager.Instance.OnSlotClicked(index, true);
        }
    });
    trigger.triggers.Add(entry);
}

    private void OnSlotClick(int index)
    {
        // 결과 슬롯 클릭 시 (제작 완료)
        if (index == 45)
        {         
            Refresh();
            return;
        }

        //  갱신
        Refresh();
        hotbarUI.Refresh();

        //  제작 슬롯(41~45)이면 제작 결과 업데이트
        if (index >= 41 && index <= 45)
            UpdateCraftingResult();
    }

    public void UpdateCraftingResult()
    {
        // 2x2 제작 슬롯 입력 구성
        ItemType[,] input = new ItemType[2, 2];
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                int index = 41 + y * 2 + x;
                var stack = inventoryData.GetSlot(index);
                input[y, x] = stack.IsEmpty ? ItemType.NONE : (ItemType)stack.item.itemType;
            }
        }

        // 제작 시도
        Itemslot result = craftingManager.trycraft(input);

        // 결과 슬롯(45) 갱신
        if (result != null)
        {
            // ItemType -> ItemDefinition 변환
            ItemDefinition def = ItemDatabase.GetDefinition(result.type);
            if (def != null)
                inventoryData.SetSlot(45, new ItemStack(def, result.count));
            else
                inventoryData.SetSlot(45, new ItemStack());
        }
        else
        {
            inventoryData.SetSlot(45, new ItemStack());
        }

        Refresh();
    }

    //public void ShowDragIcon(bool show)
    //{
    //    dragIcon.enabled = show;
    //    if (show && heldStack.item != null)
    //    {
    //        dragIcon.sprite = heldStack.item.icon;

    //    }
    //}

    // public void ShowDragIcon(bool show, ItemStack stack = null)
    // {
    //     dragIcon.enabled = show;

    //     if (show && stack != null && stack.item != null)
    //     {
    //         dragIcon.sprite = stack.item.icon;
    //     }
    // }

    public void Refresh()
    {
        if (inventoryData == null || slotButtons == null)
        {
            Debug.LogWarning("⚠️ InventoryUI.Refresh() skipped — data not ready.");
            return;
        }

        int count = Mathf.Min(slotButtons.Count, inventoryData.SlotCount);

        for (int i = 0; i < count; i++)
        {
            var slot = inventoryData.GetSlot(i);
            var button = slotButtons[i];
            if (button == null) continue;

            var icon = button.transform.GetChild(0).GetComponent<Image>();
            var countText = button.transform.GetChild(1).GetComponent<Text>();
            var shadow = button.transform.GetChild(2).GetComponent<Text>();
            if (i == 45)
            {
                icon.raycastTarget = false;
            }
            else
            {
                icon.raycastTarget = true;
            }

            if (slot == null || slot.IsEmpty)
            {
                icon.enabled = false;
                countText.text = "";
                shadow.text = "";
            }
            else
            {
                icon.enabled = true;
                icon.sprite = slot.item.icon;
                countText.text = slot.count > 1 ? slot.count.ToString() : "";
                shadow.text = countText.text;
            }
        }
        
    }



    //추가
    public void AddItemToInventory(ItemDefinition item, int count)
    {
        for (int i = 0; i < 36; i++) // 인벤토리 + 핫바
        {
            var slot = inventoryData.GetSlot(i);
            if (!slot.IsEmpty && slot.item == item && slot.count < item.maxStack)
            {
                int space = item.maxStack - slot.count;
                int move = Mathf.Min(space, count);
                slot.count += move;
                count -= move;
                if (count <= 0)
                    return;
            }
        }

        for (int i = 0; i < 36; i++)
        {
            var slot = inventoryData.GetSlot(i);
            if (slot.IsEmpty)
            {
                inventoryData.SetSlot(i, new ItemStack(item, count));
                return;
            }
        }
    }
    public void ClearInventoryUI()
    {
        // 인벤토리 데이터 전체 초기화
        if (inventoryData != null)
        {
            for (int i = 0; i < inventoryData.SlotCount; i++)
            {
                inventoryData.SetSlot(i, new ItemStack());
            }
        }

        // 제작 슬롯 초기화 (41~45)
        for (int i = 41; i < 46; i++)
        {
            inventoryData.SetSlot(i, new ItemStack());
        }

        // 장비 슬롯 초기화 (36~40)
        for (int i = 36; i < 41; i++)
        {
            inventoryData.SetSlot(i, new ItemStack());
        }

        // 핫바 초기화
        if (hotbarUI != null)
        {
            hotbarUI.ClearHotbarUI(); // 아래에 함께 정의
        }

        // 드래그 중인 아이콘 숨기기
        inventoryManager.ShowDragIcon(false);

        // UI 갱신
        Refresh();
        if (hotbarUI != null)
            hotbarUI.Refresh();
    }
}