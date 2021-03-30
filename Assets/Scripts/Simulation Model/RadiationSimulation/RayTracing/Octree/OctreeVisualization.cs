//using System;
//using System.Diagnostics;
//using System.Drawing.Drawing2D;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.IO;

//public class RayTracingVisualization : MonoBehaviour {

//    public int index = 0;

//    private Octree m_Octree;
//    private List<Ray> m_Rays                = new List<Ray> { new Ray()};
//    private List<OctreeNode[]> m_HitNodes   = new List<OctreeNode[]>();
//    private List<Triangle> m_HitTriangles   = new List<Triangle>();
//    private List<float> m_HitDistances      = new List<float>() { 100f };

//    //直射光线
//    private DateTime m_Time                 = DateTime.Now;
//    private double m_Longitude              = 119;
//    private double m_Latitude               = 26;
//    private Light[]  m_Light                  = null;
    
//    //散射光线
//    private LightCastHemisphere m_LightHemisphere = null;
//    private int m_IndexOfTriangle = 0;
//    private Vector3 m_SphereCenter = Vector3.zero;
//    private Triangle m_Triangle = new Triangle();

//    public Octree Octree                { get { return m_Octree; } set { m_Octree = value; } }
//    public List<Ray> Rays               { get { return m_Rays; } }
//    public List<OctreeNode[]> HitNodes  { get { return m_HitNodes; } }
//    public List<Triangle> HitTriangles  { get { return m_HitTriangles; } }
//    public List<float> HitDistances { get { return m_HitDistances; } }

//    //直射光线
//    public DateTime Time { get { return m_Time; } set { m_Time = value; } }
//    public double Longitude { get { return m_Longitude; } set { m_Longitude = value; } }
//    public double Latitude { get { return m_Latitude; } set { m_Latitude = value; } }
//    public Light[] Lights { get { return m_Light; } set { m_Light = value; } }

//    //散射光线
//    public LightCastHemisphere LightHemisphere { get { return m_LightHemisphere; } set { m_LightHemisphere = value; } }
//    public Vector3 SphereCenter { get { return m_SphereCenter; } set { m_SphereCenter = value; } }
//    public int IndexOfTriangle { get { return m_IndexOfTriangle; } set { m_IndexOfTriangle = value; } }

//    public void Click()
//    {

//    }

//    public bool BuildOctree()
//    {
//        GameObject root = GameObject.FindGameObjectWithTag("Tree");

//        if (root == null) return false;
       
//        Mesh.AddBoxColliderInParent(root);
//        Bounds RootBounds = root.GetComponent<Collider>().bounds;

//        Octree = new Octree(RootBounds.center, RootBounds.size, GameObjectOperation.GetTreeTriangles(), 5);

//        return true;
//    }

//    public void RayCast(Ray _Ray)
//    {
//        if (Octree == null && BuildOctree() == false)
//            return;

//        Rays.Clear();
//        HitNodes.Clear();
//        HitTriangles.Clear();
//        HitDistances.Clear();


//        Ray CurrentRay = _Ray;
//        m_Rays.Add(CurrentRay);
//        Triangle ExcludedTriangle = new Triangle();
//        for (int i = 0; i < 2; i++ )
//        {
//            //不与三角面片碰撞
//            OctreeNode[] CollisionNodes;
//            if (RayTracing.CollisionDection(CurrentRay, m_Octree, out CollisionNodes))
//                m_HitNodes.Add(CollisionNodes);
//            else
//                break;  //未碰撞到，则不继续
            

//            //与三角面片碰撞
//            float HitDistance;
//            Triangle HitTriangle;
//            if (RayTracing.CollisionDection(CurrentRay, Octree, ExcludedTriangle, out HitDistance, out HitTriangle))
//            {
//                m_HitDistances.Add(HitDistance);
//                m_HitTriangles.Add(HitTriangle);

//                ExcludedTriangle = HitTriangle;
//            }
//            else
//                break;  //未碰撞到三角面片，则不继续反射，终止循环

//            CurrentRay = RayTracing.RayReflection(CurrentRay, HitDistance, HitTriangle);
//            m_Rays.Add(CurrentRay);
//        }
//    }

