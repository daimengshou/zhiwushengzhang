/*
 * 文件名：LSTree.cs
 * 描述：L-系统整体。
 */

#if true
#define _UNMT //不采用多线程
#else
#define _MT //采用多线程
#endif

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/*
 * 变量结构体。
 * 用于存储L-系统中的全局变量。
 * 
 * @version: 1.0
 */
public struct VariableEntry
{
    public string   strName;            //变量名称（必须）
    public float?   fValue;             //变量值（必须）
    public float?   fMax;               //变量最大值
    public float?   fMin;               //变量最小值
    public string   strDescription;     //变量描述

    public VariableEntry(string name, float value)
    {
        strName = name;
        fValue = value;
        fMax = null;
        fMin = null;
        strDescription = null;

        Validate();
    }

    public VariableEntry(string name, float value, float max, float min, string description)
    {
        strName = name;
        fValue = value;
        fMax = max;
        fMin = min;
        strDescription = description;

        Validate();
    }

    public string Name
    {
        get { return strName == null ? null : strName; }
        set { strName = value;}
    }

    public float? Value
    {
        get { return fValue == null ? null : fValue; }
        set { fValue = value;}
    }

    public float? Max
    {
        get { return fMax == null ? null : fMax; }
        set { fMax = value; Validate(); }
    }

    public float? Min
    {
        get { return fMin == null ? null : fMin; }
        set { fMin = value; Validate(); }
    }

    public string Description
    {
        get { return strDescription == null ? null : strDescription; }
        set { strDescription = value; }
    }

    public void Clear()
    {
        strName = null;
        fValue = null;
        fMax = null;
        fMin = null;
        strDescription = null;
        GC.Collect();
    }

    public void Validate()
    {
        if (strName == null && fValue == null) throw new ArgumentNullException("No parameter of variable");

        if (fMax != null && fMin != null)
        {
            if (fMax < fMin) throw new InvalidOperationException("The max value is smaller than the min value");
            if (fValue < fMin || fValue > fMax) throw new InvalidOperationException("Illegal value");
        }
    }

    public override string ToString()
    {
        string result = "";

        result = result + "Variable Name:  " + strName + "\n";
        result = result + "Variable Value: " + fValue + "\n";
        result = result + "Variable Max:   " + fMax + "\n";
        result = result + "Variable Min:   " + fMin + "\n";
        result = result + "Desription:     " + strDescription + "\n";

        return result;
    }
}

/*
 * L-系统类。
 * 用于存储与处理L-系统相关数据。
 * 
 * @version: 1.0
 */
public class LSTree
{
    /*****参数*****/
    private List<VariableEntry>  m_listGlobalVariables;  //全局参数链表
    private List<LTerm>           m_listIgnore;           //上下文的无视条件   
    private LLinkedList<LTerm>    m_listAxiom;            //公理链表
    private List<LRule>          m_listProductionGroup;  //产生式集合
    private LLinkedList<LTerm>    m_listFinal;            //最终字符链表

    private TreeModel                m_ModelEntry;           //所属的模型
    private int                  m_iCurrentStep;         //当前步骤

    public LSTree()        
    {
        m_listGlobalVariables = new List<VariableEntry>();
        m_listAxiom = new LLinkedList<LTerm>();
        m_listProductionGroup = new List<LRule>();
        m_listFinal = new LLinkedList<LTerm>();
        m_iCurrentStep = 0;
    }

    public List<VariableEntry> GlobalVariablesList
    {
        get { return m_listGlobalVariables == null ? null : m_listGlobalVariables; }
        set { m_listGlobalVariables = value; }
    }

    public List<LTerm> Ignore
    {
        get { return m_listIgnore; }
        set { m_listIgnore = value; }
    }

    public LLinkedList<LTerm> Axiom
    {
        get { return m_listAxiom == null ? null : m_listAxiom; }
        set { m_listAxiom = value; }
    }

