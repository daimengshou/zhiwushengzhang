/*
 * 文件名：LTerm.cs
 * 描述：L-系统单一模块（底层单元）
 */
using System;
using System.Collections.Generic;

//L系统中单一的一个块，如F(length) F为符号，length为参数
public class LTerm
{
    private string m_cSymbol;    //符号
    private List<string> m_listParams;    //参数

    /*
     * 构造函数，创建模块
     * 构造函数包括：
     * ①参数为空 ②带符号和参数列表（包括string[]形式与list<string>形式
     * ③表示模块的字符串（string形式）
     */
    public LTerm()
    {
        m_cSymbol = null;
        m_listParams = new List<string>();
    }

    public LTerm(string symbol, params string[] aParams)
    {
        m_cSymbol = symbol;
        this.m_listParams = new List<string>(aParams);
    }

    public LTerm(string symbol, List<string> aParams)
    {
        m_cSymbol = symbol;
        this.m_listParams = aParams;
    }

    public LTerm(string statement)
    {
        if (statement.Length == 0 || statement == null) //输入的语法为空
            throw new ArgumentNullException("Input syntax error.\nMay be the syntax is empty.");

        //语句是否符合要求
        int CountOfLeftBrackets = 0;
        int CountOfRightBrackets = 0;
        for (int i = 0; i < statement.Length; i++)
        {
            if (statement[i] == '(')
                CountOfLeftBrackets++;
            else if (statement[i] == ')')
                CountOfRightBrackets++;
        }

        if (CountOfLeftBrackets != CountOfRightBrackets) 
            throw new InvalidOperationException("Input syntax error.\nMay be missing left or right brackets."); //括号不对等，输入的语法错误
        if (CountOfRightBrackets != 0 && !statement.EndsWith(")"))
            throw new InvalidOperationException("Input syntax error.\nMay be missing right bracket in the end."); //不是以右括号结束，输入的语法错误

        if (CountOfLeftBrackets != 0)  //有括号，即有参数
        {
            int indexOfLeafBracket = statement.IndexOf('(');    //最左边的左括号的索引
            int indexOfRightBracket = statement.Length - 1;     //最右边的右括号的索引

            m_cSymbol = statement.Substring(0, indexOfLeafBracket/* - 0*/);
            m_listParams = new List<string>(statement.Substring(indexOfLeafBracket + 1, indexOfRightBracket - indexOfLeafBracket - 1).Split(','));
        }
        else if (statement.Length > 1)  //无括号（无参数）的情况下，语句的长度大于1，即该语句中的符号一定不属于默认的符号
        {
            m_cSymbol = statement;
            m_listParams = new List<string>(0);
        }
        else //无括号（无参数）的情况下，语句的长度等于1，即该语句中的符号可能属于默认的符号
        {
            m_cSymbol = statement;
            m_listParams = new List<string>(1);

            switch (statement[0])
            {
                case 'F':
                case 'f': 
                    m_listParams.Add("1");
                    break;
                case '+':
                case '-':
                case '&':
                case '^':
                case '\\':
                case '/':
                case '|': 
                    m_listParams.Add("180");
                    break;
                default :
                    break; 
            }
        }
    }

    public string Symbol
    {
        get { return m_cSymbol; }
        set { m_cSymbol = value; }
    }

    public List<string> Params
    {
        get { return m_listParams; }
        set { m_listParams = value; }
    }

    /// <summary>
    /// 设置模块的符号，建议仅在以参数为空形式构造后使用
    /// </summary>
    /// <param name="symbol">修改后的符号</param>
    public void SetSymbol(string symbol)
    {
        m_cSymbol = symbol;
    }

    /// <summary>
    /// 添加参数，建议仅在以参数为空形式构造后使用
    /// </summary>
    /// <param name="para">待添加的参数</param>
    public void AddPara(string para)
    {
        if (para == null)
            throw new ArgumentNullException("The added parameter is null");
        m_listParams.Add(para);
    }

    /// <summary>
    /// 获取参数列表，并以浮点型表示
    /// </summary>
    /// <returns>浮点型参数列表</returns>
    public float[] GetParamsArrayWithFloat()
    {
        float[] paramsArray = new float[m_listParams.Count];
        for (int i = 0; i < paramsArray.Length; i++ )
        {
            paramsArray[i] = Convert.ToSingle(m_listParams[i]);
        }

        return paramsArray;
    }

    /// <summary>
    /// 检验该模块是否可行
    /// </summary>
    public void Validate()
    {
        if (m_cSymbol == null) throw new ArgumentNullException("No symbol");

        //if (m_listParams.Count == 0) throw new ArgumentNullException("No parameters");
    }

    /// <summary>
    /// 清除模块数据
    /// </summary>
    public void Clear()
    {
        m_cSymbol = null;
        m_listParams.Clear();
        m_listParams = null;
        GC.Collect();
    }

    public override string ToString()
    {
        string result = "";
        result += m_cSymbol;

        if (m_listParams.Count == 0)
            return result;

        result += "(";

        if (m_cSymbol.Equals("%"))  //存在Mesh名替换的情况
        {
            for (int i = 0; i < m_listParams.Count; i++)
            {
                //判断是否为数字，如果为数字则判断时候需要替换
                string MeshName = StringValidate.IsNumeric(m_listParams[i]) ? MeshResource.GetInstance().GetNameOf(Convert.ToInt32(m_listParams[i])) : m_listParams[i];

                if (i != 0)
                    result += ",";

                if (MeshName != null)
                    result += MeshName;
                else
                    result += m_listParams[i];
            }
        }
        else
        {
            for (int i = 0; i < m_listParams.Count; i++)
            {
                if (i != 0)
                    result += ",";
                result += m_listParams[i];
            }
        }


        result += ")";
        return result;
    }
}
