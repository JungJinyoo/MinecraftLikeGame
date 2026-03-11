using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftingtableUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Crafting craftingScript; // Inspector에서 연결
    public craftingmanager craftingManager => craftingScript.manager;
    public GameObject ResoultslotPrefab;
    public Transform gridProduction;
    public Transform gridProductionResult;
    public Canvas canvas;
    public InventoryData inventoryData;
    private List<Button> slotButtons;

    public Image dragIcon;

    InventoryUI inventory;
    public InventoryManager inventoryManager;
    // Start is called before the first frame update
    private void Awake()
    {
        if (inventoryData != null)
        {
            inventoryData.CInit(10);
        }
        inventory = GameObject.FindGameObjectWithTag("InventoryUI").GetComponent<InventoryUI>();
    }
    void Start()
    {
        slotButtons = new List<Button>();

        for (int i = 0; i < 9; i++)
        {
            CreateSlot(i, gridProduction);
        }

        for (int i = 9; i < 10; i++)
        {
            CreateResoultSlot(i, gridProductionResult);
        }
        inventoryManager.dragIcon.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 드래그 중 아이콘 따라다니기
        if (dragIcon.enabled)
        {
            inventoryManager.dragIcon.rectTransform.position = Input.mousePosition;
        }

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
        UpdateCraftingResult(); //heldstack에 안쌓임.<-수정완료했음 홀드스택에 쌓이게 바꿈.
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

        // 좌클릭
        btn.onClick.AddListener(() => InventoryManager.Instance.OnCSlotClicked(index, false));

        // 우클릭
        var trigger = go.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) =>
        {
            var pointer = (PointerEventData)data;
            if (pointer.button == PointerEventData.InputButton.Right)
            {
                InventoryManager.Instance.OnCSlotClicked(index, true);
            }
        });
        trigger.triggers.Add(entry);

        //btn.onClick.AddListener(() => OnSlotLeftClick(index));
        btn.onClick.AddListener(() => OnSlotClick(index));
        //slots.Add(new ItemStack());
    }

    void CreateResoultSlot(int index, Transform parent, Sprite baseSprite = null, Sprite highlightedSprite = null)
    {
        var go = Instantiate(ResoultslotPrefab, parent);
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

        // 좌클릭
        btn.onClick.AddListener(() => InventoryManager.Instance.OnCSlotClicked(index, false));

        // 우클릭
        var trigger = go.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) =>
        {
            var pointer = (PointerEventData)data;
            if (pointer.button == PointerEventData.InputButton.Right)
            {
                InventoryManager.Instance.OnCSlotClicked(index, true);
            }
        });
        trigger.triggers.Add(entry);

        //btn.onClick.AddListener(() => OnSlotLeftClick(index));
        btn.onClick.AddListener(() => OnSlotClick(index));
        //slots.Add(new ItemStack());
    }

    private void OnSlotClick(int index)
    {
        // 결과 슬롯 클릭 시
        if (index == 9)
        {
        Refresh();
        return;
        }
        //  UI 갱신
        Refresh();
        //제작 결과 업데이트
        //UpdateCraftingResult();
    }

    private void UpdateCraftingResult()
    {
        // 3x3 제작 슬롯 입력 구성
        ItemType[,] input = new ItemType[3, 3];
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                int index = y * 3 + x;
                var stack = inventoryData.CGetSlot(index);
                input[y, x] = stack.IsEmpty ? ItemType.NONE : (ItemType)stack.item.itemType;
            }
        }

        // 제작 시도
        Itemslot result = craftingManager.trycraft(input);

        // 결과 슬롯(9) 갱신
        if (result != null)
        {
            // ItemType -> ItemDefinition 변환
            ItemDefinition def = ItemDatabase.GetDefinition(result.type);
            if (def != null)
                inventoryData.CSetSlot(9, new ItemStack(def, result.count));
            else
                inventoryData.CSetSlot(9, new ItemStack());
        }
        else
        {
            inventoryData.CSetSlot(9, new ItemStack());
        }

        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < inventoryData.CslotCount; i++)
        {
            var slot = inventoryData.CGetSlot(i);
            var icon = slotButtons[i].transform.GetChild(0).GetComponent<Image>();
            var countText = slotButtons[i].transform.GetChild(1).GetComponent<Text>();
            var countTextShadow = slotButtons[i].transform.GetChild(2).GetComponent<Text>();


            if (slot.IsEmpty)
            {
                icon.enabled = false;
                countText.text = "";
                countTextShadow.text = "";
            }
            else
            {
                icon.enabled = true;
                icon.sprite = slot.item.icon;
                countText.text = slot.count > 1 ? slot.count.ToString() : "";
                countTextShadow.text = slot.count > 1 ? slot.count.ToString() : "";
            }
        }

    }
}
