using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairedIndex<T>
    where T : OrganIndex
{
    public T PreIndex { get; set; }
    public T CurIndex { get; set; }

    public PairedIndex(T pre, T cur)
    {
        PreIndex = pre;
        CurIndex = cur;
    }

    /// <summary>
    /// 数据继承
    /// </summary>
    /// <param name="isSameGC">两个索引是否是同一生长周期</param>
    public void DataInheritance(bool isSameGC = false)
    {
        SynchronizeBiomass();
        AgeInheritance(isSameGC);
        MorphologicalInheritance();
    }

    /// <summary>
    /// 同步前后两个索引的生物量
    /// </summary>
    public void SynchronizeBiomass()
    {
        if (PreIndex == null) 
            CurIndex.Biomass = 0;
        else
            CurIndex.Biomass = PreIndex.Biomass;
    }

    /// <summary>
    /// 继承前后两个索引的年龄
    /// </summary>
    /// <param name="isSameGC">两个索引是否是同一生长周期</param>
    public void AgeInheritance(bool isSameGC = false)
    {
        if (PreIndex == null)
            CurIndex.Age = 1;
        else if (isSameGC)
            CurIndex.Age = PreIndex.Age;
        else
            CurIndex.Age = PreIndex.Age + 1;
    }

    /// <summary>
    /// 继承前后两个索引的形态
    /// 当不再发育时，形态数据不再计算
    /// 则根据继承的形态数据计算模型大小
    /// </summary>
    public void MorphologicalInheritance()
    {
        if (PreIndex == null) return;

        switch (CurIndex.Type)
        {
            case OrganType.Branch:
                BranchMorphologicalInheritance();
                break;
            case OrganType.Leaf:
                LeafMorphologicalInheritance();
                break;
            case OrganType.Flower:
                MaleMorphologicalInheritance();
                break;
            case OrganType.Fruit:
                FemaleMorphologicalInheritance();
                break;
        }
    }

    /// <summary>
    /// 基类的形态数据继承
    /// </summary>
    private void OrganMorphologicalInheritance()
    {
        CurIndex.Length = PreIndex.Length;
        CurIndex.Radius = PreIndex.Radius;
    }

    private void BranchMorphologicalInheritance()
    {
        OrganMorphologicalInheritance();    //基类中的数据继承
    }

    private void LeafMorphologicalInheritance()
    {
        OrganMorphologicalInheritance();    //基类中的数据继承

        LeafIndex curLeafIndex = CurIndex as LeafIndex;
        LeafIndex preLeafIndex = PreIndex as LeafIndex;

        curLeafIndex.Width = preLeafIndex.Width;
        curLeafIndex.LeafArea = preLeafIndex.LeafArea;
        curLeafIndex.LeafArea_Uninsected = preLeafIndex.LeafArea_Uninsected;

        curLeafIndex.LimitRatio = preLeafIndex.LimitRatio;
        curLeafIndex.Texture_PreDay = GameObjectOperation.GetTexture(preLeafIndex.Belong);
        curLeafIndex.MeshHashCode_PreDay = preLeafIndex.LeafMesh.GetHashCode();
    }

    private void FemaleMorphologicalInheritance()
    {
        OrganMorphologicalInheritance();    //基类中的数据继承

        (CurIndex as FemaleIndex).HairLength = (PreIndex as FemaleIndex).HairLength;
        (CurIndex as FemaleIndex).CornLength = (PreIndex as FemaleIndex).CornLength;
    }

    private void MaleMorphologicalInheritance()
    {
        OrganMorphologicalInheritance();    //基类中的数据继承

        (CurIndex as MaleIndex).Volum = (PreIndex as MaleIndex).Volum;
    }


}

public class IndexMatch
{
    /// <summary>
    /// 对枝干索引进行匹配
    /// </summary>
    public static List<PairedIndex<BranchIndex>> BranchIndexes(List<BranchIndex> preBranchIndexes, List<BranchIndex> curBranchIndexes)
    {
        List<PairedIndex<BranchIndex>> result = new List<PairedIndex<BranchIndex>>();

        for (int indexPre = 0, indexCur = 0; indexCur < curBranchIndexes.Count; indexCur++)
        {
            if (indexPre < preBranchIndexes.Count &&
                preBranchIndexes[indexPre].IsMatch(curBranchIndexes[indexPre])) //两节间匹配
            {
                result.Add(
                    new PairedIndex<BranchIndex>(preBranchIndexes[indexPre], curBranchIndexes[indexCur])    //添加匹配完成的结果到结果列表中
                    );

                indexPre++;
            }
            else            //两节间未能匹配
            {
                result.Add(
                    new PairedIndex<BranchIndex>(null, curBranchIndexes[indexCur])
                    );
            }
        }

        return result;
    }

    /// <summary>
    /// 对器官索引（除枝干外）进行匹配
    /// </summary>
    public static List<PairedIndex<OrganIndex>> OrganIndexes(List<OrganIndex> preOrganIndexes, List<OrganIndex> curOrganIndexes)
    {
        List<PairedIndex<OrganIndex>> result = new List<PairedIndex<OrganIndex>>();

        for (int indexPre = 0, indexCur = 0; indexCur < curOrganIndexes.Count; indexCur++)
        {
            if (indexPre < preOrganIndexes.Count &&
                curOrganIndexes[indexCur].IsMatchWithPrevious(preOrganIndexes[indexPre]))
            {
                result.Add(
                    new PairedIndex<OrganIndex>(preOrganIndexes[indexPre], curOrganIndexes[indexCur])   //添加匹配完成的结果到结果列表中
                    );

                indexPre++;
            }
            else
            {
                result.Add(
                    new PairedIndex<OrganIndex>(null, curOrganIndexes[indexCur])
                    );
            }
        }

        return result;
    }
}
