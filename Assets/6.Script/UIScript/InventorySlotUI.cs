using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public Text countText;
    public Text countTextShadow;
    public int slotIndex;

    Canvas canvas;
    RectTransform dragIconRt;
    Image dragIcon;
    private int activeHotbar = 0;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        // 드래그 아이콘 준비(씬에 하나만 만들어서 재사용 가능)
    }

    void Update()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SelectHotbar(i);
        }
    }

    void SelectHotbar(int index) { activeHotbar = index; /* 장비 교체 등 */ }

    public void Set(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty)
        {
            icon.enabled = false;
            countText.text = "";
            countTextShadow.text = "";
        }
        else
        {
            icon.enabled = true;
            icon.sprite = stack.item.icon;
            countText.text = stack.count > 1 ? stack.count.ToString() : "";
            countTextShadow.text = stack.count > 1 ? stack.count.ToString() : "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭: 선택/드래그 대신 빠른 이동(shift+click), 우클릭: 한개 사용/반나누기 등 구현
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // 예: 핫바로 이동하거나 스택을 분리해서 넣기 등
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // dragIcon 보이게, set sprite
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRt != null)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out pos);
            dragIconRt.anchoredPosition = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 끝: 숨기기, 드롭 대상에 따라 Swap/Merge/Return
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 다른 슬롯에서 드롭되었을 때 호출. 상호작용 로직 처리
    }
}