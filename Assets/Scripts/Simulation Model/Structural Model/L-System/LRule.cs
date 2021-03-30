/*
 * 文件名：LRule.cs
 * 描述：L-系统规则（或称产生式）。
 */
using System;
using System.Data;
using System.Collections.Generic;

/*
 * L-系统规则前驱类。
 * 用于存储、处理L-系统前驱。
 * 当前版本前驱内容包括：符号、产生条件与左右侧上下文（上下文未测试，慎用）。
 * 
 * @version: 1.0
 */
public class Predecessor
{
    private List<LTerm> m_listLeftContext;   //左侧上下文
    private List<LTerm> m_listRightContext;  //右侧上下文
    private LTerm m_term;                    //符号
    private string m_strCondition;          //产生条件

    /*
     * 构造函数，创建前驱
     * 构造函数包括：
     * ①唯一的模块（LTerm形式）与产生条件（string形式，可忽略）
     * ②左右上下文（List<LTerm>形式）、唯一的模块（LTerm形式）与产生条件（string形式，可忽略）
     */
    #region 构建函数
    public Predecessor(LTerm term = null, string condition = null)
    {
        m_listLeftContext = new List<LTerm>();
        m_listRightContext = new List<LTerm>();
        m_term = term;
        m_strCondition = condition;
    }

    public Predecessor(List<LTerm> leftContext, List<LTerm> rightContext, LTerm term, string condition = null)
    {
        m_listLeftContext = leftContext;
        m_listRightContext = rightContext;
        m_term = term;
        m_strCondition = condition;
    }
    #endregion
    #region 获取成员变量
    public List<LTerm> LeftContext
    {
        get { return m_listLeftContext; }
        set { m_listLeftContext = value; }
    }

    public List<LTerm> RightContext
    {
        get { return m_listRightContext; }
        set { m_listRightContext = value; }
    }

    public LTerm Term
    {
        get { return m_term; }
        set { m_term = value; }
    }

    public string Condition
    {
        get { return m_strCondition; }
        set { m_strCondition = value; }
    }
    #endregion

    /*
     * 添加左右上下文。
     * 该功能未经测试，慎用！
     */
    #region 添加上下文
    public void AddLeftContext(string Symbol)
    {
        AddLeftContext(new LTerm(Symbol));
    }

    public void AddLeftContext(LTerm ContextTerm)
    {
        if (m_listLeftContext == null)
            m_listLeftContext = new List<LTerm>();

        m_listLeftContext.Add(ContextTerm);
    }

    public void AddLeftContext(IEnumerable<LTerm> ContextTerms)
    {
        if (m_listLeftContext == null)
            m_listLeftContext = new List<LTerm>();

        m_listLeftContext.AddRange(ContextTerms);
    }

    public void AddRightContext(string Symbol)
    {
        AddRightContext(new LTerm(Symbol));
    }

    public void AddRightContext(LTerm ContextTerm)
    {
        if (m_listRightContext == null)
            m_listRightContext = new List<LTerm>();

        m_listRightContext.Add(ContextTerm);
    }

    public void AddRightContext(IEnumerable<LTerm> ContextTerms)
    {
        if (m_listRightContext == null)
            m_listRightContext = new List<LTerm>();

        m_listRightContext.AddRange(ContextTerms);
    }
    #endregion

    public void Clear()
    {
        m_listLeftContext.Clear();
        m_listRightContext.Clear();
        m_term.Clear();
        m_strCondition = null;
    }
}

/*
 * L-系统后继类。
 * 用于存储、处理L-系统后继。
 * 当前版本后继包括：产生概率、结果链表。
 * 
 * @version: 1.0
 */
public class Successor
{
    private float? m_fProbability;             //产生概率 eg. 0.5
    private LLinkedList<LTerm> m_listResult;     //产生的结果链表 eg. F(length + 1)A(length)B(1)
    private bool m_boolDefault = true;         //是否为默认值（判断条件：初始化时是否有概率输入）

    public float? Probability
    {
        get { return m_fProbability; }
        set { m_fProbability = value; m_boolDefault = false; }
    }

    public LLinkedList<LTerm> Result
    {
        get { return m_listResult; }
        set { m_listResult = value; }
    }

    public bool isDefault()
    {
        return m_boolDefault;
    }

