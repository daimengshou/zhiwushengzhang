using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchIndex : OrganIndex, IMorphologicalSim
{
    
    #region 成员变量
    private int         m_Level = -1;                   //几级枝干
    private int         m_BottomVerticesIndex = -1;     //用于绘制该枝干的底端起始顶点索引
    private int         m_TopVerticesIndex = -1;        //用于绘制该枝干的顶端起始顶点索引
    private BranchIndex m_Previous = null;              //枝干的前驱

    private LTerm        m_Term = null;                  //枝干对应的符号
    #endregion

    public BranchIndex()
    {
        this.Type = OrganType.Branch;
    }

    public BranchIndex(int Level, int Index, int BottomVerticesIndex, int TopVerticesIndex, Vector3 Rotation, BranchIndex From, BranchIndex Previous, GameObject _GameObject, int Age = 0) 
        : base(Index, OrganType.Branch, From, _GameObject, Age)
    {
        m_Level = Level;
        m_BottomVerticesIndex = BottomVerticesIndex;
        m_TopVerticesIndex = TopVerticesIndex;
        m_Rotation = Rotation;
        m_Previous = Previous;
    }

    #region 获取成员变量
    public int Level
    {
        get { return m_Level; }
        set { m_Level = value; }
    }

    public BranchIndex Previous
    {
        get { return m_Previous; }
        set { m_Previous = value; }
    }

    public Vector3 RelativeRotation
    {
        get { return Rotation - From.Rotation; }
    }

    public int BottomVerticesIndex
    {
        get { return m_BottomVerticesIndex; }
        set { m_BottomVerticesIndex = value; }
    }

    public int TopVerticesIndex
    {
        get { return m_TopVerticesIndex; }
        set { m_TopVerticesIndex = value; }
    }

    public float Length
    {
        get { return m_Length; }
        set { m_Length = value; }
    }

    public LTerm CorrespondingTerm
    {
        get { return m_Term; }
        set { m_Term = value; }
    }
    #endregion

    /// <summary>
    /// 判断是否是第一个枝干
    /// </summary>
    /// <returns></returns>
    public bool IsFirstBranch()
    {
        return m_Level == 0 && m_Index == 0;
    }

    /// <summary>
    /// 判断是否是分支后的第一个枝干
    /// </summary>
    /// <returns></returns>
    public bool IsFirstBranchInBranch()
    {
        return m_Previous == m_From;
    }

    public bool IsTheLevelTheSame(BranchIndex _BranchIndex)
    {
        return this.Level == _BranchIndex.Level;
    }

    public bool IsTheSameIndex(BranchIndex _BranchIndex)
    {
        return IsTheLevelTheSame(_BranchIndex) && this.Index == _BranchIndex.Index;
    }

    public bool IsTheSameRelativeRotationAngles(BranchIndex _BranchIndex)
    {
        return this.RelativeRotation.Equals(_BranchIndex.RelativeRotation);
    }

    public bool IsMatch(BranchIndex _BranchIndex)
    {
        if (!IsTheLevelTheSame(_BranchIndex)) return false; //不属于同一级的枝干

        switch (Convert.ToInt32(IsFirstBranchInBranch()) + Convert.ToInt32(_BranchIndex.IsFirstBranchInBranch()))
        {
            case 0:
                return IsTheSameIndex(_BranchIndex);
            case 1:
                return false;
            case 2:
                return IsTheSameRelativeRotationAngles(_BranchIndex);
            default:
                throw new Exception("Unknown Exception.");
        }
    }

    /// <summary>
    /// 形态模拟
    /// 根据生物量进行计算
    /// </summary>
    public void MorphologicalSim()
    {
        /*
         * 若年龄大于最大发育年龄
         * 则形态不会发生改变
         */
        if (Age > MaizeParams.STEM_MAX_DEVELOPMENT_AGE) return;

        /*
         * 体积
         * 用于后续计算长度和半径
         */
        double volume = Biomass / MaizeParams.STEM_DEN;

        /*
         * 计算长度和半径
         * 后续根据节间的长度和半径确定绘制的GameObject大小
         */
        if (Level == 0 && Index < MaizeParams.SHORT_INTERNODE_NUM)  //该节间为短节间
        {
            Length = (float)(
                Math.Sqrt(MaizeParams.STEM_SHAPE_A * MaizeParams.STEM_SHORT_K) * 
                Math.Pow(volume, (1 + MaizeParams.STEM_SHAPE_B * MaizeParams.STEM_SHORT_K) / 2));

            double area = 
                Math.Sqrt(1 / (MaizeParams.STEM_SHAPE_A * MaizeParams.STEM_SHORT_K)) *
                Math.Pow(volume, (1 - MaizeParams.STEM_SHAPE_B * MaizeParams.STEM_SHORT_K) / 2);

            Radius = (float)(Math.Sqrt(area / Math.PI)) * 0.8f;
        }
        else if(Level == 0 && Index >= (MaizeParams.INTERNODE_NUM - MaizeParams.LONG_INTERNODE_NUM))    //顶端的长节间
        {
            Length = (float)(
                Math.Sqrt(MaizeParams.STEM_LONG_SHAPE_A) *
                Math.Pow(volume, (1 + MaizeParams.STEM_LONG_SHAPE_B) / 2));

            double area =
                Math.Sqrt(1 / (MaizeParams.STEM_LONG_SHAPE_A)) *
                Math.Pow(volume, (1 - MaizeParams.STEM_LONG_SHAPE_B) / 2);

            Radius = (float)(Math.Sqrt(area / Math.PI));
        }
        else
        {
            Length = (float)(
                Math.Sqrt(MaizeParams.STEM_SHAPE_A) *
                Math.Pow(volume, (1 + MaizeParams.STEM_SHAPE_B) / 2));

            double area =
                Math.Sqrt(1 / (MaizeParams.STEM_SHAPE_A)) *
                Math.Pow(volume, (1 - MaizeParams.STEM_SHAPE_B) / 2);

            Radius = (float)(Math.Sqrt(area / Math.PI));
        }

        Radius *= 1.2f;
    }

    public override string ToString()
    {
        string str = "";

        str += "Branch Index" + Level + "[" + Index + "]" + ": " + "\n";
        str += "  Age: " + Age + "\n";
        str += "  Sink Strength: " + SinkStrength + "\n"; 
        str += "  Biomass: " + Biomass + "\n";
        str += "  Length: " + Length + "\n";
        str += "  Radius: " + Radius + "\n";

        return str;
    }
}
