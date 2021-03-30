using System;
using System.Collections.Generic;
using UnityEngine;

public class Light
{
    private Ray m_Ray;
    private float m_HitDistance;
    private Triangle m_HitTriangle;
    private float m_Energy;

    public Ray Ray { get { return m_Ray; } set { m_Ray = value; } }
    public Vector3 Origin { get { return Ray.origin; } set { m_Ray.origin = value; } }
    public Vector3 Direction { get { return Ray.direction; } set { m_Ray.direction = value; } }

    public float HitDistance { get { return m_HitDistance; } }
    public Triangle HitTriangle { get { return m_HitTriangle; } }

    public float Energy { get { return m_Energy; } set { m_Energy = value; if (m_Energy < 0) m_Energy = 0; } }

    public Light(Ray _Ray, float energy = 0)
    {
        if (energy < 0)
            throw new InvalidOperationException("The ligth energy is negative.");

        m_Ray = _Ray;
        m_HitDistance = -1;
        m_HitTriangle = new Triangle();
        m_Energy = energy;
    }

    public Light(Vector3 origin, Vector3 direction, float energy = 0)
    {
        if (energy < 0)
            throw new InvalidOperationException("The ligth energy is negative.");

        m_Ray = new Ray(origin, direction);
        m_HitDistance = -1;
        m_HitTriangle = new Triangle();
        m_Energy = energy;
    }

    public Vector3 GetPoint(float Distance)
    {
        return Ray.GetPoint(Distance);
    }

    public Vector3 GetHitPoint()
    {
        if (HitDistance < 0 && !IntersectGround())
            return GetPoint(100f);
        else
            return GetPoint(HitDistance);
    }

    /// <summary>
    /// 判断光线与AABB包围盒是否相交
    /// </summary>
    public bool IntersectAABB(Bounds AABB)
    {
        return AABB.IntersectRay(Ray, out m_HitDistance);
    }

    public bool IntersectAABB(Bounds AABB, out float Distance)
    {
        bool result = AABB.IntersectRay(Ray, out m_HitDistance);

        Distance = m_HitDistance;

        return result;
    }

    public bool IntersectAABB(Bounds AABB, out float NearDistance, out float FarDistance)
    {
        bool result = Intersect.IntersectRayAABB(Ray, AABB, out NearDistance, out FarDistance);

        m_HitDistance = NearDistance;

        return result;
    }

    /// <summary>
    /// 判断光线是否与三角面片相交
    /// </summary>
    public bool IntersectTriangle(Triangle _Triangle)
    {
        return IntersectTriangle(_Triangle, out m_HitDistance);
    }

    /// <summary>
    /// 判断光线是否与三角面片相交
    /// </summary>
    /// <param name="Distance">光线起点与相交点之间的距离</param>
    public bool IntersectTriangle(Triangle _Triangle, out float Distance)
    {
        bool isIntersected = Intersect.IntersectRayTriangleWithTexture(Ray, _Triangle, out m_HitDistance);

        Distance = m_HitDistance;

        if (isIntersected) m_HitTriangle = _Triangle;

        return isIntersected;
    }

    /// <summary>
    /// 判断光线是否与多个三角面片相交
    /// </summary>
    public bool IntersectTriangles(List<Triangle> Triangles)
    {
        return Intersect.IntersectRayTriangles(Ray, Triangles, out m_HitDistance, out m_HitTriangle);
    }

    /// <summary>
    /// 判断光线是否与多个三角面片相交
    /// </summary>
    /// <param name="_HitTriangle">相交的三角面片</param>
    public bool IntersectTriangles(List<Triangle> Triangles, out Triangle _HitTriangle)
    {
        return IntersectTriangles(Triangles, new Triangle(), out m_HitDistance, out _HitTriangle);
    }

