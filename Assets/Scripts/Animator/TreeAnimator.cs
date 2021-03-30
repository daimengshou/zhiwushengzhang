using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WormholeFragment
{
    public int InitThreshold;
    public double ThresholdDetla;

    public WormholeFragment(int init, double detla)
    {
        InitThreshold = init;
        ThresholdDetla = detla;
    }
}

public class TreeAnimator
{
    private List<GameObject> m_PreGameObjects;      //第一个关键帧包括的物体
    private List<GameObject> m_CurGameObjects;      //第二个关键帧包括的物体

    private int m_AnimationIndex;                       //动画播放索引
    private int m_AnimationMaxIndex;                    //动画播放最大索引
    private List<List<Vector3>> m_AnimationFragment;    //动画片段
    private List<WormholeFragment> m_WormholeFragment;  //用于模拟虫洞变化的动画片段
    private static bool isPlaying = false;                                //是否播放动画
    private static int playingCount = 0;                //正在播放动画的个数

    private List<PairedIndex<BranchIndex>> m_PairedBranchIndexes;
    private List<PairedIndex<OrganIndex>> m_PairedOrganIndexes;



    public void PlayAnimation(List<PairedIndex<BranchIndex>> pairedBranchIndexes, 
                              List<PairedIndex<OrganIndex>> pairedOrganIndexes,
                              int animationMaxIndex = 100)
    {
        /*
         * 初始化各变量
         * 便于后续调用
         */
        m_PairedBranchIndexes = pairedBranchIndexes;
        m_PairedOrganIndexes = pairedOrganIndexes;

        m_AnimationIndex = 1;
        m_AnimationMaxIndex = animationMaxIndex;

        /*
         * 计算动画片段
         * 后续将根据该动画片段计算顶点的移动
         */
        ComputeAnimationFragment();

        /*
         * 隐藏第二帧的渲染内容
         * 保证呈现的画面正确
         */
        foreach (GameObject curGameObject in m_CurGameObjects)
        {
            curGameObject.SetActive(false);
        }

        playingCount++;

        /*
         * 委托函数
         * 用于每一帧调用对应的函数
         * 实现动画效果
         */
        GameObject.Find("Animator").GetComponent<AnimatorObject>().playing += PlayingAnimation;
        
    }

    private void PlayingAnimation()
    {
        UpdateGameObjects();

        if (m_AnimationIndex == m_AnimationMaxIndex)    //最后一帧
        {
            /*
             * 清除刷新动画函数的注册
             * 防止数据异常
             */
            GameObject.Find("Animator").GetComponent<AnimatorObject>().playing -= PlayingAnimation;

            /*
             * 清除前一关键帧的对象
             * 防止场景中出现多个对象
             */
            ClearPreGameObjects();

            playingCount--;
            //isPlaying = false;
        }
        else
        {
            m_AnimationIndex++;
        }
    }

    private void UpdateGameObjects()
    {
        for (int i = 0; i < m_PreGameObjects.Count; i++)
        {
            UpdateGameObject(m_PreGameObjects[i], m_CurGameObjects[i], m_AnimationFragment[i], m_WormholeFragment[i]);
        }
    }

    /// <summary>
    /// 更新该帧内的对象
    /// </summary>
    private void UpdateGameObject(GameObject preGameObject, GameObject curGameObject, List<Vector3> veticesFragment, WormholeFragment wormholeFragment)
    {
        if (m_AnimationIndex < m_AnimationMaxIndex)
        {
            /*
             * 若该片段为NULL或该片段的个数为0
             * 则说明该对象无顶点
             * 不继续进行计算
             */
            if (!GameObjectValidate.HavaVertices(preGameObject) || veticesFragment == null) return; //09-05

            //09-05
            if (veticesFragment.Count != 0)
            {
                Vector3[] vertices = GameObjectOperation.GetVertices(preGameObject);

                /*
                 * 根据片段中记录的顶点偏移数据
                 * 计算对象在该帧移动后的位置
                 * 其中该片段记录的是世界坐标系下的数据
                 * 因此要转换成对象所在的局部坐标系下
                 */
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] += veticesFragment[i];
                }

                /*
                 * 更新该对象
                 * 防止渲染出现异常
                 */
                GameObjectOperation.UpdateMeshInfo(preGameObject, vertices);
            }

            /*
             * 虫洞逐帧变化模拟
             * 当阈值增量为0，说明不变化
             * 当阈值增量不为0
             * 开启虫洞模拟 （_SimWormhole的值为1），并赋予相应的阈值
             */
            if (wormholeFragment.ThresholdDetla == 0) return;

