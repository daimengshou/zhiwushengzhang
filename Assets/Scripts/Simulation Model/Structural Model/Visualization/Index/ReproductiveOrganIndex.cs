using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FemaleIndex : OrganIndex, IMorphologicalSim
{
    private float m_volume = 0;  //体积 m^3

    private float m_HairLength = 0;     //果实须的长度
    private float m_CornLength = 0;     //果实的长度

    private Mesh m_HairMesh;
    private Mesh m_CornMesh;

    public float Volum { get { return m_volume; } set { m_volume = value; } }
    public float HairLength { get { return m_HairLength; } set { m_HairLength = value; } }
    public float CornLength { get { return m_CornLength; } set { m_CornLength = value; } }

    public Mesh HairMesh { get { return m_HairMesh; } set { m_HairMesh = value; } }
    public Mesh CornMesh { get { return m_CornMesh; } set { m_CornMesh = value; } }

    public enum FemaleType { Hair, Corn }

    public FemaleIndex()
    {
        this.Type = OrganType.Fruit;
    }

    public void MorphologicalSim()
    {
        MorphologicalSim(TreeModel.BranchIndexes, TreeModel.OrganIndexes);
    }

    public void MorphologicalSim(List<BranchIndex> branchIndexes, List<OrganIndex> organIndexes)
    {
        GrowthPeriod period = FunctionSim.GrowthPeriodJudgment(branchIndexes, organIndexes);    //判断生长时期

        if (period <= GrowthPeriod.TASSELING_STAGE)
            MorphologicalSim(FemaleType.Hair);
        else
            MorphologicalSim(FemaleType.Corn);
    }

    public void MorphologicalSim(FemaleType type = FemaleType.Hair)
    {
        /*
         * 若年龄已经超过最大发育年龄
         * 则形态不发生改变
         */
        if (Age > MaizeParams.FEMALE_MAX_DEVELOPMENT_AGE) return;
        if (Biomass <= Mathf.Epsilon) return;

        float scale = 0.01f;    //单位cm转m

        /*
         * 计算体积和长度
         * 后续根据体积和长度确定绘制的GameObject大小
         */
        float volum = (float)(Biomass / MaizeParams.FEMALE_DEN) * scale * scale * scale;

        switch (type)
        {
            case FemaleType.Hair:
                HairLength = Mathf.Pow(volum, 1.0f / 3) * 3;
                break;
            case FemaleType.Corn:
                CornLength = Mathf.Pow(volum, 1.0f / 3) * 2;
                break;
        }
    }

    public override string ToString()
    {
        string str = "";

        str += "FemaleIndex" + Index + "  From " + From.Level + "[" + From.Index + "]" + "\n";

        str += "  Age: " + Age + "\n";
        str += "  Sink Strength: " + SinkStrength + "\n";
        str += "  Biomass: " + Biomass + "\n";
        str += "  Hair Length: " + HairLength + "\n";
        str += "  Corn Length: " + CornLength + "\n";

        return str;
    }
}

public class MaleIndex : OrganIndex, IMorphologicalSim
{
    private float m_volume = 0;     //体积 m^3
    private Mesh m_Mesh = null;

    public float Volum { get { return m_volume; } set { m_volume = value; } }
    public Mesh MaleMesh { get { return m_Mesh; } set { m_Mesh = value; } }

    public MaleIndex()
    {
        this.Type = OrganType.Flower;
    }

    public void MorphologicalSim()
    {
        /*
         * 若年龄已经超过最大发育年龄
         * 则形态不发生改变
         */
        if (Age > MaizeParams.MALE_MAX_DEVELOPMENT_AGE) return;

        float scale = 0.01f;    //单位cm转m

        /*
         * 计算体积和长度
         * 后续根据体积和长度确定绘制的GameObject大小
         */
        Volum = (float)(Biomass / MaizeParams.MALE_DEN) * scale * scale * scale;
        Length = Mathf.Pow(Volum, 1.0f / 3) * 2.5f;
    }

    public override string ToString()
    {
        string str = "";

        str += "MaleIndex" + Index + "  From " + From.Level + "[" + From.Index + "]" + "\n";

        str += "  Age: " + Age + "\n";
        str += "  Sink Strength: " + SinkStrength + "\n";
        str += "  Biomass: " + Biomass + "\n";
        str += "  Length: " + Length + "\n";

        return str;
    }
}

