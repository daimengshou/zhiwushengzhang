using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GrowthPeriod
{
    SEEDING_STAGE,      /*苗期*/
    JOINTING_STAGE,     /*拔节期*/
    TASSELING_STAGE,    /*抽雄期*/
    GRAIN_STAGE,        /*粒期*/
    MATURE_STAGE        /*成熟期*/
}

public class FunctionSim
{
    /// <summary>
    /// 获取平均PAR值（umol/（s * m^2））
    /// </summary>
    public static double PARRecption_Mean(TreeModel treeModel)
    {
        double leafArea;

        return PARRecption_Mean(treeModel, out leafArea);
    }

    /// <summary>
    /// 获取一定时间内平均PAR值（umol/（s * m^2））
    /// </summary>
    /// <param name="leafArea">叶片面积</param>
    public static double PARRecption_Mean(TreeModel treeModel, out double leafArea)
    {
        List<OrganIndex> organIndexes = treeModel.OrganIndexes;

        double energy = 0;
        leafArea = 0;

        foreach (OrganIndex organIndex in organIndexes)
        {
            if (organIndex.Type != OrganType.Leaf) continue;    //非叶片索引

            LeafIndex leafIndex = organIndex as LeafIndex;
            energy += leafIndex.TotalEnergy;    //辐射能量累加（单位 W）
            leafArea += leafIndex.LeafArea;      //叶面积累加（单位 ㎡）
        }

        double radiation = energy / leafArea; //辐照度（单位 W * m^-2）

        return SolarSim.IrradianceToPPFD(radiation);
    }

    /// <summary>
    /// 获取一定时间内单个叶片的PAR值（umol/（s * m^2））
    /// </summary>
    public static double PARRecption(LeafIndex index)
    {
        return SolarSim.IrradianceToPPFD(index.TotalEnergy / index.LeafArea);
    }

    /// <summary>
    /// 计算一定时间内一定面积累积的生物量（ug）
    /// </summary>
    /// <param name="duration">持续时间（s）</param>
    /// <param name="type">环境因素（正常 or 缺氮 or 缺磷 or 缺钾）</param>
    /// <returns>累积生物量（ug）</returns>
    public static double CumulativeBiomass(TreeModel treeModel, int duration, LightResponseType type = LightResponseType.NORMAL)
    {
        double CO2Absorption = 0.0;

        List<OrganIndex> organIndexes = treeModel.OrganIndexes;

        foreach (OrganIndex organIndex in organIndexes)
        {
            if (organIndex.Type != OrganType.Leaf) continue;

            /*
             * 叶片已经开始衰老
             * 不能进行光合作用
             */
            if (organIndex.Age > 12) continue;

            /*
             * 光合有效辐射，单位umol·m^-2·s^-1
             * 作用：用于计算光合速率
             */
            double PAR = PARRecption((organIndex as LeafIndex));

            /*
             * 光合速率，单位 umol·m^-2·s^-1
             * 作用：用于计算二氧化碳吸收量
             */
            double photosyntheticRate = LightResponse.PhotosyntheticRate(PAR, type);

            /*
             * 二氧化碳吸收量 单位 微克（ug）
             * 计算公式： 摩尔质量 * 光合速率 * 持续时间 * 面积
             * 作用：用于计算生物量
             */
            CO2Absorption += 44 * photosyntheticRate * duration * (organIndex as LeafIndex).LeafArea;
        }

        /*
         * 二氧化碳吸收量转换为生物量
         */
        return CO2Absorption * 30.0 / 44.0;
    }

    /// <summary>
    /// 根据积温计算生长周期
    /// </summary>
    public static int ComputeGrowthCycle(TreeModel treeModel)
    {
        return (int)(0.021 * treeModel.AccumulatedTemperature);
    }

    public static int ComputeDaysInGC(TreeModel treeModel)
    {
        EnvironmentParams envirParams = treeModel.EnvironmentParams;

        double dailyAT = (envirParams.DailyMaxTemperature + envirParams.DailyMinTemperature) / 2.0 - 8.0; //日有效积温

        return (int)Math.Ceiling(1 / (0.021 * dailyAT));
    }

    /// <summary>
    /// 判断植物在什么生长阶段
    /// </summary>
    public static GrowthPeriod GrowthPeriodJudgment(TreeModel treeModel)
    {
        return GrowthPeriodJudgment(treeModel.BranchIndexes, treeModel.OrganIndexes);
    }

