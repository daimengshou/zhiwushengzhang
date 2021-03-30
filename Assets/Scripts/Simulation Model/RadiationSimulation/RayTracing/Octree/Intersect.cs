using System;
using System.Collections.Generic;
using UnityEngine;


public class Intersect
{
    public static bool VerticalPointInLine(Vector2 Point, Vector2 LinePoint0, Vector2 LinePoint1, out Vector2 FootPoint)
    {
        float A = LinePoint1.y - LinePoint0.y;
        float B = LinePoint0.x - LinePoint1.x;
        float C = LinePoint1.x * LinePoint0.y - LinePoint0.x * LinePoint1.y;

        float D = A * A + B * B;

        if (Mathf.Approximately(D, 0))  //两个点相等
        {
            FootPoint = Vector2.zero;
            return false;
        }

        if (Mathf.Approximately(A * Point.x + B * Point.y + C, 0))
        {
            FootPoint = Point;
            return BetweenTwoPoints(Point,LinePoint0, LinePoint1);
        }

        FootPoint = new Vector2((B * B * Point.x - A * B * Point.y - A * C) / D, (-A * B * Point.x + A * A * Point.y - B * C) / D);

        return BetweenTwoPoints(FootPoint, LinePoint0, LinePoint1);
    }

    private static bool BetweenTwoPoints(Vector2 Point, Vector2 LPoint, Vector2 RPoint)
    {
        Vector2 LDirection = LPoint - Point;
        Vector2 RDirection = RPoint - Point;

        return LDirection.x * RDirection.x <= 0 && LDirection.y * RDirection.y <= 0;
    }


    //待测试
    private static bool IntersectPlaneAABB(Vector3 Normal, float d, Vector3 BoxExtent)
    {
        Vector3 Min = Vector3.zero;
        Vector3 Max = Vector3.zero;

        for (int i = 0; i < 3; i++)
        {
            if (Normal[i] > 0) { Min[i] = -BoxExtent[i]; Max[i] = BoxExtent[i]; }
            else { Min[i] = BoxExtent[i]; Max[i] = -BoxExtent[i]; }
        }

        if (Vector3.Dot(Normal, Min) + d > 0) return false;
        if (Vector3.Dot(Normal, Max) + d >= 0) return true;

        return false;
    }

    public static bool IntersectRayAABB(Ray _Ray, Bounds AABB, out float NearDistance, out float FarDistance)
    {
        Vector3 MinPoint = AABB.min;    //包围盒最大顶点
        Vector3 MaxPoint = AABB.max;    //包围盒最小顶点

        NearDistance = float.MinValue;
        FarDistance = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            bool isParallel = _Ray.direction[i] == 0;
            bool isOutside = _Ray.origin[i] < MinPoint[i] || _Ray.origin[i] > MaxPoint[i];

            if (isParallel && isOutside) return false;    //与平面平行且在平面之外,则必定不与该包围盒相交
            else if (isParallel && !isOutside) continue;    //与平面平行但在平面之内，则不与该平面相交

            float t1 = (MinPoint[i] - _Ray.origin[i]) / _Ray.direction[i];  //求相交距离
            float t2 = (MaxPoint[i] - _Ray.origin[i]) / _Ray.direction[i];

            NearDistance = Mathf.Min(t1, t2) > NearDistance ? Mathf.Min(t1, t2) : NearDistance; //复制最小值
            FarDistance = Mathf.Max(t1, t2) < FarDistance ? Mathf.Max(t1, t2) : FarDistance;    //复制最大值

            if (NearDistance > FarDistance || FarDistance < 0) return false;
        }