    public List<LRule> ProductionGroup
    {
        get { return m_listProductionGroup == null ? null : m_listProductionGroup; }
        set { m_listProductionGroup = value; }
    }

    public LLinkedList<LTerm> FinalList
    {
        get { return m_listFinal == null ? null : m_listFinal; }
    }

    public TreeModel Belong
    {
        get { return m_ModelEntry; }
        set { m_ModelEntry = value; }
    }

    public int CurrentStep
    {
        get { return m_iCurrentStep; }
    }

    /// <summary>
    /// 添加上下文无视条件（待测试，慎用！）
    /// </summary>
    /// <param name="ignore">无视条件</param>
    public void AddIgnore(List<LTerm> ignore)
    {
        if (m_listIgnore != null) throw new InvalidOperationException("Ignore already exists!");    //无视的字符已经存在

        m_listIgnore = ignore;
    }

    /// <summary>
    /// 当不存在公理时，添加公理。
    /// </summary>
    /// <param name="axiom">待添加的公理</param>
    public void AddAxiom(LLinkedList<LTerm> axiom)
    {
        if (m_listAxiom.Count > 0) throw new InvalidOperationException("Axiom already exists!");    //公理已经存在

        m_listAxiom = axiom;
    }

    /// <summary>
    /// 添加产生式至产生式集合，并自动归类相同前驱的产生式
    /// </summary>
    /// <param name="production">待添加的产生式</param>
    public void AddProduction(LRule production)
    {
        production.Validate();  //验证该产生式是否可用

        LRule SameTypeProduction = FindSameTypeOfProduction(production);    //寻找相同类型的产生式

        if (SameTypeProduction == null) //没有相同类型的产生式
        {
            production.Belong = this;
            m_listProductionGroup.Add(production);  //插入该产生式
        }
        else    //有相同类型的产生式
        {
            for (int i = 0; i < production.Successors.Count; i++)
                SameTypeProduction.AddSuccessor(production.Successors[i].Probability.Value, production.Successors[i].Result); //在该类型中添加后续
        }
    }

    /// <summary>
    /// 添加全局变量
    /// </summary>
    /// <param name="name">待添加的全局变量名称</param>
    /// <param name="value">待添加的全局变量值</param>
    public void AddGlobalVariable(VariableEntry variable)
    {
        variable.Validate();    //验证该全局变量是否可用
        m_listGlobalVariables.Add(variable);
    }

    /// <summary>
    /// 添加全局变量
    /// </summary>
    /// <param name="name">待添加的全局变量名称</param>
    /// <param name="value">待添加的全局变量值</param>
    /// <param name="max">待添加的全局变量最大值</param>
    /// <param name="min">待添加的全局变量最小值</param>
    /// <param name="description">待添加的全局变量描述</param>
    public void AddGlobalVariable(string name, float value)
    {
        AddGlobalVariable(new VariableEntry(name, value));
    }

    /// <summary>
    /// 根据输入的产生式在产生式的集合中寻找与其相同类型的产生式。相同类型的主要特征是前驱一样（相同的符号、参数以及条件）。
    /// </summary>
    /// <param name="production">根据该产生式进行寻找</param>
    /// <returns>返回找到的相同类型的产生式</returns>
    public void AddGlobalVariable(string name, float value, float max, float min, string description)
    {
        AddGlobalVariable(new VariableEntry(name, value, max, min, description));
    }