    /// <summary>
    /// 判断光线是否与多个三角面片相交
    /// </summary>
    /// <param name="_HitDistance">光线起点与相交点之间的距离</param>
    /// <param name="_HitTriangle">相交的三角面片</param>
    public bool IntersectTriangles(List<Triangle> Triangles, out float _HitDistance, out Triangle _HitTriangle)
    {
        return IntersectTriangles(Triangles, new Triangle(), out _HitDistance, out _HitTriangle);
    }

    /// <summary>
    /// 判断光线是否与多个三角面片相交
    /// </summary>
    /// <param name="ExcludedTriangle">排除的三角面片</param>
    public bool IntersectTriangles(List<Triangle> Triangles, Triangle ExcludedTriangle)
    {
        return Intersect.IntersectRayTriangles(Ray, Triangles, ExcludedTriangle, out m_HitDistance, out m_HitTriangle);
    }

    /// <summary>
    /// 判断光线是否与多个三角面片相交
    /// </summary>
    /// <param name="ExcludedTriangle">排除的三角面片</param>
    /// <param name="_HitDistance">光线起点与相交点之间的距离</param>
    /// <param name="_HitTriangle">相交的三角面片</param>
    public bool IntersectTriangles(List<Triangle> Triangles, Triangle ExcludedTriangle, out float _HitDistance, out Triangle _HitTriangle)
    {
        bool result = Intersect.IntersectRayTriangles(Ray, Triangles, ExcludedTriangle, out m_HitDistance, out m_HitTriangle);

        _HitDistance = HitDistance;
        _HitTriangle = HitTriangle;

        return result;
    }

    /// <summary>
    /// 判断光线是否与地面相交
    /// </summary>
    public bool IntersectGround()
    {
        return Intersect.IntersectRayGround(Ray, out m_HitDistance);
    }

    /// <summary>
    /// 反射
    /// </summary>
    public Light Reflection()
    {
        if (HitDistance < 0) return null;   //未碰撞到物体

        Vector3.Reflect(Direction, HitTriangle.Normal);

        return null; //待修改
    }

    public string ToString()
    {
        return ToString(null);
    }

    public string ToString(string format)
    {
        return Ray.ToString(format) + "Energy" + Energy.ToString(format);
    }
}

public class LightCastPlane
{
    private Plane m_Plane;

    private Vector3 m_LeftBottom;
    private Vector3 m_RightBottom;
    private Vector3 m_RightTop;
    private Vector3 m_LeftTop;

    public Plane LightPlane { get { return m_Plane; } }
    public Vector3 Normal { get { return LightPlane.normal; } }
    public float Distance { get { return LightPlane.distance; } }

    public Vector3 LeftBottom { get { return m_LeftBottom; } set { m_LeftBottom = value; } }
    public Vector3 RightBottom { get { return m_RightBottom; } set { m_RightBottom = value; } }
    public Vector3 RightTop { get { return m_RightTop; } set { m_RightTop = value; } }
    public Vector3 LeftTop { get { return m_LeftTop; } set { m_LeftTop = value; } }

    public LightCastPlane(Plane plane)
    {
        m_Plane = plane;
        m_LeftBottom = m_RightBottom = m_RightTop = m_LeftTop = Vector3.zero;
    }

