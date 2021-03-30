using System;
using System.Collections.Generic;
using UnityEngine;


class RayTracing
{
    public static bool CollisionDection(Light _Light, Octree _Octree)
    {
        return CollisionDection(_Light.Ray, _Octree);
    }

    public static bool CollisionDection(Ray _Ray, Octree _Octree)
    {
        Triangle HitTriangle;

        return CollisionDection(_Ray, _Octree, out HitTriangle);
    }

    public static bool CollisionDection(Light _Light, Octree _Octree, out Triangle HitTriangle)
    {
        return CollisionDection(_Light.Ray, _Octree, out HitTriangle);
    }

    public static bool CollisionDection(Ray _Ray, Octree _Octree, out Triangle HitTriangle)
    {
        float HitDistance;

        return CollisionDection(_Ray, _Octree, out HitDistance, out HitTriangle);
    }

    /// <summary>
    /// 碰撞检测（与三角面片碰撞）
    /// </summary>
    /// 
    public static bool CollisionDection(Light _Light, Octree _Octree, out float HitDistance, out Triangle HitTriangle)
    {
        return CollisionDection(_Light.Ray, _Octree, out HitDistance, out HitTriangle);
    }

    public static bool CollisionDection(Ray _Ray, Octree _Octree, out float HitDistance, out Triangle HitTriangle)
    {
        return CollisionDection(_Ray, _Octree, new Triangle(), out HitDistance, out HitTriangle);
    }

    public static bool CollisionDection(Light _Light, Octree _Octree, Triangle ExludedTriangle, out float HitDistance, out Triangle HitTriangle)
    {
        return CollisionDection(_Light.Ray, _Octree, ExludedTriangle, out HitDistance, out HitTriangle);
    }

    public static bool CollisionDection(Ray _Ray, Octree _Octree, Triangle ExludedTriangle, out float HitDistance, out Triangle HitTriangle)
    {
        float Distance;
        HitDistance = -1;
        HitTriangle = new Triangle();

        if (!_Octree.Root.Bounds.IntersectRay(_Ray, out Distance)) return false;    //计算出与外包围盒的交点

        //寻找匹配的叶节点
        //当距离小于0的时候，说明该射线的起始位置在包围盒内，因此直接根据该起始位置寻找
        OctreeNode CurrentNode = FindMatchNode(_Ray, Distance < 0 ? 0 : Distance, _Octree.Root, null);

        while (CurrentNode != null && !float.IsInfinity(Distance))
        {
            if (CurrentNode.HaveTriangles() && /*有三角面片*/
                Intersect.IntersectRayTriangles(_Ray, CurrentNode.Triangles, ExludedTriangle, out HitDistance, out HitTriangle) && /*射线与三角面片相交*/
                CurrentNode.Bounds.Contains(_Ray.GetPoint(HitDistance)) /*交点在包围盒内*/)
            {
                return true;
            }

            //无三角面片或光线与三角面片不相交的情况下
            Distance = GetExitPosition(_Ray, CurrentNode);

            if (Distance == -1 || float.IsInfinity(Distance)) break;   //没有交点

            CurrentNode = FindMatchNode(_Ray, Distance, _Octree.Root, CurrentNode);   //寻找下一个节点
        }

        return false;   //光线与当前物体无交点
    }

    public static bool CollisionDection(Light _Light, Octree _Octree, Triangle ExludedTriangle)
    {
        float Distance;

        if (!_Octree.Root.Bounds.IntersectRay(_Light.Ray, out Distance)) return false;

        OctreeNode CurrentNode = FindMatchNode(_Light.Ray, Distance < 0 ? 0 : Distance, _Octree.Root, null);

        while (CurrentNode != null && !float.IsInfinity(Distance))
        {
            if (CurrentNode.HaveTriangles() && /*有三角面片*/
                _Light.IntersectTriangles(CurrentNode.Triangles, ExludedTriangle) && /*射线与三角面片相交*/
                CurrentNode.Bounds.Contains(_Light.GetHitPoint()) /*交点在包围盒内*/)
            {
                return true;
            }

            Distance = GetExitPosition(_Light.Ray, CurrentNode);

            if (Distance == -1 || float.IsInfinity(Distance)) break;   //没有交点

            CurrentNode = FindMatchNode(_Light.Ray, Distance, _Octree.Root, CurrentNode);   //寻找下一个节点
        }

        return false; //光线与当前物体无交点
    }

    /// <summary>
    /// 碰撞检测（不与三角面片碰撞）
    /// </summary>
    public static bool CollisionDection(Light _Light, Octree _Octree, out OctreeNode[] Nodes)
    {
        return CollisionDection(_Light.Ray, _Octree, out Nodes);
    }

    public static bool CollisionDection(Ray _Ray, Octree _Octree, out OctreeNode[] Nodes)
    {
        float Distance;
        Nodes = null;

        List<OctreeNode> tempNodes = new List<OctreeNode>();

        if (!_Octree.Root.Bounds.IntersectRay(_Ray, out Distance)) return false;    //计算出与外包围盒的交点

        //寻找匹配的叶节点
        //当距离小于0的时候，说明该射线的起始位置在包围盒内，因此直接根据该起始位置寻找
        OctreeNode CurrentNode = FindMatchNode(_Ray, Distance < 0 ? 0 : Distance, _Octree.Root, null);

        while (CurrentNode != null && !float.IsInfinity(Distance))
        {
            tempNodes.Add(CurrentNode);

            Distance = GetExitPosition(_Ray, CurrentNode);

            if (Distance == -1 || float.IsInfinity(Distance)) break;

            CurrentNode = FindMatchNode(_Ray, Distance, _Octree.Root, CurrentNode);
        }

        Nodes = tempNodes.ToArray();

        return true;
    }