    /// <summary>
    /// 判断植物在什么生长阶段
    /// </summary>
    public static GrowthPeriod GrowthPeriodJudgment(List<BranchIndex> branchIndexes, List<OrganIndex> organIndexes)
    {
        if (branchIndexes.Count < 9)                    //低于9GC，苗期
            return GrowthPeriod.SEEDING_STAGE; 
        if (!HaveMale(organIndexes))                    //无雄蕊，则在拔节期
            return GrowthPeriod.JOINTING_STAGE;
        else if (AreMaleDeveloped(organIndexes))        //有雄蕊，且还在发育，在抽雄期
            return GrowthPeriod.TASSELING_STAGE;
        else if (!IsMature(organIndexes))               //雄蕊停止发育，但还未成熟，在粒期
            return GrowthPeriod.GRAIN_STAGE;
        else                                            //已经成熟，在成熟期
            return GrowthPeriod.MATURE_STAGE;
    }

    private static bool HaveMale(List<OrganIndex> organIndexes)
    {
        foreach (OrganIndex organIndex in organIndexes)
        {
            if (organIndex.Type == OrganType.Flower)
                return true;
        }

        return false;
    }

    private static bool AreMaleDeveloped(List<OrganIndex> organIndexes)
    {
        foreach (OrganIndex organIndex in organIndexes)
        {
            if (organIndex.Type == OrganType.Flower && organIndex.Age <= MaizeParams.MALE_MAX_DEVELOPMENT_AGE)
                return true;
        }

        return false;
    }

    private static bool IsMature(List<OrganIndex> organIndexes)
    {
        foreach (OrganIndex organIndex in organIndexes)
        {
            if (organIndex.Type != OrganType.Fruit) continue;

            if (organIndex.Age <= MaizeParams.FEMALE_MAX_DEVELOPMENT_AGE)
                return false;
            else
                return true;
        }

        return false;
    }

    /// <summary>
    /// 光合产物分配给各个器官
    /// </summary>
    /// <param name="period">当前的发育期</param>
    public static void PhotosynthateAllocation(TreeModel treeModel, double biomass, GrowthPeriod period)
    {
        switch (period)
        {
            case GrowthPeriod.SEEDING_STAGE :
                PhotosynthateAllocation_SeedingStage(treeModel, biomass);     break;
            case GrowthPeriod.JOINTING_STAGE :
                PhotosynthateAllocation_JointingStage(treeModel, biomass);    break;
            case GrowthPeriod.TASSELING_STAGE :
                PhotosynthateAllocation_TasselingStage(treeModel, biomass);   break;
            case GrowthPeriod.GRAIN_STAGE :
                PhotosynthateAllocation_GrainStage(treeModel, biomass);       break;
            case GrowthPeriod.MATURE_STAGE :
                PhotosynthateAllocation_MatureStage(biomass);      break;
        }
    }

    private static void PhotosynthateAllocation_SeedingStage(TreeModel treeModel, double biomass)
    {
        PhotosynthateAllocation_JointingStage(treeModel, biomass); //与拔节期相同
    }

    private static void PhotosynthateAllocation_JointingStage(TreeModel treeModel, double biomass)
    {
        double rootRatio, stemRatio, leafRatio;
        ActualAllocationRatio(treeModel, out rootRatio, out stemRatio, out leafRatio);     //各类器官的分配比

        OrganPhotosynthateAllocation(treeModel, biomass * (1 - rootRatio));
    }

    private static void PhotosynthateAllocation_TasselingStage(TreeModel treeModel, double biomass)
    {
        double rootRatio, stemRatio, leafRatio, femaleRatio;
        ActualAllocationRatio(treeModel, out rootRatio, out stemRatio, out leafRatio, out femaleRatio);

        OrganPhotosynthateAllocation(treeModel, biomass * (1 - rootRatio));
    }

    private static void PhotosynthateAllocation_GrainStage(TreeModel treeModel, double biomass)
    {
        //OrganPhotosynthateAllocation_GrainStage(biomass);
        OrganPhotosynthateAllocation(treeModel, biomass);
    }

    private static void PhotosynthateAllocation_MatureStage(double biomass)
    {

    }