    /*
     * 构造函数，创建后继。
     * 构造函数包括：
     * ①参数为空；
     * ②仅包含后继链表（LLinkedList<LTerm>形式）；
     * ③概率（float形式，且不能大于1）与后继链表（LLinkedList<LTerm>形式）。
     */
    #region 构造函数
    public Successor()
    {
        this.m_fProbability = 1.0f;
        m_boolDefault = true;
    }

    public Successor(LLinkedList<LTerm> listSuccessor)
    {
        this.m_fProbability = 1.0f;
        this.m_listResult = listSuccessor;
        m_boolDefault = true;   //无概率参数输入，因此为默认
    }

    public Successor(float? fProbability, LLinkedList<LTerm> listSuccessor)
    {
        this.m_fProbability = fProbability;
        this.m_listResult = listSuccessor;
        m_boolDefault = false;  //有概率参数输入，该后续不为默认
    }
    #endregion

    /// <summary>
    /// 检验该后续是否可行
    /// </summary>
    public void Validate()
    {
        if (m_fProbability == null)
            throw new ArgumentNullException("Probability is null.");
        if (m_listResult == null || m_listResult.First == null)
            throw new ArgumentNullException("Result list is null");
    }
}

/*
 * L-系统规则（产生式）类。
 * 该类用于存储与处理L-系统规则。
 * 产生式如 F(length):(length < 5) --> (0.5)F(length + 1)A(length)B(1)。
 * 该类包括唯一前驱与多个后继（每个后继均有自身被应用概率）。
 * 
 * @version：1.0
 */
public class LRule
{
    private Predecessor m_Predecessor;          //前驱               eg. F(length) : length < 5
    private List<Successor> m_listSuccessor;    //后续               eg. (0.5)F(length + 1)A(length)B(1) 因随机L系统允许相同的前驱有不同的后续，且每个后续均有自己的发生概率，因此采用list
    private LSTree m_RuleDataEntry;             //所属的规则集合
    /****一些用于内部计算的标识符****/
    private int m_iSuccessorIndex = 0;

    /*
     * 构建L-系统规则。
     * 构造函数包括：
     * ①参数为空；
     * ②参数为单一模块（前驱的符号，LTerm形式）与结果链表（LLinkedList<LTerm>形式）；
     * ③参数为单一模块（前驱的符号，LTerm形式）、产生条件（string形式）、结果链表（LLinkedList<LTerm>形式）；
     * ④参数为单一模块（前驱的符号，LTerm形式）、产生条件（string形式）、应用概率（float形式）、结果链表（LLinkedList<LTerm>形式）；
     */
    #region 构建函数
    public LRule()
    {
        m_Predecessor = new Predecessor();
        m_listSuccessor = new List<Successor>();
        m_listSuccessor.Add(new Successor());
    }

    public LRule(LTerm stTerm, LLinkedList<LTerm> lstResult)
    {
        m_Predecessor = new Predecessor(stTerm);
        this.m_listSuccessor = new List<Successor>();
        m_listSuccessor.Add(new Successor(lstResult));
    }

    public LRule(LTerm stTerm, string strCondition, LLinkedList<LTerm> lstResult)
    {
        m_Predecessor = new Predecessor(stTerm, strCondition);
        this.m_listSuccessor = new List<Successor>();
        m_listSuccessor.Add(new Successor(lstResult));
    }

    public LRule(LTerm stTerm, string strCondition, float fProbability, LLinkedList<LTerm> lstResult)
    {
        m_Predecessor = new Predecessor(stTerm, strCondition);
        this.m_listSuccessor = new List<Successor>();
        m_listSuccessor.Add(new Successor(fProbability, lstResult));
    }
    #endregion

    #region 获取成员变量
    public LTerm Predecessor
    {
        get { return m_Predecessor.Term; }
    }

    public string Condition
    {
        get { return m_Predecessor.Condition; }
    }

    public List<LTerm> LeftContext
    {
        get { return m_Predecessor.LeftContext; }
    }

    public List<LTerm> RightContext
    {
        get { return m_Predecessor.RightContext; }
    }

    public List<Successor> Successors
    {
        get { return m_listSuccessor; }
    }