//    public void SetSun(DateTime _Time, double _Longitude, double _Laitude, float d)
//    {
//        Time = _Time;
//        Longitude = _Longitude;
//        Latitude = _Laitude;
        
//        Lights = SolarSim.DirectLights(_Time.Year, _Time.Month, _Time.Day, _Time.Hour, _Time.Minute, _Longitude, _Laitude, Octree, 0.1f);

//        /*
//         * 计算太阳高度角和太阳方位角，确定场景中Directional Light的旋转角度
//         */
//        double SolarAltitude_Deg = SolarSim.SolarAltitude(_Time.Year, _Time.Month, _Time.Day, _Time.Hour, _Time.Minute, _Longitude, _Laitude);   //高度角
//        double SolarAzimuth_Deg  = SolarSim.SolarAzimuth(_Time.Year, _Time.Month, _Time.Day, _Time.Hour, _Time.Minute, _Longitude, _Laitude);    //方位角

//        GameObject.Find("Directional Light").transform.rotation = Quaternion.Euler(new Vector3((float)SolarAltitude_Deg, -(float)SolarAzimuth_Deg, 0));
//    }

//    public void SetLightHemiSphere()
//    {
//        //if (LightHemisphere != null) return;

//        if (Octree == null) BuildOctree();

//        LightHemisphere = new LightCastHemisphere(SphereCenter, Octree.Root.Bounds);

//        LightHemisphere.Split(100, 100);

//        List<Triangle> Triangles = GameObjectOperation.GetTreeTriangles();

//        for (int i = 0; i < Triangles.Count; i++ )
//        {
//            if (IndexOfTriangle != i) continue;

//            //LightHemisphere.GetSkyTransmissivity(100, 100, new Triangle[] { Triangles[i] }, -1);

//            m_Triangle = Triangles[i];
//        }
//    }

//    public void DailySunshineSimulation()
//    {

//#if DEBUG
//        Stopwatch sw = new Stopwatch();
//        sw.Start();
//        SolarSim.DailySunShineSimluation(m_Time, m_Longitude, m_Latitude, m_Octree, 0.001f, 100, 100);
//        sw.Stop();

//        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);  //对运算时间进行测试
//#else
//        SolarParams.DailySunShineSimluation(m_Time, m_Longitude, m_Latitude, m_Octree, 0.1f, 100, 100);
//#endif

//        /*
//         * 获取当前场景中所有的器官索引
//         * 判断每个索引的类型
//         * 如果索引的类型为叶子，则转换成LeafIndex
//         * 输出该叶片的直射光能量
//         */
//        List<OrganIndex> OrganIndexes = Scene.GetInstance().TreeModel.OrganIndexes;

//        foreach (OrganIndex _OrganIndex in OrganIndexes)
//        {
//            if (_OrganIndex.Type != OrganType.Leaf) continue;

//            LeafIndex _LeafIndex = _OrganIndex as LeafIndex;

//            UnityEngine.Debug.Log(_LeafIndex.DirectionEnergy.ToString("f4"));
//        }
//    }

//    public bool isDisplayOctree = false;
//    public bool isDisplaySpecifiedNode = false;
//    public bool isDispalyCollisionNodes = false;
//    public bool isDisplayRay = false;
//    public bool isDisplayHitTriangle = false;

//    public bool isDisplayDirectionLight = false;
//    public bool isDisplayScatterLight = false;

//    public ComputeShader shader;

//#if UNITY_EDITOR
//    private void OnDrawGizmos()
//    {
//        if (isDisplayDirectionLight) DrawDirectionLight();

//        if (Octree == null) return;

//        if (isDisplayOctree) DrawOctree();
//        if (isDisplaySpecifiedNode) DrawSpecifiedNode();
//        if (isDispalyCollisionNodes) DrawCollisionNodes();
//        if (isDisplayRay) DrawRay();
//        if (isDisplayHitTriangle) DrawHitTriangle();

//        if (isDisplayScatterLight) DrawScatterLight();

//    }

//    private void DrawOctree()
//    {
//        DrawOctreeNodes(Octree.Root);
//    }

//    private int Index;
//    private void DrawSpecifiedNode()
//    {
//        Index = 0;
//        DrawSpecifiedNode(Octree.Root.Children);
//    }

