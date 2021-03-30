/*
 * XML文件的编辑器
 * 仅在编辑模式下使用
 */
#if UNITY_EDITOR

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class XMLEditor : EditorWindow
{
    private Vector2 ScrollPos;

    [MenuItem("Window/XML Editor")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(XMLEditor), false, "XML Editor");
    }

    void OnGUI()
    {
        if (!EditorApplication.isPlaying)   //仅在游戏运行时使用
        {
            if (!isEmpty) Init();
            return;
        }

        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);     //滚动条

        AddModelEditorButton(LScene.GetInstance().TreeModels[0]);
        AddGlobalVariablesButton();
        AddMeshesButton();
        AddApplyButton();

        EditorGUILayout.EndScrollView();
    }

    private bool isEmpty = true;

    private string Name = "";     //名字
    private int InitStep = 0;      //初始生命周期
    private int MaxStep = 0;       //最大生命周期

    private string LRule = "";                        //规则
    private string BranchTexturePath = "";            //枝干纹理路径

    private List<VariableEntry> GlobalVariables = null; //全局变量
    private List<Mesh> Meshes = null;                   //模型

    void AddModelEditorButton(TreeModel treeModel)
    {
        if (isEmpty)
            LoadModelInfo(treeModel);

        ////如读取数据后还为空，说明无数据，则不显示任何内容
        //if (isEmpty) return;

        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("Rule");
        LRule = EditorGUILayout.TextArea(LRule);
        GUILayout.EndVertical();

        Name = EditorGUILayout.TextField("Name", Name);

        MaxStep = EditorGUILayout.IntField("Max Step", MaxStep);
        InitStep = EditorGUILayout.IntSlider("Init Step", InitStep, 1, MaxStep);

        BranchTexturePath = AddOpenPathButton("Branch Texture Path", BranchTexturePath);
    }

    void AddGlobalVariablesButton()
    {
        GUILayout.BeginVertical("Box");
        GUILayout.Label("Global Variables");

        if (GlobalVariables != null)
        {
            for (int i = 0; i < GlobalVariables.Count; i++)
            {
                VariableEntry variables = GlobalVariables[i];

                GUILayout.BeginVertical("Box");

                variables.Name = EditorGUILayout.TextField("Name", variables.Name);
                variables.Value = EditorGUILayout.FloatField("Value", variables.Value == null ? 0 : variables.Value.Value);
                variables.Max = EditorGUILayout.FloatField("Max", variables.Max == null ? 0 : variables.Max.Value);
                variables.Min = EditorGUILayout.FloatField("Min", variables.Min == null ? 0 : variables.Min.Value);

                variables.Description = EditorGUILayout.TextField("Description", variables.Description);

                if (GUILayout.Button("Delete"))
                {
                    GlobalVariables.RemoveAt(i);
                    i--;
                    GUILayout.EndVertical();
                    continue;
                }

                GUILayout.EndVertical();

                if (variables.Name.Equals(GlobalVariables[i].Name) &&
                    variables.Value == GlobalVariables[i].Value &&
                    variables.Max == GlobalVariables[i].Max &&
                    variables.Min == GlobalVariables[i].Min &&
                    variables.Description.Equals(GlobalVariables[i]))
                    continue;

                GlobalVariables[i] = variables;
            }
        }

        if (GUILayout.Button("Add"))
        {
            if (GlobalVariables == null) GlobalVariables = new List<VariableEntry>();
            GlobalVariables.Add(new VariableEntry("", 0, 0, 0, ""));
        }

        GUILayout.EndVertical();
    }

    void AddMeshesButton()
    {
        GUILayout.BeginVertical("Box");
        GUILayout.Label("Meshes");

        if (Meshes != null)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                Mesh mesh = Meshes[i];

                GUILayout.BeginVertical("Box");

                string tempName = EditorGUILayout.TextField("Name", mesh.Name);
                string tempMeshPath = AddOpenPathButton("Mesh Path", mesh.MeshPath);
                string tempTexPath = AddOpenPathButton("Texture Path", mesh.TexturePath);

                mesh.Size = EditorGUILayout.FloatField("Size", mesh.Size);
                mesh.MaxSize = EditorGUILayout.FloatField("Max Size", mesh.MaxSize);

                mesh.Type = (OrganType)EditorGUILayout.Popup("Organ Type", mesh.Type.GetHashCode(), Enum.GetNames(typeof(OrganType)));

                if (GUILayout.Button("Delete"))
                {
                    Meshes.RemoveAt(i);
                    i--;
                    GUILayout.EndVertical();
                    continue;
                }

                GUILayout.EndVertical();

                if (tempName.Equals(mesh.Name) && tempMeshPath.Equals(mesh.MeshPath) && tempTexPath.Equals(mesh.TexturePath)) continue;
                Meshes[i] = new Mesh(tempName, tempMeshPath, tempTexPath, mesh.Size, mesh.MaxSize, mesh.Type);
            }
        }

        if (GUILayout.Button("Add"))
        {
            if (Meshes == null) Meshes = new List<Mesh>();
            Meshes.Add(new Mesh("", "", "", 0f, 0f, OrganType.Branch));
        }

        GUILayout.EndVertical();
    }

    private string curPath = System.Environment.CurrentDirectory + "\\Assets\\Resources";   //记录当前打开的路径
    string AddOpenPathButton(string name, string value)
    {
        GUILayout.BeginHorizontal();

        string path = EditorGUILayout.TextField(name, value);
        if (GUILayout.Button("", GUILayout.Width(25f), GUILayout.Height(15f)))
        {
            string tempPath =
                EditorUtility.OpenFilePanel(name, curPath, "");

            if (tempPath != null && tempPath.Length > 0)
            {
                curPath = path =
                    tempPath.Replace('/', '\\').
                    TrimStart((System.Environment.CurrentDirectory + "\\Assets\\Resources\\").ToCharArray());
            }
        }

        GUILayout.EndHorizontal();

        return path;
    }

    void AddApplyButton()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("");    //为使按钮靠左，添加不显示的Label

        if (GUILayout.Button("Save", GUILayout.Width(50f), GUILayout.Height(20f)))
        {
            XML_IO.Save("maize.xml", Name, InitStep, MaxStep, LRule, BranchTexturePath, GlobalVariables, Meshes);
        }

        if (GUILayout.Button("Apply", GUILayout.Width(50f), GUILayout.Height(20f)))
        {
            XML_IO.Save("maize.xml", Name, InitStep, MaxStep, LRule, BranchTexturePath, GlobalVariables, Meshes);
            GameObject.Find("BuildButton").GetComponent<BuildButton>().Click();
        }

        GUILayout.EndHorizontal();
    }

    void LoadModelInfo(TreeModel treeModel)
    {
        if (treeModel == null || treeModel.IsEmpty()) return;   //为空

        Init();

        //基本信息
        Name = treeModel.Name;
        InitStep = treeModel.InitStep;
        MaxStep = treeModel.MaxStep;

        //规则的输入
        LSTree lstree = treeModel.RuleData;

        //公理
        LRule += "@" + lstree.Axiom.ToString() + "\n";

        //产生式
        foreach (LRule production in lstree.ProductionGroup)
        {
            LRule += production.ToString() + "\n";
        }

        //全局变量
        GlobalVariables = lstree.GlobalVariablesList;

        //模型集合
        Meshes = treeModel.Meshes;

        //枝干纹理
        BranchTexturePath = treeModel.TexturePath;

        //用于判断是否为空，为空则不显示任何内容
        isEmpty = false;
    }

    void Init()
    {
        isEmpty = true;

        Name = "";     //名字
        InitStep = 0;      //初始生命周期
        MaxStep = 0;       //最大生命周期

        LRule = "";                        //规则
        BranchTexturePath = "";            //枝干纹理路径

        GlobalVariables = null; //全局变量
        Meshes = null;                   //模型
    }
}

#endif