    public LSTree Belong
    {
        get { return m_RuleDataEntry; }
        set { m_RuleDataEntry = value; }
    }
    #endregion

    #region 添加成员变量
    /// <summary>
    /// 往产生式中添加前驱（前驱唯一）
    /// </summary>
    /// <param name="predecessor"></param>
    public void AddPredecessor(LTerm predecessor)
    {
        m_Predecessor.Term = predecessor;
    }

    /// <summary>
    /// 往产生式中添加前驱（前驱唯一）
    /// </summary>
    /// <param name="symbol">前驱的符号</param>
    /// <param name="para">前驱的参数</param>
    public void AddPredecessor(string symbol, params string[] para)
    {
        LTerm term = new LTerm(symbol, new List<string>(para));
        m_Predecessor.Term = term;
    }

    /// <summary>
    /// 往产生式中添加前驱（前驱唯一）
    /// </summary>
    /// <param name="symbol">前驱的符号</param>
    /// <param name="para">前驱的参数</param>
    public void AddPredecessor(string symbol, List<string> para)
    {
        LTerm term = new LTerm(symbol, para);
        m_Predecessor.Term = term;
    }

    /// <summary>
    /// 往产生式中添加前驱（前驱唯一）
    /// </summary>
    /// <param name="statement">前驱的语句，如F(length)</param>
    public void AddPredecessor(string statement)
    {
        LTerm term = new LTerm(statement);
        m_Predecessor.Term = term;
    }

    public void AddCondition(string condition)
    {
        m_Predecessor.Condition = condition;
    }

    public void AddLeftContext(string symbol)
    {
        m_Predecessor.AddLeftContext(symbol);
    }

    public void AddLeftContext(LTerm ContextTerm)
    {
        m_Predecessor.AddLeftContext(ContextTerm);
    }

    public void AddLeftContext(IEnumerable<LTerm> ContextTerms)
    {
        m_Predecessor.AddLeftContext(ContextTerms);
    }

    public void AddRightContext(string symbol)
    {
        m_Predecessor.AddRightContext(symbol);
    }

    public void AddRightContext(LTerm ContextTerm)
    {
        m_Predecessor.AddRightContext(ContextTerm);
    }

    public void AddRightContext(IEnumerable<LTerm> ContextTerms)
    {
        m_Predecessor.AddRightContext(ContextTerms);
    }

    //添加概率
    public void AddProbability(float probability)
    {
        if (m_listSuccessor[m_listSuccessor.Count - 1].isDefault())     //如果最后一个后续为默认值，则更改该后续的概率
            m_listSuccessor[m_listSuccessor.Count - 1].Probability = probability;
        else    //如果最后一个后续不是默认值，则插入一个新的后续
        {
            m_listSuccessor.Add(new Successor());
            m_listSuccessor[m_listSuccessor.Count - 1].Probability = probability;
        }
    }

    //添加结果链表
    public void AddResult(LLinkedList<LTerm> lstResult)
    {
        if (m_listSuccessor[m_listSuccessor.Count - 1].Result == null)   //如果最后一个后续不存在结果链表，则更改其结果链表
        {
            m_listSuccessor[m_listSuccessor.Count - 1].Result = lstResult;
        }
        else    //如果最后一个后续存在结果链表，则插入一个新的后续
        {
            m_listSuccessor.Add(new Successor(lstResult));
        }
    }

    public void AddSuccessor(LLinkedList<LTerm> lstResult)
    {
        m_listSuccessor.Add(new Successor(lstResult));
    }

    public void AddSuccessor(float probability, LLinkedList<LTerm> lstResult)
    {
        m_listSuccessor.Add(new Successor(probability, lstResult));
    }

    public void SetBelong(LSTree ruleData)
    {
        this.m_RuleDataEntry = ruleData;
    }
    #endregion

    /// <summary>
    /// 调整该产生式中所有后续的概率
    /// </summary>
    public void AdjustProbability()
    {
        if (m_listSuccessor.Count == 1) return; //如果只有一个后续则不用调整

        float? sumProbability = 0f;
        for (int i = 0; i < m_listSuccessor.Count; i++)
        {
            sumProbability += m_listSuccessor[i].Probability;   //统计概率总和
        }

        for (int i = 0; i < m_listSuccessor.Count; i++)
        {
            m_listSuccessor[i].Probability = m_listSuccessor[i].Probability / sumProbability;   //调整各后续的概率
        }
    }

