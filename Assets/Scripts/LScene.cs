using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LScene{

    private static LScene instance = null;   //实例

    public List<TreeModel> TreeModels { get; private set; }
    public List<TreeGroup> TreeGroups { get; private set; }

    private LScene()
    {
        var treeGroups = (GameObject.FindObjectsOfType(typeof(Transform)) as Transform[]).AsEnumerable()
            .Select(c => c.GetComponent<TreeGroup>())
            .Where(e => e != null);

        TreeModels = new List<TreeModel>();

        foreach(var treeGroup in treeGroups)
        {
            foreach(var treeModel in treeGroup.TreeModels)
            {
                TreeModels.Add(treeModel);
            }
        }

        TreeGroups = new List<TreeGroup>();
        TreeGroups.AddRange(treeGroups);
    }

    public static LScene GetInstance()
    {
        if (instance == null)
            instance = new LScene();

        return instance;
    }

    public static void Destroy()
    {
        instance = null;
    }

    //天数
    public int Day = 0;

    //是否有真实背景
    public bool HaveBackground = true;

    public int Duration = 40;

    //是否有关键帧
    public bool HaveAnimator = true;

    public float PlaybackSpeed = 1;

    const int NORMAL_ANIMATION_COUNT = 40;
    public int AnimationCount { get { return (int)(NORMAL_ANIMATION_COUNT / PlaybackSpeed); } }

    public bool IsPlaying()
    {
        return TreeAnimator.IsPlaying() || CameraMove.IsPlay();
    }

    public bool IsSettingParmsUsingIcon = false;

    public SystemLanguage Language = SystemLanguage.Chinese;

    public void LoadTrees()
    {
        PreComputerForLoadingTrees();

        foreach(var treeModel in TreeModels)
        {
            treeModel.Clear();
            XML_IO.Open("maize.xml", treeModel);
        }

        SceneInitForLoadingTrees();
        ChartInitForLoadingTrees();
    }

    public void NextDay()
    {
        Chart chart = Chart.GetInstance();

        for (int i = 0; i < TreeGroups.Count; i++)
        {
            var treeGroup = TreeGroups[i];

            treeGroup.NextDay();

            chart.AddBiomass(i, treeGroup.Biomass);
            chart.AddHeight(i, treeGroup.Height);
            chart.AddLeafArea(i, treeGroup.LeafArea);

            if (treeGroup.EnvironmentParams.ChangeDay == Day)
                chart.AddEnvironmentMarker(i, Day - 1,
                    treeGroup.EnvironmentParams.ToString(SystemLanguage.English), treeGroup.EnvironmentParams.ToString(SystemLanguage.Chinese));
        }

        chart.UpdateSmartChart();

        Day++;
        SetDayLabel();
    }

    public void InitDay()
    {
        Day = 0;
        SetDayLabel();
    }

    private void PreComputerForLoadingTrees()
    {
        new MaizeParams();
    }

    private void SceneInitForLoadingTrees()
    {
        InitDay();
    }

    private void ChartInitForLoadingTrees()
    {
        Chart chart = Chart.GetInstance();

        chart.InitDatas();
        chart.UpdateSmartChart();
    }

    private void SetDayLabel()
    {
        GameObject.Find("Day Label").GetComponent<Text>().text = 
            Language == SystemLanguage.Chinese ?
            "天数：" + Day :
            "Day: " + Day;
    }


    //测试
    public double k = 4.7;

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        UnityEditor.EditorApplication.isPaused = true;
#endif
        UnityEngine.Application.Quit();
    }
}