    public void SetRectBounds(Bounds bounds)
    {
        /*
         * 将包围盒的八个顶点投影到平面上
         * 寻找水平方向上最大值和最小值（最大的y值和最小的y值）
         * 寻找垂直方向上的最大最小值（与该投射光线的平面垂直，且与水平面垂直，根据计算出的距离判断）
         * 将水平方向上的值和垂直方向上的值与该平面相交求得唯一交点
         */


        /*
         * 垂直平面与水平面垂直，因此垂直平面的y = 0
         * 垂直平面与投射光线的平面垂直，因此 Normal.x * Normal_Vertical.x + Normal.y * Normal_Vertical.y + Normal.z * Normal_Vertical.z = 0
         * 因此Normal_Vertical.y = 0, 所以 Normal.x * Normal_Vertical.x + Normal.z * Normal_Vertical.z = 0
         * 假设Normal_Vertical = 1, 所以 Normal.x + Normal.z * Normal_Vertical.z = 0
         * 由此推断出 Normal_Vertical.z = -Normal.x / Normal.z
         */
        Vector3 Normal_Vertical = new Vector3(1, 0, -Normal.x / Normal.z);  //垂直方向的平面的法向量

        float Max_Y = 0, Min_Y = 0, Max_Distance = 0, Min_Distance = 0;

        for (int i = 0; i < 8; i++)
        {
            float x = (i / 2) % 2 == 0 ? bounds.extents.x : -bounds.extents.x;
            float y = (i / 4) == 0 ? bounds.extents.y : -bounds.extents.y;
            float z = (i % 2) == 0 ? bounds.extents.z : -bounds.extents.z;

            Vector3 Point = Vector3.ProjectOnPlane(bounds.center + new Vector3(x, y, z), Normal) - Normal * Distance;   //将包围盒顶点投影到平面上

            if (i == 0)
            {
                Max_Y = Min_Y = Point.y;
                Max_Distance = Min_Distance = Vector3.Dot(Normal_Vertical, Point);
            }
            else
            {
                Max_Y = Mathf.Max(Max_Y, Point.y);
                Min_Y = Mathf.Min(Min_Y, Point.y);


                /*
                 * 平面方程为 Normal * Point + Distance = 0
                 * Distance = -Normal * Point（点积）
                 * 由于 Distance 越大，代表的平面处的位置越低或越靠左
                 * 为了防止后续命名混乱，为此使 Distance = -Distance
                 * 即 Distance = Normal * Point（点积）
                 */
                float VerticalPlaneDistance = Vector3.Dot(Normal_Vertical, Point);

                Max_Distance = Mathf.Max(Max_Distance, VerticalPlaneDistance);
                Min_Distance = Mathf.Min(Min_Distance, VerticalPlaneDistance);
            }
        }

        LeftBottom = GetBoundsPoint(Normal_Vertical, Min_Distance, Min_Y);
        RightBottom = GetBoundsPoint(Normal_Vertical, Max_Distance, Min_Y);
        RightTop = GetBoundsPoint(Normal_Vertical, Max_Distance, Max_Y);
        LeftTop = GetBoundsPoint(Normal_Vertical, Min_Distance, Max_Y);
    }