    /// <summary>
    /// 根据输入的参数输出该产生式产生的结果链表。
    /// 如该产生式为 F(length)-->F(length+1)A(length)，输入的参数为1，则输出的结果为F(2)A(1)
    /// </summary>
    /// <param name="node">当前的节点，用于确定其上下文</param>
    /// <param name="inputParams">输入的参数，用于计算，且个数一定要与前驱中的形参的个数相同</param>
    /// <returns>返回最终产生的结果链表</returns>
    public LLinkedList<LTerm> GetResultList(LLinkedListNode<LTerm> node, params float[] inputParams)
    {
        Validate();
        ValidateInputParams(inputParams);

        List<LTerm> FinalLeftContext, FinalRightLeftContext;

        if (!isContextual(node, out FinalLeftContext, out FinalRightLeftContext))    //不满足上下文的条件
            return null;

        LLinkedList<LTerm> result = new LLinkedList<LTerm>();   //存储最终产生的结果

        DataTable table = new DataTable();  //用于计算各参数 参考代码：https://www.cnblogs.com/luther/p/3863274.html
        //添加形参及公式
        table.BeginLoadData();

        //添加全局变量形参
        List<VariableEntry> GlobalVariablesList = m_RuleDataEntry.GlobalVariablesList; //获取全局变量的链表
        for (int i = 0; i < GlobalVariablesList.Count; i++)
        {
            GlobalVariablesList[i].Validate();  //验证该全局变量是否可用
            table.Columns.Add(GlobalVariablesList[i].Name, typeof(float));  //添加全局变量列
        }

        table.Columns.Add("RANDOM", typeof(float));     //添加随机变量列

        //添加Mesh形参
        List<Mesh> Meshs = (m_RuleDataEntry.Belong).Meshes;  //获取存放所有Mesh的链表
        for (int i = 0; i < Meshs.Count; i++)
        {
            table.Columns.Add(Meshs[i].Name, typeof(int));
        }

        //添加输入参数的形参
        for (int i = 0; i < m_Predecessor.Term.Params.Count; i++)
        {
            table.Columns.Add(m_Predecessor.Term.Params[i], typeof(float)); //添加形参列
        }

        //添加上下文参数的形参
        foreach (LTerm term in m_Predecessor.LeftContext)    //添加左侧上下文的形参
        {
            for (int i = 0; i < term.Params.Count; i++)
                table.Columns.Add(term.Params[i], typeof(float));
        }

        foreach (LTerm term in m_Predecessor.RightContext)   //添加右侧上下文的形参
        {
            for (int i = 0; i < term.Params.Count; i++)
                table.Columns.Add(term.Params[i], typeof(float));
        }

        //添加公式列
        table.Columns.Add("expression", typeof(float));
        table.Columns["expression"].Expression = "";


        //添加实参
        DataRow row = table.Rows.Add();

        for (int i = 0; i < GlobalVariablesList.Count; i++)
        {
            row[GlobalVariablesList[i].Name] = GlobalVariablesList[i].Value;    //添加全局变量的实参
        }

        for (int i = 0; i < Meshs.Count; i++)
        {
            row[Meshs[i].Name] = Meshs[i].NameValue;
        }

        for (int i = 0; i < m_Predecessor.Term.Params.Count; i++)
        {
            row[m_Predecessor.Term.Params[i]] = inputParams[i];     //添加输入参数的实参
        }

        //添加上下文的实参
        for (int i = 0; i < FinalLeftContext.Count; i++ )
        {
            for (int j = 0; j < FinalLeftContext[i].Params.Count; j++)
            {
                row[m_Predecessor.LeftContext[i].Params[j]] = FinalLeftContext[i].Params[j];
            }
        }

        for (int i = 0; i < FinalRightLeftContext.Count; i++)
        {
            for (int j = 0; j < FinalRightLeftContext[i].Params.Count; j++)
            {
                row[m_Predecessor.RightContext[i].Params[j]] = FinalRightLeftContext[i].Params[j];
            }
        }

            //判断是否满足条件
        if (!MeetTheConditions(table))
            return null;

        if (m_listSuccessor.Count > 1)  //有多个不同概率的后续
        {
            //根据概率选择一个后续
            m_iSuccessorIndex = RandomlySelectResultList();
        }

        //根据选择的后续，逐个计算term，输出单一的term，并添加到结果链表中
        LLinkedListNode<LTerm> srcNode = m_listSuccessor[m_iSuccessorIndex].Result.First;

        do 
        {
            LTerm destTerm = GetResultTerm(srcNode.Value, table);    //获取最终的单个term
            result.Add(destTerm);       //插入Term

            srcNode = srcNode.Next;     //移动到下一个节点
        } while (srcNode != m_listSuccessor[m_iSuccessorIndex].Result.First);

        table.EndLoadData();
        table.Clear();

        return result;
    }

