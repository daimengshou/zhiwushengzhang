using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafIndex : OrganIndex, IMorphologicalSim
{
    private Mesh m_Mesh = null;

    private float m_DirectionEnergy = 0;    //直射能量（kJ）
    private float m_ScatterEnergy   = -1;   //散射能量（kJ）

    /*
     * 由于纹理存在，故存在透明面片或部分透明的面片
     * 先计算整个模型接受的散射能量，再根据非透明像素占比估算散射能量
     */
    private float m_ModelScatterEnergy = 0;

    private double m_MeshArea            = 0;   //模型面积 ㎡
    private double m_LeafArea_Insected   = 0;   //受病虫害影响的叶片面积 ㎡
    private double m_LeafArea_Uninsected = 0;   //为受病虫害影响的叶片面积 ㎡

    private float m_Width = 0;      //叶片最大宽度 m

    private double m_SheathSinkStrength = 0;    //叶柄库强

    /*
     * 细胞纹理
     * 用于模拟病虫害
     */
    private CellularTexture m_CelluarTex = null;

    /*
     * 存储前一天的Texture
     * 为加快后续病虫害模拟速度
     */
    private Texture m_Texture_PreDay = null;
    private int m_MeshHashCode = 0;

    /*
     * 该叶片的最低被啃食比例
     * 当叶片的比例低于该比例时
     * 病虫将不再啃食该叶片
     */
    private double m_LimitRatio = 0;

    //该叶片的模型
    public Mesh LeafMesh { get { return m_Mesh; } set { m_Mesh = value; } }

    //辐射能量相关
    public float DirectionEnergy        { get { return m_DirectionEnergy;               } set { m_DirectionEnergy = value;    } }
    public float ModelScatterEnergy     { get { return m_ModelScatterEnergy;            } set { m_ModelScatterEnergy = value; } }
    public float TotalEnergy            { get { return DirectionEnergy + ScatterEnergy; } }

    //像素相关
    public int   PixelCount             { get { return LeafMesh.PixelCount;         } }
    public int   VisualPixelCount       { get { return LeafMesh.VisualPixelCount;   } }
    public float VisibilityRatio        { get { return LeafMesh.VisibilityRatio;    } }

    //面积相关
    public double MeshArea            { get { return m_MeshArea;            } }
    public double LeafArea            { get { return m_LeafArea_Insected;   } set { m_LeafArea_Insected = value;   } }
    public double LeafArea_Uninsected { get { return m_LeafArea_Uninsected; } set { m_LeafArea_Uninsected = value; } }

    /*
     * 未放大或缩小时模型的面积和叶面积
     * 根据该数据跟实际叶面积大小
     * 即可确定模型的大小
     */
    public double UniformMeshArea { get { return LeafMesh.UniformMeshArea; } }
    public double UniformLeafArea { get { return LeafMesh.UniformLeafArea; } }

    //形态相关
    public float Width { get { return m_Width; } set { m_Width = value; } }

    public double SheathSinkStrength { get { return m_SheathSinkStrength; } set { m_SheathSinkStrength = value; } }

    //病虫害相关
    public CellularTexture CelluarTex    { get { return m_CelluarTex; } set { m_CelluarTex = value; } }
    public double          LimitRatio    { get { return m_LimitRatio; } set { m_LimitRatio = value; } }
    public double          InsectedRatio { get { return LeafArea_Uninsected == 0 ? 
                                                        1 : LeafArea / LeafArea_Uninsected; } 
                                         }

    public Texture Texture_PreDay { get { return m_Texture_PreDay; } set { m_Texture_PreDay = value; } }
    public int MeshHashCode_PreDay { get { return m_MeshHashCode; } set { m_MeshHashCode = value; } }

    public GameObject Belong { get { return m_GameObject; } set { m_GameObject = value; } }

    public float ScatterEnergy
    {
        get
        {
            if (m_ScatterEnergy < 0) m_ScatterEnergy = ModelScatterEnergy * VisibilityRatio;

            return m_ScatterEnergy;
        }
    }

    public LeafIndex()
    {
        this.Type = OrganType.Leaf;
    }

    public LeafIndex(int Index, BranchIndex From, GameObject _GameObject, int Age = 0)
        : base(Index, OrganType.Leaf, From, _GameObject, Age)
    {
        ComputeArea(_GameObject);           //计算面积（模型面积和叶片面积）
    }

    /// <summary>
    /// 计算面积
    /// </summary>
    private void ComputeArea(GameObject _Object)
    {
        /*
         * 计算模型的面积和叶面积
         * 用于后续辐射量的计算已经病虫害模拟等
         * 其中 MaizeParams.SCALE 将计算出的模型面积的单位从Unity中一单位转换成m
         */
        m_MeshArea = GameObjectOperation.GetOrganArea(_Object) * MaizeParams.SCALE * MaizeParams.SCALE;
        m_LeafArea_Uninsected = m_MeshArea * VisibilityRatio;
    }

    /// <summary>
    /// 清除记录能量的数据
    /// 每一次重新计算一定时间内吸收的能量前都要清除，防止数据累加出现错误
    /// </summary>
    public void ClearEnergyData()
    {
        DirectionEnergy = 0;
        m_ScatterEnergy = -1;
        ModelScatterEnergy = 0;
    }

    /// <summary>
    /// 形态模拟
    /// </summary>
    public void MorphologicalSim()
    {
        /*
         * 若年龄已经超过最大发育年龄
         * 则形态不发生改变
         */
        if (Age > MaizeParams.LEAF_MAX_DEVELOPMENT_AGE) return;

        /*
         * 被虫啃食的面积比例
         * 用来计算被虫啃食后的叶面积
         */
        double insectedRatio = InsectedRatio;

        float scale = 0.01f; //cm转m，统一单位

        /*
         * 根据公式计算叶片的长度和最大宽度
         * 后续根据叶片的长度和最大宽度确定绘制的GameObject大小
         */
        Length = (float)(MaizeParams.LEAF_SHAPE_K * Math.Pow(Biomass, MaizeParams.LEAF_SHAPE_Y)) * scale;   //m

        LeafArea_Uninsected = Biomass / MaizeParams.SPECIFIC_LEAF_WEIGHT * scale * scale;  //㎡
        LeafArea = LeafArea_Uninsected * insectedRatio;

        Width = (float)(LeafArea_Uninsected / (MaizeParams.LEAF_AREA_SHAPE_RATIO * Length));   //m
    }

    public double InsectSim(double insectIntake)
    {
        /*
         * 当该叶片的最低被啃食比例小于细胞纹理的最低被啃食比例时
         * 说明该比例无或者存在一定的问题
         * 因此将该比例设置为细胞纹理的最低被啃食比例
         */
        if (LimitRatio < CelluarTex.LimitRatio)
        {
            LimitRatio = CelluarTex.LimitRatio;

            /*
             * 调整最低被啃食比例
             * 减少重复感
             */
            LimitRatio += (0.8 - LimitRatio) * RandomNumer.Double();
        }

        /*
         * 叶片当前最大可被进食量
         * 用于计算当前实际被进食量
         */
        double maxInsectIntake = (InsectedRatio - LimitRatio) * LeafArea_Uninsected;

        /*
         * 若该叶片已经老去
         * 则不会被并病虫啃食
         * 若还未老去，则根据叶片最大被进食量计算实际被进食量
         * 当最大可被进食量不大于0时，说明无可被进食部分，因此实际被进食量为0
         * 当最大可被进食量大于当前病虫进食量，则说明该叶片满足病虫进食需求，因此实际进食量为病虫进食量
         * 当最大可被进食量大于0并且小于当前病虫进食量，则说明该叶片有被进食，但无法满足病虫进食需求
         * 因此最大可被进食量均被进食，故实际进食量为最大可被进食量
         */
        double actualInsectIntake = Age >= MaizeParams.LEAF_MAX_DEVELOPMENT_AGE ? 0 : 
                                    maxInsectIntake <= 0 ? 0 :
                                    maxInsectIntake >= insectIntake ? insectIntake : maxInsectIntake;

        //根据实际进食量计算叶片的面积
        LeafArea -= actualInsectIntake;

        int threshold;

        //设置纹理
        GameObjectOperation.SetTexture(Belong, GetWormholeTex(actualInsectIntake, out threshold));

        //设置阈值
        GameObjectOperation.SetThreshold(Belong, threshold);

        //返回剩余的病虫进食量
        return insectIntake - actualInsectIntake;
    }

    private Texture GetWormholeTex(double actualInsectIntake)
    {
        int threshold;

        return GetWormholeTex(actualInsectIntake, out threshold);
    }

    private Texture GetWormholeTex(double actualInsectIntake, out int threshold)
    {
        /*
         * 阈值
         * 用于后续关键帧动画中虫洞的模拟
         * 当为0时表示虫洞未发生变化
         */
        threshold = 0;

        /*
         * 存在的各种情况：
         * 1、该叶片不受病虫影响
         *     1.1、该叶片原先无病虫害，故现在也无病虫害，即叶面积（LeafArea）与未受病虫害的叶面积（LeafArea_Uninsected）相等
         *          故该模型的纹理无需替换
         *     1.2、该叶片原先有病虫害，再分两种情况：
         *          1.2.1、与昨日的模型相同，即今日模型的哈希值与昨日模型的哈希值相同，故叶片形态、纹理与昨日相同
         *          故直接调用昨日模型的纹理即可
         *          1.2.2、与昨日的模型不同，即该叶片发育到不同的阶段（成熟或衰老），故调用的纹理与昨日不同
         *          故重新生成虫洞纹理
         * 2、该叶片受病虫影响
         *    形态不同，故直接重新生成虫洞纹理
         */
        if (LeafArea == LeafArea_Uninsected)
        {
            return null;
        }
        else if (Texture_PreDay != null &&
            actualInsectIntake <= 0 && LeafMesh.GetHashCode() == MeshHashCode_PreDay)
        {
            return Texture_PreDay;
        }
        else
        {
            if (Texture_PreDay != null && Texture_PreDay.name == "Wormhole Texture")
            {
                if (LScene.GetInstance().HaveAnimator)
                    GameObjectOperation.DestroyTexWithoutAnimation(Texture_PreDay);
                else
                    GameObject.Destroy(Texture_PreDay);

                Texture_PreDay = null;
            }

            Texture leafTex = GameObjectOperation.GetTexture(Belong);

            Texture wormholeTex = CelluarTex.CreateWormhole(InsectedRatio, leafTex, out threshold);
            wormholeTex.name = "Wormhole Texture";

            /*
             * 清除纹理
             * 释放内存
             */
            if (leafTex.name == "Wormhole Texture")
                GameObject.DestroyObject(leafTex);

            return wormholeTex;
        }
    }

    public string Print()
    {
        string str = "";

        str += "Leaf Index\n";

        str += "  -Direction Energy: " + DirectionEnergy + "\n";
        str += "  -Scatter Energy:   " + ScatterEnergy + "\n";
        str += "  -Model Scatter Energy:  " + ModelScatterEnergy + "\n";
        str += "  -Total Energy:  " + TotalEnergy + "\n";

        str += "  ------------------\n";
        str += "  -NonTransparent Pixels Count: " + VisualPixelCount + "\n";
        str += "  -Pixels Count:                " + PixelCount + "\n";
        str += "  -NonTransparent Ratio:        " + VisibilityRatio + "\n";

        str += "  ------------------\n";
        str += "  -Mesh Area:  " + MeshArea + "\n";
        str += "  -Leaf Area:  " + LeafArea_Uninsected + "\n";

        return str;
    }

    public override string ToString()
    {
        string str = "";

        str += "Leaf Index" + Index + "  From " + From.Level + "[" + From.Index + "]" + "\n";

        str += "  Age: " + Age + "\n";
        str += "  Sink Strength: " + SinkStrength + "\n";
        str += "  Sheath Sink Strength: " + SheathSinkStrength + "\n";
        str += "  Biomass: " + Biomass + "\n";
        str += "  Leaf Area: " + LeafArea_Uninsected + "\n";
        str += "  Length: " + Length + "\n";
        str += "  Width: " + Width + "\n";

        return str;
    }
}

