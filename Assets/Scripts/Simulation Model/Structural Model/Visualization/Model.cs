/*
 * 文件名：Model.cs
 * 描述：L-系统模型相关类
 */
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

/*
 * 模型信息类
 * 存储创建模型时所需的参数，避免各函数间调用过多的参数。
 * 
 * @version: 1.0
 */
public class GameObjectInfo
{
    private float m_Length = 1.0f;
    private float m_Radius = 1.0f;
    private Vector3 m_Position = Vector3.zero;
    private Vector3 m_Rotation = Vector3.zero;
    private Vector3 m_Scale = Vector3.one;

    public GameObjectInfo()
    {
    }

    public float Length
    {
        get { return m_Length; }
        set { m_Length = value; }
    }

    public float Radius
    {
        get { return m_Radius; }
        set { m_Radius = value; }
    }

    public Vector3 Position
    {
        get { return m_Position; }
        set { m_Position = value; }
    }

    public Vector3 Rotation
    {
        get { return m_Rotation; }
        set { m_Rotation = value; }
    }

    public Vector3 Scale
    {
        get { return m_Scale; }
        set { m_Scale = value; }
    }

    public GameObjectInfo Clone()
    {
        GameObjectInfo result = new GameObjectInfo();

        result.Length = Length;
        result.Radius = Radius;
        result.Position = Position;
        result.Rotation = Rotation;
        result.Scale = Scale;

        return result;
    }
}

/*
 * L-系统模型类的一部分。
 * 该部分主要用于迭代L-系统以及将迭代结果可视化。
 * 
 * @version: 1.0
 */
public partial class TreeModel : BaseTree
{
    /*****模型固有参数*****/
    private string      m_strName;              //名字
    private int         m_iInitStep;            //初始生命周期
    private int         m_iCurrentStep;         //当前生命周期
    private int         m_iMaxStep;             //总生命周期
    private LSTree      m_RuleData;             //L系统规则
    private string      m_strTexturePath;       //纹理存放位置
    private Texture2D   m_Texture;              //枝干纹理
    private float       m_fModelScale;          //模型尺寸
    private List<Mesh>  m_MeshGroup;            //Mesh集

    private List<TreeModel> m_listChildModels;      //子模型
    private TreeModel       m_parentModel;          //父模型

    /*****内部变量******/
    private GameObject m_TreeModel;                 //存储整体模型
    private GameObject m_BranchModel;               //存储枝干模型
    private List<BranchIndex> m_listBranchIndexes;  //存储枝干模型的数据
    private List<GameObject> m_listOrganModels;     //存储器官模型
    private List<OrganIndex> m_listOrganIndexes;    


#region 获取成员变量
    public string Name
    {
        get { return m_strName.Length == 0 ? null : m_strName; }
        set { m_strName = value; }
    }

    public int InitStep
    {
        get { return m_iInitStep; }
        set { m_iInitStep = value; }
    }

    public int CurrentStep
    {
        get { return m_iCurrentStep; }
        set { m_iCurrentStep = value; }
    }

    public int MaxStep
    {
        get { return m_iMaxStep; }
        set { m_iMaxStep = value; }
    }

    public LSTree RuleData
    {
        get
        {
            if (m_RuleData.Belong == null)
                m_RuleData.Belong = this;

            return m_RuleData;
        }
    }

    public string TexturePath
    {
        get { return m_strTexturePath; }
        set { m_strTexturePath = value; }
    }

    public Texture2D BranchTexture
    {
        get { return m_Texture; }
        set { m_Texture = value; }
    }


    public float ModelScale
    {
        get { return m_fModelScale; }
        set { m_fModelScale = value; }
    }

    public List<Mesh> Meshes
    {
        get { return m_MeshGroup; }
        set { m_MeshGroup = value; }
    }

    public List<TreeModel> ChildModels
    {
        get { return m_listChildModels; }
    }

    public TreeModel ParentModel
    {
        get { return m_parentModel; }
        set { m_parentModel = value; }
    }

    public GameObject TreeModelInstance
    {
        get { return m_TreeModel; }
    }

    public GameObject BranchModel
    {
        get { return m_BranchModel; }
    }

    public List<BranchIndex> BranchIndexes
    {
        get { return m_listBranchIndexes; }
    }