    private Vector3 GetBoundsPoint(Vector3 Normal_Vertical, float Distance_Vertical, float y)
    {
        /*
         * 用行列式对二元一次方程求解
         * a11x + a12y = b1
         * a21x + a22y = b2
         *     | b1 a12 |        | a11 b1 |
         *     | b2 a22 |        | a21 b2 |
         * x = ----------    y = ----------
         *     |a11 a12 |        | a11 a12 |
         *     |a21 a22 |        | a21 a22 |
         * 
         * Normal.x * x + Normal.y * y + Normal.z * z = -Distance
         * Normal_Vertical.x * x + Normal_Vertical.y * y + Normal_Vertical.z * z = Distance_Vertical
         * 其中 y 已知，因此
         * Normal.x * x + Normal.z * z = -Distance - Normal.y * y
         * Normal_Vertical.x * x + Normal_Vertical.z * z = Distance_Vertical - Normal_Vertical.y * y
         */
        float a11 = Normal.x;
        float a12 = Normal.z;
        float b1 = -Distance - Normal.y * y;
        float a21 = Normal_Vertical.x;
        float a22 = Normal_Vertical.z;
        float b2 = Distance_Vertical - Normal_Vertical.y * y;

        float x = (b1 * a22 - a12 * b2) / (a11 * a22 - a21 * a12);
        float z = (a11 * b2 - a21 * b1) / (a11 * a22 - a21 * a12);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 该平面的面积（㎡）
    /// </summary>
    public float Area()
    {
        //根据平行四边形的计算公式 S = |AB * AC|, 即向量AB与向量AC的叉积的膜即为该平面ABCD的面积
        return Vector3.Magnitude(Vector3.Cross(LeftTop - LeftBottom, RightBottom - LeftBottom)) * MaizeParams.SCALE * MaizeParams.SCALE;
    }

    /// <summary>
    /// 计算该平面接受到的直射辐射能量（W）
    /// </summary>
    public float Energy(double DayAngle_Deg, double SolarAltitude_Deg)
    {
        double Elevation = (LeftTop.y + LeftBottom.y) / 2.0;    //高程


        /*
         * 计算出的直射辐射为某一时间段某一单位面积的能量，其单位为kJ/㎡
         * 乘以平面的面积即得到平面一段时间内接收到的辐射能量（平面永远与光线垂直）
         */
        return (float)(SolarSim.DirectIrradiance(DayAngle_Deg, SolarAltitude_Deg, Elevation) * Area());
    }

    /// <summary>
    /// 计算该平面接受到的PAR能量（W）
    /// </summary>
    /// <param name="DayAngle_Deg"></param>
    /// <param name="SolarAltitude_Deg"></param>
    /// <param name="StartHour"></param>
    /// <param name="EndHour"></param>
    /// <returns></returns>
    public float PAREnergy(double DayAngle_Deg, double SolarAltitude_Deg)
    {
        float irradition = Energy(DayAngle_Deg, SolarAltitude_Deg);

        return (float)SolarSim.IrradianceToPAR(irradition);
    }
}

public class LightCastHemisphere
{
    private Vector3 m_Center;
    private float m_Radius;
    private SpherePatch[] m_SpherePatchs;

    public Vector3 Center { get { return m_Center; } set { m_Center = value; } }
    public float Radius { get { return m_Radius; } set { m_Radius = value; } }
    public SpherePatch[] SpherePatchs { get { return m_SpherePatchs; } set { m_SpherePatchs = value; } }

    public LightCastHemisphere(Vector3 CenterPoint, Bounds bounds)
    {
        Center = CenterPoint;

        Radius = bounds.size.magnitude * 1.00001f;

        m_SpherePatchs = null;
    }

    /// <summary>
    /// 获取该半球的天空散射透过率
    /// </summary>
    /// <param name="LongitudeNum">纬度方向的剖分个数</param>
    /// <param name="LatitudeNum">经度度方向的剖分个数</param>
    /// <param name="Triangles">需要检测的三角面片</param>
    /// <param name="ExcludedIndex">无需检测的三角面片索引</param>
    /// <returns></returns>
    public float GetSkyTransmissivity(int LongitudeNum, int LatitudeNum, Triangle[] Triangles, int ExcludedIndex)
    {
        /*
         * 思路说明：
         * 1、对半球剖分，按传入参数，将半球划分为 LongitudeNum * LatitudeNum 个面片
         * 2、遍历传入的三角面片，确定被该三角面片遮挡的半球面片
         * 3、统计未被遮挡的面片所占的比例，即为天空散射透过率
         */
        if (SpherePatchs == null) Split(LongitudeNum, LatitudeNum); //如果还没有面片数据，则对半球进行剖分

        for (int i = 0; i < Triangles.Length; i++ )
        {
            if (i == ExcludedIndex) continue;   //自身

            ComputerCoveredPatchsFast(Triangles[i], LongitudeNum, LatitudeNum); //计算遮挡面片
        }

        //统计被遮挡的面片个数
        int CoverNum = 0;
        for (int i = 0; i < SpherePatchs.Length; i++)
        {
            if (SpherePatchs[i].Cover) CoverNum++;
        }

        //未被遮挡的面片个数所占的比例即为天空散射透过率
        return (SpherePatchs.Length - CoverNum) * 1.0f / SpherePatchs.Length;
    }

