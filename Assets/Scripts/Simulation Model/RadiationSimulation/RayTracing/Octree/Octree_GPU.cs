using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeNode_GPU
{
    private int[] m_Children = new int[8];
    private bool m_IsPointChildren = true;

    private Vector3 m_Center;
    private Vector3 m_Size;

    public int[] Children { get { return m_Children; } set { m_Children = value; } }
    public bool IsPointChildren { get { return m_IsPointChildren; } set { m_IsPointChildren = value; } }
    public Vector3 Center { get { return m_Center; } set { m_Center = value; } }
    public Vector3 Size { get { return m_Size; } set { m_Size = value; } }

    public OctreeNode_GPU(OctreeNode node)
    {
        m_Center = node.Bounds.center;
        m_Size = node.Bounds.size;

        m_IsPointChildren = node.IsBranchNode();
    }
}

public class Octree_GPU
{
    /*
     * 一维节点数组
     * 用于记录所有八叉树节点
     * 根据每个节点中记录的子节点位置即为该数组中的索引位置
     * 根据记录的索引，以及在该数组中获取该子节点
     */
    public List<OctreeNode_GPU> Nodes { get; set; }

    /*
     * 三角面片索引数组
     * 记录每个叶子节点包含的所有三角面片在三角面片数组中的索引位置
     * 当值为-1时，即表示该叶子节点记录的位置结束
     */
    public List<int> TriangleIndexes { get; set; }

    /*
     * 三角面片数组（数据数组）
     * 详细记录该八叉树包含的所有三角面片
     * 根据三角面片索引数组中记录的索引数据
     * 即可在该数组中获取对应的三角面片
     */
    public List<Triangle_GPU> Triangles{ get; set; }

    public Octree_GPU(Octree octree)
    {
        Triangle2Triangle_GPU(octree.Root.Triangles);
        Octree2Octree_GPU(octree);
    }

    /// <summary>
    /// 将Triangle转换成Triangle_GPU
    /// </summary>
    /// <param name="triangles"></param>
    private void Triangle2Triangle_GPU(List<Triangle> triangles)
    {
        Triangles = new List<Triangle_GPU>();

        foreach(Triangle tri in triangles)
        {
            Triangles.Add(new Triangle_GPU(tri));
        }
    }

    private void Octree2Octree_GPU(Octree octree)
    {
        /*
         * 对八叉树进行广度优先遍历
         * 将其降维成一位数组
         */
        Queue<OctreeNode> queueCPU = new Queue<OctreeNode>();
        Queue<OctreeNode_GPU> queueGPU = new Queue<OctreeNode_GPU>();

        queueCPU.Enqueue(octree.Root);
        queueGPU.Enqueue(OctreeNode2OctreeNode_GPU(octree.Root));

        while (queueGPU.Count != 0)
        {
            OctreeNode nodeCPU = queueCPU.Peek();
            OctreeNode_GPU nodeGPU = queueGPU.Peek();

            /*
             * 该节点为叶子节点（无孩子）
             * 继续该节点数据存储的位置（在一维数组中）
             */
            if (nodeCPU.IsLeafNode())
            {
                nodeGPU.Children[0] = TriangleIndexes.Count;

                AddTriangleIndexes(nodeCPU.Triangles);
                continue;
            }

            /*
             * 获取nodeCPU的子节点
             * 并将各个子节点转换成GPU
             * 并入队列
             */
            for(int i = 0; i < nodeCPU.Children.Length; i++)
            {
                OctreeNode childCPU = nodeCPU.Children[i];
                OctreeNode_GPU childGPU = OctreeNode2OctreeNode_GPU(childCPU);

                queueCPU.Enqueue(childCPU);
                queueGPU.Enqueue(childGPU);
                
                Nodes.Add(childGPU);
                
                nodeGPU.Children[i] = Nodes.Count;
            }
        }
    }

    private OctreeNode_GPU OctreeNode2OctreeNode_GPU(OctreeNode node)
    {
        return new OctreeNode_GPU(node);
    }

    private void AddTriangleIndexes(List<Triangle> triangles)
    {
        foreach(Triangle triangle in triangles)
        {
            TriangleIndexes.Add(Triangles.FindIndex(tri =>  
                tri.Vertices[0].Equals(triangle.Vertices[0]) &&
                tri.Vertices[1].Equals(triangle.Vertices[1]) &&
                tri.Vertices[2].Equals(triangle.Vertices[2])));
        }

        //终止符
        TriangleIndexes.Add(-1);
    }
}
