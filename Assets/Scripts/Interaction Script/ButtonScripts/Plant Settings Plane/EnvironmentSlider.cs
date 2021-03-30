using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SliderType
{
    MAX_TEMPERATURE, MIN_TEMPERATURE, 
    MAX_RELATIVE_HUMIDITY, MIN_RELATIVE_HUMIDITY
}

public class EnvironmentSlider : MonoBehaviour {

    public SliderType type = SliderType.MAX_TEMPERATURE;

    private Text label;
    private Text numbelLabel;

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        LabelsInit();

        PlantSettingPlane plane = PlantSettingPlane.GetInstance();

        int value = 20;

        switch (type)
        {
            case SliderType.MAX_TEMPERATURE:
                value = plane.DailyMaxTemperature;
                break;
            case SliderType.MIN_TEMPERATURE:
                value = plane.DailyMinTemperature;
                break;
            case SliderType.MAX_RELATIVE_HUMIDITY:
                value = plane.DailyMaxRelativeHumidity;
                break;
            case SliderType.MIN_RELATIVE_HUMIDITY:
                value = plane.DailyMinRelativeHumidity;
                break;
        }

        SetLabel();
        GetComponent<Slider>().value = value;
        SetNumberLabel(value);
    }

    void LabelsInit()
    {
        if (label == null)
            label = transform.parent.Find("Label").GetComponent<Text>();

        if (numbelLabel == null)
            numbelLabel = transform.Find("Number Label").GetComponent<Text>();
    }

    public void SliderValue(float value)
    {
        PlantSettingPlane plane = PlantSettingPlane.GetInstance();
        Slider slider = GetComponent<Slider>();

        switch (type)
        {
            case SliderType.MAX_TEMPERATURE:

                if (value >= plane.DailyMinTemperature)
                    plane.DailyMaxTemperature = (int)value;
                else
                    slider.value = plane.DailyMinTemperature;

                break;
            case SliderType.MIN_TEMPERATURE:

                if (value <= plane.DailyMaxTemperature)
                    plane.DailyMinTemperature = (int)value;
                else
                    slider.value = plane.DailyMaxTemperature;

                break;
            case SliderType.MAX_RELATIVE_HUMIDITY:

                if (value >= plane.DailyMinRelativeHumidity)
                    plane.DailyMaxRelativeHumidity = (int)value;
                else
                    slider.value = plane.DailyMinRelativeHumidity;

                break;
            case SliderType.MIN_RELATIVE_HUMIDITY:

                if (value <= plane.DailyMaxRelativeHumidity)
                    plane.DailyMinRelativeHumidity = (int)value;
                else
                    slider.value = plane.DailyMaxRelativeHumidity;

                break;
        }

        SetNumberLabel((int)slider.value);
    }

    private void SetNumberLabel(int value)
    {
        if (numbelLabel == null) return;

        numbelLabel.text = value.ToString();
    }

    public void SetLabel()
    {
        SystemLanguage language = LScene.GetInstance().Language;

        switch (type)
        {
            case SliderType.MIN_TEMPERATURE:
            case SliderType.MAX_TEMPERATURE:
                label.text =
                    language == SystemLanguage.Chinese ?
                    "最低和最高温度" :
                    "Min & Max Temperature";
                break;
            case SliderType.MIN_RELATIVE_HUMIDITY:
            case SliderType.MAX_RELATIVE_HUMIDITY:
                label.text =
                    language == SystemLanguage.Chinese ?
                    "最低和最高相对湿度" :
                    "Min & Max Relative Humidity";
                break;
        }
    }
}
