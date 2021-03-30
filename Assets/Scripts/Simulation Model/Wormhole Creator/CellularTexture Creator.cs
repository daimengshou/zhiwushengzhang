#define TEST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


struct PixelInfo
{
    public float    Distance;       //与最近点之间的距离
    public Vector2  NearestPoint;   //最近点

    //最近点X、Y坐标
    public int X_NearestPoint { get { return (int)NearestPoint.x; } }
    public int Y_NearestPoint { get { return (int)NearestPoint.y; } }


    public PixelInfo(Vector2 NearestPoint, float distance)
    {
        this.NearestPoint = NearestPoint;
        Distance = distance;
    }

    public override string ToString()
    {
        return "Nearest Point: " + NearestPoint.ToString("f4") + " Distance: " + Distance;
    }
}

public partial class CellularTexture
{
    /*
     * 距离缩放尺度
     * 为使不同距离的像素颜色不同
     * 需要通过该尺度调整
     */
    private const int SCALE = 10;

    //Test
    private static Vector2[] centerPoints = null;


    public static Texture2D Create(int count_CenterPoints, int width, int height, int count_PointsPerGroup = 10, float radius = 15, string barrierPath = null, Texture2D referenceTex = null)
    {
        //添加障碍物
        Texture2D barrierTexture = null;
        if (barrierPath != null)
        {
            barrierTexture = Resources.Load(barrierPath, typeof(Texture2D)) as Texture2D;
        }

        return Create(width, height, count_CenterPoints, count_PointsPerGroup, radius, referenceTex, barrierTexture);
    }

