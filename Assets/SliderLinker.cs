using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderLinker : MonoBehaviour
{
    [System.Serializable]
    public class SliderTextPair
    {
        public Slider slider;  // ������ �����̴�
        public Text valueText; // ������ �ؽ�Ʈ
    }
    public SliderTextPair[] sliders;
}
