using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class heldItemManager : MonoBehaviour
{
    public static heldItemManager Instance { get; private set; }
   
    public ItemStack heldStack = new ItemStack();
    public Image dragIcon;
    public Canvas canvas;

     private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); //안전장치 게임에 하나만 존재해야하는 싱글톤이니까 

        dragIcon.enabled = false;
    }

    private void Update()
    {
        if (dragIcon.enabled)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out var pos);
            dragIcon.rectTransform.anchoredPosition = pos;
        }
    }

    public void ShowIcon(bool show, Sprite sprite = null)
    {
        dragIcon.enabled = show;
        if (show && sprite != null)
            dragIcon.sprite = sprite;
    }
}
