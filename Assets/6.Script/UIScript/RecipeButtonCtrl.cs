using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeButtonCtrl : MonoBehaviour
{
    public Button targetButton;      // 변경할 버튼
    public Sprite normalSprite;      // 기본 이미지
    public Sprite clickedSprite;     // 클릭 시 이미지

    private RectTransform rectTransform;
    private Image buttonImage;
    private bool isClicked = false;

    private Vector2 defaultSize;
    private Vector2 defaultPosition;

    private RectTransform childImageRect;
    private Vector2 childDefaultPos;

    // 모든 ButtonCtrl을 관리하는 리스트
    private static List<RecipeButtonCtrl> allButtons = new List<RecipeButtonCtrl>();

    private void Awake()
    {
        // 리스트에 자신 추가
        allButtons.Add(this);
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 리스트에서 제거
        allButtons.Remove(this);
    }

    private void Start()
    {
        rectTransform = targetButton.GetComponent<RectTransform>();
        buttonImage = targetButton.GetComponent<Image>();

        // 원래 상태 저장
        defaultSize = rectTransform.sizeDelta;
        defaultPosition = rectTransform.anchoredPosition;

        // 자식 이미지 RectTransform 가져오기
        if (transform.childCount > 0)
        {
            childImageRect = transform.GetChild(0).GetComponent<RectTransform>();
            childDefaultPos = childImageRect.anchoredPosition;
        }

        targetButton.onClick.RemoveListener(OnButtonClick);
        targetButton.onClick.AddListener(OnButtonClick);

        // 초기 이미지 설정
        buttonImage.sprite = normalSprite;
    }

    public void OnButtonClick()
    {
        foreach (RecipeButtonCtrl btn in allButtons)
        {
            btn.SetClicked(btn == this);
        }
    }


    private void SetClicked(bool clicked)
    {
        isClicked = clicked;

        if (isClicked)
        {
            buttonImage.sprite = clickedSprite;
            rectTransform.sizeDelta = new Vector2(140, 104);
            rectTransform.anchoredPosition = defaultPosition; // 자기 자리 유지
            childImageRect.anchoredPosition = childDefaultPos + new Vector2(-8, 0);
        }
        else
        {
            buttonImage.sprite = normalSprite;
            rectTransform.sizeDelta = new Vector2(120, 104);
            rectTransform.anchoredPosition = defaultPosition; // 자기 자리 유지
            childImageRect.anchoredPosition = childDefaultPos;
        }
    }
}