    /// <summary>
    /// 根据输入的产生式在产生式的集合中寻找与其相同类型的产生式。相同类型的主要特征是前驱一样（相同的符号、参数以及条件）。
    /// </summary>
    /// <param name="production">根据该产生式进行寻找</param>
    /// <returns>返回找到的相同类型的产生式</returns>
    private LRule FindSameTypeOfProduction(LRule production)
    {
        List<LRule> productionList = m_listProductionGroup.FindAll((rule) =>
            rule.Predecessor.Symbol.Equals(production.Predecessor.Symbol) /*相同的符号*/&&
            rule.Predecessor.Params.Count == production.Predecessor.Params.Count /*相同的参数个数*/&&
            ((rule.Condition == null && production.Condition == null) || /*均无条件或*/
             (rule.Condition != null && production.Condition != null && rule.Condition.Equals(production.Condition)) /*均有条件，且条件相同*/
            ));

        if (productionList.Count > 1)   //如果存在多个相同的类型，则说明产生式集合存在一定问题，调整该集合并重新寻找
        {
            AdjustProductionGroup();
            return FindSameTypeOfProduction(production);
        }

        //当有且只有一个相同类型的产生式时，返回该产生式
        if (productionList.Count == 1)
        {
            return productionList[0];
        }
        
        return null; //没有相同类型的产生式，返回NULL
    }

    /// <summary>
    /// 调整产生式集合，使相同类型的产生式（前驱一样）合并
    /// </summary>
    private void AdjustProductionGroup()
    {
        List<List<LRule>> classificationList = ProductionClassification();

        m_listProductionGroup.Clear();  //清除原有列表中所有的数据
        for (int i = 0; i < classificationList.Count; i++)
        {
            if (classificationList[i].Count == 1)   //该类产生式只有一个，直接插入
            {
                m_listProductionGroup.Add(classificationList[i][0]);
            }
            else
            {
                MergeProductions(classificationList[i]);    //将列表中所有的产生式都合并到第一个产生式中
                m_listProductionGroup.Add(classificationList[i][0]);    //插入该产生式
            }
        }
    }

    /// <summary>
    /// 对产生式进行分类，使相同类型的产生式归为一类
    /// </summary>
    /// <returns></returns>
    private List<List<LRule>> ProductionClassification()
    {
        List<List<LRule>> classificationList = new List<List<LRule>>();

        for (int i = 0; i < m_listProductionGroup.Count; i++)
        {
            bool isFound = false;
            LRule production = m_listProductionGroup[i];
            //对每个分类结果进行查询
            for (int j = 0; j < classificationList.Count; j++)
            {
                //如果在该分类结果中存在与当前的产生式相同的前驱以及条件，则将该产生式插入该分类结果中
                if (classificationList[j].Exists(
                    (rule) => rule.Predecessor.Symbol.Equals(production.Predecessor.Symbol) &&
                              rule.Predecessor.Params.Count == production.Predecessor.Params.Count &&
                              rule.Condition.Equals(production.Condition)))
                {
                    classificationList[j].Add(production);
                    isFound = true;
                    break;
                }
            }

            //未能找到符合当前产生式的分类结果，说明第一次出现该类产生式，因此新建一个分类结果用于存储该类产生式
            if (!isFound)
            {
                List<LRule> classification = new List<LRule>();
                classification.Add(production);
                classificationList.Add(classification);
            }
        }

        return classificationList;
    }

    /// <summary>
    /// 将列表中所有的产生式都合并到第一个产生式中（前提是该列表所有的产生式的类型均相同）
    /// </summary>
    /// <param name="productionList">拥有多个相同类型的产生式的列表</param>
    private void MergeProductions(List<LRule> productionList)
    {
        //判断是否所有的产生式均属于同一类型
        for (int i = 1; i < productionList.Count; i++)
        {
            if (!productionList[i].Predecessor.Symbol.Equals(productionList[0].Predecessor.Symbol) ||           /*符号不相同*/
                 productionList[i].Predecessor.Params.Count != productionList[0].Predecessor.Params.Count ||    /*参数个数不相同*/
                !productionList[i].Condition.Equals(productionList[0].Condition))                               /*条件不相同*/
                throw new InvalidOperationException("The productions in the list are not of the same type.");
        }

        //合并到第一个生产式中
        for (int i = 1; i < productionList.Count; i++)
        {
            for (int j = 0; j < productionList[i].Successors.Count; i++)
            {
                productionList[0].AddSuccessor(productionList[i].Successors[j].Probability.Value, productionList[i].Successors[j].Result);
                productionList[i].Clear();
            }
        }
    }

