using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class HealthBarUI : MonoBehaviour
{
    public FirstPersonController player;  // 플레이어 연결
    public GameObject heartPrefab;         // 하트 이미지 프리팹
    public Sprite fullHeart;
    public Sprite halfHeart;
    public float spacing = 5f;

    private Image[] hearts;
    private int maxHearts = 10; // 총 하트 수

    void Start()
    {
        hearts = new Image[maxHearts];
        if(player == null)
        {
            Debug.Log("플레이어생성전");
        }
        
        for (int i = 0; i < maxHearts; i++)
        {
            GameObject heart = Instantiate(heartPrefab, transform);
            RectTransform rt = heart.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (rt.sizeDelta.x + spacing), 0);
            hearts[i] = heart.GetComponent<Image>();
        }

        UpdateHearts();
    }

    void Update()
    {
        UpdateHearts();
    }

    void UpdateHearts()
    {
        // 한 하트 = 10hp 기준
        float hp = player.hp;
        for (int i = 0; i < maxHearts; i++)
        {
            if (hp >= 10)
            {
                hearts[i].sprite = fullHeart;
                hearts[i].enabled = true;
            }
            else if (hp >= 5)
            {
                hearts[i].sprite = halfHeart;
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false; // 체력이 없는 경우 안보이게
            }

            hp -= 10; // 다음 하트 계산
        }
    }
}