//    private void DrawCollisionNodes()
//    {
//        if (m_HitNodes == null) return;

//        foreach (OctreeNode[] HitNodes in m_HitNodes)
//            DrawOctreeNodes(HitNodes);
//    }

//    private bool DrawSpecifiedNode(OctreeNode[] Children)
//    {
//        foreach (OctreeNode Child in Children)
//        {
//            if (Child.IsBranchNode() && DrawSpecifiedNode(Child.Children)) return true; //已经绘制成功
//            else if (Child.IsBranchNode()) continue;
//            else if (Index == index)
//            {
//                DrawOctreeNode(Child);
//                DrawTriangles(Child);

//                Index++;
//                return true;
//            }
//            else Index++;
//        }

//        return false;
//    }

//    private void DrawOctreeNodes(OctreeNode Parent)
//    {
//        Gizmos.color = Color.green;
//        Gizmos.DrawWireCube(Parent.Bounds.center, Parent.Bounds.size);

//        if (Parent.Children == null) return;

//        foreach (OctreeNode Child in Parent.Children)
//        {
//            DrawOctreeNodes(Child);
//        }
//    }

//    private void DrawOctreeNodes(OctreeNode[] Nodes)
//    {
//        if (Nodes == null) return;

//        foreach (OctreeNode Node in Nodes)
//        {
//            DrawOctreeNode(Node);
//        }
//    }

//    private void DrawOctreeNode(OctreeNode Node)
//    {
//        DrawBounds(Node.Bounds);
//    }

//    private void DrawOctreeNode(string Index)
//    {
//        OctreeNode Node = Octree.Root;

//        for (int i = 0; i < Index.Length; i++)
//        {
//            Node = Node.Children[Convert.ToInt32(Index[i].ToString())];
//        }

//        DrawBounds(Node.Bounds);
//    }

//    private void DrawBounds(Bounds bounds)
//    {
//        Gizmos.color = Color.green;
//        Gizmos.DrawWireCube(bounds.center, bounds.size);
//    }

//    private void DrawTriangles(OctreeNode Node)
//    {
//        if (Node.Triangles == null) return;

//        foreach (Triangle Tri in Node.Triangles)
//        {
//            DrawTriangle(Tri);
//        }
//    }

//    private void DrawTriangle(Triangle Tri)
//    {
//        DrawTriangle(Tri.v0, Tri.v1, Tri.v2);
//    }

//    private void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawLine(v0, v1);
//        Gizmos.DrawLine(v1, v2);
//        Gizmos.DrawLine(v2, v0);
//    }

//    private void DrawRay()
//    {
//        if (m_Rays == null) return;

//        Gizmos.color = Color.yellow;

//        for (int i = 0; i < m_Rays.Count; i++ )
//        {
//            if (HitDistances.Count < i + 1)
//                Gizmos.DrawLine(m_Rays[i].origin, m_Rays[i].GetPoint(100f));
//            else
//                Gizmos.DrawLine(m_Rays[i].origin, m_Rays[i].GetPoint(m_HitDistances[i]));
//        }
//    }

//    private void DrawHitTriangle()
//    {
//        if (m_HitNodes == null) return;

//        foreach (Triangle Tri in m_HitTriangles)
//        {
//            DrawTriangle(Tri);
//            DrawNormal(Tri);
//        }
//    }

//    private void DrawNormal(Triangle Tri)
//    {
//        Gizmos.color = Color.red;

//        Gizmos.DrawLine(Tri.Center, Tri.Center + Tri.Normal);
//    }

//    private GameObject DirectionLight = null;

//    private void DrawDirectionLight()
//    {
//        if (Lights == null) return;

//        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.01f);

//        foreach (Light ray in Lights)
//        {
//            Gizmos.DrawLine(ray.Origin, ray.GetHitPoint());
//        }

//        //if (DirectionLight == null) { DirectionLight = GameObject.CreatePrimitive(PrimitiveType.Plane); DirectionLight.SetActive(false); }

//        //if (Lights[0].Direction.Equals(Vector3.zero)) { DirectionLight.SetActive(false); return; };   //不存在光线

//        //Gizmos.DrawWireMesh(DirectionLight.GetComponent<MeshFilter>().mesh, Lights[0].Origin, Quaternion.FromToRotation(Vector3.up, Lights[0].Direction));
//    }

