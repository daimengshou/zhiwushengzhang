using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using cakeslice;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum MousePositionType
{
    Unknow, Enter, Exit
}

public class OutlineControl: MonoBehaviour
{
    [Serializable]
    public class OutlineControlEvent : UnityEvent { }

    [SerializeField]
    int DefaultColorID = 0;

    public BaseTree BaseTree;

    List<Outline> m_outlines = new List<Outline>();
    MousePositionType positionType = MousePositionType.Unknow;      //记录当前鼠标位置的类型，在对象内部or对象外部

    [SerializeField]
    private OutlineControlEvent onMouseDown = new OutlineControlEvent();
    [SerializeField]
    private OutlineControlEvent onMouseEnter = new OutlineControlEvent();
    [SerializeField]
    private OutlineControlEvent onMouseExit = new OutlineControlEvent();
    [SerializeField]
    private OutlineControlEvent onMouseStay = new OutlineControlEvent();

    public Color TexColor = Color.white;
    public Color pointerColor;

    private bool highLight = false;

    /*
     * 用于确定该部分是否被发现
     */
    public bool detected = false;
    private void Start()
    {
    }

    void Update()
    {
        //MouseDection();

        //if (!detected)
        //    MouseExit();
        //else if (Input.GetMouseButtonDown(0))
        //    MouseDown();
        //else
        //    MouseEnter();

        //if (BaseTree.Selected && !highLight)
        //    HighLight();
        //else if (!BaseTree.Selected && highLight)
        //    Unhighlight();
    }

    private void LateUpdate()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (!detected)
                MouseExit();
            else if (Input.GetMouseButtonDown(0))
                MouseDown();
            else
                MouseEnter();
        }


        if (BaseTree.Selected && !highLight)
            HighLight();
        else if (!BaseTree.Selected && highLight)
            Unhighlight();
    }

    void OnEnable()
    {
        IEnumerable<OutlineEffect> effects = Camera.allCameras.AsEnumerable()
            .Select(c => c.GetComponent<OutlineEffect>())
            .Where(e => e != null);

        foreach (OutlineEffect effect in effects)
        {
            effect.AddControl(this);
        }
    }

    void OnDisable()
    {
        IEnumerable<OutlineEffect> effects = Camera.allCameras.AsEnumerable()
            .Select(c => c.GetComponent<OutlineEffect>())
            .Where(e => e != null);

        foreach (OutlineEffect effect in effects)
        {
            effect.RemoveControl(this);
        }
    }

    /// <summary>
    /// 记录该GameObject在离屏渲染的纹理中的颜色
    /// 由于指针移动，该颜色可能会发生改变
    /// 因此需要在生成离屏渲染纹理的时候记录
    /// </summary>
    public void RecordColor()
    {
        //if (m_outlines == null || m_outlines.Count == 0) TexColor = Color.red;
        //else
        //    switch (m_outlines[0].color)
        //    {
        //        case 0: TexColor = Color.red; break;
        //        case 1: TexColor = Color.green; break;
        //        case 2:
        //        default: TexColor = Color.blue; break;
        //    }
        if (m_outlines == null || m_outlines.Count == 0)
            TexColor = OutlineEffect.Instance.materialColors[0];
        else
            TexColor = OutlineEffect.Instance.materialColors[m_outlines[0].color];

        /*
         * 读取的颜色均为不透明
         * 因此需要将记录的颜色都设置成不透明
         */
        TexColor.a = 1.0f;
    }

    void MouseDection()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        /*
         * 获取指针位置
         * 用于读取对应纹理中的颜色
         */
        Vector2 mousePosition = Input.mousePosition;

        int x = (int)mousePosition.x;
        int y = (int)mousePosition.y;

        /*
         * 获取离屏渲染的纹理
         * 便于确定指针指向位置的颜色
         */
        Texture2D tex = OutlineEffect.Instance.interactionTexture;

        if (tex != null &&
            x >= 0 && x < tex.width &&
            y >= 0 && y < tex.height)
        {
            pointerColor = tex.GetPixel(x, y);

            if (pointerColor != TexColor)
                MouseExit();
            else if (Input.GetMouseButtonDown(0))
                MouseDown();
            else
                MouseEnter();
        }
    }

    public void OnDetected()
    {

    }

    float time = 0.0f;
    const float TIME_DURATION = 0.5f;

    void MouseEnter()
    {
        //指针类型不为进入，则出发进入事件
        if (positionType != MousePositionType.Enter)
            onMouseEnter.Invoke();

        BaseTree.Selected = true;

        time += Time.deltaTime;
        if (time > TIME_DURATION)
        {
            onMouseStay.Invoke();
        }

        positionType = MousePositionType.Enter;
    }

    void MouseExit()
    {
        //Unhighlight();
        BaseTree.Selected = false;

        if (positionType == MousePositionType.Enter)
        {
            //从物体内部到物体外部
            time = 0.0f;

            onMouseExit.Invoke();
        }

        positionType = MousePositionType.Exit;
    }

    void MouseDown()
    {
        onMouseDown.Invoke();
    }

    private void HighLight()
    {
        foreach (Outline outline in m_outlines)
            outline.Highlight();

        highLight = true;
    }

    private void Unhighlight()
    {
        foreach (Outline outline in m_outlines)
            outline.Unhighlight();

        highLight = false;
    }

    public void UpdateOutlines()
    {
        List<GameObject> objects = GameObjectOperation.GetGameObjects(gameObject);

        foreach (GameObject go in objects)
        {
            if (go.GetComponent<Renderer>() == null) continue;

            Outline outline = go.GetComponent<Outline>();
            if (outline == null)
            {
                outline = go.AddComponent<Outline>();
                m_outlines.Add(outline);
            }

            outline.defaultColor = DefaultColorID;
            outline.Unhighlight();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(OutlineControl))]
[CanEditMultipleObjects]
public class OutlineControlEditor : Editor
{
    SerializedProperty m_OnMouseDown;
    SerializedProperty m_OnMouseEnter;
    SerializedProperty m_OnMouseExit;
    SerializedProperty m_OnMouseStay;

    void OnEnable()
    {
        m_OnMouseDown = serializedObject.FindProperty("onMouseDown");
        m_OnMouseEnter = serializedObject.FindProperty("onMouseEnter");
        m_OnMouseExit = serializedObject.FindProperty("onMouseExit");
        m_OnMouseStay = serializedObject.FindProperty("onMouseStay");
    }
}
#endif