    /// <summary>
    /// 对球面进行剖分
    /// </summary>
    /// <param name="LongitudeNum">纬度方向的剖分个数</param>
    /// <param name="LatitudeNum">经度方向的剖分个数</param>
    public void Split(int LongitudeNum, int LatitudeNum)
    {
        int Count = LongitudeNum * LatitudeNum; //总面片数

        SpherePatchs = new SpherePatch[Count];    //球面片

        float DetlaHorizontal = Mathf.PI * 2.0f / LongitudeNum;
        float DetlaVertical = Mathf.PI / (LatitudeNum * 2.0f);

        //根据高度角和水平角确定半球内所有面片的中心点
        for (int i = 0; i < LongitudeNum; i++)
            for (int j = 0; j < LatitudeNum; j++)
            {
                float HorizontalAngle_Rad = DetlaHorizontal * i;    //水平角
                float VerticalAngle_Rad = DetlaVertical * j;        //高度角

                SpherePatchs[i * LatitudeNum + j] = new SpherePatch(Center + Radius * new Vector3(Mathf.Sin(HorizontalAngle_Rad) * Mathf.Cos(VerticalAngle_Rad), Mathf.Sin(VerticalAngle_Rad), Mathf.Cos(HorizontalAngle_Rad) * Mathf.Cos(VerticalAngle_Rad)));
            }
    }

    /// <summary>
    /// 当面片部分存在半球上时调用，获取其在半球上的所有顶点
    /// </summary>
    private Vector3[] GetVerticesInHemisphere(Triangle Tri)
    {
        List<Vector3> HighPoints = new List<Vector3>(); //存储三角形中高于平面的点
        List<Vector3> LowPoints = new List<Vector3>();  //存储三角形中低于平面的点

        for (int i = 0; i < 3; i++)
        {
            if (Tri.Vertices[i].y >= Center.y) HighPoints.Add(Tri.Vertices[i]);
            else                               LowPoints.Add(Tri.Vertices[i]);
        }

        Vector3[] Vertices = null;

        /*
         * 根据较高点的个数判断
         * 如果仅有一个较高点，则仅有两条边与半球底面相交，最终在底面之上的顶点有三个
         * 如果有两个较高点，则也仅有两条边与半球底面相交，最终在底面之上的顶点有四个
         */
        if (HighPoints.Count == 1)
        {
            Vertices = new Vector3[3];

            Vertices[0] = HighPoints[0];
            Intersect.IntersectLineHorizantalPlane(HighPoints[0], LowPoints[0], Center.y, out Vertices[1]);
            Intersect.IntersectLineHorizantalPlane(HighPoints[0], LowPoints[1], Center.y, out Vertices[2]);
        }
        else
        {
            Vertices = new Vector3[4];

            Vertices[0] = HighPoints[0];
            Vertices[1] = HighPoints[1];

            Intersect.IntersectLineHorizantalPlane(HighPoints[0], LowPoints[0], Center.y, out Vertices[2]);
            Intersect.IntersectLineHorizantalPlane(HighPoints[1], LowPoints[0], Center.y, out Vertices[3]);
        }

        return Vertices;
    }

    private void ComputerCoveredPatchs(Vector3[] Points)
    {
        for (int i = 0; i < SpherePatchs.Length; i++)
        {
            if (SpherePatchs[i].Cover) continue;

            if (Intersect.IntersectRayTriangle(new Ray(Center, SpherePatchs[i].Center - Center), Points[0], Points[1], Points[2]))
                SpherePatchs[i].Cover = true;
        }
    }