//    private void DrawScatterLight()
//    {
//        SetLightHemiSphere();

//        if (LightHemisphere.SpherePatchs == null) return;

//        for (int i = 0; i < 100; i++)
//        {
//            for (int j = 0; j < 99; j++)
//            {
//                DrawRect(LightHemisphere.SpherePatchs[i* 100 + j].Center, LightHemisphere.SpherePatchs[i * 100 + j + 1].Center,
//                         LightHemisphere.SpherePatchs[((i + 1) % 100) * 100 + j + 1].Center, LightHemisphere.SpherePatchs[((i + 1) % 100) * 100 + j].Center,
//                         LightHemisphere.SpherePatchs[i * 100 + j].Cover ? Color.red : new Color(1, 0.92f, 0.016f, 0.1f));

//                //Gizmos.color = new Color(1, 0.92f, 0.016f, 0.1f);
//                //Gizmos.DrawLine(LightHemisphere.Center, LightHemisphere.SpherePatchs[i * 100 + j].Center);
//            }

//            DrawRect(LightHemisphere.SpherePatchs[i * 100 + 99].Center, LightHemisphere.Center + new Vector3(0, LightHemisphere.Radius, 0),
//                     LightHemisphere.Center + new Vector3(0, LightHemisphere.Radius, 0), LightHemisphere.SpherePatchs[((i + 1) % 100) * 100 + 99].Center,
//                     LightHemisphere.SpherePatchs[i * 100 + 99].Cover ? Color.red : new Color(1, 0.92f, 0.016f, 0.1f));

//            //Gizmos.DrawLine(LightHemisphere.Center, LightHemisphere.SpherePatchs[i * 100 + 99].Center);
//        }

//        //DrawTriangle(m_Triangle);
//        //Vector3[] Vertices = m_Triangle.CenterProjectToSphere(LightHemisphere.Center, LightHemisphere.Radius);
//        //DrawTriangle(Vertices[0], Vertices[1], Vertices[2]);

//        //Gizmos.color = Color.red;
//        //Gizmos.DrawLine(LightHemisphere.Center, Vertices[0]);
//        //Gizmos.DrawLine(LightHemisphere.Center, Vertices[1]);
//        //Gizmos.DrawLine(LightHemisphere.Center, Vertices[2]);
//    }

//    private void DrawRect(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
//    {
//        DrawRect(v0, v1, v2, v3, new Color(1, 0.92f, 0.016f, 0.1f));
//    }

//    private void DrawRect(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color color)
//    {
//        Gizmos.color = color;

//        Gizmos.DrawLine(v0, v1);
//        Gizmos.DrawLine(v1, v2);
//        Gizmos.DrawLine(v2, v3);
//        Gizmos.DrawLine(v3, v0);
//    }
//#endif
//}

//public class RayTracingWindow : EditorWindow
//{
//    private Vector2 ScrollPos;

//    [MenuItem("Window/Ray Tracing")]

//    public static void ShowWindow()
//    {
//        EditorWindow.GetWindow(typeof(RayTracingWindow), false, "Ray Tracing");

//        Initialize();
//    }

//    void OnGUI()
//    {
//        Initialize();

//        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

//        RayTracingVisualization _RayTracingVisualization = GameObject.Find("RayTracingVisualization").GetComponent<RayTracingVisualization>();

//        GUILayout.BeginVertical("Box");
//        GUILayout.Label("Display Node");

//        AddDisplayIndexField(_RayTracingVisualization);
//        AddBuildButton(_RayTracingVisualization);
//        AddPrevAndNextButton(_RayTracingVisualization);

//        GUILayout.EndVertical();

//        GUILayout.BeginVertical("Box");
//        GUILayout.Label("Ray Tracing");
//        AddRayButton(_RayTracingVisualization);
//        GUILayout.EndVertical();

//        GUILayout.BeginVertical("Box");
//        GUILayout.Label("Solar Parameters");
//        AddTimeButton(_RayTracingVisualization);
//        GUILayout.EndVertical();

//        AddScatterLightButton(_RayTracingVisualization);

//        AddDisplayButtons(_RayTracingVisualization);