    /// <summary>
    /// 判断是否符合上下文条件
    /// </summary>
    /// <returns></returns>
    private bool isContextual(LLinkedListNode<LTerm> node, out List<LTerm> FinnalLeftContext, out List<LTerm> FinnalRightContext)
    {
        if (!isLeftContextual(node, out FinnalLeftContext))    //判断左侧上下文是否符合条件
        {
            FinnalRightContext = new List<LTerm>();
            return false;
        }

        if (!isRightContextual(node, out FinnalRightContext))   //判断右侧上下文是否符合条件
            return false;

        return true;
    }

    private bool isLeftContextual(LLinkedListNode<LTerm> node, out List<LTerm> FinnalLeftContext)
    {
        List<LTerm> LeftContext = m_Predecessor.LeftContext;
        FinnalLeftContext = new List<LTerm>();   //初始化输出的链表

        if (LeftContext == null || LeftContext.Count == 0)  //无左侧的上下文
            return true;

        int indexOfContext = LeftContext.Count - 1;
        //LLinkedListNode<Term> headNode = Scene.GetInstance().TreeModel.RuleData.FinalList.First;   //头节点
        LLinkedListNode<LTerm> headNode = Belong.FinalList.First;
        LLinkedListNode<LTerm> currentNode = node.Previous;

        while (currentNode != headNode.Previous && indexOfContext >= 0) //当当前节点已经到达头结点或上下文的索引已超出范围
        {
            if (isIgnored(currentNode.Value) || currentNode.Value.Symbol.Equals("["))   //该节点为被无视节点
            {
                currentNode = currentNode.Previous;
                continue;
            }

            if (currentNode.Value.Symbol.Equals("]"))   //当当前节点的符号为“]”时，需要寻找与其匹配的“[”，而这中间的节点均无视（不属于节点的上下文）
            {
                currentNode = FindMatchingBracket(currentNode.Previous, "]").Previous;
                continue;
            }

            if (currentNode.Value.Symbol.Equals(LeftContext[indexOfContext].Symbol) || LeftContext[indexOfContext].Symbol.Equals("*"))    //与当前的上下文中的符号相同
            {
                FinnalLeftContext.Add(currentNode.Value);   //添加最终结果链表中的结果，用于后续条件的计算
                indexOfContext--;   //检验前一个节点的符号是否相同
                currentNode = currentNode.Previous;
            }
            else //与当前的上下文中的符号不匹配
            {
                return false;
            }
        }

        if (indexOfContext >= 0)    //当当前节点已经到达头结点而上下文的索引还未能超出范围，说明节点的左侧上下文与匹配的左侧上下文个数不相等
            return false;
        else
        {
            FinnalLeftContext.Reverse();    //翻转，因为是逆向插入
            return true;
        }
    }

