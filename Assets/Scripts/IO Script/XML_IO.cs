using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//该类用于读写XML文件
public class XML_IO
{
    /// <summary>
    /// 打开url路径下的XML文件
    /// </summary>
    /// <param name="url">文件路径</param>
    /// <param name="model">将从XML读取的内容存放在该Model中</param>
    /// <returns></returns>
    public static bool Open(string url, TreeModel model)
    {
        XmlTextReader reader = new XmlTextReader(url);  //打开该文件，自带检测该文件是否存在

        model.Initialize(); //初始化

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Model": ParseModelElement(reader, model); break;
                    case "Variable": ParseVariableElement(reader, model); break;
                    case "Mesh": ParseMeshElement(reader, model);break;
                }
            }
        }

        reader.Close();

        return true;
    }

    /// <summary>
    /// 解析XML文件中Model元素的内容
    /// </summary>
    /// <param name="modelElement">读取的整个Model元素</param>
    /// <param name="treeModel">将读取的Model元素的内容存放在该Model中</param>
    private static void ParseModelElement(XmlTextReader modelElement, TreeModel treeModel)
    {
        treeModel.Name = modelElement.GetAttribute("Name").Trim();  //模型名字
        treeModel.CurrentStep = treeModel.InitStep = 
            Convert.ToInt32(modelElement.GetAttribute("Step"));  //当前生命周期
        treeModel.MaxStep = Convert.ToInt32(modelElement.GetAttribute("AllSteps"));      //总生命周期
        ParseRuleAttribute(modelElement.GetAttribute("Rule"), treeModel.RuleData);   //L系统规则

        string texturePath = modelElement.GetAttribute("Texture");
        treeModel.TexturePath = texturePath;
        int indexOfFormat = texturePath.LastIndexOf('.');
        texturePath = texturePath.Remove(indexOfFormat);
        treeModel.BranchTexture = Resources.Load(texturePath, typeof(Texture2D)) as Texture2D;  //纹理

        ////纹理尺寸
        //treeModel.BranchTextureScale = new Vector2(Convert.ToSingle(modelElement.GetAttribute("TexCordU"))/*u坐标*/,
        //                                           Convert.ToSingle(modelElement.GetAttribute("TexCordV"))/*v坐标*/);

        ////模型尺寸
        //treeModel.ModelScale = Convert.ToSingle(modelElement.GetAttribute("Size"));

    }

    /// <summary>
    /// 解析XML文件中Model元素的Rule属性
    /// </summary>
    /// <param name="ruleAttribute">Rule属性</param>
    /// <param name="ruleData">将Rule属性的内容存放在ruleData中</param>
    private static void ParseRuleAttribute(string ruleAttribute, LSTree ruleData)
    {
        ruleAttribute = ruleAttribute.Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace(" ", "");   //清除空格等符号
        string[] separtor = { "%%**" };
        string[] ruleArray = ruleAttribute.Split(separtor, StringSplitOptions.RemoveEmptyEntries);  //将字符串分类

        for (int i = 0; i < ruleArray.Length; i++)
        {
            ParseRule(ruleArray[i], ruleData);
        }
    }

    /// <summary>
    /// 对规则进行分析，并存入对应的模型中
    /// </summary>
    /// <param name="rule">规则</param>
    /// <param name="ruleData">将解析出的内容存放在ruleData中</param>
    private static void ParseRule(string rule, LSTree ruleData)
    {
        if (rule == null || rule.Length == 0) throw new ArgumentNullException("Empty rule.");

        if (rule[0] == '#' && rule.Substring(0, 7).ToLower().Equals("#ignore"))   //上下文的无视条件
        {
            ruleData.AddIgnore(GetTermList(rule.Substring(8)).ToList());
        }
        else if (rule[0] == '@') //公理
        {
            if (ruleData.Axiom.Count > 0) throw new InvalidOperationException("Too much axiom");
            ruleData.AddAxiom(GetTermList(rule.Substring(1)));
        }
        else //产生式
        {
            string[] separtor = { "-->" };
            string[] subRules = rule.Split(separtor, StringSplitOptions.RemoveEmptyEntries);//将产生式分解成前半部分（包括前驱和条件）和后半部分（包括后续，即概率和结果）

            if (subRules.Length != 2) throw new InvalidOperationException("Invalid Production");
            LRule production = new LRule();
            
            //前半部分
            string[] PredecessorAndConditions = subRules[0].Split(':');
            string Predecessor;

            //解析右侧上下文
            string[] PredecessorAndRightContext = PredecessorAndConditions[0].Split('>');
            if (PredecessorAndRightContext.Length == 2) //存在右侧上下文
            {
                string RigthContext = PredecessorAndRightContext[1];
                production.AddRightContext(GetTermList(RigthContext).ToList());
            }

            //解析左侧上下文
            string[] LeftContextAndPredecessor = PredecessorAndRightContext[0].Split('<');
            if (LeftContextAndPredecessor.Length == 2)  //存在左侧上下文
            {
                string LeftContext = LeftContextAndPredecessor[0];
                Predecessor = LeftContextAndPredecessor[1];
                production.AddLeftContext(GetTermList(LeftContext).ToList());
            }
            else
            {
                Predecessor = LeftContextAndPredecessor[0];
            }

            production.AddPredecessor(Predecessor); //添加前驱
            if (PredecessorAndConditions.Length == 2)
            {
                production.AddCondition(PredecessorAndConditions[1].Remove(PredecessorAndConditions[1].Length - 1, 1).Remove(0, 1));    //添加去掉头尾的括号的条件
            }
            
            //后半部分
            string ProbabilityAndResult = subRules[1];
            if (ProbabilityAndResult[0] == '(')  //有概率
            {
                int iIndexOfProbability = ProbabilityAndResult.IndexOf(')');
                production.AddProbability(Convert.ToSingle(ProbabilityAndResult.Substring(1, iIndexOfProbability - 1)));    //添加概率
                production.AddResult(GetTermList(ProbabilityAndResult.Substring(iIndexOfProbability + 1)));     //添加结果
            } 
            else    //无概率
            {
                production.AddResult(GetTermList(ProbabilityAndResult));
            }

            ruleData.AddProduction(production);

        }
    }

    /// <summary>
    /// 根据字符串获取其链表
    /// </summary>
    /// <param name="subRule">需要解析的字符串</param>
    /// <returns>通过字符串解析得到的链表</returns>
    private static LLinkedList<LTerm> GetTermList(string subRule)
    {
        LLinkedList<LTerm> result = new LLinkedList<LTerm>();

        string[] termArray = SplitTerm(subRule);    //将字符串分解成各个代表Term的字符串

        for (int i = 0; i < termArray.Length; i++)
        {
            result.Add(new LTerm(termArray[i]));     //逐个添加到链表中
        }

        return result;
    }

    /// <summary>
    /// 将字符串分解成各个代表Term的字符串。
    /// 如字符串A(a,b)B(c,d)C(e,f)分解成三个字符串A(a,b)、B(c,d)、C(e,f)
    /// </summary>
    /// <param name="rule">待分解的字符串</param>
    /// <returns>分解后的字符串集合</returns>
    private static string[] SplitTerm(string rule)
    {
        if (rule == null || rule.Length == 0)
            return null;

        List<string> result = new List<string>();

        int indexOfStart = -1;  //用于记录单个Term字符串的起始位置
        int iBalanceOfBrackets = 0; //用于表示小括号的平衡（0表示达到平衡，即左括号和右括号的个数相同）
        for (int i = 0; i < rule.Length; i++ )
        {
            if (indexOfStart == -1) //未开始记录
            {
                if (rule.Length == i + 1 || rule[i + 1] != '(') //无参数
                {
                    result.Add(rule[i].ToString());
                }
                else //有参数，开始记录
                {
                    indexOfStart = i;
                }
            }
            else //已经开始记录
            {
                switch (rule[i])
                {
                    case '(': iBalanceOfBrackets += 1; break;
                    case ')':
                        iBalanceOfBrackets -= 1;
                        if (iBalanceOfBrackets == 0)
                        {
                            result.Add(rule.Substring(indexOfStart, i - indexOfStart + 1));
                            indexOfStart = -1;
                        }
                        break;
                }
            }
        }

        return result.ToArray();
    }

    //解析XML文件中Variable元素的内容
    private static void ParseVariableElement(XmlTextReader variableElement, TreeModel treeModel)
    {
        LSTree ruleData = treeModel.RuleData;

        string name = variableElement.GetAttribute("VName");

        string strValue = variableElement.GetAttribute("VValue");
        if (strValue == null || strValue.Length == 0)   //确保value的值存在
            throw new ArgumentNullException("No VValue Attribute in Variable Element.");
        float value = Convert.ToSingle(variableElement.GetAttribute("VValue"));

        string strMax = variableElement.GetAttribute("VMax");
        string strMin = variableElement.GetAttribute("VMin");
        string description = variableElement.GetAttribute("VDesc");

        if (strMax != null && strMax.Length > 0 && strMin != null && strMin.Length > 0) //有最大最小值
        {
            float max = Convert.ToSingle(strMax);
            float min = Convert.ToSingle(strMin);
            ruleData.AddGlobalVariable(name, value, max, min, description);
        }
        else    //无最大最小值
        {
            ruleData.AddGlobalVariable(name, value);
        }
    }

    //解析XML文件中
    private static void ParseMeshElement(XmlTextReader meshElement, TreeModel treeModel)
    {
        string name = meshElement.GetAttribute("MName");
        if (name == null || name.Length == 0)
            throw new ArgumentNullException("No MName Attribute in Mesh Element.");

        string MeshPath = meshElement.GetAttribute("MPath");
        if (MeshPath == null || MeshPath.Length == 0)
            throw new ArgumentNullException("No MPath Attribute in Mesh Element.");

        string TexturePath = meshElement.GetAttribute("MTexPath");
        if (TexturePath == null || TexturePath.Length == 0)
            throw new ArgumentNullException("No MTexPath Attribute in Mesh Element.");

        string MaxSize = meshElement.GetAttribute("MMaxSize");
        if (MaxSize == null || MaxSize.Length == 0)
            throw new ArgumentNullException("No MMaxSize Attribute in Mesh Element.");

        string size = meshElement.GetAttribute("MSize");
        if (size == null || size.Length == 0)
            throw new ArgumentNullException("No MSize Attribute in Mesh Element.");

        string type = meshElement.GetAttribute("MType");
        if (type == null || type.Length == 0)
            throw new ArgumentNullException("No MType Attribute in Mesh Element.");

        treeModel.AddMesh(name, MeshPath, TexturePath, Convert.ToSingle(size), Convert.ToSingle(MaxSize), (OrganType)Enum.Parse(typeof(OrganType), type));
    }

    public static bool Save(string url,
                            string name, int initStep, int maxStep, string lRule, string branchTexPath,
                            List<VariableEntry> variables, List<Mesh> meshes)
    {
        if (name == null || name.Length == 0 ||
            initStep < 1 || maxStep < 1 ||
            lRule == null || lRule.Length == 0)
            return false;

        XmlTextWriter writer = new XmlTextWriter(url, System.Text.Encoding.UTF8);

        writer.Formatting = Formatting.Indented;

        writer.WriteStartDocument();    //书写<?xml version="1.0" encoding="utf-8"?>

        writer.WriteStartElement("L-System_ForUnity-1.0");   //书写根元素

        WriteStartModel(writer, name, initStep, maxStep, lRule, branchTexPath); //开始书写模型
        WriteVariable(writer, variables);   //书写全局变量
        WriteMesh(writer, meshes);          //书写Mesh
        WriteEndModel(writer);              //结束书写模型
        
        writer.WriteEndElement();
        writer.WriteEndDocument();

        writer.Close();

        return true;
    }

    private static void WriteStartModel(XmlTextWriter writer, string name, int initStep, int maxStep, string lRule, string branchTexPath)
    {
        writer.WriteStartElement("Model");

        writer.WriteAttributeString("Name", name);
        writer.WriteAttributeString("Step", initStep.ToString());
        writer.WriteAttributeString("AllSteps", maxStep.ToString());
        writer.WriteAttributeString("Rule", lRule.Replace("\n", "%%**"));
        writer.WriteAttributeString("Texture", branchTexPath);
    }

    private static void WriteEndModel(XmlTextWriter writer)
    {
        writer.WriteEndElement();
    }

    private static void WriteVariable(XmlTextWriter writer, List<VariableEntry> variables)
    {
        foreach (VariableEntry variable in variables)
        {
            writer.WriteStartElement("Variable");

            writer.WriteAttributeString("VName", variable.Name);
            writer.WriteAttributeString("VValue", variable.Value == null ? "0" : variable.Value.Value.ToString());
            writer.WriteAttributeString("VMax", variable.Max == null ? "0" : variable.Max.Value.ToString());
            writer.WriteAttributeString("VMin", variable.Min == null ? "0" : variable.Min.Value.ToString());
            writer.WriteAttributeString("VDesc", variable.Description);

            writer.WriteEndElement();
        }
    }

    private static void WriteMesh(XmlTextWriter writer, List<Mesh> meshes)
    {
        foreach (Mesh mesh in meshes)
        {
            writer.WriteStartElement("Mesh");

            writer.WriteAttributeString("MName", mesh.Name);
            writer.WriteAttributeString("MPath", mesh.MeshPath);
            writer.WriteAttributeString("MTexPath", mesh.TexturePath);
            writer.WriteAttributeString("MMaxSize", mesh.MaxSize.ToString());
            writer.WriteAttributeString("MSize", mesh.Size.ToString());
            writer.WriteAttributeString("MType", mesh.Type.ToString());

            writer.WriteEndElement();
        }
    }
    
}