    /// <summary>
    /// 计算被遮挡的半球面片（快速）
    /// </summary>
    private void ComputerCoveredPatchsFast(Triangle Tri, int LongitudeNum, int LatitudeNum)
    {
        /*
         * 思路说明：
         * 将三角面片各个顶点投影到半球上
         * 根据投影点确定面片在半球上的边界（即在半球上做包围盒）
         * 遍历边界中所有的半球面片，确定其与三角面片是否相交
         */
        Vector3[] ProjectedPoints = GetProjectPoints(Tri);

        if (ProjectedPoints == null) return;    //该三角面片不在半球内

        int VerticalStart, VerticalEnd, HorizontalStart, HorizontalEnd;     //水平方向和垂直方向的起始位置和结束位置

        if (ProjectedPoints.Length == 3)
            GetBoundsInHemishpere(ProjectedPoints, LongitudeNum, LatitudeNum, out VerticalStart, out VerticalEnd, out HorizontalStart, out HorizontalEnd);
        else /*if (ProjectedPoints.Length == 6)*/
        {
            /*
             * 当投影点有6个时，则有两个三角面片
             * 分别计算其边界，并将边界合并
             */
            int VerticalStart0, VerticalEnd0, HorizontalStart0, HorizontalEnd0;
            GetBoundsInHemishpere(new Vector3[3] { ProjectedPoints[0], ProjectedPoints[1], ProjectedPoints[2] },
                LongitudeNum, LatitudeNum, out VerticalStart0, out VerticalEnd0, out HorizontalStart0, out HorizontalEnd0);

            int VerticalStart1, VerticalEnd1, HorizontalStart1, HorizontalEnd1;
            GetBoundsInHemishpere(new Vector3[3] { ProjectedPoints[3], ProjectedPoints[4], ProjectedPoints[5] },
                LongitudeNum, LatitudeNum, out VerticalStart1, out VerticalEnd1, out HorizontalStart1, out HorizontalEnd1);

            VerticalStart = Mathf.Min(VerticalStart0, VerticalStart1);
            VerticalEnd = Mathf.Max(VerticalEnd0, VerticalEnd1);

            HorizontalStart = Mathf.Min(HorizontalStart0, HorizontalStart1);
            HorizontalEnd = Mathf.Max(HorizontalEnd0, HorizontalEnd1);
        }

        //判断侧面且起始水平角大于结束水平角的面片是否被遮挡（如 起始经度为355， 而结束经度为5， 实际跨度355-360， 0-5）
        if (HorizontalStart > HorizontalEnd)
        {
            for (int i = HorizontalStart; i < LongitudeNum; i++)
            {
                for (int j = VerticalStart; j < VerticalEnd; j++)
                {
                    int index = i * LatitudeNum + j;

                    if (Intersect.IntersectRayTriangleWithTexture(new Ray(Center, SpherePatchs[index].Center - Center), Tri))
                        SpherePatchs[index].Cover = true;
                }
            }

            HorizontalStart = 0;
        }

        //判断起始水平角小于结束水平角的面片是否被遮挡
        for (int i = HorizontalStart; i < HorizontalEnd; i++)
        {
            for (int j = VerticalStart; j < VerticalEnd; j++)
            {
                int index = i * LatitudeNum + j;

                if (Intersect.IntersectRayTriangleWithTexture(new Ray(Center, SpherePatchs[index].Center - Center), Tri))
                    SpherePatchs[index].Cover = true;
            }
        }
    }

    /// <summary>
    /// 获取三角面片在半球上的投影点（仅投影顶点和与半球底面相交的部分）
    /// </summary>
    private Vector3[] GetProjectPoints(Triangle Tri)
    {
        if (Tri.MinHeight >= Center.y)                          //均在半球内
            return Tri.CenterProjectToSphere(Center, Radius);
        else if (Tri.MaxHeight >= Center.y)                     //部分在半球内
        {
            Vector3[] Vertices = GetVerticesInHemisphere(Tri);  //计算在底面以上的所有顶点

            if (Vertices.Length == 3)
            {
                return (new Triangle(Vertices)).CenterProjectToSphere(Center, Radius);
            }
            else /*if (Vertices.Length == 4)*/
            {
                List<Vector3> ProjectedPoints = new List<Vector3>();

                //将四边形拆分成两个三角形进行投影
                ProjectedPoints.AddRange(new Triangle(new Vector3[3] { Vertices[0], Vertices[1], Vertices[2] }).CenterProjectToSphere(Center, Radius));
                ProjectedPoints.AddRange(new Triangle(new Vector3[3] { Vertices[2], Vertices[3], Vertices[1] }).CenterProjectToSphere(Center, Radius));

                return ProjectedPoints.ToArray();
            }
        }
        else                                                    //不在半球内
            return null;
    }