        return true;
    }

    public static bool IntersectRayAABB(Ray _Ray, Bounds AABB, out Vector3 NearVector, out Vector3 FarVector)
    {
        float NearDistance, FarDistance;

        if (IntersectRayAABB(_Ray, AABB, out NearDistance, out FarDistance))
        {
            NearVector = _Ray.origin + NearDistance * _Ray.direction;
            FarVector = _Ray.origin + FarDistance * _Ray.direction;

            return true;
        }
        else
        {
            NearVector = FarVector = Vector3.zero;

            return false;
        }
    }

    public static bool IntersectRayTriangleWithTexture(Ray _Ray, Triangle _Triangle)
    {
        float Distance;
        return IntersectRayTriangleWithTexture(_Ray, _Triangle, out Distance);
    }

    /// <summary>
    /// 射线与三角形求交（有纹理）
    /// </summary>
    public static bool IntersectRayTriangleWithTexture(Ray _Ray, Triangle _Triangle, out float Distance)
    {
        float u, v;

        if (!IntersectRayTriangleWithoutTexture(_Ray, _Triangle, out u, out v, out Distance)) return false; //射线与面片无交点

        Vector2 UV = _Triangle.GetUV(u, v); //获取该交点的UV坐标

        Texture2D Texture = GameObjectOperation.GetTexture(_Triangle.Index.Belong) as Texture2D;   //获取纹理

        if (Texture.GetPixelBilinear(UV.x, UV.y).a > 0f) //获取该点的透明度，如果大于0.5则有碰撞
            return true;
        else
            return false;
    }

    //参考代码：https://www.cnblogs.com/graphics/archive/2010/08/09/1795348.html
    /// <summary>
    /// 射线与三角形求交（无纹理）
    /// </summary>
    public static bool IntersectRayTriangleWithoutTexture(Ray _Ray, Triangle _Triangle, out float Distance)
    {
        float u, v;
        return IntersectRayTriangleWithoutTexture(_Ray, _Triangle, out u, out v, out Distance);
    }

    /// <summary>
    /// 射线与三角形求交（无纹理）
    /// </summary>
    /// <param name="u">三角形内一点的参数化表达参数（V = (1 - u - v)V0 + uV1 + vV2）</param>
    /// <param name="v">三角形内一点的参数化表达参数（V = (1 - u - v)V0 + uV1 + vV2）</param>
    public static bool IntersectRayTriangleWithoutTexture(Ray _Ray, Triangle _Triangle, out float u, out float v, out float Distance)
    {
        Distance = -1;
        u = v = -1;

        Vector3 e1 = _Triangle.v1 - _Triangle.v0;
        Vector3 e2 = _Triangle.v2 - _Triangle.v0;

        Vector3 P = Vector3.Cross(_Ray.direction, e2);

        //行列式
        float det = Vector3.Dot(e1, P);

        Vector3 T;
        if (det > 0) { T = _Ray.origin - _Triangle.v0; }
        else { T = _Triangle.v0 - _Ray.origin; det = -det; }

        if (det < 0.0001f) return false;

        u = Vector3.Dot(T, P);
        if (u < 0.0f || u > det) return false;

        Vector3 Q = Vector3.Cross(T, e1);
        v = Vector3.Dot(_Ray.direction, Q);
        if (v < 0.0f || u + v > det) return false;

        Distance = Vector3.Dot(e2, Q) / det;

        u /= det;
        v /= det;

        return true;    
    }

    public static bool IntersectRayTriangle(Ray _Ray, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 e1 = v1 - v0;
        Vector3 e2 = v2 - v0;

        Vector3 P = Vector3.Cross(_Ray.direction, e2);

        //行列式
        float det = Vector3.Dot(e1, P);

        Vector3 T;
        if (det > 0) { T = _Ray.origin - v0; }
        else { T = v0 - _Ray.origin; det = -det; }

        if (det < 0.0001f) return false;

        float u = Vector3.Dot(T, P);
        if (u < 0.0f || u > det) return false;

        Vector3 Q = Vector3.Cross(T, e1);
        float v = Vector3.Dot(_Ray.direction, Q);
        if (v < 0.0f || u + v > det) return false;

        return true;
    }

    public static bool IntersectRayTriangles(Ray _Ray, List<Triangle> Triangles)
    {
        Triangle HitTriangle;
        return IntersectRayTriangles(_Ray, Triangles, out HitTriangle);
    }

    public static bool IntersectRayTriangles(Ray _Ray, List<Triangle> Triangles, out Triangle HitTriangle)
    {
        float HitDistance;
        return IntersectRayTriangles(_Ray, Triangles, out HitDistance, out HitTriangle);
    }

    public static bool IntersectRayTriangles(Ray _Ray, List<Triangle> Triangles, out float HitDistance, out Triangle HitTriangle)
    {
        return IntersectRayTriangles(_Ray, Triangles, new Triangle(), out HitDistance, out HitTriangle);
    }

    public static bool IntersectRayTriangles(Ray _Ray, List<Triangle> Triangles, Triangle ExcludedTriangle, out float HitDistance, out Triangle HitTriangle)
    {
        List<Triangle> HitTriangles = new List<Triangle>();
        List<float> HitDistances = new List<float>();

        foreach (Triangle _Triangle in Triangles)   //对所有三角面片进行遍历，确定与射线碰撞且与其起点最近的面片
        {
            if (_Triangle.Equals(ExcludedTriangle)) continue;   //为排除三角面片

            float Distance;
            if (IntersectRayTriangleWithTexture(_Ray, _Triangle, out Distance) && Distance > 0)
            {
                HitTriangles.Add(_Triangle);
                HitDistances.Add(Distance);
            }
        }

        if (HitTriangles.Count == 0) { HitDistance = -1.0f; HitTriangle = new Triangle(); return false; }

        HitDistance = Mathf.Min(HitDistances.ToArray());
        HitTriangle = HitTriangles[HitDistances.IndexOf(HitDistance)];
        return true;
    }

    public static bool IntersectRayGround(Ray _Ray, out float HitDistance)
    {
        Vector3 Normal = Vector3.up;

        HitDistance = (Vector3.Dot(Normal, Vector3.zero) - Vector3.Dot(Normal, _Ray.origin)) / Vector3.Dot(Normal, _Ray.direction);

        return HitDistance >= 0;
    }

    public static bool IntersectPointAABB(Vector3 Point, Bounds AABB)
    {
        Vector3 Max = AABB.max;
        Vector3 Min = AABB.min;

        return Point.x >= Min.x && Point.x <= Max.x &&
               Point.y >= Min.y && Point.y <= Max.y &&
               Point.z >= Min.z && Point.z <= Max.z;
    }

    public static bool IntersectLineHorizantalPlane(Vector3 LinePoint0, Vector3 LinePoint1, float Height, out Vector3 Point)
    {
        if ((LinePoint0.y - Height) * (LinePoint1.y - Height) > 0)  //线段的两个端点都在水平面的一边
        {
            Point = Vector3.zero;
            return false;
        }

        //用相似三角形的原理求出xz的坐标
        float x = (Height - LinePoint1.y) * (LinePoint0.x - LinePoint1.x) / (LinePoint0.y - LinePoint1.y) + LinePoint1.x;
        float z = (Height - LinePoint1.y) * (LinePoint0.z - LinePoint1.z) / (LinePoint0.y - LinePoint1.y) + LinePoint1.z;

        Point = new Vector3(x, Height, z);

        return true;
    }

    public static bool IntersectTriangleAABB(Vector3 TriVertex0, Vector3 TriVertex1, Vector3 TriVertex2, Bounds AABB)
    {
        Vector3 v0 = TriVertex0 - AABB.center;
        Vector3 v1 = TriVertex1 - AABB.center;
        Vector3 v2 = TriVertex2 - AABB.center;

        Vector3 e0 = v1 - v0;
        Vector3 e1 = v2 - v1;
        Vector3 e2 = v0 - v2;

        if (!AxisText_X(v0, v2, e0, AABB) || !AxisText_Y(v0, v2, e0, AABB) || !AxisTest_Z(v1, v2, e0, AABB)) return false;
        if (!AxisText_X(v0, v2, e1, AABB) || !AxisText_Y(v0, v2, e1, AABB) || !AxisTest_Z(v0, v1, e1, AABB)) return false;
        if (!AxisText_X(v0, v1, e2, AABB) || !AxisText_Y(v0, v1, e2, AABB) || !AxisTest_Z(v1, v2, e2, AABB)) return false;

        if (Mathf.Min(v0.x, v1.x, v2.x) > AABB.extents.x || Mathf.Max(v0.x, v1.x, v2.x) < -AABB.extents.x) return false;
        if (Mathf.Min(v0.y, v1.y, v2.y) > AABB.extents.y || Mathf.Max(v0.y, v1.y, v2.y) < -AABB.extents.y) return false;
        if (Mathf.Min(v0.z, v1.z, v2.z) > AABB.extents.z || Mathf.Max(v0.z, v1.z, v2.z) < -AABB.extents.z) return false;

        Vector3 normal = Vector3.Cross(e0, e1);
        float d = -Vector3.Dot(normal, v0);

        if (!IntersectPlaneAABB(normal, d, AABB.extents)) return false;

        return true;
    }

