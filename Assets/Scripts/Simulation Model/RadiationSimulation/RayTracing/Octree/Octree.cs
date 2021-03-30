using UnityEngine;
using System;
using System.Collections.Generic;

public class OctreeNode
{
#region 变量
    private Bounds              m_Bounds;       //AABB包围盒
    private int                 m_Depth;        //当前深度
    private List<Triangle>      m_Triangles;    //包含的三角面片的信息
    private string              m_Index;        //索引（八进制数）
    private OctreeNode          m_Parent;       //父节点
    private OctreeNode[]        m_Children;     //其子节点
#endregion

    public Bounds Bounds
    {
        get { return m_Bounds; }
        set { m_Bounds = value; }
    }

    public int Depth
    {
        get { return m_Depth; }
        set { m_Depth = value; }
    }

    public List<Triangle> Triangles
    {
        get { return m_Triangles; }
        set { m_Triangles = value; }
    }

    public string Index
    {
        get { return m_Index; }
        set { m_Index = value; }
    }

    public OctreeNode Parent
    {
        get { return m_Parent; }
        set { m_Parent = value; }
    }

    public OctreeNode[] Children
    {
        get { return m_Children; }
        set { m_Children = value; }
    }

    public OctreeNode()
    {

    }

    public OctreeNode(Vector3 center, Vector3 size, int depth, List<Triangle> TriangleList, string Index, OctreeNode Parent)
    {
        m_Bounds = new Bounds(center, size);
        m_Depth = depth;
        m_Triangles = TriangleList;
        m_Index = Index;
        m_Parent = Parent;
        m_Children = null;
    }

    public void AddTriangle(Triangle _Triangle)
    {
        if (Triangles == null) Triangles = new List<Triangle>();

        Triangles.Add(_Triangle);
    }

    public void AddTriangles(IEnumerable<Triangle> _Triangles)
    {
        if (Triangles == null) Triangles = new List<Triangle>();

        Triangles.AddRange(_Triangles);
    }

    public bool IsLeafNode()
    {
        return Children == null;
    }

    public bool IsBranchNode()
    {
        return Children != null;
    }

    public bool HaveTriangles()
    {
        return Triangles != null && Triangles.Count > 0;
    }

    public void Clear()
    {
        if (IsLeafNode())
        {
            InternalClear();
        }
        else
        {
            for (int i = 0; i < Children.Length; i++)
                Children[i].Clear();

            InternalClear();
        }
    }

    private void InternalClear()
    {
        Triangles.Clear();
        Children = null;
        Parent = null;
    }

    public override string ToString()
    {
        return "Index: " + Index + " Center: " + Bounds.center + " Size: " + Bounds.size;
    }
}

public class Octree
{
#region 变量
    private OctreeNode  m_Root;         //根节点
    private int         m_MaxDepth;     //最大深度
#endregion

    public OctreeNode Root
    {
        get { return m_Root; }
        set { m_Root = value; }
    }

    public int MaxDepth
    {
        get { return m_MaxDepth; }
        set { m_MaxDepth = value; }
    }

    public Octree(Vector3 Center, Vector3 Size, List<Triangle> TriangleList, int MaxDepth = 10)
    {
        this.Root = new OctreeNode(Center, Size, 0, new List<Triangle>(), "", null);
        this.MaxDepth = MaxDepth;

        SplitBounds(m_Root, TriangleList, 0);
    }

    private void SplitBounds(OctreeNode Node, List<Triangle> TriangleList, int Depth)
    {
        if (Depth >= MaxDepth) return;  //已经超过最大的深度

        if (Depth == MaxDepth - 1)  //已经达到最大深度
        {
            AddContainedTriangles(Node, TriangleList);  //准确添加三角面片
            return;
        }
        else                        //未达到最大深度
        {
            AddContainedTrianglesRoughly(Node, TriangleList);   //粗略添加三角面片
            
            if (!Node.HaveTriangles()) return;  //该节点不包含三角面片，则不继续分割

            //有包含三角面片，对该节点进行分割，并遍历分割后的节点
            AddChildNode(Node);

            foreach (OctreeNode ChildNode in Node.Children)
            {
                SplitBounds(ChildNode, Node.Triangles, Depth + 1);
            }
        }
    }

    /// <summary>
    /// 添加该节点包含的物体
    /// </summary>
    private void AddContainedTrianglesRoughly(OctreeNode Node, List<Triangle> TriangleList)
    {
        foreach (Triangle Tri in TriangleList)
        {
            if (Node.Bounds.Intersects(Tri.AABB))   //三角面片的包围盒与节点的包围盒相交
                Node.AddTriangle(Tri);
        }
    }

