using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlantSim.Buttons;

public class PropertyPlane : MonoBehaviour {

    public int               GrowthCycle     { get; set; }
    public double            Biomass         { get; set; }
    public double            Height          { get; set; }
    public double            LeafArea        { get; set; }
    public double            WaterContent    { get; set; }
    public LightResponseType NutrientType    { get; set; }
    public bool              HaveInsects     { get; set; }
    public SunshineType SunshineType { get; set; }
    public TemperatureType TemperatureType { get; set; }

    Text GCLabel;
    Text BiomassLabel;
    Text HeightLabel;
    Text LeafAreaLabel;
    Text WaterContentLabel;
    Text NutrientLabel;
    Text InsectLabel;
    Text TemperatureLabel;
    Text SunshineLabel;

    void Awake()
    {
        if (GCLabel == null)
            GCLabel = transform.Find("GC Label").GetComponent<Text>();
        
        if (BiomassLabel == null)
            BiomassLabel = transform.Find("Biomass Label").GetComponent<Text>();
        
        if (HeightLabel == null)
            HeightLabel = transform.Find("Height Label").GetComponent<Text>();
        
        if (LeafAreaLabel == null)
            LeafAreaLabel = transform.Find("Leaf Area Label").GetComponent<Text>();
        
        if (WaterContentLabel == null)
            WaterContentLabel = transform.Find("Water Content Label").GetComponent<Text>();
        
        if (NutrientLabel == null)
            NutrientLabel = transform.Find("Nutrient Label").GetComponent<Text>();
        
        if (InsectLabel == null)
            InsectLabel = transform.Find("Insect Label").GetComponent<Text>();

        if (TemperatureLabel == null)
            TemperatureLabel = transform.Find("Temperature Label").GetComponent<Text>();

        if (SunshineLabel == null)
            SunshineLabel = transform.Find("Sunshine Label").GetComponent<Text>();
               
    }
	
	// Update is called once per frame
	void Update () 
    {
        PostionAdjust();
	}

    void OnEnable()
    {
        PostionAdjust();
        PropertyDisplay();
    }


    Vector3 POSITION_DETLA = new Vector2(7, -18);

    void PostionAdjust()
    {
        GetComponent<RectTransform>().position = Input.mousePosition + POSITION_DETLA;
    }

    void PropertyDisplay()
    {
        SetGCLabel();
        SetBiomassLabel();
        SetHeightLabel();
        SetLeafAreaLabel();
        SetWaterContentLabel();
        SetNutrientLabel();
        SetInsectLabel();
        SetTemperatureLabel();
        SetSunshineLabel();
    }

    void SetGCLabel()
    {
        GCLabel.text =
            (LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "生长周期： " : "Growth Cycle: ")
            + GrowthCycle;
    }

    void SetBiomassLabel()
    {
        BiomassLabel.text = 
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "生物量： " + Biomass.ToString("f1") + "克" :
            "Biomass: " + Biomass.ToString("f1") + "g";
    }

    void SetHeightLabel()
    {
        HeightLabel.text = 
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "株高: " + Height.ToString("f3") + "米" :
            "Height: " + Height.ToString("f3") + "m";
    }

    void SetLeafAreaLabel()
    {
        LeafAreaLabel.text = 
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "叶面积： " + LeafArea.ToString("f5") + "m2" :
            "Leaf Area: " + LeafArea.ToString("f5") + "m2";
    }

    string[] WC_Labels_En = new string[3] { "Plenty", "Low", "Too low" };
    string[] WC_Labels_Tw = new string[3] { "充足", "缺乏", "极度缺乏" };
    void SetWaterContentLabel()
    {
        //int index = MaizeParams.WaterContents.IndexOf(WaterContent);

        //if (LScene.GetInstance().Language == SystemLanguage.Chinese)
        //    WaterContentLabel.text = "水分含量： " + WC_Labels_Tw[index];
        //else
        //    WaterContentLabel.text = "Water Content: " + WC_Labels_En[index];

        if (WaterContent <= MaizeParams.WP)
        {
            WaterContentLabel.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                "水分含量： 极度缺乏" : "Water Content: Too low";
        }
        else if (WaterContent < MaizeParams.FC - 0.03)
        {
            WaterContentLabel.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                "水分含量： 缺乏" : "Water Content: Low";
        }
        else if (WaterContent <= MaizeParams.FC)
        {
            WaterContentLabel.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                "水分含量： 充足" : "Water Content: Plenty";
        }
        else
        {
            WaterContentLabel.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                "水分含量： 过多" : "Water Content: Too high";
        }
    }

    void SetNutrientLabel()
    {
        string label = "";

        SystemLanguage language = LScene.GetInstance().Language;

        switch (NutrientType)
        {
            case LightResponseType.NORMAL: 
                label = 
                    language == SystemLanguage.Chinese ?
                    "无" : "None"; 
                break;
            case LightResponseType.N_LACK: 
                label = 
                    language == SystemLanguage.Chinese ?
                    "氮" : "N"; 
                break;
            case LightResponseType.P_LACK: 
                label = 
                    language == SystemLanguage.Chinese ?
                    "磷" : "P"; 
                break;
            case LightResponseType.K_LACK: 
                label = 
                    language == SystemLanguage.Chinese ?
                    "钾" : "K"; 
                break;
            case LightResponseType.ALL_LACK:
                label =
                    language == SystemLanguage.Chinese ?
                    "全部" : "All";
                break;
        }

        NutrientLabel.text = 
            (language == SystemLanguage.Chinese ?
            "缺少的无机盐： " :
            "Lack of nutrient: ")
            + label;
    }

    //温度
    void SetTemperatureLabel()
    {
        string label = "";

        SystemLanguage language = LScene.GetInstance().Language;

        switch (TemperatureType)
        {
            case TemperatureType.Normal:
                label =
                    language == SystemLanguage.Chinese ?
                    "常温" : "Normal";
                break;
            case TemperatureType.High:
                label =
                    language == SystemLanguage.Chinese ?
                    "高温" : "High";
                break;
            case TemperatureType.Low:
                label =
                    language == SystemLanguage.Chinese ?
                    "低温" : "Low";
                break;
        }

        TemperatureLabel.text =
             (language == SystemLanguage.Chinese ?
             "温度情况： " :
             "Temperatrue is: ")
             + label;
    }

    //温光照
    void SetSunshineLabel()
    {
        string label = "";

        SystemLanguage language = LScene.GetInstance().Language;

        switch (SunshineType)
        {
            case SunshineType.Normal:
                label =
                    language == SystemLanguage.Chinese ?
                    "正常" : "Normal";
                break;
            case SunshineType.High:
                label =
                    language == SystemLanguage.Chinese ?
                    "高光照" : "High";
                break;
            case SunshineType.Low:
                label =
                    language == SystemLanguage.Chinese ?
                    "低光照" : "Low";
                break;
        }

        SunshineLabel.text =
             (language == SystemLanguage.Chinese ?
             "光照情况： " :
             "Illumination is: ")
             + label;
    }


    void SetInsectLabel()
    {
        if (LScene.GetInstance().Language == SystemLanguage.Chinese)
        {
            InsectLabel.text =
                "害虫： " +
                (HaveInsects ?
                "有" : "无");
        }
        else
            InsectLabel.text = 
                "Pest: " + HaveInsects.ToString();
    }
}