//        AddDailySunshineSimulationButton(_RayTracingVisualization);

//        EditorGUILayout.EndScrollView();
//    }

//    void AddDisplayIndexField(RayTracingVisualization _RayTracingVisualization)
//    {
//        GUILayout.BeginHorizontal();
//        _RayTracingVisualization.index = EditorGUILayout.IntField("Index: ", _RayTracingVisualization.index);

//        GUILayout.EndHorizontal();
//    }

//    void AddBuildButton(RayTracingVisualization _RayTracingVisualization)
//    {
//        if (GUILayout.Button("Build Octree"))
//            _RayTracingVisualization.BuildOctree();
//    }

//    void AddPrevAndNextButton(RayTracingVisualization _RayTracingVisualization)
//    {
//        GUILayout.BeginHorizontal();

//        AddPrevButton(_RayTracingVisualization);
//        AddNextButton(_RayTracingVisualization);

//        GUILayout.EndHorizontal();
//    }

//    void AddNextButton(RayTracingVisualization _RayTracingVisualization)
//    {
//        if (GUILayout.Button("Next"))
//            _RayTracingVisualization.index++;
//    }

//    void AddPrevButton(RayTracingVisualization _RayTracingVisualization)
//    {
//        if (GUILayout.Button("Prev"))
//            _RayTracingVisualization.index--;
//    }

//    void AddRayButton(RayTracingVisualization _RayTracingVisualization)
//    {
//        Vector3 origin    = EditorGUILayout.Vector3Field("Origin   ", _RayTracingVisualization.Rays[0].origin);
//        Vector3 direction = EditorGUILayout.Vector3Field("Direction", _RayTracingVisualization.Rays[0].direction);
        
//        _RayTracingVisualization.RayCast(new Ray(origin, direction));

//        if (GUILayout.Button("Ray"))
//        {
//            origin = new Vector3(RandomNumer.Single() * 10 - 5, RandomNumer.Single() * 10 - 5, RandomNumer.Single() * 10 - 5);

//            _RayTracingVisualization.RayCast(new Ray(origin, new Vector3(0f, 2f, 0f) - origin));
//        }
//    }

//    void AddTimeButton(RayTracingVisualization _RayTracingVisualization)
//    {
//        EditorGUILayout.LabelField("Date", _RayTracingVisualization.Time.ToString());

//        int Year   = EditorGUILayout.IntField("Year ", _RayTracingVisualization.Time.Year);
//        int Month  = EditorGUILayout.IntField("Month", _RayTracingVisualization.Time.Month) - 1;
//        int Day    = EditorGUILayout.IntField("Day  ", _RayTracingVisualization.Time.Day) - 1;

//        //判断输入的参数是否正确
//        if (Year < 1) Year = 1;

//        int Hour = EditorGUILayout.IntField("Hour  ", _RayTracingVisualization.Time.Hour);
//        int Min = EditorGUILayout.IntField("Minute", _RayTracingVisualization.Time.Minute);

//        DateTime _DateTime = new DateTime(Year, 1, 1, 0, 0, 0);
//        _DateTime = _DateTime.AddMonths(Month).AddDays(Day).AddHours(Hour).AddMinutes(Min);

//        double Longitude = EditorGUILayout.DoubleField("Longitude", _RayTracingVisualization.Longitude);
//        double Latitude = EditorGUILayout.DoubleField("Latitude", _RayTracingVisualization.Latitude);

//        if (Longitude < 73.67) Longitude = 73.67;
//        if (Longitude > 135) Longitude = 135;
//        if (Latitude < 3.867) Latitude = 3.867;
//        if (Latitude > 53.5) Latitude = 53.5;

//        if (!_RayTracingVisualization.isDisplayDirectionLight) return;

//        _RayTracingVisualization.SetSun(_DateTime, Longitude, Latitude, 0.1f);
//    }

//    void AddScatterLightButton(RayTracingVisualization _RayTracingVisualization)
//    {
//        GUILayout.BeginVertical("Box");

//        GUILayout.Label("Scatter Light Options");

//        _RayTracingVisualization.SphereCenter = EditorGUILayout.Vector3Field("Hemisphere Center", _RayTracingVisualization.SphereCenter);

