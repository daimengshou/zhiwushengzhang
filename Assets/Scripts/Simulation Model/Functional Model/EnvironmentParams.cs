using System;
using System.Collections.Generic;
using UnityEngine;
using PlantSim.Buttons;

public enum PhotosyntheticModels
{
    LightResponse, BeerRule
}

[Serializable]
public class EnvironmentParams
{
    //日最高、最低温度（℃）
    public int DailyMaxTemperature = 25;
    public int DailyMinTemperature = 25;

    //日最高、最低相对湿度（%）
    public int DailyMaxRelativeHumidity = 80;
    public int DailyMinRelativeHumidity = 80;

    //光照
    public float SunshineFactor = 1.0f;

    //土壤含水量
    public double WaterContent = 0.35;

    //养分缺失情况
    public LightResponseType NutrientType = LightResponseType.NORMAL;

    //温度情况 
    public TemperatureType TemperatureType = TemperatureType.Normal;

    //光照情况
    public SunshineType SunshineType = SunshineType.Normal;

    //风速（m/s）
    public double WindSpeed = 1;

    //有无病虫害
    public bool HaveInsects = false;

    //光合作用模型
    public PhotosyntheticModels PhotosyntheticModel = PhotosyntheticModels.BeerRule;

    public int ChangeDay = 1;

    public double DailyMeanTemperature 
    { 
        get { return (DailyMaxTemperature + DailyMinTemperature) / 2.0; } 
    }

    public double DailyMeanRelativeHumidity
    {
        get { return (DailyMaxRelativeHumidity + DailyMinRelativeHumidity) / 2.0; }
    }

    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <param name="envirParams"></param>
    public void DepthCopy(EnvironmentParams envirParams)
    {
        DailyMaxTemperature = envirParams.DailyMaxTemperature;
        DailyMinTemperature = envirParams.DailyMinTemperature;

        DailyMaxRelativeHumidity = envirParams.DailyMaxRelativeHumidity;
        DailyMinRelativeHumidity = envirParams.DailyMinRelativeHumidity;

        WaterContent = envirParams.WaterContent;
        NutrientType = envirParams.NutrientType;
        TemperatureType = envirParams.TemperatureType;
        SunshineType = envirParams.SunshineType;


        WindSpeed = envirParams.WindSpeed;

        HaveInsects = envirParams.HaveInsects;

        PhotosyntheticModel = envirParams.PhotosyntheticModel;

        ChangeDay = envirParams.ChangeDay;
    }

    string[] WC_Labels_En = new string[4] {"Too high", "Plenty", "Low", "Too low" };
    string[] WC_Labels_Tw = new string[4] {"过多", "充足", "缺乏", "极度缺乏" };

    public string ToString(SystemLanguage language = SystemLanguage.Chinese)
    {
        //水分含量的文字索引
        //int WC_index = MaizeParams.WaterContents.IndexOf(WaterContent);
        int WC_index = 0;
        if (WaterContent <= MaizeParams.WP)
        {
            WC_index = 3;
        }
        else if (WaterContent < MaizeParams.FC - 0.03)
        {
            WC_index = 2;
        }
        else if (WaterContent <= MaizeParams.FC)
        {
            WC_index = 1;
        }
        else
        {
            WC_index = 0;
        }

        //养分
        string nutrient_label = "";
        switch (NutrientType)
        {
            case LightResponseType.NORMAL:
                nutrient_label =
                    language == SystemLanguage.Chinese ?
                    "无" : "None";
                break;
            case LightResponseType.N_LACK:
                nutrient_label =
                    language == SystemLanguage.Chinese ?
                    "氮" : "N";
                break;
            case LightResponseType.P_LACK:
                nutrient_label =
                    language == SystemLanguage.Chinese ?
                    "磷" : "P";
                break;
            case LightResponseType.K_LACK:
                nutrient_label =
                    language == SystemLanguage.Chinese ?
                    "钾" : "K";
                break;
        }

        //温度
        string temperature_label = "";
        switch (TemperatureType)
        {
            case TemperatureType.Normal:
                temperature_label =
                    language == SystemLanguage.Chinese ?
                    "常温" : "Normal";
                //Debug.Log("Normal");
                break;
            case TemperatureType.High:
                temperature_label =
                    language == SystemLanguage.Chinese ?
                    "高温" : "High";
                //Debug.Log("High");
                break;
            case TemperatureType.Low:
                temperature_label =
                    language == SystemLanguage.Chinese ?
                    "低温" : "Low";
                //Debug.Log("Low");
                break;
        }

        //光照
        string sunshine_label = "";
        switch (SunshineType)
        {
            case SunshineType.Normal:
                sunshine_label =
                    language == SystemLanguage.Chinese ?
                    "正常" : "Normal";
                break;
            case SunshineType.High:
                sunshine_label =
                    language == SystemLanguage.Chinese ?
                    "高光照" : "High";
                break;
            case SunshineType.Low:
                sunshine_label =
                    language == SystemLanguage.Chinese ?
                    "低光照" : "Low";
                break;
        }


        //虫害
        string insect_label = language == SystemLanguage.Chinese ?
            HaveInsects ? "有" : "无" :
            HaveInsects.ToString();

        return 
            language == SystemLanguage.Chinese ?
            "\n" +
            "      温度：" + DailyMinTemperature + " ~ " + DailyMaxTemperature + "      \n" +
            "      相对湿度：" + DailyMinRelativeHumidity + " ~ " + DailyMaxRelativeHumidity + "      \n" +
            "      水分含量：" + WC_Labels_Tw[WC_index] + "      \n" +
            "      缺少的无机盐：" + nutrient_label + "      \n" +
            "      害虫：" + insect_label + "      \n\n"
            :
            "\n" +
            "      Temperature: " + DailyMinTemperature + " ~ " + DailyMaxTemperature + "      \n" +
            "      Relative Humidity: " + DailyMinRelativeHumidity + " ~ " + DailyMaxRelativeHumidity + "      \n" +
            "      Water Content: " + WC_Labels_En[WC_index] + "      \n" +
            "      Lack of Nutrient: " + nutrient_label + "      \n" +
            "      Pest:" + insect_label + "      \n\n";
    }
}

public interface IEnvirParams
{
    EnvironmentParams EnvironmentParams { get; set; }
}