    private bool isRightContextual(LLinkedListNode<LTerm> node, out List<LTerm> FinalRightContext)
    {
        List<LTerm> RigthContext = m_Predecessor.RightContext;
        FinalRightContext = new List<LTerm>();

        if (RightContext == null || RightContext.Count == 0)    //无右侧的上下文
            return true;

        int indexOfContext = 0;
        LLinkedListNode<LTerm> headNode = Belong.FinalList.First;
            //Scene.GetInstance().TreeModel.RuleData.FinalList.First;   //头节点
        LLinkedListNode<LTerm> currentNode = node.Next;

        while (currentNode != headNode && indexOfContext < RigthContext.Count)  //当当前节点已经到达头结点或上下文的索引已超出范围
        {
            if (isIgnored(currentNode.Value))   //该节点为被无视节点
            {
                currentNode = currentNode.Next;
                continue;
            }

            //当字符串为A[A]B时，前驱为A > B --> B
            //第一个A有两个后续，括号中的A和不在括号中的B
            //前驱的右侧上下文只匹配不在括号中的B，因此要跳过[A]
            if (!RigthContext[indexOfContext].Symbol.Equals("[") && currentNode.Value.Symbol.Equals("["))
            {
                currentNode = FindMatchingBracket(currentNode.Next, "[").Next;
                continue;
            }

            if (RigthContext[indexOfContext].Symbol.Equals("]"))
            {
                //假设前驱是 S > G[H]M， 字符串为 SG[HI[JK]L]MNO
                //当前的RightContext[indexOfContext]为G[H]M中的]
                //当前的节点currentNode为I
                //需要跳过H后续的I[JK]L，因此需要寻找L后的“]”
                currentNode = FindMatchingBracket(currentNode, "[");
            }

            if (currentNode.Value.Symbol.Equals(RightContext[indexOfContext].Symbol))    //与当前的上下文中的符号相同
            {
                FinalRightContext.Add(currentNode.Value);   //插入最终结果链表的结果，用于后续条件的计算
                indexOfContext++;   //检验后一个节点的符号是否相同
                currentNode = currentNode.Next;
            }
            else if(!currentNode.Value.Symbol.Equals("]") && RigthContext[indexOfContext].Symbol.Equals("*"))   //与任意字符匹配
            {
                FinalRightContext.Add(currentNode.Value);
                indexOfContext++;
                currentNode = currentNode.Next;
            }
            else //与当前的上下文中的符号不匹配
            {
                return false;
            }

        }

        if (indexOfContext < RigthContext.Count)    //当当前节点已经到达头结点而上下文的索引还未能超出范围，说明节点的左侧上下文与匹配的左侧上下文个数不相等
            return false;
        else
            return true;
    }

    private bool isIgnored(LTerm term)
    {
        if (m_RuleDataEntry.Ignore == null)
            return false;

        return m_RuleDataEntry.Ignore.Exists(MatchTerm => MatchTerm.Symbol.Equals(term.Symbol));
    }

    /// <summary>
    /// 寻找匹配的括号
    /// </summary>
    /// <param name="node">从该节点开始寻找</param>
    /// <param name="strBracket">待匹配的括号</param>
    /// <returns></returns>
    private LLinkedListNode<LTerm> FindMatchingBracket(LLinkedListNode<LTerm> node, string strBracket)
    {
        LLinkedListNode<LTerm> headNode = Belong.FinalList.First;
            //Scene.GetInstance().TreeModel.RuleData.FinalList.First;   //头节点
        int iBalance = 0; //用于标识是否平衡，当出现[时，iBalance加一，当出现]时，iBalance减一，当iBalance等于0时，左右括号个数相等
        if (strBracket.Equals("]"))
        {
            iBalance = -1;
            LLinkedListNode<LTerm> currentNode = node;
            do 
            {
                if (currentNode.Value.Symbol.Equals("["))
                    iBalance += 1;
                else if (currentNode.Value.Symbol.Equals("]"))
                    iBalance -= 1;

                if (iBalance == 0)
                    return currentNode;

                currentNode = currentNode.Previous;
            } while (node.Previous != headNode.Previous);
        }
        else if (strBracket.Equals("["))
        {
            iBalance = 1;
            LLinkedListNode<LTerm> currentNode = node;
            do 
            {
                if (currentNode.Value.Symbol.Equals("["))
                    iBalance += 1;
                else if (currentNode.Value.Symbol.Equals("]"))
                    iBalance -= 1;

                if (iBalance == 0)
                    return currentNode;

                currentNode = currentNode.Next;
            } while (currentNode != headNode);
        }
        else    //即不是左括号又不是右括号，则返回原来的节点
            return node;

        throw new Exception("Missing left or right braket.");   //节点个数不相等
    }

