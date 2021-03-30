using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ToucanSystems;

public enum ChartType
{
    Biomass, Height, LeafArea
}

public class Chart : MonoBehaviour {

    private static Chart instance;

    public static Chart GetInstance()
    {
        if (instance == null)
            instance = GameObject.Find("Canvas").transform.
                Find("Chart Plane").transform.
                Find("Chart").transform.Find("SmartChart").GetComponent<Chart>();

        return instance;
    }

    void OnEnable()
    {
        Init();
        UpdateSmartChart();
    }

    void Start()
    {
        UpdateLabel();
        LanguageNotification.GetInstance().AddListener(UpdateLabel);
    }
	
	// Update is called once per frame
	void Update () 
    {
        Detection();
	}

    private SmartChart smartChart;
    public Text title;
    public Text XAxisLabel;

    private List<List<Vector2>> biomass_plants = new List<List<Vector2>>();
    private List<List<Vector2>> height_plants = new List<List<Vector2>>();
    private List<List<Vector2>> leafArea_plants = new List<List<Vector2>>();

    private List<List<EnvironmentMarker>> envirMarkers = new List<List<EnvironmentMarker>>();

    public void AddBiomass(int index, double biomass)
    {
        //防止index越界
        while (index >= biomass_plants.Count)
            biomass_plants.Add(new List<Vector2>());

        biomass_plants[index].Add(new Vector2(biomass_plants[index].Count, (float)biomass));
    }

    public void AddHeight(int index, double height)
    {
        //防止index越界
        while (index >= height_plants.Count)
            height_plants.Add(new List<Vector2>());

        height_plants[index].Add(new Vector2(height_plants[index].Count, (float)height));
    }

    public void AddLeafArea(int index, double leafArea)
    {
        //防止index越界
        while (index >= leafArea_plants.Count)
            leafArea_plants.Add(new List<Vector2>());

        leafArea_plants[index].Add(new Vector2(leafArea_plants[index].Count, (float)leafArea));
    }

    public void AddEnvironmentMarker(int index_plant, int index_marker, string value_en, string value_tw)
    {
        while (index_plant >= envirMarkers.Count)
            envirMarkers.Add(new List<EnvironmentMarker>());

        envirMarkers[index_plant].Add(new EnvironmentMarker(index_marker, value_en, value_tw));
    }

    private ChartType chartType = ChartType.Biomass;
    public void SetChartType(ChartType type)
    {
        chartType = type;
        UpdateSmartChart();
        UpdateLabel();
    }

    private Texture2D screenShot;
    public void PointerEnter()
    {
        StartCoroutine(SetScreenShot());
    }

    IEnumerator SetScreenShot()
    {
        yield return new WaitForEndOfFrame();

        Rect rect = new Rect(0, 0, Screen.width, Screen.height);

        screenShot = new Texture2D(Screen.width, Screen.height);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
    }

    public void PointerExit()
    {
        GameObject.DestroyImmediate(screenShot);
        screenShot = null;
    }

    private Color32[] lineColors;
    private void Detection()
    {
        if (screenShot == null) return;

        /*
         * 获取指针位置
         * 用于读取对应纹理中的颜色
         */
        Vector2 mousePosition = Input.mousePosition;

        int x = (int)mousePosition.x;
        int y = (int)mousePosition.y;


        Color pixelColor = screenShot.GetPixel(x, y);

        for (int i = 0; i < lineColors.Length; i++)
        {
            if (lineColors[i] == pixelColor)
                LScene.GetInstance().TreeGroups[i].Selected = true;
            else
                LScene.GetInstance().TreeGroups[i].Selected = false;
        }
    }

    public void UpdateSmartChart()
    {
        if (smartChart == null || !gameObject.activeInHierarchy) return;

        SmartChartData[] datas = smartChart.chartData;
        List<List<Vector2>> plantDatas = null;

        switch (chartType)
        {
            case ChartType.Biomass:
                plantDatas = biomass_plants;
                break;
            case ChartType.Height:
                plantDatas = height_plants;
                break;
            case ChartType.LeafArea:
                plantDatas = leafArea_plants;
                break;
            default:
                throw new System.Exception("Chart type error!");
        }

        //导入图标数据
        for (int i = 0; i < datas.Length; i++ )
        {
            datas[i].data = i >= plantDatas.Count ?  
                new Vector2[0] : plantDatas[i].ToArray();

            if (i < envirMarkers.Count)
                datas[i].envirMarker = envirMarkers[i].ToArray();
        }

        smartChart.chartData = datas;

        if (datas[0].data.Length > 0)
        {
            Vector2 maxPoint = datas[0].data[datas[0].data.Length - 1];
            smartChart.maxXValue = maxPoint.x > 0 ? maxPoint.x : 1;
            smartChart.minXValue = 0;
            smartChart.maxYValue = MaxYValue(datas);
            smartChart.minYValue = 0;
        }

        smartChart.UpdateChart();
    }

    private float MaxYValue(SmartChartData[] datas)
    {
        float maxValue = 0;

        for (int i = 0; i < datas.Length; i++)
        {
            for (int j = 0; j < datas[i].data.Length; j++)
            {
                if (datas[i].data[j].y > maxValue)
                    maxValue = datas[i].data[j].y;
            }
        }

        return maxValue > 0 ? maxValue : 1;
    }

    private void Init()
    {
        if (smartChart == null)
            smartChart = transform.GetComponent<SmartChart>();

        if (lineColors == null)
        {
            lineColors = new Color32[smartChart.chartData.Length];
            for (int i = 0; i < smartChart.chartData.Length; i++)
                lineColors[i] = smartChart.chartData[i].dataLineColor;
        }
    }

    public void InitDatas()
    {
        ClearDatas<Vector2>(ref biomass_plants);
        ClearDatas<Vector2>(ref height_plants);
        ClearDatas<Vector2>(ref leafArea_plants);
        ClearDatas<EnvironmentMarker>(ref envirMarkers);
    }

    private void ClearDatas<T>(ref List<List<T>> dataList)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            dataList[i].Clear();
        }

        dataList.Clear();
        dataList = new List<List<T>>();
    }

    public void UpdateLabel()
    {
        if (title == null)
            transform.parent.Find("Title").GetComponent<Text>();

        if (XAxisLabel == null)
            transform.parent.Find("X Axis Label").GetComponent<Text>();

        if (LScene.GetInstance().Language == SystemLanguage.Chinese)
        {
            switch (chartType)
            {
                case ChartType.Biomass:
                    title.text = "生物量";
                    break;
                case ChartType.Height:
                    title.text = "株高";
                    break;
                case ChartType.LeafArea:
                    title.text = "叶片面积";
                    break;
                default:
                    throw new System.Exception("Chart type error!");
            }

            XAxisLabel.text = "天数";
        }
        else
        {
            switch (chartType)
            {
                case ChartType.Biomass:
                    title.text = "Biomass";
                    break;
                case ChartType.Height:
                    title.text = "Height";
                    break;
                case ChartType.LeafArea:
                    title.text = "Leaf Area";
                    break;
                default:
                    throw new System.Exception("Chart type error!");
            }

            XAxisLabel.text = "Day";
        }
    }
}
