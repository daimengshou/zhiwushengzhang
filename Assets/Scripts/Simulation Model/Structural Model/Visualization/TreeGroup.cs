using System.Collections.Generic;
using UnityEngine;

public class TreeGroup : BaseTree
{
    public List<TreeModel>   TreeModels        { get; private set; }

    public List<Vector3> TreeModelPoints = new List<Vector3>();
    public int TreeModelCount { get { return TreeModelPoints.Count; } }


    // Start is called before the first frame update
    void Start()
    {
        if (EnvironmentParams == null)
            EnvironmentParams = new EnvironmentParams();

        Selected = false;

        InitTreeModels();
    }

    void InitTreeModels()
    {
        TreeModels = new List<TreeModel>(TreeModelCount);

        for(int i = 0; i < TreeModelCount; i++)
        {
            TreeModel treeModel = gameObject.AddComponent<TreeModel>();

            treeModel.Initialize();
            treeModel.EnvironmentParams = EnvironmentParams;
            treeModel.TreeLocalPosition = TreeModelPoints[i];
            TreeModels.Add(treeModel);
        }
    }

    const int NORMAL_ANIMATION_COUNT = 40;

    public void NextDay()
    {
        foreach(var treeModel in TreeModels)
        {
            if (LScene.GetInstance().HaveAnimator)
            {
                if (treeModel.ComputeGrowthCycle() < 1)
                    treeModel.NextDay(true);
                else
                    treeModel.NextDay(false);

                ////出苗后
                if (treeModel.ComputeGrowthCycle() >= 1 && !treeModel.IsStopDevelopment)
                {
                    TreeAnimator animator = new TreeAnimator();
                    animator.PlayAnimation(treeModel.PairedBranchIndexes, treeModel.PairedOrganIndexes, LScene.GetInstance().AnimationCount);
                }
            }
            else
            {
                treeModel.NextDay(true);
            }
        }
    }

    #region ITreeParams
    public override int GrowthCycle
    {
        get
        {
            int GC = 0;
            foreach(var treeModel in TreeModels)
            {
                GC += treeModel.ComputeGrowthCycle();
            }

            return GC / TreeModels.Count;
        }
    }

    public override double Biomass
    {
        get
        {
            double biomass = 0;
            foreach (var treeModel in TreeModels)
            {
                biomass += treeModel.Biomass;
            }

            return biomass / TreeModels.Count;
        }
    }

    public override double AbovegroundBiomass
    {
        get
        {
            double biomass = 0;
            foreach (var treeModel in TreeModels)
            {
                biomass += treeModel.AbovegroundBiomass;
            }

            return biomass / TreeModels.Count;
        }
    }

    public override double Height
    {
        get
        {
            double height = 0;
            foreach (var treeModel in TreeModels)
            {
                height += treeModel.Height;
            }

            return height / TreeModels.Count;
        }
    }

    public override double LeafArea
    {
        get
        {
            double leafArea = 0;
            foreach (var treeModel in TreeModels)
            {
                leafArea += treeModel.LeafArea;
            }

            return leafArea / TreeModels.Count;
        }
    }
    #endregion


}