    public List<GameObject> OrganModels
    {
        get { return m_listOrganModels; }
    }

    public List<OrganIndex> OrganIndexes
    {
        get { return m_listOrganIndexes; }
    }
#endregion

    /// <summary>
    /// 新添加一个Mesh
    /// </summary>
    /// <param name="Name">Mesh的名字</param>
    /// <param name="MeshPath">Mesh存放的路径</param>
    /// <param name="TexturePath">Mesh纹理存放的路径</param>
    /// <param name="Size">Mesh的尺寸</param>
    /// <param name="MaxSize">Mesh的最大尺寸</param>
    public void AddMesh(string Name, string MeshPath, string TexturePath, float Size, float MaxSize, OrganType Type)
    {
        m_MeshGroup.Add(new Mesh(Name, MeshPath, TexturePath, Size, MaxSize, Type, this));
    }

    private void AddMeshIndex(int Index, OrganType Type, BranchIndex From, GameObject _GameObject)
    {
        if (m_listOrganIndexes == null) m_listOrganIndexes = new List<OrganIndex>();

        if (Type == OrganType.Leaf)     //叶子
            m_listOrganIndexes.Add(new LeafIndex(Index, From, _GameObject) as OrganIndex);
        else    //其他器官
            m_listOrganIndexes.Add(new OrganIndex(Index, Type, From, _GameObject));
    }

    public List<BranchIndex> GetBranchIndexes()
    {
        return BranchIndexes;
    }

    public List<LeafIndex> GetLeafIndexes()
    {
        List<LeafIndex> result = new List<LeafIndex>();

        foreach (OrganIndex organIndex in OrganIndexes)
        {
            if (organIndex.Type != OrganType.Leaf) continue;

            result.Add(organIndex as LeafIndex);
        }

        return result;
    }

    public List<OrganIndex> GetFemaleIndexes()
    {
        List<OrganIndex> result = new List<OrganIndex>();

        foreach (OrganIndex organIndex in OrganIndexes)
        {
            if (organIndex.Type != OrganType.Flower) continue;

            result.Add(organIndex);
        }

        return result;
    }

    public bool IsEmpty()
    {
        return MaxStep <= 0;
    }


    /// <summary>
    /// 下一步
    /// </summary>
    public void NextStep()
    {
        if (m_iCurrentStep < m_iMaxStep)
        {
            m_RuleData.NextStep();
            m_iCurrentStep++;
        }
    }


    private const float DEFAULT_HEIGHT = 2.0f;  //unity圆柱体的默认长度
    private const float DEFAULT_RADIUS = 0.5f;  //unity圆柱体的默认半径

    private LLinkedListNode<LTerm> _CurrentNode;     //用于记录当前的节点位置
    private int GetIndexWithSameType(OrganType type, BranchIndex from)
    {
        OrganIndex lastIndex;
        return GetIndexWithSameType<OrganIndex>(type, from, out lastIndex);
    }

    private int GetIndexWithSameType<T>(OrganType type, BranchIndex from, out T lastIndex)
        where T : OrganIndex
    {
        int index = 0;
        lastIndex = null;

        for (int i = m_listOrganIndexes.Count - 1; i >= 0; i--)
        {
            if (m_listOrganIndexes[i].From != from) break;

            if (m_listOrganIndexes[i].Type == type)
            {
                index = m_listOrganIndexes[i].Index + 1;
                lastIndex = m_listOrganIndexes[i] as T;
                break;
            }
        }

        return index;
    }

    /// <summary>
    /// 设置所有孩子的Tag
    /// </summary>
    /// <param name="Fahter"></param>
    /// <param name="TagName"></param>
    private void SetTagInParent(Transform Fahter, string TagName)
    {
        foreach (Transform Child in Fahter)
        {
            Child.tag = TagName;

            if (Child.childCount > 0)
                SetTagInParent(Child, TagName);
        }
        
    }

