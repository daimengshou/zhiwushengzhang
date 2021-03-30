using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NutrientDropdown : MonoBehaviour {

    private Text label;

    public void ValueChange(int value)
    {
        PlantSettingPlane.GetInstance().NutrientType = (LightResponseType)value;
    }

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        LabelInit();

        UpdateLabel();
        UpdateOptions();

        int value = (int)PlantSettingPlane.GetInstance().NutrientType;

        GetComponent<Dropdown>().value = value;

        //为保证显示的语言正确，需要重新设定Label
        transform.Find("Label").GetComponent<Text>().text =
            GetComponent<Dropdown>().options[value].text;
    }

    void LabelInit()
    {
        if (label == null)
            label = transform.parent.GetComponent<Text>();
    }

    public void UpdateLabel()
    {
        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "养分" :
            "Nutrient" ;
    }

    public void UpdateOptions()
    {
        List<Dropdown.OptionData> datas = GetComponent<Dropdown>().options;

        if (LScene.GetInstance().Language == SystemLanguage.Chinese)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                switch (i)
                {
                    case 0 :
                        datas[i].text = "正常";
                        break;
                    case 1 :
                        datas[i].text = "缺氮";
                        break;
                    case 2 :
                        datas[i].text = "缺磷";
                        break;
                    case 3 :
                        datas[i].text = "缺钾";
                        break;
                    default:
                        throw new System.IndexOutOfRangeException("Index out of range of nutrient dropdown.");
                }
            }
        }
        else
        {
            for (int i = 0; i < datas.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        datas[i].text = "Normal";
                        break;
                    case 1:
                        datas[i].text = "Lack of N";
                        break;
                    case 2:
                        datas[i].text = "Lack of P";
                        break;
                    case 3:
                        datas[i].text = "Lack of K";
                        break;
                    default:
                        throw new System.IndexOutOfRangeException("Index out of range of nutrient dropdown.");
                }
            }
        }
    }

}