    private void GetBoundsInHemishpere(Vector3[] ProjectedPoints, int LongitudeNum, int LatitudeNum, 
                                       out int VerticalStart, out int VerticalEnd, out int HorizontalStart, out int HorizontalEnd)
    {
        float DetlaHorizontal = Mathf.PI * 2.0f / LongitudeNum;
        float DetlaVertical = Mathf.PI / (LatitudeNum * 2.0f);

        float MaxVertical = float.MinValue, MinVertical = float.MaxValue; //最大最小高度角

        Vector3[] VerticalOrder = new Vector3[3];   //以高度角排序的顶点
        float[] HorizontalAngles = new float[3];    //记录各水平角的度数
        float[] HorizontalInterval = new float[3];  //记录各水平角之间的间隔

        for (int i = 0; i < 3; i++)
        {
            Vector3 Direction = ProjectedPoints[i] - Center;

            float VerticalAngle = Mathf.Asin(Direction.y / Direction.magnitude);     //高度角
            float HorizontalAngle = Mathf.Asin(Direction.x / Mathf.Sqrt(Direction.x * Direction.x + Direction.z * Direction.z));    //水平角

            //水平角调整
            if (Direction.z < 0) HorizontalAngle = Mathf.PI - HorizontalAngle;
            if (Direction.x < 0 && Direction.z > 0) HorizontalAngle = Mathf.PI * 2 + HorizontalAngle;

            //确定最大最小高度角
            if (VerticalAngle < MinVertical) MinVertical = VerticalAngle;
            if (VerticalAngle > MaxVertical) MaxVertical = VerticalAngle;

            //手动从小到大对水平角排序（速度最快）
            switch (i)
            {
                case 0: HorizontalAngles[0] = HorizontalAngle; break;
                case 1: if (HorizontalAngle < HorizontalAngles[0]) { HorizontalAngles[1] = HorizontalAngles[0]; HorizontalAngles[0] = HorizontalAngle; }
                    else { HorizontalAngles[1] = HorizontalAngle; }
                    break;
                case 2: if (HorizontalAngle < HorizontalAngles[0]) { HorizontalAngles[2] = HorizontalAngles[1]; HorizontalAngles[1] = HorizontalAngles[0]; HorizontalAngles[0] = HorizontalAngle; }
                    else if (HorizontalAngle < HorizontalAngles[1]) { HorizontalAngles[2] = HorizontalAngles[1]; HorizontalAngles[1] = HorizontalAngle; }
                    else { HorizontalAngles[2] = HorizontalAngle; }
                    break;
            }

            //手动从小到大对高度角排序
            switch (i)
            {
                case 0:
                    VerticalOrder[0] = ProjectedPoints[0];
                    break;
                case 1:
                    if (ProjectedPoints[1].y < VerticalOrder[0].y) { VerticalOrder[1] = VerticalOrder[0]; VerticalOrder[0] = ProjectedPoints[1]; }
                    else { VerticalOrder[1] = ProjectedPoints[1]; }
                    break;
                case 2:
                    if (ProjectedPoints[2].y < VerticalOrder[0].y) { VerticalOrder[2] = VerticalOrder[1]; VerticalOrder[1] = VerticalOrder[0]; VerticalOrder[0] = ProjectedPoints[2]; }
                    else if (ProjectedPoints[2].y < VerticalOrder[1].y) { VerticalOrder[2] = VerticalOrder[1]; VerticalOrder[1] = ProjectedPoints[2]; }
                    else { VerticalOrder[2] = ProjectedPoints[2]; }
                    break;
            }
        }

        if (MinVertical < 0) MinVertical = 0;

        //垂足点
        Vector2 FootPoint;

        //求中心到投影后的三角形最短的距离，即垂足点，该点即为最高高度角所在的点
        if (Intersect.VerticalPointInLine(new Vector2(Center.x, Center.z), new Vector2(VerticalOrder[2].x, VerticalOrder[2].z), new Vector2(VerticalOrder[1].x, VerticalOrder[1].z), out FootPoint))
        {
            float VerticalAngle = Mathf.Asin(Mathf.Sqrt(1 - (FootPoint.x * FootPoint.x + FootPoint.y * FootPoint.y) / (Radius * Radius)));

            if (VerticalAngle > MaxVertical) MaxVertical = VerticalAngle;
        }

        //各水平角之间的差值
        HorizontalInterval[0] = HorizontalAngles[1] - HorizontalAngles[0];
        HorizontalInterval[1] = HorizontalAngles[2] - HorizontalAngles[1];
        HorizontalInterval[2] = HorizontalAngles[0] + Mathf.PI * 2 - HorizontalAngles[2];

        //确定覆盖的球面是否是天顶角以及水平角起始位置
        bool isZenithAngle = Triangle.ContainNotInculdeBoundary(new Vector3(ProjectedPoints[0].x, Center.y, ProjectedPoints[0].z), new Vector3(ProjectedPoints[1].x, Center.y, ProjectedPoints[1].z), new Vector3(ProjectedPoints[2].x, Center.y, ProjectedPoints[2].z), Center);

        //确定水平角和高度角的起始、终止索引
        if (!isZenithAngle)
        {
            int index = 0;

            for (; index < 3; index++)
            {
                if (HorizontalInterval[index] > Mathf.PI || Mathf.Approximately(HorizontalInterval[index], Mathf.PI))
                {
                    break;
                }
            }

            VerticalStart = Mathf.CeilToInt(MinVertical / DetlaVertical);
            VerticalEnd = (int)(MaxVertical / DetlaVertical);

            if (index >= 3)
            {
                Debug.Log("Error!");
            }

            HorizontalStart = (int)(HorizontalAngles[(index + 1) % 3] / DetlaHorizontal);
            HorizontalEnd = (int)(HorizontalAngles[index] / DetlaHorizontal);
        }
        else
        {
            VerticalStart = Mathf.CeilToInt(MinVertical / DetlaVertical);
            VerticalEnd = LatitudeNum;

            HorizontalStart = 0;
            HorizontalEnd = LongitudeNum;
        }

        if (VerticalEnd != LatitudeNum) VerticalEnd++;
        if (HorizontalEnd != LongitudeNum) HorizontalEnd++;
    }

    /// <summary>
    /// 移动半球中心
    /// </summary>
    public void MoveTo(Vector3 CenterPoint)
    {
        Vector3 Direction = CenterPoint - Center;   //移动方向

        Center = CenterPoint;

        if (SpherePatchs == null) return;   //未剖分

        //移动半球的每个面片
        foreach (SpherePatch Patch in SpherePatchs)
        {
            Patch.MoveForward(Direction);
        }
    }
}

public struct SpherePatch
{
    private Vector3 m_Center;
    private bool m_Cover;

    public Vector3 Center { get { return m_Center; } set { m_Center = value; } }
    public bool Cover { get { return m_Cover; } set { m_Cover = value; } }

    public SpherePatch(Vector3 Center)
    {
        m_Center = Center;
        m_Cover = false;
    }

    public void MoveForward(Vector3 Direction)
    {
        Center = Center + Direction;
        Cover = false;
    }
}