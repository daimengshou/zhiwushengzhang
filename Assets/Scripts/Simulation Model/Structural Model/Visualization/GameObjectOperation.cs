/*
 * 文件名：GameObjectOperation.cs
 * 描述：GameObject类操作系统，编写常用操作，减少重复代码。
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectOperation : MonoBehaviour{

    private static GameObjectOperation instance = null;

    public static GameObjectOperation GetInstance()
    {
        if (instance == null)
            instance = GameObject.Find("GameObjectOperation").GetComponent<GameObjectOperation>();

        return instance;
    }
    
    /// <summary>
    /// 获取该物体以及该物体包含的所有子物体的集合
    /// </summary>
    public static List<GameObject> GetGameObjects(GameObject Father)
    {
        List<GameObject> GameObjects = new List<GameObject>();
        GameObjects.Add(Father);

        foreach (Transform Child in Father.transform)
        {
            GameObjects.AddRange(GetGameObjects(Child.gameObject));
        }

        return GameObjects;
    }

    /// <summary>
    /// 获取该物体包含的子物体的个数林 
    /// </summary>
    public static int GetChildCount(GameObject Father)
    {
        return Father.transform.childCount;
    }

    /// <summary>
    /// 获取该物体所有子物体的个数
    /// </summary>
    public static int GetSumOfChildCount(GameObject Father)
    {
        int result = Father.transform.childCount;

        foreach (Transform Child in Father.transform)
        {
            result += GetSumOfChildCount(Child.gameObject);
        }

        return result;
    }

    /// <summary>
    /// 获取顶点
    /// </summary>
    public static Vector3[] GetVertices(GameObject _Object)
    {
        if (!GameObjectValidate.HavaVertices(_Object)) return null;

        return _Object.GetComponent<MeshFilter>().mesh.vertices;
    }

    /// <summary>
    /// 获取世界坐标系下的顶点坐标
    /// </summary>
    public static Vector3[] GetVerticesInWorld(GameObject _Object)
    {
        if (!GameObjectValidate.HavaVertices(_Object)) return null;     //无顶点

        Matrix4x4 matrix = _Object.transform.localToWorldMatrix;        //转换矩阵

        Vector3[] Vertices = _Object.GetComponent<MeshFilter>().mesh.vertices;  //局部坐标系下的顶点坐标
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i] = matrix.MultiplyPoint3x4(Vertices[i]); //转换到世界坐标系中
        }

        return Vertices;
    }

    /// <summary>
    /// 获取世界坐标系下的部分坐标
    /// </summary>
    public static Vector3[] GetVerticesInWorld(GameObject _Object, int Start, int Length)
    {
        if (!GameObjectValidate.HavaVertices(_Object)) return null;     //无顶点

        Matrix4x4 matrix = _Object.transform.localToWorldMatrix;        //转换矩阵
        Vector3[] Vertices = _Object.GetComponent<MeshFilter>().mesh.vertices;  //局部坐标系下的顶点坐标

        if (Start >= Vertices.Length || Start + Length > Vertices.Length || Length <= 0) return null;   //起始的位置超出边界或终点位置超出边界或长度过小

        Vector3[] result = new Vector3[Length];

        for (int i = Start; i < Start + Length; i++)
        {
            result[i - Start] = matrix.MultiplyPoint3x4(Vertices[i]); //转换到世界坐标系
        }

        return result;
    }

    /// <summary>
    /// 获取UV坐标
    /// </summary>
    public static Vector2[] GetUV(GameObject _Object)
    {
        if (!GameObjectValidate.HavaVertices(_Object)) return null;

        return _Object.GetComponent<MeshFilter>().mesh.uv;
    }

    /// <summary>
    /// 获取三角面片索引
    /// </summary>
    public static int[] GetTriangleIndexes(GameObject _Object)
    {
        if (!GameObjectValidate.HavaVertices(_Object)) return null;

        return _Object.GetComponent<MeshFilter>().mesh.triangles;
    }

    /// <summary>
    /// 获取三角面片
    /// </summary>
    public static List<Triangle> GetTriangles(OrganIndex _OrganIndex)
    {
        if (_OrganIndex.Type == OrganType.Branch)
        {
            return GetBranchTriangles(_OrganIndex);
        }
        else
        {
            return GetOrganTriangles(_OrganIndex);
        }
    }

    /// <summary>
    /// 获取整颗树的三角面片
    /// </summary>
    public static List<Triangle> GetTreeTriangles(TreeModel treeModel)
    {
        List<Triangle> TreeTriangles = new List<Triangle>();

        if (treeModel.BranchModel == null) return null;
        
        //获取枝干的三角面片
        List<Triangle> branchTriangles = GetBranchTriangles(treeModel, treeModel.BranchModel);
        if (branchTriangles == null) return null;

        TreeTriangles.AddRange(GetBranchTriangles(treeModel, treeModel.BranchModel));

        //获取除枝干外所有的三角面片
        foreach (OrganIndex Index in treeModel.OrganIndexes)
        {
            TreeTriangles.AddRange(GetTriangles(Index));
        }

        return TreeTriangles;
    }

    /// <summary>
    /// 获取除叶片外所有的三角面片
    /// </summary>
    public static List<Triangle> GetTreeTrianglesWithoutLeaves(TreeModel treeModel)
    {
        List<Triangle> TreeTriangles = new List<Triangle>();

        //获取枝干三角面片
        if (treeModel.BranchModel != null)
            TreeTriangles.AddRange(GetBranchTriangles(treeModel, treeModel.BranchModel));

        //获取除枝干和叶片外所有的三角面片
        foreach (OrganIndex Index in treeModel.OrganIndexes)
        {
            if (Index.Type == OrganType.Leaf) continue;

            TreeTriangles.AddRange(GetTriangles(Index));
        }

        return TreeTriangles;   
    }

    /// <summary>
    /// 获取叶片上的三角面片
    /// </summary>
    public static List<Triangle> GetLeafTriangles(TreeModel treeModel)
    {
        List<Triangle> LeafTriangles = new List<Triangle>();

        foreach (OrganIndex Index in treeModel.OrganIndexes)
        {
            if (Index.Type != OrganType.Leaf) continue;

            LeafTriangles.AddRange(GetTriangles(Index));
        }

        return LeafTriangles;
    }

    /// <summary>
    /// 获取枝干的三角面片
    /// </summary>
    public static List<Triangle> GetBranchTriangles(TreeModel treeModel, GameObject Branch)
    {
        if (!Branch.tag.Equals("Branch")) return null;  //非枝干

        List<BranchIndex> BranchIndexes = treeModel.BranchIndexes; //获取所有枝干索引

        if (BranchIndexes == null || BranchIndexes.Count == 0) return null;

        if (BranchIndexes[0].Belong != Branch) return null; //枝干索引到的对象不为当前对象

        List<Triangle> Triangles = new List<Triangle>();    

        Vector3[] Vertices = GetVerticesInWorld(Branch);
        Vector2[] UV = GetUV(Branch);

        for (int i = 0; i < BranchIndexes.Count; i++)
        {
            BranchIndex Index = BranchIndexes[i];
            GetBranchTriangles(ref Vertices, ref UV, ref Index, ref Triangles);
        }

        return Triangles;
    }

    /// <summary>
    /// 获取法向量
    /// </summary>
    public static Vector3[] GetNormals(GameObject _Object)
    {
        if (!GameObjectValidate.HavaVertices(_Object)) return null;

        return _Object.GetComponent<MeshFilter>().mesh.normals;
    }

    /// <summary>
    /// 获取纹理
    /// </summary>
    public static Texture GetTexture(GameObject _Object)
    {
        if (_Object == null) return null;

        if (!GameObjectValidate.HaveTexture(_Object))   //当前对象无纹理
        {
            foreach (Transform Child in _Object.transform)  //遍历其子对象，若有纹理则返回该纹理
            {
                Texture texture = GetTexture(Child.gameObject);

                if (texture != null) return texture;
            }

            return null;
        }
        else
            return _Object.GetComponent<MeshRenderer>().material.mainTexture;
    }

    public static void SetTexture(GameObject _Object, Texture tex)
    {
        if (tex == null) return;

        if (!GameObjectValidate.HavaVertices(_Object))
        {
            foreach (Transform child in _Object.transform)
            {
                SetTexture(child.gameObject, tex);
            }
        }
        else
            _Object.GetComponent<MeshRenderer>().material.mainTexture = tex;
    }

    public static void ClearTexture(GameObject _object)
    {
        if (!GameObjectValidate.HavaVertices(_object))
        {
            foreach (Transform child in _object.transform)
            {
                ClearTexture(child.gameObject);
            }
        }
        else
            _object.GetComponent<MeshRenderer>().material.mainTexture = null;
    }

    public static void SetThreshold(GameObject _object, int threshold)
    {
        Color color = CellularTexture.DEC2Color(threshold);
        List<Material> materials = GetMaterials(_object);

        foreach (Material material in materials)
        {
            if (material.shader.name != "Custom/Leaves") continue;

            material.SetColor("_Threshold", color);
        }
    }

    public static Material GetMaterial(GameObject _object)
    {
        if (_object == null) return null;

        MeshRenderer mr = _object.GetComponent<MeshRenderer>();

        if (mr != null)
            return mr.material;
        else
            return null;
    }

    public static List<Material> GetMaterials(GameObject _object)
    {
        if (_object == null) return null;

        List<Material> materials = new List<Material>();

        MeshRenderer mr = _object.GetComponent<MeshRenderer>();

        if (mr != null && mr.materials != null)
            materials.AddRange(mr.materials);

        foreach (Transform child in _object.transform)
        {
            materials.AddRange(GetMaterials(child.gameObject));
        }

        return materials;
    }

    public static void ClearMaterials(GameObject _object, float delay = 0)
    {
        if (_object == null) return;

        List<Material> materials = GetMaterials(_object);

        foreach (var mar in materials)
        {
            GameObject.Destroy(mar, delay);
        }

        materials.Clear();
        materials = null;
    }

    public static List<UnityEngine.Mesh> GetMeshes(GameObject _object)
    {
        if (_object == null) return null;

        List<UnityEngine.Mesh> meshes = new List<UnityEngine.Mesh>();

        if (GameObjectValidate.HavaVertices(_object))
            meshes.Add(_object.GetComponent<MeshFilter>().mesh);

        foreach (Transform child in _object.transform)
        {
            meshes.AddRange(GetMeshes(child.gameObject));
        }

        return meshes;
    }

    public static void ClearMeshes(GameObject _object, float delay = 0)
    {
        if (_object == null) return;

        List<UnityEngine.Mesh> meshes = GetMeshes(_object);

        foreach (var mesh in meshes)
        {
            GameObject.Destroy(mesh, delay);
        }

        meshes.Clear();
        meshes = null;
    }

    /// <summary>
    /// 获取面积
    /// </summary>
    public static float GetOrganArea(GameObject _Object)
    {
        List<Triangle> triangles = GetOrganTriangles(null, _Object);

        float sum = 0;
        foreach (Triangle tri in triangles)
        {
            sum += tri.Area();
        }

        return sum;
    }

    /// <summary>
    /// 用GPU进行运算（舍弃，由于传输数据导致速度不如CPU快）
    /// </summary>
    public static float GetOrganAreaFast(GameObject _Object)
    {
        Vector3[] vertices = GetVerticesInWorld(_Object);
        int[] triangles = GetTriangleIndexes(_Object);

        ComputeShader shader = GameObject.Find("ComputeMemory").GetComponent<ComputeMemory>().shader;   //获取计算shader
        int kernel = shader.FindKernel("AreaCompute");  //核函数

        /*
         * 传入参数：
         * 顶点（世界坐标系下）、三角面片顶点索引
         */
        ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, 12);
        shader.SetBuffer(kernel, "Vertices", verticesBuffer);
        verticesBuffer.SetData(vertices);

        ComputeBuffer trianglesBuffer = new ComputeBuffer(triangles.Length, sizeof(int));
        shader.SetBuffer(kernel, "Triangles", trianglesBuffer);
        trianglesBuffer.SetData(triangles);

        /*
         * 传出参数
         * 每个三角面片的面积
         */
        float[] trianglesArea = new float[triangles.Length / 3];
        ComputeBuffer trianglesAreaBuffer = new ComputeBuffer(trianglesArea.Length, 4);
        shader.SetBuffer(kernel, "TrianglesArea", trianglesAreaBuffer);

        //执行
        shader.Dispatch(kernel, trianglesArea.Length, 1, 1);

        //获取传出参数
        trianglesAreaBuffer.GetData(trianglesArea);

        //释放
        verticesBuffer.Release();
        trianglesBuffer.Release();
        trianglesAreaBuffer.Release();

        float sum = 0;
        foreach(float area in trianglesArea)
        {
            sum += area;
        }

        return sum;
    }

    /// <summary>
    /// 更新物体
    /// </summary>
    public static void UpdateMeshInfo(GameObject _Object, Vector3[] vertices, Vector2[] uv = null, int[] triangles = null)
    {
        var mesh = _Object.GetComponent<MeshFilter>().mesh;

        mesh.vertices = vertices;
        if (uv != null) mesh.uv = uv;
        if (triangles != null) mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    private static List<Triangle> GetBranchTriangles(OrganIndex _OrganIndex)
    {
        GameObject Branch = _OrganIndex.Belong;

        List<Triangle> Triangles = new List<Triangle>(40);

        int BottomVerticesIndex = ((BranchIndex)_OrganIndex).BottomVerticesIndex;   //底端顶点坐标索引
        int TopVerticesIndex = ((BranchIndex)_OrganIndex).TopVerticesIndex;         //顶端顶点坐标索引

        Vector3[] BottomVertices = GetVerticesInWorld(Branch, BottomVerticesIndex, 20); //底端顶点坐标
        Vector3[] TopVertices = GetVerticesInWorld(Branch, TopVerticesIndex, 20);       //顶端顶点坐标

        if (BottomVertices == null || TopVertices == null || BottomVertices.Length != TopVertices.Length) return null;

        Vector2[] UV = GetUV(Branch);   //纹理坐标

        for (int i = 0; i < 20; i++)
        {
            Triangles[i * 2] = new Triangle(new Vector3[3] { BottomVertices[(i + 1) % 20], BottomVertices[i], TopVertices[i]},
                                            new Vector2[3] { UV[BottomVerticesIndex + (i + 1) % 20], UV[BottomVerticesIndex + i], UV[TopVerticesIndex + i] },
                                            _OrganIndex);

            Triangles[i * 2 + 1] = new Triangle(new Vector3[3] { TopVertices[i], TopVertices[(i + 1) % 20], BottomVertices[(i + 1) % 20] },
                                                new Vector2[3] { UV[TopVerticesIndex + i], UV[TopVerticesIndex + (i + 1) % 20], UV[BottomVerticesIndex + (i + 1) % 20] },
                                                _OrganIndex);
        }

        return Triangles;
    }

    private static void GetBranchTriangles(ref Vector3[] Vertices, ref Vector2[] UV, ref BranchIndex Index, ref List<Triangle> Triangles)
    {
        int BottomIndex = Index.BottomVerticesIndex;
        int TopIndex = Index.TopVerticesIndex;

        for (int i = 0; i < 20; i++)
        {
            Triangles.Add(new Triangle(Vertices[BottomIndex + (i + 1) % 20], Vertices[BottomIndex + i], Vertices[TopIndex + i],
                                       UV[BottomIndex + (i + 1) % 20], UV[BottomIndex + i], UV[TopIndex + i],
                                       Index));

            Triangles.Add(new Triangle(Vertices[TopIndex + i], Vertices[TopIndex + (i + 1) % 20], Vertices[BottomIndex + (i + 1) % 20],
                                       UV[TopIndex + i], UV[TopIndex + (i + 1) % 20], UV[BottomIndex + (i + 1) % 20],
                                       Index));
        }
    }

    private static List<Triangle> GetOrganTriangles(OrganIndex _OrganIndex)
    {
        return GetOrganTriangles(_OrganIndex, _OrganIndex.Belong);
    }

    private static List<Triangle> GetOrganTriangles(OrganIndex _OrganIndex, GameObject _Object)
    {
        List<Triangle> Triangles = new List<Triangle>();

        int[] TriangleIndexes = GetTriangleIndexes(_Object);
        if (TriangleIndexes != null)
        {
            Vector3[] Vertices = GetVerticesInWorld(_Object);   //世界坐标系下的顶点坐标
            Vector2[] UV = GetUV(_Object);                      //纹理坐标

            for (int i = 0; i < TriangleIndexes.Length; i = i + 3)
            {
                Triangles.Add(new Triangle(new Vector3[3] { Vertices[TriangleIndexes[i]], Vertices[TriangleIndexes[i + 1]], Vertices[TriangleIndexes[i + 2]] },
                                           new Vector2[3] { UV[TriangleIndexes[i]],       UV[TriangleIndexes[i + 1]],       UV[TriangleIndexes[i + 2]]       }, 
                                           _OrganIndex));
            }
        }

        //添加子对象的三角面片
        foreach (Transform Child in _Object.transform)
        {
            Triangles.AddRange(GetOrganTriangles(_OrganIndex, Child.gameObject));
        }

        return Triangles;
    }

    public static void DestroyAllChildren(GameObject _object)
    {
        foreach (Transform child in _object.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public static void DestroyTexWithoutAnimation(Texture tex)
    {
        GetInstance().InternalDestroyTexWithoutAnimation(tex);
    }

    private void InternalDestroyTexWithoutAnimation(Texture tex)
    {
        StartCoroutine(CoroutineDestroyTexWithoutAnimation(tex));
    }

    private IEnumerator CoroutineDestroyTexWithoutAnimation(Texture tex)
    {
        yield return null;

        yield return new WaitUntil(() => { return !TreeAnimator.IsPlaying(); });

        GameObject.Destroy(tex);
    }
}
