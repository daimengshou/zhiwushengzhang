using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

partial class TreeModel : BaseTree
{
    private const float SCALE = 0.01f / MaizeParams.SCALE;  //将单位从cm转换成unity中的一个单位

    private InsectSim insectSim = null;

    public double LAI { get; set; }

    //记录匹配完成的前后两关键帧的枝干以及其他器官
    public List<PairedIndex<BranchIndex>> PairedBranchIndexes   { get; private set; }
    public List<PairedIndex<OrganIndex>> PairedOrganIndexes     { get; private set; }

    public override double Biomass                { get; set; }
    public override double AbovegroundBiomass     { get; set; }
    public double AccumulatedTemperature          { get; set; }
    public override int    GrowthCycle            { get { return ComputeGrowthCycle(); } }
    public override double Height                 { get { return ComputeHeight(); } }
    public override double LeafArea               { get { return ComputeLeafArea(); } }

    void Start()
    {
        cylinderObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinderObject.SetActive(false);
    }

    private void Init()
    {
        Biomass = 0;
        AbovegroundBiomass = 0;

        insectSim = new InsectSim(100);
    }

    public bool IsStopDevelopment = false;

    public void NextDay(bool isClear = true)
    {
        if (CurrentStep <= 1) { Init(); }

        int preGC = ComputeGrowthCycle();
        int curGC = FunctionSim.ComputeGrowthCycle(this);

        //未出苗
        if (preGC < 1 && preGC == curGC)
        {
            TemperatureAccumulation();
            return;
        }

        double biomass = PreviousDaylightOutput(out IsStopDevelopment);

        if (IsStopDevelopment)
        {
            return;
        }

        bool isSameGC = true;

        if (preGC != curGC)
        {
            /*
             * 当当前生长周期与实际计算得到的生长周期不同时
             * 则发生拓扑变化
             */
            NextStep();

            /*
             * 生长周期发生变化
             * 用该标识符记录
             */
            isSameGC = false;
        }

        /*
         * 对植物的形态进行模拟
         * 随后用于下一阶段生物量的产生
         */
        MorphologicalSim(biomass, isClear, isSameGC);

        /*
         * 积温累加
         * 用于判断当前生长周期
         */
        TemperatureAccumulation();

        /*
         * 计算叶面积指数
         * 用于计算第二天生物量分配
         */
        ComputeLAI();
    }

    private double PreviousDaylightOutput(out bool stopDevelopment)
    {
        double biomass =
            SolarSim.SunShineSim(this, EnvironmentParams.PhotosyntheticModel);

        if (biomass < Mathf.Epsilon)
        {
            stopDevelopment = true;
            return 0;
        }
        else
        {
            stopDevelopment = false;

            Biomass += biomass;
            return biomass;
        }
    }

    private void DataCheck()
    {
        ValidateStep();         //检测步骤是否出错
        ValidateFinalList();    //检测最终的链表是否出错
    }

    public void MorphologicalSim(double biomass, bool isClear = true, bool isSameGC = false)
    {
        DataCheck();    //数据检查

        /*
         * 包含枝干和器官的索引
         * 其每一个索引包含了后续生成GameObject所需的形态信息（旋转角度）以及
         * 生成形态信息所需的数据（生物量、年龄）
         */
        List<OrganIndex> indexes = GetIndexes(isClear, isSameGC);

        /*
         * 分配生物量给各个器官
         * 累积的生物量为后续形态模拟提供参考依据
         */
        FunctionSim.PhotosynthateAllocation(this, biomass, FunctionSim.GrowthPeriodJudgment(BranchIndexes, OrganIndexes));

        /*
         * 遍历每个Index
         * 计算形态数据
         * 根据形态数据绘制GameObject
         */
        int curLevel = 0;
        Vector3 curPosition = Vector3.zero;
        Stack<Vector3> positionStack = new Stack<Vector3>();
        foreach (OrganIndex index in indexes)
        {
            switch (index.Type)
            {
                case OrganType.Branch:
                    BranchIndex branchIndex = index as BranchIndex;

                    if (branchIndex.Level > curLevel)
                        positionStack.Push(curPosition);
                    else if (branchIndex.Level < curLevel)
                        curPosition = positionStack.Pop();

                    curLevel = branchIndex.Level;
                    curPosition = RecordBranchModel(branchIndex, curPosition);

                    break;
                case OrganType.Leaf:

                    LeafIndex leafIndex = index as LeafIndex;
                    CreateLeafModel(leafIndex, curPosition);

                    break;
                case OrganType.Flower:

                    MaleIndex maleIndex = index as MaleIndex;
                    CreateFlowerModel(maleIndex, curPosition);

                    break;
                case OrganType.Fruit:

                    FemaleIndex femaleIndex = index as FemaleIndex;
                    CreateFruitModel(femaleIndex, curPosition);

                    break;
            }
        }

        CreateBranchModel();

        /*
         * 病虫害模拟
         * 测试
         */
        InsectSimulation();

        PostProcessing();
    }

