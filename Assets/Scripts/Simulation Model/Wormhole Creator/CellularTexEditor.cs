/*
 * 细胞纹理编辑器
 * 仅在编辑模式下使用
 */
#if UNITY_EDITOR

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using System;

public class CellularTexEditor : EditorWindow
{
    private Vector2 ScrollPos;

    [MenuItem("Window/L-System/Cellular Texture Editor")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CellularTexEditor), false, "Celluar Texture Editor");
    }

    void OnGUI()
    {
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

        AddCreateButton();
        AddWormholeCreateButton();
        AddTestButton();

        EditorGUILayout.EndScrollView();
    }

    int width = 256;
    int height = 1536;

    int centerPointsCount = 10;
    int pointsPerGroupCount = 10;
    float radius = 15f;

    Texture2D mainTex = null;
    Texture2D barrierTex = null;
    Texture2D celluarTex = null;

    GameObject celluarTexDisplayer = null;

    void AddCreateButton()
    {
        GUILayout.BeginVertical("Box");
        GUILayout.Label("Create Celluar Texture");

        /*
         * 单元纹理的宽度和长度
         */
        width = EditorGUILayout.IntField("Texture Width", width);
        height = EditorGUILayout.IntField("Texture height", height);

        /*
         * 中心点个数、以中心点为圆心，半径为radius，随机布点的个数
         * 像素根据与这些点的距离绘制颜色
         */
        centerPointsCount = EditorGUILayout.IntField("Points Count", centerPointsCount);
        pointsPerGroupCount = EditorGUILayout.IntField("Count of points per group", pointsPerGroupCount);
        radius = EditorGUILayout.FloatField("Radius", radius);

        /*
         * 叶片纹理和障碍物纹理
         * 用于辅助生成合适的细胞纹理
         */
        mainTex = EditorGUILayout.ObjectField("Reference Texture", mainTex, typeof(Texture2D), true) as Texture2D;
        barrierTex = EditorGUILayout.ObjectField("Barrier Texture", barrierTex, typeof(Texture2D), true) as Texture2D;

        /*
         * 细胞纹理展示的GameObject
         * 在细胞纹理生成后用于可视化
         */
        celluarTexDisplayer = EditorGUILayout.ObjectField("Displayer", celluarTexDisplayer, typeof(GameObject), true) as GameObject;

        if (GUILayout.Button("Create"))
        {
            celluarTex = CellularTexture.Create(width, height, centerPointsCount, pointsPerGroupCount, radius, mainTex, barrierTex);

            SaveCellularTex(celluarTex);
            //SaveCellularTex(CellularTexture.Create(width, height, centerPointsCount, pointsPerGroupCount, radius, true, mainTex, barrierTex),
            //    "\\Assets\\Resources\\Textures\\texture_temp.png");
            

            if (celluarTexDisplayer != null)
                celluarTexDisplayer.GetComponent<MeshRenderer>().material.mainTexture = celluarTex;
        }

        GUILayout.EndVertical();
    }

    private bool isRealTime = false;
    private float remainingLeafArea_Percentage = 100f;
    private GameObject wormholeDisplayer = null;

    private CellularTexture celluarTex_test = null;

    void AddWormholeCreateButton()
    {
        GUILayout.BeginVertical("Box");
        GUILayout.Label("Create Wormhole");

        /*
         * 是否实时创建虫洞
         */
        isRealTime = EditorGUILayout.Toggle("Real Time", isRealTime);

        /*
         * 确定细胞纹理和主纹理
         * 后续创造虫洞会覆盖GameObject的纹理
         * 因此需要存储主纹理
         */
        celluarTex = EditorGUILayout.ObjectField("Celluar Texture", celluarTex, typeof(Texture2D), true) as Texture2D;
        mainTex = EditorGUILayout.ObjectField("Main Texture", mainTex, typeof(Texture2D), true) as Texture2D;

        /*
         * 叶片面积保留百分比
         * 根据该百分比计算虫洞大小
         */
        remainingLeafArea_Percentage = EditorGUILayout.FloatField("Percentage", remainingLeafArea_Percentage);

        /*
         * 展示虫洞的GameObject
         * 在虫洞纹理生成后可视化
         */
        wormholeDisplayer = EditorGUILayout.ObjectField("Displayer", wormholeDisplayer, typeof(GameObject), true) as GameObject;

        if ((GUILayout.Button("Create") || isRealTime) && (celluarTex != null && mainTex != null))
        {
            //Texture2D tex =
            //    CellularTexture.CreateWormhole(remainingLeafArea_Percentage, 215489,
            //                                   System.Environment.CurrentDirectory + "\\Assets\\Resources\\Textures\\texture.tmp",
            //                                   mainTex, celluarTex);

            if (celluarTex_test == null)
                celluarTex_test = new CellularTexture(System.Environment.CurrentDirectory + "\\Assets\\Resources\\Textures\\texture.png",215489);

            Texture2D tex = celluarTex_test.CreateWormhole(remainingLeafArea_Percentage / 100.0, mainTex) as Texture2D;

            if (wormholeDisplayer != null)
            {
                Texture2D tempTex = wormholeDisplayer.GetComponent<MeshRenderer>().material.mainTexture as Texture2D;

                /*
                 * 清除纹理
                 * 及时释放内存
                 */
                GameObject.DestroyImmediate(tempTex);

                wormholeDisplayer.GetComponent<MeshRenderer>().material.mainTexture = tex;
            }

        }

        if (GUILayout.Button("Output"))
        {
            Texture2D tex = wormholeDisplayer.GetComponent<MeshRenderer>().material.mainTexture as Texture2D;

            if (tex != null)
                SaveWormholeTex(tex);
        }

        GUILayout.EndVertical();
    }

    void SaveCellularTex(Texture2D tex, string path = "\\Assets\\Resources\\Textures\\texture.png")
    {
        FileStream fs = new FileStream(System.Environment.CurrentDirectory + path, FileMode.Create);
        byte[] bytes = tex.EncodeToPNG();
        fs.Write(bytes, 0, bytes.Length);
        fs.Close();

        CellularTexture.SaveAccumulationCountFile(System.Environment.CurrentDirectory + "\\Assets\\Resources\\Textures\\texture.tmp", tex);
    }

    void SaveWormholeTex(Texture2D tex)
    {
        FileStream fs = new FileStream(System.Environment.CurrentDirectory + "\\Assets\\Resources\\Textures\\wormhole.png", FileMode.Create);
        byte[] bytes = tex.EncodeToPNG();
        fs.Write(bytes, 0, bytes.Length);
        fs.Close();
    }

    private List<KeyValuePair<int, int>> accumulationList = null;

    void AddTestButton()
    {
        if (GUILayout.Button("Test"))
        {
            Debug.Log(Path.GetDirectoryName(System.Environment.CurrentDirectory + "\\Assets\\Resources\\Textures\\wormhole.png"));
        }

    }


}

#endif