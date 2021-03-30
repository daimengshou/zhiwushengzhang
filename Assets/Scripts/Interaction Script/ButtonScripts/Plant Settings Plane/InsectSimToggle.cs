using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InsectSimToggle : MonoBehaviour {

    private Text label;

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        LabelInit();

        GetComponent<Toggle>().isOn = PlantSettingPlane.GetInstance().HaveInsects;

        UpdateLabel();
    }

    void LabelInit()
    {
        if (label == null)
            label = transform.Find("Label").GetComponent<Text>();
    }

    public void ValueChange(bool value)
    {
        PlantSettingPlane.GetInstance().HaveInsects = value;
    }

    public void UpdateLabel()
    {
        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "虫害模拟" :
            " Insect Simulation";
    }
}