    public Vector3 TreeLocalPosition { get; set; }

    private void PostProcessing()
    {
        //保证模型在正确的位置
        m_TreeModel.transform.SetParent(transform);
        m_TreeModel.transform.localPosition = TreeLocalPosition;
    }

    private List<OrganIndex> GetIndexes(bool isClear, bool isSameGC = false)
    {
        /*
         * 获取当前枝干索引和器官索引
         * 用于后续不同时期索引匹配以及下一个GC索引中部分数据继承
         */
        List<BranchIndex> curBranchIndexes = BranchIndexes;
        List<OrganIndex> curOrganIndexes = OrganIndexes;

        if (isClear)
        {
            ClearActiveModel();
        }

        InitialActiveModelWithoutClearing();    //初始化模型，但不清除之前渲染的模型，防止重复

        List<OrganIndex> organIndexList = new List<OrganIndex>();

        GetIndexesFrom(RuleData.FinalList.First, new BranchIndex(), new GameObjectInfo(), ref organIndexList);  //计算下一个GC的索引（未继承当前GC索引的数据）

        /*
         * 数据继承
         * 将上一个GC中索引的数据（生物量、年龄）继承到当前GC中
         * 该类数据用于后续形态模拟
         */
        DataOfIndexInheritance(curBranchIndexes, curOrganIndexes, isSameGC);

        return organIndexList;
    }

    private void GetIndexesFrom(LLinkedListNode<LTerm> node, BranchIndex fromBranch, GameObjectInfo objectInfo, ref List<OrganIndex> indexList)
    {
        if (node == null)
            throw new ArgumentNullException("No start node.");

        LLinkedListNode<LTerm> headNode = m_RuleData.FinalList.First;

        BranchIndex curBranchIndex = fromBranch;

        do 
        {
            if (node.Value != null)
            {
                switch (node.Value.Symbol[0])   //符号解析
                {
                    case 'F':
                        objectInfo.Length = Convert.ToSingle(node.Value.Params[0]);
                        BranchIndex branchIndex = GetBranchIndex(curBranchIndex, fromBranch, objectInfo);
                        
                        indexList.Add(branchIndex);    //在器官索引列表中添加，后续绘制用，绘制完成后删除
                        AddBranchIndex(branchIndex);        //在枝干索引列表中添加，不删除

                        curBranchIndex = branchIndex;       //设置当前的枝干索引
                        
                        break;
                    case 'f':
                        objectInfo.Position = MoveForward(objectInfo);
                        break;
                    case '!':
                        objectInfo.Radius = Convert.ToSingle(node.Value.Params[0]);
                        break;
                    case '+':
                        objectInfo.Rotation -= new Vector3(Convert.ToSingle(node.Value.Params[0]), 0, 0);
                        break;
                    case '-':
                        objectInfo.Rotation += new Vector3(Convert.ToSingle(node.Value.Params[0]), 0, 0);
                        break;
                    case '&':
                        objectInfo.Rotation -= new Vector3(0, Convert.ToSingle(node.Value.Params[0]), 0);
                        break;
                    case '^':
                        objectInfo.Rotation += new Vector3(0, Convert.ToSingle(node.Value.Params[0]), 0);
                        break;
                    case '\\':
                        objectInfo.Rotation += new Vector3(0, 0, Convert.ToSingle(node.Value.Params[0]));
                        break;
                    case '/':
                        objectInfo.Rotation -= new Vector3(0, 0, Convert.ToSingle(node.Value.Params[0]));
                        break;
                    case '%':
                        OrganIndex organIndex = GetOrganIndex(MeshResource.GetInstance().GetNameOf(Convert.ToInt32(node.Value.Params[0]))/*将标识码转换成对应的名字*/,
                            curBranchIndex, objectInfo);

                        if (organIndex != null)
                        {
                            indexList.Add(organIndex);
                            AddOrganIndex(organIndex);
                        }

                        break;
                    case '[':
                        node = node.Next;

                        GetIndexesFrom(node, curBranchIndex, objectInfo.Clone(), ref indexList);   //入栈

                        node = _CurrentNode;    //将节点设置成分支结束的节点

                        break;
                    case ']':
                        _CurrentNode = node;

                        node = headNode.Previous;   //出栈，中断该函数
                        break;
                }
            }

            node = node.Next;
        } while (node != headNode);
    }

