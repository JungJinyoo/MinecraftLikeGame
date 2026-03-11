using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public InventoryData inventoryData;
    public Transform gridHotbar;
    public GameObject slotPrefab;

    public int selectedIndex = 0;          // 현재 선택된 슬롯 (0~8)
    public Image highlightImage;           // 선택된 슬롯 강조용 이미지 (UI 오브젝트)
    public Vector3 highlightOffset = Vector3.zero; // 강조 이미지 위치 보정값 (필요시)

    public InventoryUI inventoryUI;

    private List<Button> hotbarButtons;

    //테스트용 추가
    public static HotbarUI Instance;

    float ClickHoldTime = 0f;
    public float useHoldDuration = 2f;

    public Animator playerAnimator;
    private bool isEating = false;


    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        while (inventoryData == null || inventoryData.SlotCount < 36)
            yield return null;

        hotbarButtons = new List<Button>();
        for (int i = 27; i < 36; i++)
        {
            var go = Instantiate(slotPrefab, gridHotbar);
            var btn = go.GetComponent<Button>();
            hotbarButtons.Add(btn);
            int index = i;
            btn.onClick.AddListener(() => OnHotbarClick(index));
        }
        Refresh();
    }

    private void Update()
    {
        if (hotbarButtons == null || hotbarButtons.Count == 0) return;

        // 🔸 마우스 휠로 선택 변경
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            selectedIndex = (selectedIndex + 1) % hotbarButtons.Count;
        else if (scroll < 0f)
            selectedIndex = (selectedIndex - 1 + hotbarButtons.Count) % hotbarButtons.Count;

        // 🔸 숫자키 1~9로 선택
        for (int i = 0; i < hotbarButtons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                selectedIndex = i;
        }

        // 🔸 하이라이트 위치 업데이트
        UpdateHighlight();

        // 테스트용 추가(음식 먹는거)
        // 우클릭 누르는 중이면 시간 누적
        if (Input.GetMouseButton(1))
        {
            ClickHoldTime += Time.deltaTime;

            if (!isEating)
            {
                isEating = true;
                playerAnimator.SetBool("isEating", true);
            }

            // 2초 이상 누르면 아이템 사용
            if (ClickHoldTime >= useHoldDuration)
            {
                int currentSlot = selectedIndex + 27;
                InventoryManager.Instance.UseItem(currentSlot);

                // 한 번만 사용되게 초기화
                ClickHoldTime = 0f;
            }
        }
        // 우클릭 떼면 타이머 초기화
        if (Input.GetMouseButtonUp(1))
        {
            isEating = false;
            ClickHoldTime = 0f;
            playerAnimator.SetBool("isEating", false); // 애니메이션 종료
        }
    }

    void UpdateHighlight()
    {
        if (highlightImage == null) return;

        // 현재 선택된 슬롯의 RectTransform
        RectTransform targetSlot = hotbarButtons[selectedIndex].GetComponent<RectTransform>();

        // 하이라이트 이미지 위치를 해당 슬롯 위치로 이동
        highlightImage.rectTransform.position = targetSlot.position + highlightOffset;
    }

    void OnHotbarClick(int index)
    {
        // InventoryUI의 OnSlotClick(index) 로직을 재사용하거나
        // 이벤트로 통신하게 만들어도 됩니다.
    }

    public void Refresh()
    {
        //if (inventoryData == null) return;
        if (inventoryData == null || hotbarButtons == null || hotbarButtons.Count == 0)
        {
            Debug.LogWarning("⚠️ HotbarUI.Refresh() skipped — hotbarButtons not ready.");
            return;
        }

        for (int i = 0; i < hotbarButtons.Count; i++)
        {
            int index = 27 + i;
            if (index >= inventoryData.SlotCount)
            {
                Debug.LogWarning($"HotbarUI: index {index} out of range!");
                continue;
            }
            var slot = inventoryData.GetSlot(index);
            var icon = hotbarButtons[i].transform.GetChild(0).GetComponent<Image>();
            var countText = hotbarButtons[i].transform.GetChild(1).GetComponent<Text>();
            var countTextShadow = hotbarButtons[i].transform.GetChild(2).GetComponent<Text>();

            if (icon == null || countText == null || countTextShadow == null)
            {
                Debug.LogError($"Hotbar button {i} missing UI component!");
                continue;
            }

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
     //핫바 추가
    public ItemType GetSelectedItemType()
    {
        var slot = inventoryData.GetSlot(selectedIndex + 27); //핫바슬롯 동기화를 위한 +27
        if (slot.IsEmpty) return ItemType.HAND; // 빈칸이면 맨손
        return (ItemType)slot.item.itemType;
    }

    //테스트용 추가
    public void ClearHotbarUI()
    {
        if (inventoryData == null) return;

        // 일반적으로 핫바 슬롯은 27~35번
        for (int i = 27; i < 36; i++)
        {
            inventoryData.SetSlot(i, new ItemStack());
        }

        Refresh();
    }

}