    private void ValidateStep()
    {
        if (m_RuleData.CurrentStep < 0) //规则的生命周期小于0，生命周期错误
            throw new InvalidProgramException("Wrong step of rule data.");
        if (m_iCurrentStep < 0) //模型的生命周期小于0，生命周期错误
            throw new InvalidProgramException("Wrong step of model.");

        if (m_RuleData.CurrentStep < m_iCurrentStep)    //规则的生命周期小于模型的生命周期，则使两者生命周期相等
        {
            do
            {
                m_RuleData.NextStep();
            } while (m_RuleData.CurrentStep != m_iCurrentStep);
        }

        if (m_RuleData.CurrentStep > m_iCurrentStep) //规则的生命周期大于模型的生命周期，则使两者生命周期相等
            m_iCurrentStep = m_RuleData.CurrentStep;
    }

    private void ValidateFinalList()
    {
        if (m_RuleData == null || m_RuleData.FinalList == null || m_RuleData.FinalList.Count == 0)
            throw new ArgumentNullException("No Final List.");
    }

    public void Initialize()
    {
        m_strName = "";
        m_iCurrentStep = 0;
        m_iMaxStep = 0;
        m_RuleData = new LSTree();
        m_Texture = null;

        AccumulatedTemperature = 0.0;

        InitialMeshGroup();
        InitialActiveModel();

        ClearParentModel();
        ClearChildModels();
    }

    private void InitialMeshGroup()
    {
        if (m_MeshGroup == null)
            m_MeshGroup = new List<Mesh>();
        else
            m_MeshGroup.Clear();
    }

    /// <summary>
    /// 初始化模型
    /// </summary>
    private void InitialActiveModel()
    {
        ClearActiveModel();     //清除模型
        InitialActiveModelWithoutClearing();
    }

    /// <summary>
    /// 初始化树模型
    /// </summary>
    private void InitialTreeModel()
    {
        if (m_TreeModel != null)
            ClearActiveModel();

        InitialTreeModelWithoutClearing();
    }

    /// <summary>
    /// 初始化枝干模型
    /// </summary>
    private void InitialBranchModel()
    {
        ClearBranchModel();

        InitialBranchModelWithoutClearing();
    }

    /// <summary>
    /// 初始化器官模型
    /// </summary>
    private void InitialOrganModel()
    {
        ClearOrganModels();

        InitialOrganModelWithoutClearing();
    }

    /// <summary>
    /// 初始化模型但不清除已经渲染的模型
    /// </summary>
    private void InitialActiveModelWithoutClearing()
    {
        InitialTreeModelWithoutClearing();
        InitialBranchModelWithoutClearing();
        InitialOrganModelWithoutClearing();
    }

    /// <summary>
    /// 初始化树模型但不清理已经渲染的模型
    /// </summary>
    private void InitialTreeModelWithoutClearing()
    {
        m_TreeModel = new GameObject("Tree");
        m_TreeModel.tag = "Tree";
        m_TreeModel.transform.position = Vector3.zero;
    }

    /// <summary>
    /// 初始化枝干模型但不清理已经渲染的模型
    /// </summary>
    private void InitialBranchModelWithoutClearing()
    {
        m_BranchModel = new GameObject();
        m_BranchModel.AddComponent<MeshFilter>();       //添加必要组件

        m_BranchModel.AddComponent<MeshRenderer>();
        m_BranchModel.GetComponent<MeshRenderer>().material.mainTexture = m_Texture;                   //设置纹理
        m_BranchModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Nature/SpeedTree");  //设置着色器

        m_BranchModel.name = "Branch";  //设置名称
        m_BranchModel.tag = "Branch";   //设置标识符

        m_BranchModel.transform.position = Vector3.zero;
        m_BranchModel.transform.rotation = Quaternion.Euler(Vector3.zero);
        m_BranchModel.transform.localScale = Vector3.one;

        if (m_TreeModel == null)    //父模型不存在，则初始化
            InitialTreeModelWithoutClearing();

        m_BranchModel.transform.SetParent(m_TreeModel.transform);

        m_listBranchIndexes = new List<BranchIndex>();
    }

    /// <summary>
    /// 初始化器官模型但不清理已经渲染的模型
    /// </summary>
    private void InitialOrganModelWithoutClearing()
    {
        m_listOrganModels = new List<GameObject>();

        m_listOrganIndexes = new List<OrganIndex>();
    }