    /// <summary>
    /// 向前移动一定距离但不渲染
    /// </summary>
    /// <param name="BottomPosition">移动前的位置</param>
    /// <param name="Length">移动的长度</param>
    /// <param name="Angles">旋转角度</param>
    /// <param name="OrderOfAngles">旋转的先后顺序</param>
    /// <returns>移动后的位置</returns>
    private Vector3 MoveForward(GameObjectInfo ObjectInfo)
    {
        GameObject tempObject = new GameObject();
        tempObject.transform.position = ObjectInfo.Position + new Vector3(0, ObjectInfo.Length, 0); //移动到指定位置

        RotationGameObject(tempObject, ObjectInfo.Position, ObjectInfo.Rotation);   //旋转

        Vector3 Position = tempObject.transform.position;
        GameObject.Destroy(tempObject, 0f);

        return Position;
    }

    private BranchIndex GetBranchIndex(BranchIndex preBranchIndex, BranchIndex fromBranchIndex, GameObjectInfo objectInfo)
    {
        BranchIndex index = new BranchIndex();

        /*
         * 存储索引信息：
         * 枝干级别、枝干在该级上的索引
         * 用于后续判断不同时期的两个枝干索引是否为相同索引
         */
        index.Level = fromBranchIndex.Level + 1;
        index.Index = preBranchIndex == fromBranchIndex ? 0 : preBranchIndex.Index + 1;

        /*
         * 存储空间信息：
         * 长度、半径、旋转方向
         * 用于后续绘制
         */
        index.Length = objectInfo.Length;
        index.Radius = objectInfo.Radius;
        index.Rotation = objectInfo.Rotation;

        /*
         * 存储上下文信息：
         * 前驱枝干和从属枝干
         * 用于后续判断不同时期的两个枝干索引是否为相同索引
         */
        index.Previous = preBranchIndex;
        index.From = fromBranchIndex;

        /*
         * 未绘制对象
         * 顶端顶点索引和底端顶点索引未知
         */
        index.Belong = m_BranchModel;

        return index;
    }

    private OrganIndex GetOrganIndex(string meshName, BranchIndex fromBranchIndex, GameObjectInfo objectInfo)
    {
        /*
         * 根据名字确定其对应的Mesh
         * 根据Mesh即可确定该器官类型、模型
         */
        Mesh matchedMesh = m_MeshGroup.Find(mesh => mesh.Name.Equals(meshName));                    //寻找Mesh集中对应的Mesh
        if (matchedMesh == null) throw new InvalidOperationException("Error Mesh Name in Rules");   //未能找到对应Mesh

        switch (matchedMesh.Type)
        {
            case OrganType.Leaf:
                return GetLeafIndex(matchedMesh, fromBranchIndex, objectInfo);
            case OrganType.Flower:
                return GetMaleIndex(matchedMesh, fromBranchIndex, objectInfo);
            case OrganType.Fruit:
                return GetFemaleIndex(matchedMesh, fromBranchIndex, objectInfo);
            default:
                return GetLeafIndex(matchedMesh, fromBranchIndex, objectInfo);
        }
    }

