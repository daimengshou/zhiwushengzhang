/*
 * 文件名：Mesh.cs
 * 描述：器官模型存储系统
 */
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 该类用于存储Mesh的名字和其值（ID）的对应关系，每一个Mesh的名字都对应一个唯一的值（ID）。
/// 该类主要用于后续解析L系统规则。
/// 
/// @version: 1.0
/// </summary>
public class MeshResource
{
    private static MeshResource instance = null;
    private List<string> m_listName;
    private List<int>    m_listValue;

    private MeshResource()
    {
        m_listName = new List<string>();
        m_listValue = new List<int>();
    }

    public static MeshResource GetInstance()
    {
        if (instance == null)
            instance = new MeshResource();

        return instance;
    }

    public static void Destroy()
    {
        instance = null;
    }

    public void Add(string name)
    {
        if (Find(name) == -1)  //没有找到该名字，则添加
        {
            m_listName.Add(name);
            if (m_listValue.Count == 0)
                m_listValue.Add(30000);
            else
                m_listValue.Add(m_listValue[m_listValue.Count - 1] + 1);
        }
    }

    public int Find(string name)
    {
        return m_listName.IndexOf(name);
    }

    public int Find(int value)
    {
        return m_listValue.IndexOf(value);
    }

    public int GetValueOf(string name)
    {
        int index = Find(name);
        if (index != -1)
            return m_listValue[index];
        else
            return -1;
    }

    public string GetNameOf(int value)
    {
        int index = Find(value);
        if (index != -1)
            return m_listName[index];
        else
            return null;
    }

}

/*
 * 存储器官模型的类
 * 存储由其他建模软件（3DMax等）生成的器官模型
 * 
 * @version: 1.0
 */
public class Mesh {
    private readonly string  m_strName;          //Mesh名字
    private readonly int     m_iNameValue;       //Mesh名字对应的值
    private readonly string  m_strMeshPath;      //Mesh的存放路径
    private readonly string  m_strTexturePath;   //Mesh纹理的存放路径
    private float   m_fSize;            //Mesh大小
    private float   m_fMaxSize;         //Mesh最大的大小
    private OrganType m_Type;           //Mesh的器官类型

    private double m_LeafLength;    //叶片长度（仅器官为叶片时使用）   2019.06.25添加
    private double m_LeafWidth;     //叶片宽度（仅器官为叶片时使用）   2019.06.25添加
    private double m_MeshArea;      //该模型的面积
    private double m_LeafArea;      //叶片面积

    private int    m_VisualPixelCount;  //可视像素（非透明像素）个数
    private int    m_PixelCount;        //总像素个数
    private float m_VisibilityRatio;   //可视比（可视像素个数百分比）

    private TreeModel   m_ModelEntry;                   //该Mesh对应的整体模型
    private GameObject  m_MeshModelInstance = null;     //该模型的实例

    public Mesh(string name, string MeshPath, string TexturePath, float size, float MaxSize, OrganType Type, TreeModel modelEntry)
    {
        m_strName = name;
        m_strMeshPath = MeshPath;
        m_strTexturePath = TexturePath;
        m_fSize = size;
        m_fMaxSize = MaxSize;
        m_ModelEntry = modelEntry;
        m_Type = Type;

        MeshResource.GetInstance().Add(m_strName);
        m_iNameValue = MeshResource.GetInstance().GetValueOf(m_strName);
    }

    /// <summary>
    /// 该构造函数仅用于记录数据
    /// </summary>
    public Mesh(string name, string meshPath, string texPath, float size, float maxSize, OrganType type)
    {
        m_strName = name;
        m_strMeshPath = meshPath;
        m_strTexturePath = texPath;
        m_fSize = size;
        m_fMaxSize = maxSize;
        m_Type = type;
    }

    public string Name
    {
        get { return m_strName; }
    }

    public int NameValue
    {
        get { return m_iNameValue; }
    }

    public string MeshPath
    {
        get { return m_strMeshPath; }
    }

    public string TexturePath
    {
        get { return m_strTexturePath; }
    }

    public float Size
    {
        get { return m_fSize; }
        set { m_fSize = value; }
    }

    public float MaxSize
    {
        get { return m_fMaxSize; }
        set { m_fMaxSize = value; }
    }

    public OrganType Type
    {
        get { return m_Type; }
        set { m_Type = value; }
    }