    public static Texture2D Create(int width, int height, 
                                   int count_CenterPoints, int count_PointsPerGroup = 10, float radius = 15,
                                   Texture2D referenceTex = null, Texture2D barrierTex = null)
    {
        Texture2D texture = new Texture2D(width, height);

        PixelInfo[] PixelInfoes = new PixelInfo[width * height];        //用于存储各像素点的信息（最近特征点和与其的距离）

        Vector2[] CenterPoints = null;

        if (centerPoints == null)
            CenterPoints = RandomPoints(count_CenterPoints, width, height, referenceTex);   //随机选择特征点
        else
            CenterPoints = centerPoints;

        centerPoints = CenterPoints;

        Vector2[] FeaturePoints = new Vector2[CenterPoints.Length * count_PointsPerGroup];
        PixelInfo[] FeaturePointInfos = new PixelInfo[FeaturePoints.Length];
        PixelInfo[] FeaturePointInfos_FeaturePoint = new PixelInfo[FeaturePoints.Length];

        for (int i = 0; i < CenterPoints.Length; i++)
        {
            for (int j = 0; j < count_PointsPerGroup; j++)
            {
                Vector2 FeaturePoint = CenterPoints[i] + Random.insideUnitCircle * radius;

                while ((FeaturePoint.x < 0 || FeaturePoint.x > width - 1 ||
                       FeaturePoint.y < 0 || FeaturePoint.y > height - 1))
                {
                    FeaturePoint = CenterPoints[i] + Random.insideUnitCircle * radius;
                }

                FeaturePoint.x = (int)FeaturePoint.x;
                FeaturePoint.y = (int)FeaturePoint.y;

                FeaturePoints[i * count_PointsPerGroup + j] = FeaturePoint;
            }
        }

        FeaturePointInfos = ComputeDistance_FC(FeaturePoints, CenterPoints);
        FeaturePointInfos_FeaturePoint = ComputeDistance_FF(FeaturePoints, FeaturePointInfos);

        SortFeaturePoints(ref FeaturePoints, ref FeaturePointInfos, ref FeaturePointInfos_FeaturePoint);

        /*
         * 计算每个像素与其最近特征点之间的距离
         */
        PixelInfoes = ComputeDistanceFast(width, height, CenterPoints, FeaturePoints, FeaturePointInfos);

        /*
         * 计算最长的距离
         * 用于实现颜色变化
         */
        float MaxDistance = GetMaxDistance(PixelInfoes);

        /*
         * 为实现特征点有先后顺序，对中心点的颜色进行干扰
         */
        for (int i = 0; i < CenterPoints.Length; i++)
        {

            texture.SetPixel((int)CenterPoints[i].x, (int)CenterPoints[i].y,
                DEC2Color((int)(MaxDistance * RandomNumer.Single() * 0.5f * SCALE)));
        }

        /*
         * 赋予每个特征点颜色
         */
        for (int i = 0; i < FeaturePoints.Length; i++)
        {
            Color FeaturePixel = texture.GetPixel((int)FeaturePointInfos_FeaturePoint[i].X_NearestPoint, (int)FeaturePointInfos_FeaturePoint[i].Y_NearestPoint);

            texture.SetPixel((int)FeaturePoints[i].x, (int)FeaturePoints[i].y, GetColor(FeaturePointInfos[i].Distance, MaxDistance, FeaturePixel));
        }

        /*
         * 赋予每个像素点颜色
         * 根据位置之间的关系，从近到远（黑到白）
         */
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color FeaturePixel = texture.GetPixel((int)PixelInfoes[i * height + j].X_NearestPoint, (int)PixelInfoes[i * height + j].Y_NearestPoint);  //获取最近特征点的颜色

                Color pixel = GetColor(PixelInfoes[i * height + j].Distance, MaxDistance, FeaturePixel);
                
                pixel.a = 1.0f;

                texture.SetPixel(i, j, pixel);
            }
        }

        //添加障碍物像素
        if (barrierTex != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (barrierTex.GetPixel(x, y).a != 0)
                        texture.SetPixel(x, y, GetTransparentColor(texture.GetPixel(x, y)));
                }
            }
        }

        //添加轮廓像素
        if (referenceTex != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (referenceTex.GetPixel(x, y).a == 0)
                    {
                        texture.SetPixel(x, y, GetTransparentColor(texture.GetPixel(x, y)));
                    }
                }
            }
        }

        texture.Apply();

        return texture;
    }

    private static Color GetColor(float distance, float maxDistance, Color referenceColor)
    {
        return DEC2Color((int)((distance) * SCALE) + Color2DEC(referenceColor));
    }

    /// <summary>
    /// 从区间为0~1的颜色转换成十进制的数字
    /// </summary>
    /// <returns></returns>
    public static int Color2DEC(Color color)
    {
        return (int)(color.r * 255) * 65536 + (int)(color.g * 255) * 256 + (int)(color.b * 255);
    }

    /// <summary>
    /// 从十进制的数字转换成区间为0~1的颜色
    /// </summary>
    /// <returns></returns>
    public static Color DEC2Color(int dec)
    {
        Color result = new Color();

        int remain = dec;

        for (int i = 2; i >= 0; i--)
        {
            int mod = remain % 256;     //余数
            remain /= 256;              

            result[i] = mod / 255.0f; 
        }

        return result;
    }
    
    private static Vector2[] RandomPoints(int count, int width, int height, Texture2D referenceTex = null)
    {
        //随机选取特征点
        int[] featurePointY = RandomNumer.Ints(count, 0, height - 1, RandomNumer.DistributionFunction.Uniform);
        int[] featurePointX = referenceTex == null ? RandomNumer.Ints(count, 0, width - 1, RandomNumer.DistributionFunction.Parabola) : GetXCoordinatesOfBoundaryPointRandomly(featurePointY, referenceTex);
        Vector2[] Points = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            Points[i] = new Vector2(featurePointX[i], featurePointY[i]);
        }

        return Points;
    }

    /// <summary>
    /// 根据多个Y坐标的值，随机（若有多个边界点的话）获取其边界点的X坐标
    /// </summary>
    private static int[] GetXCoordinatesOfBoundaryPointRandomly(int[] pointsY, Texture2D referenceTex)
    {
        int[] featurePointX = new int[pointsY.Length];
        for (int i = 0; i < pointsY.Length; i++)
        {
            featurePointX[i] = GetXCoordinateOfBoundaryPointRandomly(pointsY[i], referenceTex);
        }

        return featurePointX;
    }

    /// <summary>
    /// 根据Y坐标的值，随机（若有多个边界点的话）获取其边界点的X坐标
    /// </summary>
    private static int GetXCoordinateOfBoundaryPointRandomly(int pointY, Texture2D referenceTex)
    {
        List<int> boundaryOfX = new List<int>();
        bool isTransparent_current;         //当前像素是否透明
        bool isTransparent_prev = true;     //前一个像素是否透明

        for (int x = 0; x < referenceTex.width; x++)
        {
            isTransparent_current = referenceTex.GetPixel(x, pointY).a == 0;  //判断是否透明

            if (isTransparent_current != isTransparent_prev)    //相邻两个像素一个为透明一个为非透明，即为边界
                boundaryOfX.Add(x);

            isTransparent_prev = isTransparent_current;
        }

        isTransparent_current = true;   //当前像素超过纹理边界，即为透明

        if (isTransparent_current != isTransparent_prev)    //判断纹理一边是否为边界
            boundaryOfX.Add(referenceTex.width - 1);

        return 
            boundaryOfX.Count == 0 ?    /*若为0，则说明该行均为透明像素，即无边界*/
            RandomNumer.ParabolaVariable_Int(0, referenceTex.width - 1) : boundaryOfX[RandomNumer.Int(0, boundaryOfX.Count)];
    }

    /// <summary>
    /// 计算特征点与最近中心点之间的距离
    /// </summary>
    private static PixelInfo[] ComputeDistance_FC(Vector2[] FeaturePoints, Vector2[] CenterPoints)
    {
        ComputeShader shader = GameObject.Find("ComputeShader").GetComponent<ComputeMemory>().shader;   //获取计算shader
        int kernel = shader.FindKernel("FindNearestCenterPoint");   //核函数

        /*
         * 传入参数：
         * 特征点集、中心点集
         */
        ComputeBuffer featurePointsBuffer = new ComputeBuffer(FeaturePoints.Length, 8);
        shader.SetBuffer(kernel, "FeaturePoints", featurePointsBuffer);
        featurePointsBuffer.SetData(FeaturePoints);

        ComputeBuffer centerPointsBuffer = new ComputeBuffer(CenterPoints.Length, 8);
        shader.SetBuffer(kernel, "CenterPoints", centerPointsBuffer);
        centerPointsBuffer.SetData(CenterPoints);

        /*
         * 传出参数：
         * 特征点集信息
         */
        PixelInfo[] pixelInfos = new PixelInfo[FeaturePoints.Length];
        ComputeBuffer pixelInfosBuffer = new ComputeBuffer(pixelInfos.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelInfo)));
        shader.SetBuffer(kernel, "Result", pixelInfosBuffer);

        //执行
        shader.Dispatch(kernel, FeaturePoints.Length, 1, 1);

        //获取传出参数
        pixelInfosBuffer.GetData(pixelInfos);

        //释放
        featurePointsBuffer.Release();
        centerPointsBuffer.Release();
        pixelInfosBuffer.Release();

        return pixelInfos;
    }

    /// <summary>
    /// 计算特征点与最近特征点之间的距离（Distance_FF）
    /// </summary>
    private static PixelInfo[] ComputeDistance_FF(Vector2[] FeaturePoints, PixelInfo[] FeaturePointInfos)
    {
        PixelInfo[] result = new PixelInfo[FeaturePoints.Length];

        ComputeShader shader = GameObject.Find("ComputeShader").GetComponent<ComputeMemory>().shader;   //获取计算shader
        int kernel = shader.FindKernel("FindNearestFeaturePoint");

        /*
         * 输入参数：
         * 特征点集、特征点信息集
         */
        ComputeBuffer featurePointsBuffer = new ComputeBuffer(FeaturePoints.Length, 8);
        shader.SetBuffer(kernel, "FeaturePoints", featurePointsBuffer);
        featurePointsBuffer.SetData(FeaturePoints);

        ComputeBuffer featurePointInfosBuffer = new ComputeBuffer(FeaturePointInfos.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelInfo)));
        shader.SetBuffer(kernel, "FeaturePointInfos", featurePointInfosBuffer);
        featurePointInfosBuffer.SetData(FeaturePointInfos);

        /*
         * 传出参数
         */
        ComputeBuffer pixelInfosBuffer = new ComputeBuffer(result.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelInfo)));
        shader.SetBuffer(kernel, "Result", pixelInfosBuffer);

        //执行
        shader.Dispatch(kernel, FeaturePoints.Length, 1, 1);

        //获取传出参数
        pixelInfosBuffer.GetData(result);

        //释放
        featurePointsBuffer.Release();
        featurePointInfosBuffer.Release();
        pixelInfosBuffer.Release();

        return result;
    }

    private static void SortFeaturePoints(ref Vector2[] FeaturePoints, ref PixelInfo[] FeaturePointInfos, ref PixelInfo[] FeaturePointInfos_FeaturePoint)
    {
        for (int i = 0; i < FeaturePoints.Length - 1; i++)
        {
            int index = i;
            for (int j = i + 1; j < FeaturePoints.Length; j++)
            {
                if (FeaturePointInfos[j].Distance < FeaturePointInfos[index].Distance)
                {
                    index = j;
                }
            }

            if (index != i)
            {
                //交换特征点
                Vector2 tempPoint = FeaturePoints[i];
                FeaturePoints[i] = FeaturePoints[index];
                FeaturePoints[index] = tempPoint;

                PixelInfo tempInfo = FeaturePointInfos[i];
                FeaturePointInfos[i] = FeaturePointInfos[index];
                FeaturePointInfos[index] = tempInfo;

                tempInfo = FeaturePointInfos_FeaturePoint[i];
                FeaturePointInfos_FeaturePoint[i] = FeaturePointInfos_FeaturePoint[index];
                FeaturePointInfos_FeaturePoint[index] = tempInfo;
            }
        }
    }

    private static float GetMaxDistance(PixelInfo[] PixelInfoes)
    {
        float MaxDistance = PixelInfoes[0].Distance;

        for (int i = 1; i < PixelInfoes.Length; i++ )
        {
            if (MaxDistance < PixelInfoes[i].Distance)
                MaxDistance = PixelInfoes[i].Distance;
        }

        return MaxDistance;
    }

    private static PixelInfo[] ComputeDistance(int width, int height, Vector2[] CenterPoints)
    {
        PixelInfo[] pixelInfos = new PixelInfo[width * height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int index = 0;
                float distance = Vector2.Distance(new Vector2(x, y), CenterPoints[index]);
                for (int i = 1; i < CenterPoints.Length; i++)
                {
                    float distance_temp = Vector2.Distance(new Vector2(x, y), CenterPoints[i]);
                    if (distance_temp < distance)
                    {
                        distance = distance_temp;
                        index = i;
                    }
                }

                pixelInfos[x * height + y] = new PixelInfo(CenterPoints[index], distance);
            }

        return pixelInfos;
    }

    private static PixelInfo[] ComputeDistanceFast(int width, int height,Vector2[] CenterPoints, Vector2[] FeaturePoints, PixelInfo[] FeaturePointInfos)
    {
        ComputeShader shader = GameObject.Find("ComputeShader").GetComponent<ComputeMemory>().shader;   //获取计算shader

        int kernel = shader.FindKernel("FindNearestPoint");   //核函数

        /*
         * 传入参数：
         * 中心点集、特征点集、特征点信息、纹理宽、纹理长
         */
        ComputeBuffer CenterPointsBuffer = new ComputeBuffer(CenterPoints.Length, 8);
        shader.SetBuffer(kernel, "CenterPoints", CenterPointsBuffer);
        CenterPointsBuffer.SetData(CenterPoints);

        ComputeBuffer FeaturePointsBuffer = new ComputeBuffer(FeaturePoints.Length, 8);
        shader.SetBuffer(kernel, "FeaturePoints", FeaturePointsBuffer);
        FeaturePointsBuffer.SetData(FeaturePoints);     //设置传入的特征点集

        ComputeBuffer FeaturePointInfosBuffer = new ComputeBuffer(FeaturePointInfos.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelInfo)));
        shader.SetBuffer(kernel, "FeaturePointInfos", FeaturePointInfosBuffer);
        FeaturePointInfosBuffer.SetData(FeaturePointInfos); //设置传入的特征点信息

        shader.SetInt("width", width);
        shader.SetInt("height", height);
        /*
         * 传出参数
         * 结果（PixelInfo数组）
         */
        PixelInfo[] pixelInfos = new PixelInfo[width * height];
        ComputeBuffer PixelInfosBuffer = new ComputeBuffer(pixelInfos.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelInfo)));
        shader.SetBuffer(kernel, "Result", PixelInfosBuffer);

        //执行
        shader.Dispatch(kernel, width / 8, height / 8, 1);

        //获取输出参数
        PixelInfosBuffer.GetData(pixelInfos);

        //释放
        CenterPointsBuffer.Release();
        FeaturePointsBuffer.Release();
        FeaturePointInfosBuffer.Release();
        PixelInfosBuffer.Release();

        return pixelInfos;
    }

    private static Color GetTransparentColor(Color color)
    {
        color.a = 0;

        return color;
    }
}


public class DistanceComparable : IComparer<Vector2>
{
    public int Compare(Vector2 a, Vector2 b)
    {
        if (a.x > b.x)
            return 1;
        else if (a.x == b.x)
            return 0;
        else
            return -1;
    }
}