    /// <summary>
    /// 清除当前的数据
    /// </summary>
    public void Clear()
    {
        this.m_strName = null;
        this.m_iCurrentStep = 0;
        this.m_iMaxStep = 0;
        this.m_RuleData.Clear();
        this.m_RuleData = null;
        this.m_Texture = null;
        this.m_MeshGroup.Clear();

        this.LAI = 0;

        ClearParentModel();
        ClearChildModels();

        ClearAllWormholeTex();

        ClearActiveModel(); //清除已经渲染出来的模型
    }

    /// <summary>
    /// 清除目前已经存在的模型
    /// </summary>
    public void ClearActiveModel()
    {
        ClearOrganModels();
        ClearBranchModel();
        ClearTreeModel();
    }

    private void ClearBranchModel()
    {
        if (m_BranchModel == null) return;

        /*
         * 由于Mesh非预制体或模型直接读取
         * 无法通过摧毁其所属的GameObject销毁
         * 故读取Mesh，单独摧毁
         * 释放内存
         */
        UnityEngine.Mesh mesh = m_BranchModel.GetComponent<MeshFilter>().mesh;
        if (mesh != null)
            GameObject.DestroyImmediate(mesh);

        /*
         * 与Mesh同理
         */
        Material material = m_BranchModel.GetComponent<MeshRenderer>().material;
        if (material != null)
            GameObject.DestroyImmediate(material);

        GameObject.Destroy(m_BranchModel, 0f);

        m_BranchModel = null;
    }

    private void ClearOrganModels()
    {
        if (m_listOrganModels == null) return;

        foreach (GameObject OrganModel in m_listOrganModels)
        {
            GameObjectOperation.ClearMaterials(OrganModel, 0f);

            GameObjectOperation.ClearMeshes(OrganModel);

            GameObject.Destroy(OrganModel);
        }

        m_listOrganModels.Clear();
    }

    private void ClearTreeModel()
    {
        if (m_TreeModel == null) return;

        GameObject.Destroy(m_TreeModel, 0f);
    }

    private void ClearParentModel()
    {
        if (ParentModel == null) return;

        ParentModel.ChildModels.Remove(this); //清除父模型内的子模型指针
        ParentModel = null;
    }

    private void ClearChildModels()
    {
        if (ChildModels == null) return;

        foreach (TreeModel ChildModel in ChildModels) //清除所有的子模型
        {
            ChildModel.ParentModel = null;
        }

        m_listChildModels.Clear();
        m_listChildModels = null;
    }

    private void ClearAllWormholeTex()
    {
        if (m_listOrganModels == null) return;

        foreach (GameObject OrganModel in m_listOrganModels)
        {
            Texture tex = GameObjectOperation.GetTexture(OrganModel);

            if (tex.name == "Wormhole Texture")
            {
                GameObjectOperation.ClearTexture(OrganModel);
                GameObject.DestroyObject(tex);
            }
        }
    }

#region 静态函数
    public static Vector3 GetBranchTopCenter(GameObject branch, BranchIndex index)
    {
        if (branch == null) return Vector3.zero;

        MeshFilter mf = branch.GetComponent<MeshFilter>();
        if (mf == null) return Vector3.zero;

        UnityEngine.Mesh mesh = mf.mesh;
        if (mesh == null) return Vector3.zero;

        Vector3[] vertices = mesh.vertices;
        if (index.TopVerticesIndex == -1 || vertices.Length < index.BottomVerticesIndex + 10) return Vector3.zero;

        return branch.transform.TransformPoint((vertices[index.TopVerticesIndex] + vertices[index.TopVerticesIndex + 10]) / 2.0f);
    }

    public static Vector3 GetBranchBottomCenter(GameObject branch, BranchIndex index)
    {
        if (branch == null) return Vector3.zero;

        MeshFilter mf = branch.GetComponent<MeshFilter>();
        if (mf == null) return Vector3.zero;

        UnityEngine.Mesh mesh = mf.mesh;
        if (mesh == null) return Vector3.zero;

        Vector3[] vertices = mesh.vertices;
        if (index.TopVerticesIndex == -1 || vertices.Length < index.BottomVerticesIndex + 10) return Vector3.zero;

        return branch.transform.TransformPoint((vertices[index.BottomVerticesIndex] + vertices[index.BottomVerticesIndex + 10]) / 2.0f);
    }
#endregion


}