    /// <summary>
    /// 实际各类器官分配比
    /// 将光合产物按该比例分配给各类型器官
    /// </summary>
    private static void ActualAllocationRatio(TreeModel treeModel, out double root, out double stem, out double leaf)
    {
        double L = EnvironmentEffect.AvailabilityOfLight(treeModel);             //冠层光透过率
        double factor = EnvironmentEffect.AllocationLimitFactor(treeModel);      //影响因子

        if (GrowthPeriodJudgment(treeModel) != GrowthPeriod.SEEDING_STAGE)
            root = 3 * (MaizeParams.ROOT_POTENTIAL_ALLOCATION_RATIO / 3.0) * L / (L + 2 * factor);
        else
            root = 3 * MaizeParams.ROOT_POTENTIAL_ALLOCATION_RATIO * L / (L + 2 * factor);
        stem = 3 * MaizeParams.STEM_POTENTIAL_ALLOCATION_RATIO * factor / (2 * L + factor);
        leaf = 1 - root - stem;
    }

    /// <summary>
    /// 实际各类器官分配比（抽雄期）
    /// 将光合产物按该比例分配给各类型器官，仅适用于抽雄期
    /// </summary>
    private static void ActualAllocationRatio(TreeModel treeModel, out double root, out double stem, out double leaf, out double female)
    {
        ActualAllocationRatio(treeModel, out root, out stem, out leaf);

        root *= (1 - MaizeParams.ROOT_REALLOCATION_RATIO);
        stem *= (1 - MaizeParams.STEM_REALLOCATION_RATIO);
        leaf *= (1 - MaizeParams.LEAF_REALLOCATION_RATIO);

        female = 1 - root - stem - leaf;
    }

    /// <summary>
    /// 各节间分配光合产物
    /// </summary>
    /// <param name="biomass">分配到茎的生物量</param>
    private static void StemPhotosynthateAllocation(TreeModel treeModel, double biomass)
    {
        List<BranchIndex> indexes = treeModel.GetBranchIndexes();  //获取该类型器官的索引
        OrganPhotosynthateAllocation<BranchIndex>(MaizeParams.STEM_S, biomass, indexes);   //分配生物量
    }

    /// <summary>
    /// 各叶片分配光合产物
    /// </summary>
    /// <param name="biomass">分配到叶片的生物量</param>
    private static void LeafPhotosynthateAllocation(TreeModel treeModel, double biomass)
    {
        List<LeafIndex> indexes = treeModel.GetLeafIndexes();  //获取该类型器官的索引
        OrganPhotosynthateAllocation<LeafIndex>(MaizeParams.LEAF_S, biomass, indexes); //分配生物量
    }

    /// <summary>
    /// 各雌蕊分配光合产物
    /// </summary>
    /// <param name="biomass">分配到雌蕊的生物量</param>
    private static void FemalePhotosynthateAllocation(TreeModel treeModel, double biomass)
    {
        List<OrganIndex> indexes = treeModel.GetFemaleIndexes();    //获取该类型器官的索引
        OrganPhotosynthateAllocation<OrganIndex>(MaizeParams.FEMALE_S, biomass, indexes);     //分配生物量
    }

    /// <summary>
    /// 计算库强
    /// </summary>
    private static double GetSinkStrength(OrganType type, int age, double s)
    {
        /*
         * 当年龄大于发育年龄（EXPANDS对应的长度）或小于0时，库强为0
         * 库强 = 扩展函数（EXPANDS）与潜在库强的乘积
         */
        return age > MaizeParams.EXPANDS[(int)type].Length || age < 0 ?
            0 :
            MaizeParams.EXPANDS[(int)type][age - 1] * s;
    }

    /// <summary>
    /// 分配同类器官生物量
    /// </summary>
    private static void OrganPhotosynthateAllocation<T>(double s, double biomass, List<T> indexes)
        where T : OrganIndex    //限定该范式的类只能为继承OrganIndex的类
    {
        double totalSinkStrength = 0;   //该类器官总库强

        foreach (T index in indexes)
        {
            index.SinkStrength = GetSinkStrength(index.Type, index.Age, s);   //库强

            totalSinkStrength += index.SinkStrength;
        }

        foreach (T index in indexes)
        {
            /*
             * 计算每个器官分配到的生物量
             * 根据该器官库强占该类库强百分比确定分配到的生物量
             */
            index.Biomass += (biomass * index.SinkStrength / totalSinkStrength);
        }
    }