    public TreeModel Belong
    {
        get { return m_ModelEntry; }
        set { m_ModelEntry = value; }
    }

    public double LeafLength
    {
        get { return m_LeafLength; }
        set { m_LeafLength = value; }
    }

    public double LeafWidth
    {
        get { return m_LeafWidth; }
        set { m_LeafWidth = value; }
    }

    public double UniformMeshArea
    {
        get { return m_MeshArea; }
        set { m_MeshArea = value; }
    }

    public double UniformLeafArea
    {
        get { return m_LeafArea; }
        set { m_LeafArea = value; }
    }

    public int VisualPixelCount
    {
        get { return m_VisualPixelCount; }
        set { m_VisualPixelCount = value; }
    }

    public int PixelCount
    {
        get { return m_PixelCount; }
        set { m_PixelCount = value; }
    }

    public float VisibilityRatio
    {
        get { return m_VisibilityRatio; }
        set { m_VisibilityRatio = value; }
    }

    /// <summary>
    /// 返回实例，用于渲染
    /// </summary>
    public GameObject Instance
    {
        get
        {
            if (m_MeshModelInstance == null)
            {
                string meshPath = m_strMeshPath.Remove(m_strMeshPath.LastIndexOf('.'));           //删除格式
                m_MeshModelInstance = (Resources.Load(meshPath, typeof(GameObject)) as GameObject).transform.gameObject;       //从Resources文件中以GameObject格式读取
                m_MeshModelInstance.transform.localScale = Vector3.one;

                if (m_MeshModelInstance == null)
                    throw new UnityException("Error Mesh Path");

                /*
                 * 仅当Mesh为叶片时读取或写入
                 * 减少重复计算
                 */
                if (Type == OrganType.Leaf)
                    ReadLeafLengthAndWidth(); //06-26
            }

            return m_MeshModelInstance;
        }
    }

    /// <summary>
    /// 均一化物体的尺寸
    /// </summary>
    /// <param name="_Object">需要均一化尺寸的物体</param>
    private void ObjectNormalize(GameObject _Object)
    {
        Vector3 size = GetBoundsInParent(_Object).size;

        float[] tempArray = {size.x , size.y, size.z};
        float scale = 1.0f / UnityEngine.Mathf.Max(tempArray);  //根据最大的尺寸确定缩放比例，使得缩放后的物体的包围盒的最大尺寸为1

        _Object.transform.localScale = new Vector3(scale, scale, scale);    //调整大小
    }

    private Vector3 GetSizeInParent(GameObject parentObject)
    {
        return Vector3.zero;
    }

    /// <summary>
    /// 为父对象添加一个能包围其自身以及所有子对象的盒碰撞体
    /// </summary>
    /// <param name="parentObject">父对象</param>
    public static BoxCollider AddBoxColliderInParent(GameObject parentObject)
    {
        GameObject.DestroyImmediate(parentObject.GetComponent<Collider>());//清除原有的碰撞体

        Bounds bounds = GetBoundsInParent(parentObject);    //获取包围盒

        BoxCollider boxCollider = parentObject.AddComponent<BoxCollider>(); //添加盒碰撞体
        boxCollider.center = bounds.center;     //设置碰撞体的中心
        boxCollider.size = bounds.size;         //设置碰撞体的尺寸

        return boxCollider;
    }

    /// <summary>
    /// 获取包围其自身以及其所有子对象的包围盒
    /// </summary>
    /// <param name="parentObject">父对象</param>
    /// <returns></returns>
    public static Bounds GetBoundsInParent(GameObject parentObject)
    {
        //参考代码：https://www.xuanyusong.com/archives/3461
        Transform parent = parentObject.transform;
        Vector3 position = parent.position;
        Quaternion rotation = parent.rotation;
        Vector3 scale = parent.localScale;

        parent.position = Vector3.zero;
        parent.rotation = Quaternion.Euler(Vector3.zero);
        parent.localScale = Vector3.one;

        Vector3 center = Vector3.zero;
        Renderer[] renders = parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer child in renders)
            center += child.bounds.center;