    /// <summary>
    /// 进行一次迭代，并将结果存放在FinalList里面
    /// </summary>
    public void NextStep()
    {
        if (m_listFinal == null || m_listFinal.Count == 0)  //第一步
        {
            //m_listAxiom.CopyTo(m_listFinal);
            m_listFinal = m_listAxiom.Clone();  //将公理复制到最终结果链表中
            return;
        }

        //第二步开始
#if _UNMT
        ReplaceFinalList();
#else
        ReplaceFinalListFast();
#endif

        m_iCurrentStep++;
    }

    /// <summary>
    /// 采用非多线程的方式迭代
    /// </summary>
    private void ReplaceFinalList()
    {
        LLinkedListNode<LTerm> node = m_listFinal.First;         //从头节点开始
        LLinkedListNode<LTerm> head = m_listFinal.First;         //头节点
        LLinkedList<LTerm> FinalList = new LLinkedList<LTerm>();
        do 
        {
            LLinkedListNode<LTerm> NodeClone = new LLinkedListNode<LTerm>(node.Value);    //防止替换后原先的Node出现错误
            FinalList.Add(NodeClone);

            if (node.Value != null)
            {
                LLinkedList<LTerm> resultList = GetResultListFrom(node);

                if (resultList != null)
                    FinalList.Replace(NodeClone, resultList);
            }

            node = node.Next;
        } while (node != head);

        m_listFinal = FinalList;
    }

    /// <summary>
    /// 采用多线程的方式快速迭代
    /// </summary>
    private void ReplaceFinalListFast()
    {
        LLinkedListNode<LTerm> node = m_listFinal.First;         //从头节点开始
        LLinkedListNode<LTerm> head = m_listFinal.First;         //头节点

        LLinkedListNode<LTerm>[] nodes = new LLinkedListNode<LTerm>[m_listFinal.Count];
        LLinkedList<LTerm>[] resultLists = new LLinkedList<LTerm>[m_listFinal.Count];

        for (int i = 0; i < m_listFinal.Count; i++)
        {
            nodes[i] = new LLinkedListNode<LTerm>(node.Value);

            node = node.Next;
        }

        Parallel.For(0, m_listFinal.Count, i =>
        {
            resultLists[i] = GetResultListFrom(nodes[i]);
        });

        LLinkedList<LTerm> FinalList = new LLinkedList<LTerm>();
        for (int i = 0; i < resultLists.Length; i++)
        {
            if (resultLists[i] == null)
                FinalList.Add(nodes[i]);
            else
                FinalList.Add(resultLists[i]);
        }

        m_listFinal = FinalList;
    }

    public LLinkedList<LTerm> GetResultListFrom(LLinkedListNode<LTerm> node)
    {
        LLinkedList<LTerm> resultList = null;

        //在产生式集合中寻找与该Term匹配的产生式（相同的符号和参数个数）
        List<LRule> matchingProductions = m_listProductionGroup.FindAll((rule) =>
            rule.Predecessor.Symbol.Equals(node.Value.Symbol) /*****符号相同****/&&
            rule.Predecessor.Params.Count == node.Value.Params.Count /*****参数个数相同*****/
        );

        if (matchingProductions.Count > 0)  //有匹配的产生式
        {
            float[] paramsArray = node.Value.GetParamsArrayWithFloat(); //获取参数

            for (int i = 0; i < matchingProductions.Count; i++)
            {
                resultList = matchingProductions[i].GetResultList(node, paramsArray);   //获取结果链表

                if (resultList != null)     //当返回NULL则说明该产生式的条件不符合；返回链表则说明该产生式可行
                {
                    break;
                }
            }
        }

        return resultList;
    }

    public void Clear()
    {
        this.m_listGlobalVariables.Clear();
        this.m_listAxiom.Clear();
        this.m_listProductionGroup.Clear();
        this.m_listFinal.Clear();
        this.m_iCurrentStep = 0;
    }
}