    private LeafIndex GetLeafIndex(Mesh mesh, BranchIndex fromBranchIndex, GameObjectInfo objectInfo)
    {
        LeafIndex index = new LeafIndex();

        /*
         * 存储信息：
         * 索引值和器官类型
         * 用于后续判断不同时期的两个器官索引是否为相同索引
         */
        index.Index = GetIndexWithSameType(OrganType.Leaf, fromBranchIndex);
        index.LeafMesh = mesh;

        /*
         * 存储空间信息：
         * 半径、旋转方向
         * 用于后续绘制
         */
        index.Radius = objectInfo.Radius;
        index.Rotation = objectInfo.Rotation;

        /*
         * 存储上下文信息：
         * 从属枝干
         */
        index.From = fromBranchIndex;

        /*
         * 病虫害信息：
         * 细胞纹理
         */
        index.CelluarTex = CellularTexMemory.GetInstance().GetCellularTex(0);

        return index;
    }

    private MaleIndex GetMaleIndex(Mesh mesh, BranchIndex fromBranchIndex, GameObjectInfo objectInfo)
    {
        MaleIndex maleIndex = new MaleIndex();

        maleIndex.Index = GetIndexWithSameType(OrganType.Flower, fromBranchIndex);

        maleIndex.MaleMesh = mesh;

        maleIndex.Radius = objectInfo.Radius;
        maleIndex.Rotation = objectInfo.Rotation;

        maleIndex.From = fromBranchIndex;

        return maleIndex;
    }

    private FemaleIndex GetFemaleIndex(Mesh mesh, BranchIndex fromBranchIndex, GameObjectInfo objectInfo)
    {
        FemaleIndex femaleIndex;

        int index = GetIndexWithSameType<FemaleIndex>(OrganType.Fruit, fromBranchIndex, out femaleIndex);

        if (index >= 1)
        {
            femaleIndex.HairMesh = mesh;

            return null;
        }
        else
        {
            femaleIndex = new FemaleIndex();

            femaleIndex.Index = index;

            femaleIndex.CornMesh = mesh;

            femaleIndex.Radius = objectInfo.Radius;
            femaleIndex.Rotation = objectInfo.Rotation;

            femaleIndex.From = fromBranchIndex;

            return femaleIndex;
        }
    }

    private GameObject MeshInstantiate(Mesh mesh)
    {
        GameObject meshInstance = mesh.Instance;    //返回的模型（调用函数Instance）
        if (meshInstance == null) return null;

        GameObject meshModel = (GameObject)GameObject.Instantiate(meshInstance);   //模型实例化

        meshModel.name = mesh.Name;                             //名字
        meshModel.tag = "Organ";
        SetTagInParent(meshModel.transform, "Organ");

        return meshModel;
    }

    private void AddBranchIndex(BranchIndex index)
    {
        if (m_listBranchIndexes == null)
            m_listBranchIndexes = new List<BranchIndex>();

        index.TreeModel = this;

        m_listBranchIndexes.Add(index);
    }

    private void AddOrganIndex(OrganIndex index)
    {
        if (m_listOrganIndexes == null)
            m_listOrganIndexes = new List<OrganIndex>();

        index.TreeModel = this;

        m_listOrganIndexes.Add(index);
    }

    private void DataOfIndexInheritance(List<BranchIndex> curBranchIndexes, List<OrganIndex> curOrganIndexes, bool isSameGC = false)
    {
        /*
         * 不同GC的索引匹配
         */
        PairedBranchIndexes = IndexMatch.BranchIndexes(curBranchIndexes, BranchIndexes);
        PairedOrganIndexes = IndexMatch.OrganIndexes(curOrganIndexes, OrganIndexes);

        //继承枝干索引的数据
        foreach (PairedIndex<BranchIndex> pairedBranchIndex in PairedBranchIndexes)
        {
            pairedBranchIndex.DataInheritance(isSameGC);
        }
        
        //继承器官索引的数据
        foreach (PairedIndex<OrganIndex> pairedOrganIndex in PairedOrganIndexes)
        {
            pairedOrganIndex.DataInheritance(isSameGC);
        }
    }