    //射线反射
    public static Ray RayReflection(Ray IncidentRay, float HitDistance, Triangle HitTriangle)
    {
        Vector3 Origin = IncidentRay.GetPoint(HitDistance);

        Vector3 Direction;
        Direction = Vector3.Reflect(IncidentRay.direction, HitTriangle.Normal);

        return new Ray(Origin, Direction);
    }

    //寻找匹配的叶节点
    private static OctreeNode FindMatchNode(Ray _Ray, float Distance, OctreeNode RootNode, OctreeNode ExcludedNode)
    {
        Vector3 Point = _Ray.GetPoint(Distance);

        if (Convert.ToInt32(Point.x == RootNode.Bounds.center.x) +
            Convert.ToInt32(Point.y == RootNode.Bounds.center.y) +
            Convert.ToInt32(Point.z == RootNode.Bounds.center.z) >= 2)   //当交点有两个或两个以上的值与包围盒中心的值相同，则向前移动一小步
            Point = _Ray.GetPoint(Distance + 0.00001f);
        
        OctreeNode CurrentNode = RootNode;  //当前的节点

        /*
         * 根据交点（Point）的位置与当前包围盒中心的位置
         * 计算其位于包围盒内的方位（上下、左右以及前后）
         * 并根据该方位确定该交点的所处的当前包围盒的子包围盒
         * 如此循环直至当前包围盒不可再小为止
         */
        while (CurrentNode.IsBranchNode())
        {
            Vector3 Direction = Point - CurrentNode.Bounds.center;   //获取交点与当前包围盒中心的差值

            CurrentNode = FindMatchChildNode(Direction, CurrentNode, ExcludedNode);    //寻找下一个合适的节点

            if (CurrentNode == null) //没有匹配的子节点
                return null;
        }

        if (CurrentNode == ExcludedNode) 
            return null;
        else 
            return CurrentNode;

    }

    /// <summary>
    /// 寻找匹配的子节点
    /// </summary>
    private static OctreeNode FindMatchChildNode(Vector3 Direction, OctreeNode CurrentNode, OctreeNode ExcludedNode)
    {
        if (CurrentNode.IsLeafNode())
            return null;

        List<int> Indexes = GetIndexes(Direction);

        int Depth = CurrentNode.Depth + 1;   //子包围盒的在八叉树中的深度

        //只有一个索引值
        if (Indexes.Count == 1 && (ExcludedNode == null || !ExcludedNode.Index.Equals(CurrentNode.Children[Indexes[0]].Index))) //当排除节点为空或排除节点与该节点不相同，则返回该节点
            return CurrentNode.Children[Indexes[0]];
        else if (Indexes.Count == 1 /* && ExcludedNode.Index.Equals(CurrentNode.Children[Indexes[0]].Index) */) //当排除节点与该节点相同
            return null;

        /*
         * 当出现多个索引值的时候
         * 根据排除节点的Index以及其子包围盒的Index选择节点
         * 例如
         * 当前节点的Index为14，计算出的两个符合条件的子包围盒的Index分别为142和143
         * 而排除节点的Index为14256
         * 由于排除节点和其符合条件的子包围盒142在相同深度处（depth = 3的位置）的索引相同，即均为2
         * 故排除子包围盒142，选择包围盒143
         * 会存在无排除节点以及排除节点的深度小于当前包围子包围盒深度的情况（如排除节点的Index为142，而当前包围盒的Index为1432，其子包围盒为14321和14322）
         */
        if (ExcludedNode != null &&             /*存在无排除节点的情况*/ 
            ExcludedNode.Depth >= Depth &&      /*存在排除节点的深度小于当前包围盒子包围盒深度的情况*/
            CurrentNode.Children[Indexes[0]].Index[Depth - 1].Equals(ExcludedNode.Index[Depth - 1]))
            return CurrentNode.Children[Indexes[1]];
        else
            return CurrentNode.Children[Indexes[0]];
    }

    private static float GetExitPosition(Ray _Ray, OctreeNode Node)
    {
        float FarDistance, NearDistance;

        if (Intersect.IntersectRayAABB(_Ray, Node.Bounds, out NearDistance, out FarDistance))
            return FarDistance * 1.00001f;
        else
            return -1;
    }

    private static List<int> GetIndexes(Vector3 Direction)
    {
        List<int> Indexes;

        Indexes = GetYAxisIndexes(Direction);           //获取Y轴的索引
        Indexes = GetZAxisIndexes(Direction, Indexes);  //获取Z轴的索引
        Indexes = GetXAxisIndexes(Direction, Indexes);  //获取X轴的索引

        return Indexes;
    }

    private static List<int> GetXAxisIndexes(Vector3 Direction, List<int> Indexes)
    {
        List<int> ResultIndexes = new List<int>();

        foreach (int Index in Indexes)
        {
            if (Direction.x >= 0) ResultIndexes.Add(Index - 2);
            if (Direction.x <= 0) ResultIndexes.Add(Index);
        }

        return ResultIndexes;
    }

    private static List<int> GetYAxisIndexes(Vector3 Direction)
    {
        List<int> Indexes = new List<int>();

        if (Direction.y >= 0) Indexes.Add(3);
        if (Direction.y <= 0) Indexes.Add(7);

        return Indexes;
    }

    private static List<int> GetZAxisIndexes(Vector3 Direction, List<int> Indexes)
    {
        List<int> ResultIndexes = new List<int>();

        foreach (int Index in Indexes)
        {
            if (Direction.z >= 0) ResultIndexes.Add(Index - 1);
            if (Direction.z <= 0) ResultIndexes.Add(Index);
        }

        return ResultIndexes;
    }
}