    /// <summary>
    /// 添加该节点包含的三角面片
    /// </summary>
    private void AddContainedTriangles(OctreeNode Node, List<Triangle> TriangleList)
    {
        foreach (Triangle Tri in TriangleList)
        {
            if (Intersect.IntersectTriangleAABB(Tri.v0, Tri.v1, Tri.v2, Node.Bounds))
            {
                Node.AddTriangle(Tri);
            }
        }
    }

    /// <summary>
    /// 添加子节点
    /// </summary>
    private void AddChildNode(OctreeNode Node)
    {
        Node.Children = new OctreeNode[8];
        
        //i  x  y  z        i  x  y  z
        //0  +  +  +        4  +  -  +
        //1  +  +  -        5  +  -  -
        //2  -  +  +        6  -  -  +
        //3  -  +  -        7  -  -  -
        for (int i = 0; i < 8; i++ )
        {
            float detlaX = (i / 2) % 2 == 0 ? Node.Bounds.extents.x / 2.0f : -Node.Bounds.extents.x / 2.0f;
            float detlaY = i / 4 == 0 ? Node.Bounds.extents.y / 2.0f : -Node.Bounds.extents.y / 2.0f;
            float detlaZ = i % 2 == 0 ? Node.Bounds.extents.z / 2.0f : -Node.Bounds.extents.z / 2.0f;

            Node.Children[i] = new OctreeNode();
            Node.Children[i].Bounds = new Bounds(Node.Bounds.center + new Vector3(detlaX, detlaY, detlaZ), Node.Bounds.extents);
            Node.Children[i].Depth = Node.Depth + 1;
            Node.Children[i].Triangles= new List<Triangle>();
            Node.Children[i].Index = Node.Index + i;
            Node.Children[i].Parent = Node;
            Node.Children[i].Children = null;

            VertexCorrection(Node, Node.Children[i]);
        }
    }

    private void VertexCorrection(OctreeNode Parent, OctreeNode Child)
    {
        Vector3 ParentMin = Parent.Bounds.min;
        Vector3 ParentMax = Parent.Bounds.max;
        Vector3 ChildMin = Child.Bounds.min;
        Vector3 ChildMax = Child.Bounds.max;
        Vector3 Direction = Child.Bounds.center - Parent.Bounds.center;

        float XError = Direction.x > 0 ? ParentMax.x - ChildMax.x : ParentMin.x - ChildMin.x;
        float YError = Direction.y > 0 ? ParentMax.y - ChildMax.y : ParentMin.y - ChildMin.y;
        float ZError = Direction.z > 0 ? ParentMax.z - ChildMax.z : ParentMin.z - ChildMin.z;

        Child.Bounds = new Bounds(Child.Bounds.center, Child.Bounds.size + (new Vector3(XError, YError, ZError) * 2));
    }

    public void Clear()
    {
        Root.Clear();

        Root = null;
        MaxDepth = -1;
    }

    public static void LocalToWorld(GameObject _Object, Vector3[] _Vertices)
    {
        Matrix4x4 Matrix = _Object.transform.localToWorldMatrix;

        for (int i = 0; i < _Vertices.Length; i++ )
        {
            //_Vertices[i] = Matrix.MultiplyPoint3x4(_Vertices[i]);
            _Vertices[i] = _Object.transform.TransformPoint(_Vertices[i]);
        }
    }

    /// <summary>
    /// 构建八叉树
    /// </summary>
    public static Octree Build(TreeModel treeModel)
    {
        GameObject root = treeModel.TreeModelInstance;     //获取根节点表示的模型

        if (root == null) return null;

        /*
         * 添加包含整个植物的包围盒
         * 八叉树则以该包围盒为根节点进行构建
         */
        Mesh.AddBoxColliderInParent(root);
        Bounds rootBounds = root.GetComponent<Collider>().bounds;

        if (IsEmpty(rootBounds)) return null;

        List<Triangle> triangles = GameObjectOperation.GetTreeTriangles(treeModel);
        if (triangles == null || triangles.Count == 0) return null;

        return new Octree(rootBounds.center, rootBounds.size, GameObjectOperation.GetTreeTriangles(treeModel), 5);
    }

    /// <summary>
    /// 判断该包围盒是否为空
    /// </summary>
    private static bool IsEmpty(Bounds bounds)
    {
        Vector3 size = bounds.size;
        return Mathf.Approximately(size.x, 0) && Mathf.Approximately(size.y, 0) && Mathf.Approximately(size.z, 0);
    }
}