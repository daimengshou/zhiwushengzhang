using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotosyntheicModelDropdown : MonoBehaviour {

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

        int value = (int)PlantSettingPlane.GetInstance().PhotosyntheticModel;

        GetComponent<Dropdown>().value = value;
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
        PlantSettingPlane.GetInstance().PhotosyntheticModel = (PhotosyntheticModels)value;
    }

    public void UpdateLabel()
    {
        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "光合作用模型" :
            "Photosynthesis";
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
                        datas[i].text = "光响应";
                        break;
                    case 1:
                        datas[i].text = "比尔法则";
                        break;
                    default:
                        throw new System.IndexOutOfRangeException("Index out of range of photosynthesis model dropdown.");
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
                        datas[i].text = "Light Response";
                        break;
                    case 1:
                        datas[i].text = "Beer Rule";
                        break;
                    default:
                        throw new System.IndexOutOfRangeException("Index out of range of photosynthesis model dropdown.");
                }
            }
        }
    }
}