        center /= renders.Length;

        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer child in renders)
        {
            bounds.Encapsulate(child.bounds);
        }

        bounds.center = bounds.center - parent.position;
        bounds.size *= 1.000001f;   //最小程度的扩大，确保边缘能被包括

        parent.position = position;
        parent.rotation = rotation;
        parent.localScale = scale;

        return bounds;
    }

    private void ReadLeafLengthAndWidth()
    {
        //string filePath = System.Environment.CurrentDirectory + "\\Assets\\Resources\\" + m_strMeshPath.Remove(m_strMeshPath.LastIndexOf('.')) + ".tmp";
        string filePath = System.Environment.CurrentDirectory + "\\Data\\" + Path.GetFileNameWithoutExtension(m_strMeshPath) + ".data";

        /*
         * 当没有该类文件时
         * 自动生成，后续重复的计算
         */
        if (!System.IO.File.Exists(filePath))
        {
            WriteLeafLengthAndWidth();
            return;
        }

        FileStream stream = new FileStream(filePath, FileMode.Open);
        StreamReader reader = new StreamReader(stream);

        LeafLength = Convert.ToDouble(reader.ReadLine().Split(' ')[1]);
        LeafWidth = Convert.ToDouble(reader.ReadLine().Split(' ')[1]);
        UniformMeshArea = Convert.ToDouble(reader.ReadLine().Split(' ')[1]);
        UniformLeafArea = Convert.ToDouble(reader.ReadLine().Split(' ')[1]);

        VisualPixelCount = Convert.ToInt32(reader.ReadLine().Split(' ')[1]);
        PixelCount = Convert.ToInt32(reader.ReadLine().Split(' ')[1]);
        VisibilityRatio = Convert.ToSingle(reader.ReadLine().Split(' ')[1]);

        //读取完毕，内存释放
        reader.Close();
        stream.Close();
    }

    /// <summary>
    /// 根据当前模型的数据写叶片长度和最大宽度
    /// 后续则根据实际需要读取该文件，减少重复计算
    /// </summary>
    private void WriteLeafLengthAndWidth()
    {
        if (m_MeshModelInstance == null)
            throw new UnityException("Error Mesh Path");

        //实例化，否则无法获取顶点和UV坐标
        GameObject meshModel = (GameObject)GameObject.Instantiate(m_MeshModelInstance);

        /*
         * 获取顶点坐标和UV坐标
         * 用于后续计算长度和宽度
         */
        Vector3[] vertices = GameObjectOperation.GetVertices(meshModel.transform.GetChild(0).gameObject);
        Vector2[] uv = GameObjectOperation.GetUV(meshModel.transform.GetChild(0).gameObject);

        /*
         * 获取未放大和缩小的模型中
         * 叶片的长度、宽度和叶片面积
         * 用于后续与实际叶片长度和宽度或叶片面积做比较，确定模型的缩放比例
         */
        LeafLength = GetLeafLength(vertices, uv); //m
        LeafWidth = GetLeafWidth(vertices, uv); //m
        UniformLeafArea = ComputeLeafArea(meshModel);   //㎡

        /*
         * 清除GameObject
         * 防止模型显示
         */
        GameObject.Destroy(meshModel);

        /*
         * 记录叶片长度和宽度
         * 用于写入文件中
         */
        string str = "";
        str += "LeafLength " + LeafLength + " m\n";
        str += "LeafWidth " + LeafWidth + " m\n";
        str += "MeshArea " + UniformMeshArea + " m^2\n";
        str += "LeafArea " + UniformLeafArea + " m^2\n";

        str += "VisualPixelCount " + VisualPixelCount + "\n";
        str += "PixelCount " + PixelCount + "\n";
        str += "VisibilityRatio " + VisibilityRatio + "\n";

        /*
         * 将数据写入文件
         * 方便后续读取
         */
        //string filePath = System.Environment.CurrentDirectory + "\\Assets\\Resources\\" + m_strMeshPath.Remove(m_strMeshPath.LastIndexOf('.')) + ".tmp";
        string directoryPath = System.Environment.CurrentDirectory + "\\Data";
        if (!System.IO.Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        string filePath = directoryPath + "\\" + Path.GetFileNameWithoutExtension(m_strMeshPath) + ".data";
        FileStream stream = new FileStream(filePath, FileMode.Create);
        StreamWriter writer = new StreamWriter(stream);

        writer.WriteLine(str);  //写入

        //完成写入，内存释放
        writer.Close();
        stream.Close();
    }

    //用于判断获取的叶片数据类型（长度或宽度）
    private enum LeafInfoType
    {
        Length, Width
    }

    private double GetLeafLength(Vector3[] vertices, Vector2[] uv)
    {
        return GetLeafInfo(vertices, uv, LeafInfoType.Length);
    }

    private double GetLeafWidth(Vector3[] vertices, Vector2[] uv)
    {
        return GetLeafInfo(vertices, uv, LeafInfoType.Width);
    }

    private double GetLeafInfo(Vector3[] vertices, Vector2[] uv, LeafInfoType type)
    {
        List<Vector3> edgeVertices = new List<Vector3>();
        List<Vector2> edgeUV = new List<Vector2>();

        for (int i = 0; i < vertices.Length; i++)
        {
            /*
             * 仅在
             * 获取数据的类型为长度且UV坐标的u值为0时，或
             * 获取数据的类型为宽度且UV坐标的v值为0时
             * 记录该坐标点和对应的UV坐标。
             * 其中，
             * 顶点用于后续计算长度或宽度（根据获取数据的类型进行判断）
             * UV坐标用于后续对顶点排序
             */
            if ((type == LeafInfoType.Length && uv[i].x == 0) ||
                (type == LeafInfoType.Width && uv[i].y == 0))
            {
                edgeVertices.Add(vertices[i]);
                edgeUV.Add(uv[i]);
            }
        }

        /*
         * 根据UV坐标对顶点进行排序：
         * 当获取数据类型为长度时，则根据V值从小到大对顶点进行排序
         * 当获取数据类型为宽度时，则根据U值从小到大对顶点进行排序
         * 保证后续计算长度或宽度正确
         */
        if (type == LeafInfoType.Length)
            SortVerticesByVofUV(ref edgeVertices, ref edgeUV);
        else
            SortVerticesByUofUV(ref edgeVertices, ref edgeUV);

        double result = 0;
        for (int i = 1; i < edgeVertices.Count; i++)
        {
            result += Vector3.Distance(edgeVertices[i - 1], edgeVertices[i]);   //计算相邻两点之间的距离，并累加
        }

        return result * MaizeParams.SCALE;
    }

    /// <summary>
    /// 根据uv坐标中的v值对
    /// </summary>
    /// <param name="vertice"></param>
    /// <param name="uv"></param>
    private void SortVerticesByVofUV(ref List<Vector3> vertice, ref List<Vector2> uv)
    {
        /*
         * 坐标根据uv坐标中的v值从小到大进行排序
         * 排序后的坐标用于后续长度的计算
         */
        for (int i = 0; i < uv.Count - 1; i++)
        {
            for (int j = i + 1; j < uv.Count; j++)
            {
                if (uv[i].y > uv[j].y)
                    Swap(ref vertice, ref uv, i, j);
            }
        }
    }

    private void SortVerticesByUofUV(ref List<Vector3> vertice, ref List<Vector2> uv)
    {
        /*
         * 坐标根据uv坐标的u值从小到大进行排序
         * 排序后的坐标用于后续宽度的计算
         */
        for (int i = 0; i < uv.Count - 1; i++)
        {
            for (int j = i + 1; j < uv.Count; j++)
            {
                if (uv[i].x < uv[j].x)
                    Swap(ref vertice, ref uv, i, j);
            }
        }
    }

    private void Swap(ref List<Vector3> vertice, ref List<Vector2> uv, int i, int j)
    {
        /*
         * 交换顶点
         */
        Vector3 tempVertex = vertice[i];
        vertice[i] = vertice[j];
        vertice[j] = tempVertex;

        /*
         * 交换UV坐标
         */
        Vector2 tempUV = uv[i];
        uv[i] = uv[j];
        uv[j] = tempUV;
    }

    /// <summary>
    /// 计算叶片面积（Unity单位）
    /// </summary>
    private double ComputeLeafArea(GameObject meshModel)
    {
        Texture2D texture = GameObjectOperation.GetTexture(meshModel.transform.GetChild(0).gameObject) as Texture2D;

        VisualPixelCount = 0;
        PixelCount = texture.width * texture.height;   //总像素个数

        //统计非透明像素个数
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                if (texture.GetPixel(x, y).a > 0)
                    VisualPixelCount++;
            }
        }

        VisibilityRatio = VisualPixelCount * 1.0f / PixelCount;

        UniformMeshArea = GameObjectOperation.GetOrganArea(meshModel.transform.GetChild(0).gameObject) * 
            MaizeParams.SCALE * MaizeParams.SCALE; //㎡

        //获取该模型的面积，并乘以非透明像素所占百分比即得叶片面积
        return UniformMeshArea * VisibilityRatio;
    }
}