    private List<Vector3> temp_Vertices = new List<Vector3>();
    private List<Vector2> temp_UV = new List<Vector2>();
    private List<int> temp_Triangles = new List<int>();

    private GameObject cylinderObject;

    private Vector3 RecordBranchModel(BranchIndex index, Vector3 bottom)
    {
        /*
         * 计算形态数据
         * 根据形态数据即可绘制对应的GameObject
         */
        index.MorphologicalSim();

        if (cylinderObject.activeSelf) cylinderObject.SetActive(false);

        cylinderObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        cylinderObject.transform.position = bottom + new Vector3(0, index.Length / 2f, 0) * SCALE;  //该节间的中心位置
        cylinderObject.transform.localScale = new Vector3(index.Radius / DEFAULT_RADIUS, index.Length / DEFAULT_HEIGHT, index.Radius / DEFAULT_RADIUS) * SCALE; //尺寸

        RotationGameObject(cylinderObject, bottom, index.Rotation);     //旋转该圆柱体

        Vector3[] vertices = GameObjectOperation.GetVerticesInWorld(cylinderObject);
        Vector2[] uv = GameObjectOperation.GetUV(cylinderObject);

        if (temp_Vertices.Count == 0)   //最底端的节间
        {
            for (int i = 0; i < 40; i++)
            {
                temp_Vertices.Add(vertices[i]);
                temp_UV.Add(uv[i]);
            }

            AddTriangles(0, 20);
        }
        else    //非最底端节间
        {
            for (int i = 0; i < 20; i++)
            {
                /*
                 * 添加顶点
                 * 由于底端的顶点与前驱节间的顶点相同
                 * 故只用添加后20个顶点即可
                 */
                temp_Vertices.Add(vertices[i + 20]);

                /*
                 * 添加UV坐标
                 * 需要颠倒纹理（与上一个节间的纹理相反）
                 * 确保连接处的纹理过渡不会突兀
                 */
                temp_UV.Add(temp_UV[index.Previous.TopVerticesIndex - 20 + i]);
            }

            AddTriangles(index.Previous.TopVerticesIndex, temp_Vertices.Count - 20);
        }

        //删除GameObject，防止重复
        //GameObject.Destroy(cylinderObject);

        /*
         * 补齐index的数据
         * 用于后续生成节间
         */
        index.BottomVerticesIndex = index.Previous.TopVerticesIndex == -1 ?
            0 : index.Previous.TopVerticesIndex;

        index.TopVerticesIndex = temp_Vertices.Count - 20;

        /*
         * 返回当前顶端的中心位置
         * 由于枝干旋转，故根据顶端的顶点位置计算
         */
        return GetCenterPoint(index.TopVerticesIndex, temp_Vertices);
    }

    private void CreateBranchModel()
    {
        /*
         * 更新枝干数据
         * 否则不会显示
         */
        GameObjectOperation.UpdateMeshInfo(BranchModel, temp_Vertices.ToArray(), temp_UV.ToArray(), temp_Triangles.ToArray());

        TempVariablesInit();

        /*
         * 养分
         */
        Material material = GameObjectOperation.GetMaterial(BranchModel);
        switch (EnvironmentParams.NutrientType)
        {
            case LightResponseType.N_LACK:
                //material.SetColor("_Color", MaizeParams.lackN_Color);
                break;
            case LightResponseType.P_LACK:
                material.SetColor("_Color", MaizeParams.lackP_Color);
                break;
            case LightResponseType.K_LACK:
                material.SetColor("_Color", MaizeParams.lackK_Color);
                break;
        }
    }