    /// <summary>
    /// 分配地上器官生物量
    /// </summary>
    private static void OrganPhotosynthateAllocation(TreeModel treeModel, double biomass)
    {
        treeModel.AbovegroundBiomass += biomass;

        List<BranchIndex> branchIndexes = treeModel.BranchIndexes;
        List<OrganIndex> organIndexes = treeModel.OrganIndexes;

        double totalSink = 0.0;

        /*
         * 计算各节间的库强
         * 当节间为短节间或长节间时
         * 分别乘以对应的系数
         * 使模拟的结果更加接近真实值
         */
        foreach (BranchIndex index in branchIndexes)
        {
            index.SinkStrength = GetSinkStrength(index.Type, index.Age, MaizeParams.STEM_S);

            if (index.Level == 0 && index.Index < MaizeParams.SHORT_INTERNODE_NUM)
                index.SinkStrength *= ((1.0 / MaizeParams.SHORT_INTERNODE_NUM) * (index.Index + 1));

            //if (index.Level == 0 && index.Index >= (MaizeParams.INTERNODE_NUM - MaizeParams.LONG_INTERNODE_NUM))
            //    index.SinkStrength *= MaizeParams.STEM_LONG_K;

            totalSink += index.SinkStrength;
        }

        /*
         * 计算各器官（除节间外）的库强
         * 当叶片所处的位置为短节间或长节间时
         * 分别乘以对应的系数
         * 使模拟结果更加接近真实值
         */
        foreach (OrganIndex index in organIndexes)
        {
            switch (index.Type)
            {
                case OrganType.Leaf:
                    index.SinkStrength = GetSinkStrength(index.Type, index.Age, MaizeParams.LEAF_S);    //叶片库强
                    (index as LeafIndex).SheathSinkStrength = GetSinkStrength(OrganType.Sheath, index.Age, MaizeParams.SHEATH_S);   //叶柄库强

                    if (index.From.Level == 0 && index.From.Index < MaizeParams.SHORT_INTERNODE_NUM)
                    {
                        index.SinkStrength *= ((1.0 / MaizeParams.SHORT_INTERNODE_NUM) * (index.From.Index + 1));
                        (index as LeafIndex).SheathSinkStrength *= ((1.0 / MaizeParams.SHORT_INTERNODE_NUM) * (index.From.Index + 1));
                    }

                    if (index.From.Level == 0 && index.From.Index >= (MaizeParams.INTERNODE_NUM - MaizeParams.LONG_INTERNODE_NUM))
                    {
                        index.SinkStrength *= (1.0 - (index.From.Index - (MaizeParams.INTERNODE_NUM - MaizeParams.LONG_INTERNODE_NUM)) * 0.1);
                        (index as LeafIndex).SheathSinkStrength *= (1.0 - (index.From.Index - (MaizeParams.INTERNODE_NUM - MaizeParams.LONG_INTERNODE_NUM)) * 0.1);
                    }

                    totalSink += (index as LeafIndex).SheathSinkStrength;

                    break;
                case OrganType.Fruit:
                    index.SinkStrength = GetSinkStrength(index.Type, index.Age, MaizeParams.FEMALE_S);
                    break;
                case OrganType.Flower:
                    index.SinkStrength = GetSinkStrength(index.Type, index.Age, MaizeParams.MALE_S);
                    break;
            }

            totalSink += index.SinkStrength;
        }

        /*
         * 根据各器官（包括节间）的库强与总库强之比
         * 确定其分配到的生物量
         */
        foreach (BranchIndex index in branchIndexes)
        {
            index.Biomass += biomass * index.SinkStrength / totalSink;
        }

        foreach (OrganIndex index in organIndexes)
        {
            index.Biomass += biomass * index.SinkStrength / totalSink;
        }
    }

    private static void OrganPhotosynthateAllocation_GrainStage(TreeModel treeModel, double biomass)
    {
        treeModel.AbovegroundBiomass += biomass;

        /*
         * 在粒期（Grain Stage）
         * 除雌蕊外所有的器官均不分配到生物量
         * 根据各雌蕊库强分配生物量
         */
        List<OrganIndex> organIndexes = treeModel.OrganIndexes;

        //总库强
        double totalSink = 0.0;

        /*
         * 计算每个雌蕊的当前库强
         */
        foreach (OrganIndex index in organIndexes)
        {
            if (index.Type != OrganType.Fruit) continue;

            index.SinkStrength = GetSinkStrength(OrganType.Fruit, index.Age, MaizeParams.FEMALE_S);

            totalSink += index.SinkStrength;
        }

        if (totalSink == 0) return;


        /*
         * 根据每个雌蕊的库强与总库强之比
         * 分配生物量
         */
        foreach (OrganIndex index in organIndexes)
        {
            if (index.Type != OrganType.Fruit) continue;

            index.Biomass += biomass * index.SinkStrength / totalSink;
        }
    }
}

