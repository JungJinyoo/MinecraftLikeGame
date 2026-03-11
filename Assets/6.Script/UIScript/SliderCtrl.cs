using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro
using UnityEngine;
using UnityEngine.UI; // Slider, Text
using System.Text.RegularExpressions; 


public class SliderCtrl : MonoBehaviour
{
    [System.Serializable]
    public class SliderTextPair
    {
        public Slider slider;
        public Text valueText;
    }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public SliderTextPair[] sliders;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var pair in sliders)
        {
            if (pair.slider != null && pair.valueText != null)
            {
                UpdateText(pair.slider, pair.valueText, pair.slider.value);

                pair.slider.onValueChanged.AddListener((v) => UpdateText(pair.slider, pair.valueText, v));
            }
        }
    }
    void UpdateText(Slider slider, Text text, float val)
    {
        int percentage = Mathf.RoundToInt(val * 100);

        text.text = Regex.Replace(text.text, @"\d+", percentage.ToString());
    }

    private void OnDestroy()
    {
        foreach (var pair in sliders)
        {
            if (pair.slider != null)
                pair.slider.onValueChanged.RemoveAllListeners();
        }
    }
}
