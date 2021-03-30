#define ANIMATION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cakeslice;

using System.IO;

namespace PlantSim.Buttons
{
    public class NextButton : BaseLabelBtn
    {
        void Update()
        {
            Button button = GetComponent<Button>();

            /*
             * 当仍处于动画中时
             * 禁止按钮可交互
             * 防止数据出现
             */
            if (LScene.GetInstance().IsPlaying())
                button.interactable = false;
            else
                button.interactable = true;
        }

        public void Click()
        {
            StartCoroutine(NextWithDuration());
        }

        //private void Next()
        //{
        //    var treeGroups = LScene.GetInstance().TreeGroups;
        //    Chart chart = Chart.GetInstance();

        //    for (int i = 0; i < treeGroups.Count; i++)
        //    {
        //        var treeGroup = treeGroups[i];

        //        treeGroup.NextDay();

        //        chart.AddBiomass(i, treeGroup.Biomass);
        //        chart.AddHeight(i, treeGroup.Height);
        //        chart.AddLeafArea(i, treeGroup.LeafArea);

        //        if (treeGroup.EnvironmentParams.ChangeDay == LScene.GetInstance().Day)
        //            chart.AddEnvironmentMarker(i, LScene.GetInstance().Day - 1,
        //                treeGroup.EnvironmentParams.ToString(SystemLanguage.English), treeGroup.EnvironmentParams.ToString(SystemLanguage.Chinese));
        //    }

        //    chart.UpdateSmartChart();

        //    LScene.GetInstance().NextDay();

        //}

        private IEnumerator NextWithDuration()
        {
            LScene scene = LScene.GetInstance();

            for (int i = 0; i < LScene.GetInstance().Duration; i++)
            {
                scene.NextDay();

                yield return new WaitWhile(TreeAnimator.IsPlaying);
            }

            OutlineEffect.Instance.UpdateOutlineControl();
        }

    }
}

//public class NextButton : MonoBehaviour {

//    void Start()
//    {
//        LanguageNotification.GetInstance().AddListener(UpdateLabel);
//    }

//    void Update()
//    {
//        Button button = GetComponent<Button>();

//        /*
//         * 当仍处于动画中时
//         * 禁止按钮可交互
//         * 防止数据出现
//         */
//        if (TreeAnimator.IsPlaying() || CameraMove.IsPlay())
//            button.interactable = false;
//        else
//            button.interactable = true;
//    }

//    public void Click()
//    {
//        StartCoroutine(NextUntilCondition(50));
//	}

//    private void Next()
//    {
//        var treeGroups = LScene.GetInstance().TreeGroups;
//        Chart chart = Chart.GetInstance();

//        for(int i = 0; i < treeGroups.Count; i++)
//        {
//            var treeGroup = treeGroups[i];

//            treeGroup.NextDay();

//            chart.AddBiomass(i, treeGroup.Biomass);
//            chart.AddHeight(i, treeGroup.Height);
//            chart.AddLeafArea(i, treeGroup.LeafArea);

//            if (treeGroup.EnvironmentParams.ChangeDay == LScene.GetInstance().Day)
//                chart.AddEnvironmentMarker(i, LScene.GetInstance().Day - 1,
//                    treeGroup.EnvironmentParams.ToString(SystemLanguage.English), treeGroup.EnvironmentParams.ToString(SystemLanguage.Chinese));
//        }

//        chart.UpdateSmartChart();

//        LScene.GetInstance().NextDay();

//    }

//    /// <summary>
//    /// 一直循环直至玉米成熟
//    /// 主要用于视频录制
//    /// 并采用协程时视觉效果更佳
//    /// </summary>
//    /// <returns></returns>
//    private IEnumerator NextUnitlMature()
//    {
//        while (true)
//        {
//            var treeModels = LScene.GetInstance().TreeModels;

//            foreach (TreeModel treeModel in treeModels)
//            {
//                /*
//                    * 为使视觉效果更佳（即过渡更自然）
//                    * 采用协程
//                    * 当当前程序仍然处于动画展示过程（玉米形态变化动画和相机移动）时，
//                    * 不计算后一日的形态变化
//                    * 直至动画展示过程完成
//                    */
//                yield return new WaitUntil(() =>
//                { return !TreeAnimator.IsPlaying() && !CameraMove.IsPlay(); }
//                                            );

//                if (treeModel.CurrentStep >= treeModel.MaxStep)
//                    break;

//                Next();
//            }
//        }
//    }

//    private IEnumerator NextUntilCondition(int num)
//    {
//        for (int i = 0; i < LScene.GetInstance().Duration; i++)
//        {
//            Next();

//            yield return new WaitWhile(TreeAnimator.IsPlaying);
//        }

//        UpdateOutlineControl();
//    }

//    private void UpdateOutlineControl()
//    {
//        foreach (var control in OutlineEffect.Instance.controls)
//        {
//            control.UpdateOutlines();
//        }
//    }


//    private Text label;
//    public void UpdateLabel()
//    {
//        if (label == null)
//            label = transform.Find("Text").GetComponent<Text>();

//        label.text =
//            LScene.GetInstance().Language == SystemLanguage.Chinese ?
//            "次日" : "Next";
//    }

//    //测试
//    //private double min = 4.6;
//    //private double max = 4.9;

//    //private void Test()
//    //{
//    //    TreeModel model = Scene.GetInstance().TreeModel;
//    //    string str = "";
//    //    str += Scene.GetInstance().k + " ";

//    //    str += model.AbovegroundBiomass + " ";
//    //    str += model.Biomass + " ";
//    //    str += model.Height() + " ";
//    //    str += model.LeafArea() + "\n";

//    //    string filePath = System.Environment.CurrentDirectory + "\\ResultData\\adaption.data";
//    //    FileStream stream = new FileStream(filePath, File.Exists(filePath) ? FileMode.Append : FileMode.Create);
//    //    StreamWriter writer = new StreamWriter(stream);

//    //    writer.WriteLine(str);  //写入

//    //    //完成写入，内存释放
//    //    writer.Close();
//    //    stream.Close();

//    //    if (model.Biomass < 1850)
//    //        min = Scene.GetInstance().k;
//    //    else if (model.Biomass > 1950)
//    //        max = Scene.GetInstance().k;

//    //    Scene.GetInstance().k = (min + max) / 2.0;

//    //}

//}
