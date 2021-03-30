using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using cakeslice;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(OutlineEffect))]
public class OutlineDetection : MonoBehaviour
{
    [Serializable]
    public class OutlineDetectionEvent : SerializableCallback<List<Vector2>>{}

    [SerializeField]
    private OutlineDetectionEvent onGetDetectionPos = new OutlineDetectionEvent();

    [SerializeField]
    private OutlineEffect effect;

    private void Start()
    {
        if (effect == null)
            effect = GetComponent<OutlineEffect>();
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        //占主导地位的颜色
        Color donimantColor = GetDominantColorInDectionPos();

        var controls = effect.controls;
        foreach(var control in controls)
        {
            control.detected = control.TexColor == donimantColor;
        }
        
    }

    private Color GetDominantColorInDectionPos()
    {
        Texture2D tex = effect.interactionTexture;

        //返回需要检测位置
        List<Vector2> dectionPoints = onGetDetectionPos.Invoke();

        /*
         * 键对存储检测位置中所有颜色的个数
         * 其中
         * 键：颜色
         * 值：颜色的个数
         */
        Dictionary<Color, int> maps = new Dictionary<Color, int>();
        

        foreach(var point in dectionPoints)
        {
            //无纹理或点超过纹理边界
            if (tex == null ||
                point.x < 0 || point.x > tex.width ||
                point.y < 0 || point.y > tex.height)
                continue;

            //获取像素的颜色
            Color pointColor = tex.GetPixel((int)point.x, (int)point.y);

            //像素无颜色，则说明该位置无内容
            if (pointColor == Color.clear) continue;
            
            //统计各个颜色出现的次数
            if (maps.ContainsKey(pointColor))
            {
                maps[pointColor]++;
            }
            else
            {
                maps.Add(pointColor, 1);
            }
        }

        if (maps.Count == 0) return Color.clear;
        else
        {
            //寻找个数最大的颜色
            Color dominantColor = Color.clear;
            int dominantColorCnt = -1;

            foreach(var item in maps)
            {
                if (item.Value > dominantColorCnt)
                    dominantColor = item.Key;
            }

            return dominantColor;
        }
    }

    public List<Vector2> GetDetectionPos()
    {
        List<Vector2> result = new List<Vector2>();

        var eventData = GetCurrentPointerEventData();
        if (eventData != null &&
            eventData.pointerDrag != null &&
            eventData.pointerDrag.GetComponent<ParamIconButton>() != null /*非属性交互按钮*/)
        {
            var rect = eventData.pointerDrag.GetComponent<RectTransform>();

            var width = rect.rect.width;
            var height = rect.rect.height;

            for (int i = (int)(rect.position.x - width / 2); i <= (int)(rect.position.x + width / 2); i++)
            {
                for (int j = (int)(rect.position.y - height / 2); j <= (int)(rect.position.y + height / 2); j++)
                {
                    result.Add(new Vector2(i, j));
                }
            }
        }
        else
        {
            result.Add(Input.mousePosition);
        }

        return result;
    }

    private PointerEventData GetCurrentPointerEventData()
    {
        /*
         * 使用反射机制
         * 读取当前的PointerEventData
         */
        var inputModule = EventSystem.current.currentInputModule as StandaloneInputModule;

        var type = inputModule.GetType();

        return type.GetField("m_InputPointerEvent", BindingFlags.Instance | BindingFlags.NonPublic).
            GetValue(inputModule) as PointerEventData;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(OutlineDetection))]
public class OutlineDetectionEditor : Editor
{
    SerializedProperty m_OnGetDetectionPos;

    private void OnEnable()
    {
        m_OnGetDetectionPos = serializedObject.FindProperty("onGetDetectionPos");
    }
}
#endif