    private void TempVariablesInit()
    {
        temp_Vertices.Clear();
        temp_Vertices = new List<Vector3>();

        temp_UV.Clear();
        temp_UV = new List<Vector2>();

        temp_Triangles.Clear();
        temp_Triangles = new List<int>();
    }

    private void AddTriangles(int bottomIndex, int topIndex)
    {
        for (int i = 0; i < 20; i++)
        {
            temp_Triangles.Add(bottomIndex + (i + 1) % 20);
            temp_Triangles.Add(bottomIndex + i);
            temp_Triangles.Add(topIndex + i);

            temp_Triangles.Add(topIndex + i);
            temp_Triangles.Add(topIndex + (i + 1) % 20);
            temp_Triangles.Add(bottomIndex + (i + 1) % 20);
        }
    }

    /// <summary>
    /// 获取中心点位置
    /// </summary>
    private Vector3 GetCenterPoint(int index, List<Vector3> vertices)
    {
        /*
         * 当无顶点或顶点个数过少时
         * 中心默认为原点
         */
        if (vertices == null || vertices.Count == 0)
            return Vector3.zero;

        if (index < 0)
            return Vector3.zero;

        if (vertices.Count < index + 10)
            return Vector3.zero;

        return (vertices[index] + vertices[index + 10]) / 2.0f;
    }

    private void CreateLeafModel(LeafIndex index, Vector3 position)
    {
        /*
         * 计算形态数据：长度和最大宽度
         * 用于后续生成GameObject
         */
        index.MorphologicalSim();

        GameObject leafModel = (GameObject)GameObject.Instantiate(index.LeafMesh.Instance); //模型实例化

        leafModel.name = index.LeafMesh.Name;
        leafModel.tag = "Organ";
        SetTagInParent(leafModel.transform, "Organ");

        /*
         * 设置空间信息
         */
        leafModel.transform.position = position;
        RotationGameObject(leafModel, position, index.Rotation);

        /*
         * 养分
         */
        List<Material> materials = GameObjectOperation.GetMaterials(leafModel);
        switch (EnvironmentParams.NutrientType)
        {
            case LightResponseType.N_LACK:
                //SetIllnessColor(materials, MaizeParams.lackN_Color);
                break;
            case LightResponseType.P_LACK:
                SetIllnessColor(materials, MaizeParams.lackP_Color);
                break;
            case LightResponseType.K_LACK:
                SetIllnessColor(materials, MaizeParams.lackK_Color);
                break;
        }


        /*
         * 设置形态信息
         * 受病虫害影响后，叶面积变化较大
         * 而整体比例影响较小
         * 故采用未受到病虫害影响的叶面积计算比例
         */
        float scale = Mathf.Sqrt((float)(index.LeafArea_Uninsected / index.UniformLeafArea));
        leafModel.transform.localScale = Vector3.one * scale;

        /*
         * 设置父节点
         */
        leafModel.transform.SetParent(BranchModel.transform);

        /*
         * 信息补齐
         */
        index.Belong = leafModel;

        m_listOrganModels.Add(leafModel);
    }

    private void SetIllnessColor(List<Material> materials, Color color)
    {
        foreach (var material in materials)
        {
            if (material.shader.name != "Custom/Leaves") continue;

            material.SetFloat("_IsIllness", 1);
            material.SetColor("_IllnessColor", color);
        }
    }