            Material material = GameObjectOperation.GetMaterial(preGameObject);
            if (material == null || material.shader.name != "Custom/Leaves") return;

            material.SetFloat("_SimWormhole", 1);
            material.SetColor("_Threshold", 
                CellularTexture.DEC2Color(wormholeFragment.InitThreshold + (int)(wormholeFragment.ThresholdDetla * m_AnimationIndex))
                );
        }
        else //最后一帧
        {
            /*
             * 无需计算
             * 只需设置前一关键帧的对象不可见
             * 后一帧对象可见即可
             */
            preGameObject.SetActive(false);
            curGameObject.SetActive(true);

            GameObjectOperation.ClearMaterials(preGameObject);
            GameObjectOperation.ClearMeshes(preGameObject);

            GameObject.Destroy(preGameObject);
        }
    }

    private void AdjustTree()
    {
        AdjustBranch();
        AdjustOrgan();
    }

    /// <summary>
    /// 调整前后枝干的顶点
    /// </summary>
    private void AdjustBranch()
    {
        if (m_PairedBranchIndexes.Count == 1 && m_PairedBranchIndexes[0].PreIndex == null)
        {
            AdjustFreshBranch();
            return;
        }

        /*
         * 获取前后两个关键帧的枝干对象
         * 用于后续获取顶点以及更改顶点
         */
        GameObject preBranch = m_PairedBranchIndexes[0].PreIndex.Belong;
        GameObject curBranch = m_PairedBranchIndexes[0].CurIndex.Belong;

        Vector3[] preVertices = GameObjectOperation.GetVertices(preBranch);
        Vector3[] vertices = new Vector3[GameObjectOperation.GetVertices(curBranch).Length];    //该顶点数组用于存放修改后的前关键帧顶点

        /*
         * 遍历已经匹配好的Index
         * 根据PairedIndex中记录的前后Index的数据
         * 对前后关键帧中枝干的数据进行修改
         */
        foreach (PairedIndex<BranchIndex> pairedBranchIndex in m_PairedBranchIndexes)
        {
            BranchIndex preBranchIndex = pairedBranchIndex.PreIndex;
            BranchIndex curBranchIndex = pairedBranchIndex.CurIndex;

            if (curBranchIndex.IsFirstBranch() && preBranchIndex.IsFirstBranch())   //均为第一个枝干
            {
                CopyVertices(preVertices, preBranchIndex.BottomVerticesIndex, vertices, curBranchIndex.BottomVerticesIndex, 40);
            }
            else if (preBranchIndex != null) //若存在匹配的第一个关键帧的枝干索引，则将第一个关键帧枝干索引指向的顶点插入到第二个关键帧枝干索引指向的顶点位置中
            {
                CopyVertices(preVertices, preBranchIndex.TopVerticesIndex, vertices, curBranchIndex.TopVerticesIndex);
            }
            else//若第二个关键帧枝干不存在匹配的第一个关键帧枝干索引，则遍历其前驱，直至有存在的第一个关键帧枝干索引，并将该枝干索引指向的顶点插入到第二个关键帧枝干索引指向的顶点位置中
            {
                CopyVertices(vertices, curBranchIndex.Previous.TopVerticesIndex, vertices, curBranchIndex.TopVerticesIndex);
            }
        }

        //更新对象
        GameObjectOperation.UpdateMeshInfo(preBranch, vertices, GameObjectOperation.GetUV(curBranch), GameObjectOperation.GetTriangleIndexes(curBranch));

        AddPreObject(preBranch);
        AddCurObject(curBranch);
    }

    private void AdjustFreshBranch()
    {
        GameObject curBranch = m_PairedBranchIndexes[0].CurIndex.Belong;
        GameObject preBranch = GameObject.Instantiate(curBranch);

        /*
         * 摧毁所有的子对象
         * 防止重复
         */
        GameObjectOperation.DestroyAllChildren(preBranch);

        Vector3[] vertices = GameObjectOperation.GetVertices(curBranch);
        
        //CopyVertices(curBranch.transform.InverseTransformPoint(Vector3.zero), vertices);
        CopyVertices(TreeModel.GetBranchBottomCenter(curBranch, m_PairedBranchIndexes[0].CurIndex), vertices);

        GameObjectOperation.UpdateMeshInfo(preBranch, vertices);


        AddPreObject(preBranch);
        AddCurObject(curBranch);
    }

    private void AdjustOrgan()
    {
        foreach (PairedIndex<OrganIndex> pairedOrganIndex in m_PairedOrganIndexes)
        {
            if (pairedOrganIndex.PreIndex != null)  //非新生器官
            {
                AddPreObjects(pairedOrganIndex.PreIndex.Belong);
                AddCurObjects(pairedOrganIndex.CurIndex.Belong);
            }
            else    //新生器官
            {
                AddPreObjects(GetNewGameObject(pairedOrganIndex));
                AddCurObjects(pairedOrganIndex.CurIndex.Belong);
            }
        }
    }

    private void AddPreObject(GameObject _object)
    {
        if (m_PreGameObjects == null)
            m_PreGameObjects = new List<GameObject>();

        m_PreGameObjects.Add(_object);
    }

    private void AddCurObject(GameObject _object)
    {
        if (m_CurGameObjects == null)
            m_CurGameObjects = new List<GameObject>();

        m_CurGameObjects.Add(_object);
    }

    private void AddPreObjects(GameObject _object)
    {
        foreach (GameObject _subObject in GameObjectOperation.GetGameObjects(_object))
        {
            AddPreObject(_subObject);
        }
    }

    private void AddCurObjects(GameObject _object)
    {
        foreach (GameObject _subObject in GameObjectOperation.GetGameObjects(_object))
        {
            AddCurObject(_subObject);
        }
    }

    /// <summary>
    /// 复制顶点
    /// </summary>
    private void CopyVertices(Vector3[] srcVertices, int srcStartIndex, Vector3[] destVertices, int destStartIndex, int length = 20)
    {
        for (int i = 0; i < length; i++)
        {
            destVertices[destStartIndex + i] = srcVertices[srcStartIndex + i];
        }
    }

    private void CopyVertices(Vector3 srcVertex, Vector3[] destVertices, int destStartIndex = 0, int length = -1)
    {
        length = length == -1 && destStartIndex == 0 ? destVertices.Length : length;

        for (int i = 0; i < length; i++)
        {
            destVertices[destStartIndex + i] = srcVertex;
        }
    }

    /// <summary>
    /// 从已经匹配好的枝干索引中寻找匹配的前关键帧枝干索引
    /// </summary>
    private BranchIndex FindPairedPreviousBranchIndex(BranchIndex curBranchIndex)
    {
        return m_PairedBranchIndexes.Find(branchIndex => branchIndex.CurIndex == curBranchIndex).PreIndex;
    }

    /// <summary>
    /// 获取新生对象
    /// </summary>
    /// <returns></returns>
    private GameObject GetNewGameObject(PairedIndex<OrganIndex> pairedOrganIndex)
    {
        GameObject cloneObject = GameObject.Instantiate(pairedOrganIndex.CurIndex.Belong);
        
        /*
         * 因枝干顶点已经调整完毕
         * 故可用后一关键帧中枝干所指的顶点索引确定前一关键帧顶点的位置
         */
        Vector3 position = TreeModel.GetBranchTopCenter(m_PreGameObjects[0], pairedOrganIndex.CurIndex.From);

        foreach (GameObject _subObject in GameObjectOperation.GetGameObjects(cloneObject))
        {
            if (!GameObjectValidate.HavaVertices(_subObject)) continue;

            Vector3[] vertices = GameObjectOperation.GetVertices(_subObject);

            //该对象所有顶点均为该位置
            CopyVertices(_subObject.transform.InverseTransformPoint(position), vertices);
            GameObjectOperation.UpdateMeshInfo(_subObject, vertices);
        }

        //设置阈值为0
        GameObjectOperation.SetThreshold(cloneObject, 0);

        return cloneObject;
    }

    private void ComputeAnimationFragment()
    {
        /*
         * 初始化动画片段
         * 防止数据重复
         */
        InitialAnimationFragment();
        InitialWormholeFragment();

        /*
         * 调整前后两关键帧顶点
         * 保证前后两关键帧顶点个数相同
         * 方便后续计算顶点的移动
         */
        AdjustTree();

        for (int i = 0; i < m_PreGameObjects.Count; i++)
        {
            /*
             * 检验两个对象的Tag
             * 保证两个对象的Tag一致
             */
            GameObjectValidate.ValidateTagOfObject(m_PreGameObjects[i], m_CurGameObjects[i]);

            /*
             * 判断该组对象是否有顶点
             * 因调整后，前后两对象顶点个数相同
             * 故判断前一个对象是否有顶点即可
             */
            if (!GameObjectValidate.HavaVertices(m_PreGameObjects[i]))
            {
                m_AnimationFragment.Add(new List<Vector3>());
                m_WormholeFragment.Add(new WormholeFragment());
                continue;
            }

            //ComputeAnimationFragment(GameObjectOperation.GetVerticesInWorld(m_PreGameObjects[i]), GameObjectOperation.GetVerticesInWorld(m_CurGameObjects[i]));
            ComputeAnimationFragment(m_PreGameObjects[i], GameObjectOperation.GetVertices(m_PreGameObjects[i]), GameObjectOperation.GetVerticesInWorld(m_CurGameObjects[i]));
            ComputeWormholeFragment(m_PreGameObjects[i], m_CurGameObjects[i]);
        }
    }

    private void ComputeAnimationFragment(Vector3[] preVertices_World, Vector3[] curVertices_World)
    {
        //记录该物体每一帧每一个顶点移动的距离
        List<Vector3> animationFragment = new List<Vector3>();

        for (int i = 0; i < preVertices_World.Length; i++ )
        {
            animationFragment.Add((curVertices_World[i] - preVertices_World[i]) / m_AnimationMaxIndex);
        }

        m_AnimationFragment.Add(animationFragment);
    }

    private void ComputeAnimationFragment(GameObject preObject, Vector3[] preVertices, Vector3[] curVertices_World)
    {
        //判断时候发生改变
        bool isChanged = false;

        //记录该物体每一帧每一个顶点移动的距离
        List<Vector3> animationFragment = new List<Vector3>();

        for (int i = 0; i < preVertices.Length; i++)
        {
            Vector3 vertexDetla = preObject.transform.InverseTransformPoint(curVertices_World[i]) - preVertices[i];

            if (!isChanged && 
                Mathf.Approximately(vertexDetla.x, 0) && Mathf.Approximately(vertexDetla.y, 0) && Mathf.Approximately(vertexDetla.z, 0))
            {
                animationFragment.Add(Vector3.zero);
            }
            else
            {
                animationFragment.Add(vertexDetla / m_AnimationMaxIndex);
                isChanged = true;
            }
        }

        if (isChanged)
            m_AnimationFragment.Add(animationFragment);
        else
            m_AnimationFragment.Add(new List<Vector3>());
    }

    private void ComputeWormholeFragment(GameObject preGameObject, GameObject curGameObject)
    {
        /*
         * 从材质中的shader获取其阈值（十进制）
         * 当后一个关键帧的阈值为0时，说明无虫害变化
         * 当后一个关键帧的阈值不为0时，根据阈值变化确定每一帧的阈值的变化
         */
        Material preMaterial = GameObjectOperation.GetMaterial(preGameObject);
        Material curMaterial = GameObjectOperation.GetMaterial(curGameObject);

        if (preMaterial == null || preMaterial.shader.name != "Custom/Leaves")
        {
            m_WormholeFragment.Add(new WormholeFragment());
            return;
        }

        int preThreshold = CellularTexture.Color2DEC(preMaterial.GetColor("_Threshold"));
        int curThreshold = CellularTexture.Color2DEC(curMaterial.GetColor("_Threshold"));

        if (curThreshold == 0)
            m_WormholeFragment.Add(new WormholeFragment());
        else
            m_WormholeFragment.Add(new WormholeFragment(preThreshold, (curThreshold - preThreshold) * 1.0 / m_AnimationMaxIndex));
    }

    private void InitialAnimationFragment()
    {
        if (m_AnimationFragment == null)
            m_AnimationFragment = new List<List<Vector3>>();
        else
            m_AnimationFragment.Clear();
    }

    private void InitialWormholeFragment()
    {
        if (m_WormholeFragment == null)
            m_WormholeFragment = new List<WormholeFragment>();
        else
            m_WormholeFragment.Clear();
    }

    private void ClearPreGameObjects()
    {
        if (m_PreGameObjects == null || m_PreGameObjects.Count == 0) return;

        Transform _object = m_PreGameObjects[0].transform;

        while (true)
        {
            if (_object.parent == null || _object.tag == "Tree")
            {
                GameObject.Destroy(_object.gameObject);
                break;
            }
            else
            {
                _object = _object.parent;
            }
        }
    }

    public static bool IsPlaying()
    {
        //return isPlaying;
        return playingCount != 0;
    }
}