//        _RayTracingVisualization.IndexOfTriangle = EditorGUILayout.IntField("Index: ", _RayTracingVisualization.IndexOfTriangle);

//        GUILayout.BeginHorizontal();
//        if (GUILayout.Button("Prev"))
//            _RayTracingVisualization.IndexOfTriangle--;

//        if (GUILayout.Button("Next"))
//            _RayTracingVisualization.IndexOfTriangle++;
//        GUILayout.EndHorizontal();

//        GUILayout.EndVertical();
//    }

//    void AddDisplayButtons(RayTracingVisualization _RayTracingVisualization)
//    {
//        GUILayout.BeginVertical("Box");

//        GUILayout.Label("Display Options");

//        if (GUILayout.Button(_RayTracingVisualization.isDisplayOctree ? "Undisplay Octree" : "Display Octree"))
//            _RayTracingVisualization.isDisplayOctree = !_RayTracingVisualization.isDisplayOctree;
//        if (GUILayout.Button(_RayTracingVisualization.isDisplaySpecifiedNode ? "Undisplay Specified Node" : "Display Specified Node"))
//            _RayTracingVisualization.isDisplaySpecifiedNode = !_RayTracingVisualization.isDisplaySpecifiedNode;
//        if (GUILayout.Button(_RayTracingVisualization.isDisplayRay ? "Undisplay Ray" : "Display Ray"))
//            _RayTracingVisualization.isDisplayRay = !_RayTracingVisualization.isDisplayRay;
//        if (GUILayout.Button(_RayTracingVisualization.isDispalyCollisionNodes ? "Undisplay Collision Nodes" : "Display Collision Nodes"))
//            _RayTracingVisualization.isDispalyCollisionNodes = !_RayTracingVisualization.isDispalyCollisionNodes;
//        if (GUILayout.Button(_RayTracingVisualization.isDisplayHitTriangle ? "Undisplay Hit Triangle" : "Display Hit Triangle"))
//            _RayTracingVisualization.isDisplayHitTriangle = !_RayTracingVisualization.isDisplayHitTriangle;

//        if (GUILayout.Button(_RayTracingVisualization.isDisplayDirectionLight ? "Undisplay Direction Light" : "Display Direction Light"))
//            _RayTracingVisualization.isDisplayDirectionLight = !_RayTracingVisualization.isDisplayDirectionLight;

//        if (GUILayout.Button(_RayTracingVisualization.isDisplayScatterLight ? "Undisplay Scatter Light" : "Display Scatter Light"))
//            _RayTracingVisualization.isDisplayScatterLight = !_RayTracingVisualization.isDisplayScatterLight;


//        GUILayout.EndVertical();
//    }

//    void AddDisplayRayField(RayTracingVisualization _RayTracingVisualization)
//    {
//        GUILayout.BeginVertical("Box");
//        GUILayout.Label("Ray Parameters");

//        AddDisplayRayOriginField(_RayTracingVisualization);
//        AddDisplayRayDirectionField(_RayTracingVisualization);

//        GUILayout.EndVertical();
//    }

//    void AddDisplayRayOriginField(RayTracingVisualization _RayTracingVisualization)
//    {
//        GUILayout.BeginHorizontal();
//        GUILayout.Label("Origin    ");
//        GUILayout.TextField(_RayTracingVisualization.Rays[0].origin.ToString("f4"));
//        GUILayout.EndHorizontal();
//    }

//    void AddDisplayRayDirectionField(RayTracingVisualization _RayTracingVisualization)
//    {
//        GUILayout.BeginHorizontal();
//        GUILayout.Label("Direction");
//        GUILayout.TextField(_RayTracingVisualization.Rays[0].direction.ToString("f4"));
//        GUILayout.EndHorizontal();
//    }

//    void AddDailySunshineSimulationButton(RayTracingVisualization _RayTracingVisualization)
//    {
//        if (GUILayout.Button("Daily Sunshine Simulation"))
//        {
//            _RayTracingVisualization.DailySunshineSimulation();
//        }
//    }

//    static void Initialize()
//    {
//        if (GameObject.Find("RayTracingVisualization") == null)
//        {
//            GameObject g = new GameObject("RayTracingVisualization");
//            g.AddComponent<RayTracingVisualization>();
//        }
//    }
//}