    private void CreateFlowerModel(MaleIndex index, Vector3 position)
    {
        /*
         * 计算形态数据：长度
         * 用于后续生成GameObject
         */
        index.MorphologicalSim();

        /*
         * 创建空对象
         * 后续用于包含所有创建的flower模型
         */
        GameObject parentModel = new GameObject("flower");
        parentModel.tag = "Organ";
        parentModel.transform.position = position;

        //设置父节点
        parentModel.transform.SetParent(BranchModel.transform);

        GameObject flowerInternode = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flowerInternode.transform.position = position + new Vector3(0, 0.075f) / MaizeParams.SCALE;
        flowerInternode.transform.localScale = new Vector3(BranchIndexes[BranchIndexes.Count - 1].Radius / DEFAULT_RADIUS, 15 / DEFAULT_HEIGHT, BranchIndexes[BranchIndexes.Count - 1].Radius / DEFAULT_RADIUS) * SCALE; //尺寸
        flowerInternode.GetComponent<MeshRenderer>().material = BranchModel.GetComponent<MeshRenderer>().material;
        flowerInternode.transform.SetParent(parentModel.transform);

        /*
         * 获取模板
         * 后续用于实例化
         */
        GameObject instance = index.MaleMesh.Instance;

        /*
         * 实例化
         * 绘制以及求尺寸
         */
        GameObject flowerModel = GameObject.Instantiate(instance);

        /*
         * 获取该物体的包围盒
         * 根据包围盒的大小以及计算出的形态数据
         * 计算出绘制时的scale
         */
        Bounds modelBounds = Mesh.GetBoundsInParent(flowerModel);

        /*
         * 绘制直立的雌蕊
         */
        float scale = index.Length / (modelBounds.size.z * MaizeParams.SCALE);
        flowerModel.transform.localScale = Vector3.one * scale;
        flowerModel.transform.position = position + new Vector3(0, 0.15f, 0)/MaizeParams.SCALE;
        flowerModel.transform.parent = parentModel.transform;
        flowerModel.transform.Rotate(new Vector3(-90, 0, 0));

        /*
         * 绘制倾斜的6个雌蕊
         */
        for (int i = 0; i < 6; i++)
        {
            GameObject model = GameObject.Instantiate(instance);
            model.transform.localScale = Vector3.one * scale;
            model.transform.position = position + new Vector3(0, 0.15f, 0) / MaizeParams.SCALE;
            model.transform.parent = parentModel.transform;

            model.transform.Rotate(new Vector3(-35, i * 60, 0));
        }

        SetTagInParent(parentModel.transform, "Organ");

        /*
         * 信息补齐
         */
        index.Belong = parentModel;

        m_listOrganModels.Add(parentModel);
    }

    private void CreateFruitModel(FemaleIndex index, Vector3 position)
    {
        if (index.Biomass <= Mathf.Epsilon) return;

        /*
         * 计算形态数据：长度和最大宽度
         * 用于后续生成GameObject
         */
        index.MorphologicalSim(BranchIndexes, OrganIndexes);

        /*
         * 临时位置
         * 用于确定须的位置
         */
        Vector3 tempPosition = Vector3.zero;

        GameObject parentModel = new GameObject("Fruit");
        parentModel.transform.position = tempPosition;

        /*
         * 创建果实模型
         */
        tempPosition = CreateFruitSubModel(index.CornMesh, index.CornLength, tempPosition, parentModel);

        /*
         * 创建果实须模型
         */
        CreateFruitSubModel(index.HairMesh, index.HairLength, tempPosition, parentModel);


        parentModel.transform.position = position;
        RotationGameObject(parentModel, position, index.Rotation);

        parentModel.transform.SetParent(BranchModel.transform);
        SetTagInParent(parentModel.transform, "Organ");

        m_listOrganModels.Add(parentModel);

        index.Belong = parentModel;
    }

    private Vector3 CreateFruitSubModel(Mesh mesh, float length, Vector3 position, GameObject parentModel)
    {
        GameObject subModel = GameObject.Instantiate(mesh.Instance);
        Bounds bounds = Mesh.GetBoundsInParent(subModel);

        float scale = length / (bounds.size.z * MaizeParams.SCALE);

        if (scale == 0) scale = 0.01f;

        subModel.transform.localScale = Vector3.one * scale;
        subModel.transform.position = position;

        subModel.transform.SetParent(parentModel.transform);

        return position + Vector3.forward * length * 0.9f/ MaizeParams.SCALE;
    }

