using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrganType { Branch, Leaf, Sheath, Flower, Fruit};

public interface IMorphologicalSim
{
    void MorphologicalSim();
}

public class OrganIndex
{
    #region 成员变量
    protected int           m_Index = -1;                   //索引
    protected OrganType     m_OrganType = OrganType.Branch; //器官类型
    protected BranchIndex   m_From = null;                  //器官所属的枝干分支

    protected int           m_Age = 0;                      //年龄
    protected double        m_Biomass = 0;                  //生物量   
    protected double        m_SinkStrength = 0;             //库强

    protected Vector3       m_Rotation = Vector3.zero;      //旋转角度
    protected float         m_Radius = 0;                   //半径 cm
    protected float         m_Length = 0;                   //长度 cm
    protected GameObject    m_GameObject;                   //渲染的对象
    protected TreeModel     m_TreeModel;
    #endregion

    public int          Index           { get { return m_Index; }           set { m_Index = value; } }
    public OrganType    Type            { get { return m_OrganType; }       set { m_OrganType = value; } }
    public BranchIndex  From            { get { return m_From; }            set { m_From = value; } }

    public int          Age             { get { return m_Age; }             set { m_Age = value; } }
    public double       Biomass         { get { return m_Biomass; }         set { m_Biomass = value; } }
    public double       SinkStrength    { get { return m_SinkStrength; }    set { m_SinkStrength = value; } }

    public Vector3      Rotation        { get { return m_Rotation; }        set { m_Rotation = value; } }
    public float        Radius          { get { return m_Radius; }          set { m_Radius = value; } }
    public float        Length          { get { return m_Length; }          set { m_Length = value; } }
    public GameObject   Belong          { get { return m_GameObject; }      set { m_GameObject = value; } }
    public TreeModel    TreeModel       { get { return m_TreeModel; }       set { m_TreeModel = value; } }

    public OrganIndex()
    {

    }

    public OrganIndex(int Index, OrganType Type, BranchIndex From, GameObject _GameObject, int Age = 0)
    {
        m_Index = Index;
        m_OrganType = Type;
        m_From = From;
        m_GameObject = _GameObject;
        m_Age = Age;
    }

    public bool IsTheSameType(OrganIndex _OrganIndex)
    {
        return this.Type == _OrganIndex.Type;
    }

    public bool IsTheSameIndex(OrganIndex _OrganIndex)
    {
        return IsTheSameType(_OrganIndex) && this.Index == _OrganIndex.Index;
    }

    public bool IsMatchWithPrevious(OrganIndex preOrganIndex)
    {
        BranchIndex preBranchIndex = preOrganIndex.From;
        BranchIndex curBranchIndex = From;

        if (!preBranchIndex.IsMatch(curBranchIndex)) return false;  //不属于同一枝干

        return IsTheSameType(preOrganIndex) && /*类型相同*/
               IsTheSameIndex(preOrganIndex);  /*索引相同*/
    }
}