#region 各轴测试
    static bool AxisText_X(Vector3 v0, Vector3 v1, Vector3 e, Bounds bounds)
    {
        float p0 = e.z * v0.y - e.y * v0.z;
        float p1 = e.z * v1.y - e.y * v1.z;

        float min = Mathf.Min(p0, p1);
        float max = Mathf.Max(p0, p1);

        float rad = Mathf.Abs(e.z) * bounds.extents.y + Mathf.Abs(e.y) * bounds.extents.z;

        return !(min > rad || max < -rad);
    }

    static bool AxisText_Y(Vector3 v0, Vector3 v1, Vector3 e, Bounds bounds)
    {
        float p0 = -e.z * v0.x + e.x * v0.z;
        float p1 = -e.z * v1.x + e.x * v1.z;

        float min = Mathf.Min(p0, p1);
        float max = Mathf.Max(p0, p1);

        float rad = Mathf.Abs(e.z) * bounds.extents.x + Mathf.Abs(e.x) * bounds.extents.z;

        return !(min > rad || max < -rad);
    }

    static bool AxisTest_Z(Vector3 v0, Vector3 v1, Vector3 e, Bounds bounds)
    {
        float p0 = e.y * v0.x - e.x * v0.y;
        float p1 = e.y * v1.x - e.x * v1.y;

        float min = Mathf.Min(p0, p1);
        float max = Mathf.Max(p0, p1);

        double rad = Mathf.Abs(e.y) * bounds.extents.x + Mathf.Abs(e.x) * bounds.extents.y;

        return !(min > rad || max < -rad);
    }
#endregion
}

