using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour {

    public float speed = 5f;    // 移动速度
    private Camera mainCamera;      // 相机
	// Use this for initialization
	void Start () {
        mainCamera = GetComponentInChildren<Camera>();  //获取相机
        Adaptive = true;
	}


    private Vector3 point = new Vector3(0, 0, 0);   //参考点
    private bool LeftMouseDown = false;             //标识符，用于判断鼠标是否按下
    private bool WheelMouseDown = false;

    private bool isFocused = true;

    public bool Adaptive { get; set; }

	// Update is called once per frame
	void Update () 
    {
        IsFocused();

        LeftMouseEvent();
        WheelMouseEvent();
        ScrollWheelEvent();

        if (Adaptive)
            CameraAdaptive();
	}

    /// <summary>
    /// 判断鼠标是否在界面内
    /// </summary>
    private void IsFocused()
    {
        Vector2 vector = new Vector2(Input.mousePosition.x / ((float)Screen.width), Input.mousePosition.y / ((float)Screen.height));

        isFocused = mainCamera.rect.Contains(vector);
    }


    /// <summary>
    /// 鼠标左键触发的事件
    /// </summary>
    private void LeftMouseEvent()
    {
        if (!isFocused)
        {
            LeftMouseDown = false;
            return;
        }

        //判断鼠标是否在UI上操作
        if (EventSystem.current.IsPointerOverGameObject())
        {
            LeftMouseDown = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))            //鼠标左键按下，则标识符记录，如果松开，则标识符转变成false
            LeftMouseDown = true;
        else if (Input.GetMouseButtonUp(0))
            LeftMouseDown = false;

        if (LeftMouseDown)      //鼠标左键按下时，获取鼠标移动的位置（Z轴永远为0）
        {
            float fMouseX = Input.GetAxis("Mouse X");   //获取鼠标X轴的偏移距离
            float fMouseY = Input.GetAxis("Mouse Y");   //获取鼠标Y轴的偏移距离

            if (System.Math.Abs(fMouseX) >= System.Math.Abs(fMouseY))   //根据X、Y轴的偏移距离，适当调整偏移距离
                fMouseX = fMouseX * speed;
            else
            {
                fMouseX = fMouseX / speed;
                fMouseY = fMouseY * (speed - 2);
            }
            mainCamera.transform.RotateAround(point, Vector3.up, fMouseX);  //左右旋转

            //MeshFilter mf = Scence.GetInstance().TreeModel.TreeModelInstance.transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
            MeshFilter mf = null;
            if (mf == null) //不存在枝干，则旋转点为原点
                mainCamera.transform.RotateAround(point, mainCamera.transform.TransformVector(Vector3.left), fMouseY); //上下旋转
            else //存在枝干，则旋转点为枝干中心点
                mainCamera.transform.RotateAround(mf.mesh.bounds.center, mainCamera.transform.TransformVector(Vector3.left), fMouseY);
        }
    }

    /// <summary>
    /// 按下鼠标滚轮触发的事件
    /// </summary>
    private void WheelMouseEvent()
    {
        if (!isFocused) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(2))
            WheelMouseDown = true;
        else if (Input.GetMouseButtonUp(2))
            WheelMouseDown = false;

        if (WheelMouseDown)
        {
            float fMouseX = Input.GetAxis("Mouse X");   //获取鼠标X轴的偏移距离
            float fMouseY = Input.GetAxis("Mouse Y");   //获取鼠标Y轴的偏移距离

            Vector3 position = transform.position;



            position += transform.TransformVector(new Vector3(-fMouseX, 0, 0));
            position += transform.TransformVector(new Vector3(0, -fMouseY, 0));

            transform.position = position;
        }
    }

    /// <summary>
    /// 鼠标滚轮滚动触发的事件
    /// </summary>
    private void ScrollWheelEvent()
    {
        if (!isFocused) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (mainCamera.fieldOfView <= 100)
                mainCamera.fieldOfView += 2;
            if (mainCamera.orthographicSize <= 20)
                mainCamera.orthographicSize += 0.5f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (mainCamera.fieldOfView > 2)
                mainCamera.fieldOfView -= 2;
            if (mainCamera.orthographicSize >= 1)
                mainCamera.orthographicSize -= 0.5f;
        }
    }

    private bool isAdaptived = false;
    private Vector3 cameraPosOffset = Vector3.zero;
    private float cameraFOVOffset = 0.0f;

    /// <summary>
    /// 相机视点自适应
    /// </summary>
    private void CameraAdaptive()
    {
        if (!TreeAnimator.IsPlaying()) { isAdaptived = false; return; }
        if (isAdaptived) return;

        /*
         * 获取当前对象的视窗包围盒
         * 后续将根据该包围盒的位置自适应
         */
        Bounds bounds = ViewportBounds();

        if (IsSuitable(bounds)) return;

        /*
         * 相机位置与FOV自适应
         */
        PositionAdaptive(bounds);
        FOVAdaptive(bounds);

        isAdaptived = true;

        StartCoroutine(CameraMovingCoroutine());

        //GameObject.Find("Animator").GetComponent<AnimatorObject>().playing += CameraMoving;
    }

    private bool IsSuitable(Bounds viewportBounds)
    {
        return
            viewportBounds.center.x > 0.3f &&
            viewportBounds.center.x < 0.8f &&
            viewportBounds.center.y > 0.4f &&
            viewportBounds.center.y < 0.7f &&
            viewportBounds.max.y < 0.96f &&
            viewportBounds.size.y > 0.6f;
    }

    private void PositionAdaptive(Bounds bounds)
    {
        /*
         * 将屏幕中心坐标（viewport (0.5, 0.5)）转换成世界坐标
         * 与视窗包围盒中心在世界坐标系下的位置进行比较
         * 确定相机平移的方向和距离
         * 即将视窗包围盒中心移动到屏幕中心
         */
        Vector3 screenCenterPoint = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, bounds.center.z));

        cameraPosOffset = (mainCamera.ViewportToWorldPoint(bounds.center) - screenCenterPoint) / LScene.GetInstance().AnimationCount;
    }

    private void FOVAdaptive(Bounds viewportBounds)
    {
        /*
         * 根据视窗包围盒中占比最大的边与设定的值进行比较
         * 确定FOV的缩放比例
         * 计算缩放后的高度，并根据该高度反算出FOV
         * 公式参考：https://blog.csdn.net/i1tws/article/details/80257203
         */
        float viewport = 
            viewportBounds.size.y > viewportBounds.size.x ? viewportBounds.extents.y : viewportBounds.extents.x;
        float FOV_Adaptive = 
            Mathf.Atan(Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * (viewport / 0.45f)) * Mathf.Rad2Deg * 2;


        cameraFOVOffset = (FOV_Adaptive - mainCamera.fieldOfView) / LScene.GetInstance().AnimationCount;
    }

    private Bounds ViewportBounds()
    {
        /*
         * 获取当前植物的所有对象
         * 逐个遍历
         * 将对象的顶点坐标（世界坐标系下）转换成视窗坐标（viewport 以左下角为原点(0, 0)， 右上角为(1, 1)的坐标系）
         * 根据视窗坐标最大最小值生成包围盒
         * 用于确定当前对象在视窗中的位置，便于相机调整其自身位置
         * 使视点合适
         */
        List<GameObject> objects = new List<GameObject>();

        foreach (TreeModel treeModel in LScene.GetInstance().TreeModels)
        {
            objects.AddRange(GameObjectOperation.GetGameObjects(treeModel.TreeModelInstance));
        }

        Vector3 max = Vector3.zero, min = Vector3.zero;

        /*
         * 判断是否初始化 max 和 min
         */
        bool flag = true;

        foreach (GameObject _object in objects)
        {
            //无顶点
            if (!GameObjectValidate.HavaVertices(_object)) continue;

            Vector3[] vertices = GameObjectOperation.GetVerticesInWorld(_object);

            foreach (Vector3 vertex in vertices)
            {
                Vector3 viewport = mainCamera.WorldToViewportPoint(vertex);

                if (flag) { max = viewport; min = viewport; flag = false; continue; }
                else
                {
                    max = Vector3.Max(max, viewport);
                    min = Vector3.Min(min, viewport);
                }
            }
        }

        Bounds bounds = new Bounds((max + min) / 2.0f, max - min);

        return bounds;
    }

    private static bool isPlay = false;
    private int animationIndex = 1;
    private const int MAX_ANIMATION_INDEX = 40;

    private void CameraMoving()
    {
        if (animationIndex > MAX_ANIMATION_INDEX)
        {
            isPlay = false;

            GameObject.Find("Animator").GetComponent<AnimatorObject>().playing -= CameraMoving;
            animationIndex = 1;
        }
        else
        {
            isPlay = true;

            transform.position += cameraPosOffset;
            mainCamera.fieldOfView += cameraFOVOffset;

            animationIndex++;
        }
    }

    private IEnumerator CameraMovingCoroutine()
    {
        isPlay = true;

        for (int i = 0; i < LScene.GetInstance().AnimationCount; i++)
        {
            transform.position += cameraPosOffset;
            mainCamera.fieldOfView += cameraFOVOffset;

            yield return null;
        }

        isPlay = false;
    }

    public static bool IsPlay()
    {
        return isPlay;
    }
}
