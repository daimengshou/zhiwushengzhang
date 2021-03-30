using System;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    private Vector3[] m_Vertices;   //顶点坐标
    private Vector2[] m_UV;         //纹理坐标
    private Vector3 m_Normal;       //法向量

    private Bounds m_Bounds;        //AABB包围盒

    private OrganIndex m_OrganIndex;    //所属的索引

    public Vector3[] Vertices { get { return m_Vertices; } set { m_Vertices = value; } }
    public Vector3 v0 { get { return m_Vertices[0]; } set { m_Vertices[0] = value; } }
    public Vector3 v1 { get { return m_Vertices[1]; } set { m_Vertices[1] = value; } }
    public Vector3 v2 { get { return m_Vertices[2]; } set { m_Vertices[2] = value; } }

    public Vector2[] UV { get { return m_UV; } set { m_UV = value; } }
    public Vector2 uv0 { get { return m_UV[0]; } set { m_UV[0] = value; } }
    public Vector2 uv1 { get { return m_UV[1]; } set { m_UV[1] = value; } }
    public Vector2 uv2 { get { return m_UV[2]; } set { m_UV[2] = value; } }

    public Bounds AABB { get { return m_Bounds; } }

    public Vector3 Center { get { return (v0 + v1 + v2) / 3.0f; } }
    public float MaxHeight { get { return AABB.max.y; } }
    public float MinHeight { get { return AABB.min.y; } }

    public OrganType Type { get { return Index.Type; } }
    public OrganIndex Index { get { return m_OrganIndex; } set { m_OrganIndex = value; } }

    public Vector3 Normal
    {
        get
        {
            if (m_Normal.Equals(Vector3.zero))
                CalculationNormal();

            return m_Normal;
        }
    }

    public Triangle(Vector3[] vertices)
    {
        m_Vertices = vertices;
        m_UV = null;
        m_Normal = Vector3.zero;

        m_OrganIndex = null;

        Vector3 Max = Vector3.Max(Vector3.Max(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);
        Vector3 Min = Vector3.Min(Vector3.Min(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);

        m_Bounds = new Bounds((Max + Min) / 2.0f, Max - Min);
    }

    public Triangle(Vector3[] vertices, Vector2[] uv, OrganIndex _OrganIndex)
    {
        m_Vertices = vertices;
        m_UV = uv;
        m_Normal = Vector3.zero;

        m_OrganIndex = _OrganIndex;

        Vector3 Max = Vector3.Max(Vector3.Max(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);
        Vector3 Min = Vector3.Min(Vector3.Min(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);

        m_Bounds = new Bounds((Max + Min) / 2.0f, Max - Min);
    }

    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv1, Vector2 uv2, Vector2 uv3, OrganIndex _OrganIndex)
    {
        m_Vertices = new Vector3[3] { v0, v1, v2 };
        m_UV = new Vector2[3] { uv1, uv2, uv3 };
        m_Normal = Vector3.zero;

        m_OrganIndex = _OrganIndex;

        Vector3 Max = Vector3.Max(Vector3.Max(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);
        Vector3 Min = Vector3.Min(Vector3.Min(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);

        m_Bounds = new Bounds((Max + Min) / 2.0f, Max - Min);
    }

    public Triangle(Vector3[] vertices, Vector2[] uv, Vector3 normal, OrganIndex _OrganIndex)
    {
        m_Vertices = vertices;
        m_UV = uv;
        m_Normal = normal;

        m_OrganIndex = _OrganIndex;

        Vector3 Max = Vector3.Max(Vector3.Max(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);
        Vector3 Min = Vector3.Min(Vector3.Min(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);

        m_Bounds = new Bounds((Max + Min) / 2.0f, Max - Min);
    }

    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector3 normal, OrganIndex _OrganIndex)
    {
        m_Vertices = new Vector3[3] { v0, v1, v2 };
        m_UV = new Vector2[3] { uv1, uv2, uv3 };
        m_Normal = normal;

        m_OrganIndex = _OrganIndex;

        Vector3 Max = Vector3.Max(Vector3.Max(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);
        Vector3 Min = Vector3.Min(Vector3.Min(m_Vertices[0], m_Vertices[1]), m_Vertices[2]);

        m_Bounds = new Bounds((Max + Min) / 2.0f, Max - Min);
    }

    public void AddVertex(Vector3 vertex, int i)
    {
        if (m_Vertices == null || m_Vertices.Length != 3)
            m_Vertices = new Vector3[3];

        m_Vertices[i] = vertex;
    }

    public void AddUV(Vector2 uv, int i)
    {
        if (m_UV == null || m_UV.Length != 3)
            m_UV = new Vector2[3];

        m_UV[i] = uv;
    }

    private void CalculationNormal()
    {
        Vector3 e1 = v1 - v0;
        Vector3 e2 = v1 - v2;

        m_Normal = Vector3.Cross(e1, e2).normalized * -1.0f;
    }

    /// <summary>
    /// 获取该点的UV坐标
    /// </summary>
    public Vector2 GetUV(Vector3 point)
    {
        /* 三角形内的任意一点 V 均可用 V = (1 - u - v) * v0 + u * v1 + v * v2 表示
         * 假设三角形ABC中有一点P，则 P = (1 - u - v) * A + u * B + v * C
         * u = S(PAC) / S(ABC)
         * v = S(PAB) / S(ABC)
         * 可根据该参数计算出该点的UV坐标
         * UV(P) = (1 - u - v) * UV(A) + u * UV(B） + v * UV(C)
         */
        float area = Area();    //该三角形的面积
        float u = Area(point, v0, v2) / area;
        float v = Area(point, v0, v1) / area;

        return GetUV(u, v);
    }

    public Vector2 GetUV(float u, float v)
    {
        return (1 - u - v) * uv0 + u * uv1 + v * uv2;
    }

    public bool Equals(Triangle _Triangle)
    {
        return !IsEmpty() && !_Triangle.IsEmpty() &&
            v0.Equals(_Triangle.v0) &&
            v1.Equals(_Triangle.v1) &&
            v2.Equals(_Triangle.v2) &&
            Type.Equals(_Triangle.Type);
    }

    public bool IsEmpty()
    {
        return Vertices == null || Vertices.Length == 0;
    }

    public float Area()
    {
        return Area(v0, v1, v2);
    }

    /// <summary>
    /// 以球体的中心为投影光源，将三角形以中心投影的方式投影到球面上
    /// </summary>
    /// <param name="CenterPoint">球体的中心</param>
    /// <param name="Radius">球体的半径</param>
    public Vector3[] CenterProjectToSphere(Vector3 CenterPoint, float Radius)
    {
        Vector3[] ProjectedPoints = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            Ray ProjectRay = new Ray(CenterPoint, Vertices[i] - CenterPoint);

            ProjectedPoints[i] = ProjectRay.GetPoint(Radius);
        }

        return ProjectedPoints;
    }

    public static float Area(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 e0 = v1 - v0;
        Vector3 e1 = v2 - v0;

        return Vector3.Magnitude(Vector3.Cross(e0, e1)) / 2.0f;
    }

    /// <summary>
    /// 判断一个点是否落在三角形内（同一平面下）
    /// </summary>
    public static bool ContainNotInculdeBoundary(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 Point)
    {
        //重心法
        //参考代码： https://www.cnblogs.com/graphics/archive/2010/08/05/1793393.html
        Vector3 e0 = v2 - v0;
        Vector3 e1 = v1 - v0;
        Vector3 e2 = Point - v0;

        float dot00 = Vector3.Dot(e0, e0);
        float dot01 = Vector3.Dot(e0, e1);
        float dot02 = Vector3.Dot(e0, e2);
        float dot11 = Vector3.Dot(e1, e1);
        float dot12 = Vector3.Dot(e1, e2);

        float inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

        float u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
        if (u < 0 || u >= 1) // if u out of range, return directly
        {
            return false;
        }

        float v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
        if (v < 0 || v >= 1) // if v out of range, return directly
        {
            return false;
        }

        return u + v < 1;
    }

    public override string ToString()
    {
        return v0.ToString() + ", " + v1.ToString() + ", " + v2.ToString();
    }

    public string ToString(string format)
    {
        return v0.ToString(format) + ", " + v1.ToString(format) + ", " + v2.ToString(format);
    }
}

public struct Triangle_GPU
{
    public Vector3[] Vertices { get; set; }
    public Vector2[] UV { get; set; }

    public Triangle_GPU(Triangle tri) : this()
    {
        Vertices = tri.Vertices;
        UV = tri.UV;
    }
}