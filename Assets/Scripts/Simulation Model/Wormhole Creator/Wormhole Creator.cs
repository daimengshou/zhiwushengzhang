using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class CellularTexture
{

    private const int MAX_DEC = 256 * 256 * 256 - 1;

    /// <summary>
    /// 获取颜色阈值
    /// </summary>
    /// <param name="remainingRatio">剩余叶面积比比例</param>
    /// <param name="leafPixelsCount">代表叶片纹理的个数</param>
    /// <param name="accumulationList">细胞纹理各颜色累加个数</param>
    public static int ColorThreshold(double remainingRatio,  int leafPixelsCount,
                                        List<KeyValuePair<int, int>> accumulationList)
    {
        /*
         * 根据剩余面积百分比
         * 计算出病虫害像素的个数（消失像素的个数）
         */
        int infectPixelsCount = (int)((1.0 - remainingRatio) * leafPixelsCount);

        /*
         * 采用二分法快速查找阈值索引
         */
        int index = BinarySearchThreshold(infectPixelsCount, accumulationList);

        /*
         * 当索引为-1时
         * 表示未找到，即所有像素均消失
         */
        if (index == -1)
            return 0;
        else if (index == accumulationList.Count)
            return MAX_DEC;
        else
            return accumulationList[index].Key;
    }

    /// <summary>
    /// 二分法快速查找阈值索引
    /// </summary>
    /// <param name="disappearanceCount">消失像素个数</param>
    /// <param name="accumulationList">细胞纹理各颜色累加个数</param>
    private static int BinarySearchThreshold(int disappearanceCount, List<KeyValuePair<int, int>> accumulationList)
    {
        int min = 0;
        int max = accumulationList.Count - 1;

        while (min < max)
        {
            int mid = (min + max) / 2;

            if ((mid == 0 ? 0 : accumulationList[mid - 1].Value) <= disappearanceCount && 
                 accumulationList[mid].Value > disappearanceCount)
                return mid;
            else if (accumulationList[mid].Value <= disappearanceCount)
            {
                min = min == mid ? mid + 1 : mid;
            }
            else
            {
                max = max == mid ? max - 1 : mid;
            }
        }

        //未找到
        if (min == 0)
            return -1;
        else
            return max + 1;
    }

    /// <summary>
    /// 创建虫洞纹理
    /// </summary>
    /// <param name="remainingRatio">剩余叶面积比例</param>
    /// <param name="leafPixelsCount">代表叶片的像素个数</param>
    /// <param name="tempFilePath">临时文件存放位置</param>
    /// <param name="referenceTex">参考纹理（叶片纹理）</param>
    /// <param name="cellularTex">细胞纹理</param>
    public static Texture CreateWormhole(double remainingRatio, int leafPixelsCount,
                                               string tempFilePath,
                                               Texture referenceTex, Texture2D cellularTex)
    {
        return CreateWormhole(remainingRatio, leafPixelsCount,
                              LoadAccumulationCountFile(tempFilePath, cellularTex),
                              referenceTex, cellularTex);
    }

    /// <summary>
    /// 创建虫洞纹理
    /// </summary>
    /// <param name="remainingRatio">剩余叶面积比例</param>
    /// <param name="leafPixelsCount">代表叶片的像素个数</param>
    /// <param name="accumulationList">细胞纹理各颜色累加个数</param>
    /// <param name="referenceTex">参考纹理（叶片纹理）</param>
    /// <param name="cellularTex">细胞纹理</param>
    public static Texture CreateWormhole(double remainingRatio,  int leafPixelsCount,
                                               List<KeyValuePair<int, int>> accumulationList,
                                               Texture referenceTex, Texture cellularTex)
    {
        return CreateWormhole(ColorThreshold(remainingRatio, leafPixelsCount, accumulationList), referenceTex, cellularTex);
    }

    public static Texture CreateWormhole(int threshold, Texture referenceTex, Texture cellularTex)
    {
        if (referenceTex == null || cellularTex == null)
            throw new ArgumentNullException("No Texture");

        ComputeShader shader = GameObject.Find("ComputeShader").GetComponent<ComputeMemory>().shader;   //获取计算shader
        int kernel = shader.FindKernel("WormholeCreate");   //核函数

        /*
         * 输入参数：
         * 参考纹理（叶片纹理）、细胞纹理（模拟路径）、阈值
         */
        shader.SetTexture(kernel, "MainTex", referenceTex);
        shader.SetTexture(kernel, "CelluarTex", cellularTex);
        shader.SetInt("threshold", threshold);

        /*
         * 输出参数：
         * 需要渲染的纹理
         */
        RenderTexture renderTex = RenderTexture.GetTemporary(referenceTex.width, referenceTex.height);//new RenderTexture(referenceTex.width, referenceTex.height, 16);
        renderTex.enableRandomWrite = true;
        renderTex.Create();

        shader.SetTexture(kernel, "ResultTex", renderTex);

        //执行
        shader.Dispatch(kernel, referenceTex.width / 8, referenceTex.height / 8, 1);

        return RenderTex2Tex2D(renderTex);
    }

    public static Texture2D RenderTex2Tex2D(RenderTexture renderTex)
    {
        RenderTexture.active = renderTex;

        Texture2D outputTex = new Texture2D(renderTex.width, renderTex.height);
        Rect rect = new Rect(0, 0, renderTex.width, renderTex.height);
        outputTex.ReadPixels(rect, 0, 0);
        outputTex.Apply();

        RenderTexture.active = null;

        //renderTex.Release();
        RenderTexture.ReleaseTemporary(renderTex);

        return outputTex;
    }

    /************************************************************************/
    /*                       非静态成员和函数                               */
    /************************************************************************/
    private int m_LeafPixelsCount = 0;
    private double m_LimitRatio = 0;
    private Texture2D m_CellularTex = null;
    private List<KeyValuePair<int, int>> m_AccumulationList = null;

    /*
     * 叶面积剩余百分比的极限
     * 当低于这个数值时
     * 病虫将不再啃食该叶片
     */
    public double LimitRatio { get { return m_LimitRatio; } }

    /// <param name="texPath">细胞纹理的存放路径</param>
    /// <param name="leafPixelsCount">代表叶片的像素个数</param>
    public CellularTexture(string texPath, int leafPixelsCount)
    {
        m_LeafPixelsCount = leafPixelsCount;

        texPath = texPath.Remove(texPath.LastIndexOf('.'));

        LoadCelluarTex(texPath);
        LoadAccumulationCountFile(System.Environment.CurrentDirectory + "\\Data\\" + Path.GetFileNameWithoutExtension(texPath) + ".data");

        SetLimitRatio();
    }

    private void LoadCelluarTex(string path)
    {
        path = path.Replace((System.Environment.CurrentDirectory + "\\Assets\\Resources\\"), "");

        m_CellularTex = Resources.Load<Texture2D>(path);
    }

    private void LoadAccumulationCountFile(string path)
    {
        m_AccumulationList = LoadAccumulationCountFile(path, m_CellularTex);
    }

    private void SetLimitRatio()
    {
        m_LimitRatio = 1.0 - m_AccumulationList[m_AccumulationList.Count - 1].Value * 1.0 / m_LeafPixelsCount; 
    }

    /// <summary>
    /// 创建虫洞纹理
    /// </summary>
    /// <param name="remainingRatio">剩余叶面积比例</param>
    /// <param name="referenceTex">参考纹理（叶片纹理）</param>
    /// <returns></returns>
    public Texture CreateWormhole(double remainingRatio, Texture referenceTex)
    {
        return CreateWormhole(remainingRatio, m_LeafPixelsCount,
            m_AccumulationList, referenceTex, m_CellularTex);
    }

    public Texture CreateWormhole(double remainingRatio, Texture referenceTex, out int threshold)
    {
        threshold = ColorThreshold(remainingRatio, m_LeafPixelsCount, m_AccumulationList);

        return CreateWormhole(threshold, referenceTex, m_CellularTex);
    }
}