    /// <summary>
    /// 判断该产生式是否满足当前的条件，如F(length+1):length>5，若length小于或等于5则无法满足条件。
    /// length的值通过table传递。
    /// </summary>
    /// <param name="table">传递实参，用于条件判断</param>
    /// <returns>返回布尔型，确定该产生式是否满足当前的条件</returns>
    private bool MeetTheConditions(DataTable table)
    {
        if (m_Predecessor.Condition == null) return true;  //没有条件

        string strCondtion = m_Predecessor.Condition.Replace("==", "=").Replace("=!", "<>").Replace("&", " and ").Replace("||", " or ");   //不等于字符串的切换
        table.Columns["expression"].Expression = strCondtion;

        if (table.Rows[0][table.Columns.Count - 1].ToString().Equals("0"))   //不满足条件
            return false;
        else
            return true;
    }

    /// <summary>
    /// 根据每个后续的概率随机选择一个后续，并返回后续的索引
    /// </summary>
    /// <returns>随机选择的后续索引值</returns>
    private int RandomlySelectResultList()
    {
        int result = 0;

        int countOfSuccessor = m_listSuccessor.Count;
        float[] probabilityArray = new float[countOfSuccessor];

        for (int i = 0; i < countOfSuccessor; i++)
        {
            if (i == 0)
                probabilityArray[0] = m_listSuccessor[0].Probability.Value;
            else
                probabilityArray[i] = m_listSuccessor[i].Probability.Value + probabilityArray[i - 1];
        }

        //根据随机产生的数字判断使用哪个结果链表
        double randomNum = RandomNumer.Double() * probabilityArray[countOfSuccessor-1];
        for (int i = 0; i < countOfSuccessor; i++)
        {
            if (randomNum < probabilityArray[i])
            {
                result = i;
                break;
            }
        }

        return result;
    }


    /// <summary>
    /// 根据含有形参的Term以及各实参计算出最终的Term并返回。
    /// 如 F(length+1) 为含有形参的Term，length 为 1，则返回F(2)
    /// </summary>
    /// <param name="scrTerm">含有形参的Term，如F(length+1)</param>
    /// <param name="table">包含各实参的DataTable类</param>
    /// <returns>返回计算完成后的Term</returns>
    private LTerm GetResultTerm(LTerm scrTerm, DataTable table)
    {
        if (scrTerm == null)
            return null;

        LTerm destTerm = new LTerm();

        destTerm.Symbol = scrTerm.Symbol; //将符号赋值给新的term

        table.Rows[table.Rows.Count - 1]["RANDOM"] = RandomNumer.Single();  //写入随机数

        for (int i = 0; i < scrTerm.Params.Count; i++)
        {
            table.Columns["expression"].Expression = scrTerm.Params[i];            //根据参数中的公式进行赋值，如F(length + 1)中的length + 1
            destTerm.Params.Add(table.Rows[0][table.Columns.Count - 1].ToString());  //根据实参、公式计算出结果，并赋值给输出的term中的参数列表
        }

        return destTerm;
    }

    /// <summary>
    /// 对该产生式进行验证，确定其可用
    /// </summary>
    public void Validate()
    {
        //该结构体的前驱、结果和概率必须不为null
        m_Predecessor.Term.Validate();

        for (int i = 0; i < m_listSuccessor.Count; i++)
        {
            m_listSuccessor[i].Validate();
        }
    }

    /// <summary>
    /// 对输入的参数进行验证，确定其可用
    /// </summary>
    /// <param name="inputParams"></param>
    private void ValidateInputParams(params float[] inputParams)
    {
        if (inputParams.Length != m_Predecessor.Term.Params.Count)
            throw new InvalidOperationException("Unequal number between input parameters and reference parameters");
    }

    public void Clear()
    {
        this.m_Predecessor.Clear();
        this.m_listSuccessor.Clear();
        this.m_listSuccessor = null;
        GC.Collect();
    }

    public override string ToString()
    {
        string result = "";

        for (int i = 0; i < m_listSuccessor.Count; i++)
        {
            result += m_Predecessor.Term.ToString();

            if (m_Predecessor.Condition != null)
            {
                result = result + ":(" + m_Predecessor.Condition + ")";
            }

            result += " --> ";

            if (m_listSuccessor[i].Probability != 1.0)
            {
                result = result + "(" + m_listSuccessor[i].Probability + ")";
            }

            result += m_listSuccessor[i].Result.ToString();

            if (i != m_listSuccessor.Count - 1)
                result += "\n";
        }

        return result;
    }
}