    private void InsectSimulation()
    {
        if (ComputeGrowthCycle() < 15) return;

        double intake = 0;

        //有病虫，则进行模拟
        if (EnvironmentParams.HaveInsects)
        {
            insectSim.NextDay(EnvironmentParams.DailyMeanTemperature, EnvironmentParams.DailyMeanRelativeHumidity);

            /*
             * 计算当日病虫的进食需求量
             * 其中 10000 为转换系数
             * 将cm^2转换成m^2
             */
            intake = insectSim.TotalIntakes() / 10000.0;
        }

        /*
         * 病虫从下至上开始啃食
         * 遍历每个叶片，模拟病虫害
         * 并计算剩余的进食需求量
         * 当需求量不大于0时，说明无进食需求
         * 病虫则不再继续进食
         */
        foreach (OrganIndex index in OrganIndexes)
        {
            if (index.Type != OrganType.Leaf) continue;

            intake = (index as LeafIndex).InsectSim(intake);
        }

    }

    private void RotationGameObject(GameObject _Object, Vector3 BottomPosition, Vector3 Rotation)
    {
        //按照Z轴 -> X轴 -> Y轴旋转
        _Object.transform.RotateAround(BottomPosition, Vector3.forward, Rotation.y);
        _Object.transform.RotateAround(BottomPosition, Vector3.right, Rotation.z);
        _Object.transform.RotateAround(BottomPosition, Vector3.up, Rotation.x);

        if (_Object.GetComponent<MeshFilter>() != null && _Object.GetComponent<MeshFilter>().name.Equals("Cylinder"))
            _Object.transform.Rotate(Vector3.up, -GetRotationInInspector(_Object.transform).y/*直接获取transform中的欧拉角度会存在万向锁问题*/);      //自身旋转，使两个模型之间的纹理的相对位置不变
    }

    /// <summary>
    /// 获取面板中的旋转角度（解决万向锁问题）
    /// 因反射的函数为Editor中的函数，打包后报错，因此舍弃
    /// 目前万向锁问题未解决（玉米模型暂不存在万向锁问题）
    /// </summary>
    private Vector3 GetRotationInInspector(Transform t)
    {
        ////参考代码：https://www.jianshu.com/p/59acdd1c9db8
        //var type = t.GetType();
        //var mi = type.GetMethod("GetLocalEulerAngles", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        //var rotationOrderPro = type.GetProperty("rotationOrder", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        //var rotationOrder = rotationOrderPro.GetValue(t, null);
        //var EulerAnglesInspector = mi.Invoke(t, new[] { rotationOrder });
        //return (Vector3)EulerAnglesInspector;
        return t.localEulerAngles;
    }

    /// <summary>
    /// 总叶面积(㎡)
    /// </summary>
    /// <returns></returns>
    public double ComputeLeafArea()
    {
        double totalLeafArea = 0.0;

        foreach (OrganIndex index in OrganIndexes)
        {
            if (index.Type != OrganType.Leaf) continue;

            totalLeafArea += (index as LeafIndex).LeafArea;
        }

        return totalLeafArea;
    }

    /// <summary>
    /// 占地面积(㎡)
    /// </summary>
    public double FloorArea()
    {
        if (TreeModelInstance.GetComponent<Collider>() == null)
            Mesh.AddBoxColliderInParent(TreeModelInstance);

        Bounds bounds = TreeModelInstance.GetComponent<Collider>().bounds;

        return bounds.size.x * bounds.size.z * MaizeParams.SCALE * MaizeParams.SCALE;
    }

    public void ComputeLAI()
    {
        LAI = ComputeLeafArea() / FloorArea();
    }

    public double ComputeHeight()
    {
        if (OrganModels == null || OrganModels.Count == 0) return 0;

        return OrganModels[OrganModels.Count - 1].transform.position.y * MaizeParams.SCALE;
    }

    public int ComputeGrowthCycle()
    {
        return CurrentStep - 1;
    }

    /// <summary>
    /// 累加积温
    /// </summary>
    public void TemperatureAccumulation(double temperature)
    {
        AccumulatedTemperature += (temperature - 8);
    }

    public void TemperatureAccumulation()
    {
        TemperatureAccumulation((EnvironmentParams.DailyMaxTemperature + EnvironmentParams.DailyMinTemperature) / 2.0);
    }
}
