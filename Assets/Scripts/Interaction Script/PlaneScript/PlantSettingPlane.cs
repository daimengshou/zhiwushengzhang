using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlantSettingPlane : MonoBehaviour
{
    //public  TreeModel           TreeModel         { get; set; }
    public EnvironmentParams    EnvironmentParams { get; set; }

    private Text label;

    private static PlantSettingPlane instance = null;

    public static PlantSettingPlane GetInstance()
    {
        if (instance == null)
            instance = GameObject.Find("Canvas").
                transform.Find("Plant Settings").GetComponent<PlantSettingPlane>();

        return GameObject.Find("Canvas").
                transform.Find("Plant Settings").GetComponent<PlantSettingPlane>();
    }


    void OnEnable()
    {
        UpdateLabel();
    }

    void OnDisable()
    {
        EnvironmentParams = null;
    }

    public void UpdateLabel()
    {
        if (label == null)
            label = transform.Find("Label").Find("Text").GetComponent<Text>();

        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "植物属性设置" :
            "Plant Setting";
    }

    public int DailyMaxTemperature
    {
        get { Validate(); return EnvironmentParams.DailyMaxTemperature; }
        set { Validate(); EnvironmentParams.DailyMaxTemperature = value; ChangeDayUpdate(); }
    }

    public int DailyMinTemperature
    {
        get { Validate(); return EnvironmentParams.DailyMinTemperature; }
        set { Validate(); EnvironmentParams.DailyMinTemperature = value; ChangeDayUpdate(); }
    }

    public int DailyMaxRelativeHumidity
    {
        get { Validate(); return EnvironmentParams.DailyMaxRelativeHumidity; }
        set { Validate(); EnvironmentParams.DailyMaxRelativeHumidity = value; ChangeDayUpdate(); }
    }

    public int DailyMinRelativeHumidity
    {
        get { Validate(); return EnvironmentParams.DailyMinRelativeHumidity; }
        set { Validate(); EnvironmentParams.DailyMinRelativeHumidity = value; ChangeDayUpdate(); }
    }

    public double WaterContent
    {
        get { Validate(); return EnvironmentParams.WaterContent; }
        set { Validate(); EnvironmentParams.WaterContent = value; ChangeDayUpdate(); }
    }

    public bool HaveInsects
    {
        get { Validate(); return EnvironmentParams.HaveInsects; }
        set { Validate(); EnvironmentParams.HaveInsects = value; ChangeDayUpdate(); }
    }

    public PhotosyntheticModels PhotosyntheticModel
    {
        get { Validate(); return EnvironmentParams.PhotosyntheticModel; }
        set { Validate(); EnvironmentParams.PhotosyntheticModel = value; ChangeDayUpdate(); }
    }

    public LightResponseType NutrientType
    {
        get { Validate(); return EnvironmentParams.NutrientType; }
        set { Validate(); EnvironmentParams.NutrientType = value; ChangeDayUpdate(); }
    }

    private void ChangeDayUpdate()
    {
        EnvironmentParams.ChangeDay = 
            LScene.GetInstance().Day < 1 ? 1 : LScene.GetInstance().Day;
    }

    private void Validate()
    {
        //if (TreeModel == null)
        //    throw new ArgumentNullException("No Tree Model");

        //if (EnvironmentParams == null)
        //    EnvironmentParams = TreeModel.EnvironmentParams;

        if (EnvironmentParams == null)
            throw new ArgumentNullException("No environment parameters.");
    }
}
