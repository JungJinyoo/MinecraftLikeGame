using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class HungerBarUI : MonoBehaviour
{
    public FirstPersonController player;
    public GameObject hungerPrefab;   // πË∞Ì«ƒ UI «¡∏Æ∆’ (full ¿ÃπÃ¡ˆ)
    public Sprite fullFood;
    public Sprite halfFood;
    public float spacing = 5f;
    public int maxUnits = 10; // √— πË∞Ì«ƒ ¥‹¿ß

    private Image[] units;



    void Start()
    {
        units = new Image[maxUnits];
        for (int i = 0; i < maxUnits; i++)
        {
            GameObject unit = Instantiate(hungerPrefab, transform);
            RectTransform rt = unit.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * (rt.sizeDelta.x + spacing), 0);
            units[i] = unit.GetComponent<Image>();
        }
        UpdateHunger();
    }

    void Update()
    {
        UpdateHunger();
    }

    void UpdateHunger()
    {
        float hunger = player.hunger; // 0~100
        for (int i = 0; i < maxUnits; i++)
        {
            if (hunger >= (i + 1) * 10) // full
            {
                units[i].sprite = fullFood;
                units[i].enabled = true;
            }
            else if (hunger >= (i * 10 + 5)) // half
            {
                units[i].sprite = halfFood;
                units[i].enabled = true;
            }
            else // æ»∫∏¿”
            {
                units[i].enabled = false;
            }
        }
    }
}