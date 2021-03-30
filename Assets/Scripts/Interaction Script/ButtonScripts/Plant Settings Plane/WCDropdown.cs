using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WCDropdown : MonoBehaviour {

    /*
     * 土壤中水分含量
     * 分别对应 充足、缺少和极度缺少
     */
    private double[] WaterContents = { 0.35, 0.28, 0.21 };
    private Text label;

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        LabelInit();

        UpdateLabel();
        UpdateOptions();

        int value = MaizeParams.WaterContents.IndexOf(PlantSettingPlane.GetInstance().WaterContent);

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

    public void ValueChange(int value)
    {
        PlantSettingPlane.GetInstance().WaterContent = MaizeParams.WaterContents[value];
    }

    public void UpdateLabel()
    {
        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "水分含量" :
            "Water Content";
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
                    case 0:
                        datas[i].text = "充足";
                        break;
                    case 1:
                        datas[i].text = "缺乏";
                        break;
                    case 2:
                        datas[i].text = "极度缺乏";
                        break;
                    default:
                        throw new System.IndexOutOfRangeException("Index out of range of water content dropdown.");
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
                        datas[i].text = "Plenty";
                        break;
                    case 1:
                        datas[i].text = "Low";
                        break;
                    case 2:
                        datas[i].text = "Too Low";
                        break;
                    default:
                        throw new System.IndexOutOfRangeException("Index out of range of water content dropdown.");
                }
            }
        }
    }
}
