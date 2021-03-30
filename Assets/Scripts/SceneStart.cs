using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class SceneStart : MonoBehaviour
{
    [SerializeField]
    private EnvironmentParams environmentParams;

    public void OnSceneStartForExp()
    {
        LScene scene = LScene.GetInstance();

        bool defaultParam = scene.HaveAnimator;

        scene.LoadTrees();

        //不进行动画效果
        scene.HaveAnimator = false;
        for (int i = 0; i < 20; i++)
        {
            scene.NextDay();
        }

        OutlineEffect.Instance.UpdateOutlineControl();

        if (environmentParams != null)
        {
            foreach(var treeGroup in scene.TreeGroups)
            {
                treeGroup.EnvirParamsDepthCopy(environmentParams);
            }
        }

        scene.HaveAnimator = defaultParam;
    }
}
