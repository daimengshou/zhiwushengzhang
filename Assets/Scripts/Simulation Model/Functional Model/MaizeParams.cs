using System;
using System.Collections.Generic;
using UnityEngine;


public class MaizeParams
{
    /******种子生物量******/
    public const double SEED_BIOMASS = 0.15;

    /******各器官生物量潜在分配比例******/
    public const float ROOT_POTENTIAL_ALLOCATION_RATIO = 0.3f;     //根潜在分配比例
    public const float STEM_POTENTIAL_ALLOCATION_RATIO = 0.3f;     //茎潜在分配比例
    public const float LEAF_POTENTIAL_ALLOCATION_RATIO = 0.4f;     //叶潜在分配比例

    public const float ROOT_REALLOCATION_RATIO = 0.1f;     //根再分配比例
    public const float STEM_REALLOCATION_RATIO = 0.3f;     //茎再分配比例
    public const float LEAF_REALLOCATION_RATIO = 0.1f;     //叶再分配比例

    /******各器官最大发育年龄******/
    public const int STEM_MAX_DEVELOPMENT_AGE = 12;     //茎最大发育年龄
    public const int LEAF_MAX_DEVELOPMENT_AGE = 12;     //叶最大发育年龄
    public const int SHEATH_MAX_DEVELOPMENT_AGE = 12;   //叶柄最大发育年龄
    public const int MALE_MAX_DEVELOPMENT_AGE = 2;      //雄蕊最大发育年龄
    public const int FEMALE_MAX_DEVELOPMENT_AGE = 18;   //雌蕊最大发育年龄

    /******茎、叶、雄蕊、雌蕊库强计算参数******/
    public const float LEAF_A   = 3.59f;
    public const float SHEATH_A = 3.05f;
    public const float STEM_A   = 3.34f;
    public const float FEMALE_A = 8.34f;

    public const float LEAF_B   = 5.38f;
    public const float SHEATH_B = 3.69f;
    public const float STEM_B   = 1.65f;
    public const float FEMALE_B = 2.6f;

    public const float LEAF_S   = 1f;           //叶片库强
    public const float SHEATH_S = 0.6f;         //叶柄库强
    public const float STEM_S   = 1.4f;         //节间库强
    public const float FEMALE_S = 806.47f;      //雌蕊库强
    public const float MALE_S = 0.305f;

    public static readonly double[][] EXPANDS;

    //短节间个数
    public const int SHORT_INTERNODE_NUM = 6;
    //长节间个数
    public const int LONG_INTERNODE_NUM = 6;
    //总节间个数
    public const int INTERNODE_NUM = 21;

    /******形态参数******/
    public const double STEM_SHAPE_A = 2.3;   //节间形态参数A
    public const double STEM_SHAPE_B = 0.22;   //节间形态参数B
    public const double STEM_LONG_SHAPE_A = 6.5;
    public const double STEM_LONG_SHAPE_B = 0.15;
    public const double STEM_SHORT_K = 0.5;   //短节间系数
    public const double STEM_LONG_K = 1.5;      //长节间系数
    public const double STEM_DEN = 1;         //节间密度
    public const double STEM_LONG_DEN = 0.7;

    public const double LEAF_SHAPE_K = 23.74;               //叶片形态参数A
    public const double LEAF_SHAPE_Y = 0.43;                //叶片形态参数B
    public const double LEAF_AREA_SHAPE_RATIO = 0.72;       //叶片面积与形态（最大宽度乘以长度）之比
    public const double SPECIFIC_LEAF_WEIGHT = 0.024;       //比叶重

    public const double MALE_DEN = 0.02;     //雄蕊密度
    public const double FEMALE_DEN = 1;  //雌蕊密度

    /******光合作用持续时间******/
    public static int[] LEAF_PHOTOSYNTHESIS_DURATION = new int[21] { 5, 7, 9, 11, 12, 12, 12, 11, 10, 8, 7, 7, 6, 6, 5, 5, 5, 5, 5, 5, 5 };

    /******玉米各阶段温度范围******/
    public static float[][] MAIZE_DEVELOPMENT_TEMPERATURE = new float[5][]
    {
        /*
         * 用于描述玉米各阶段的温度范围：
         * 最低温度、最适温度和最高温度
         * 用于计算温度胁迫因子
         * 参考文献：麻雪艳，周广胜. 基于光合产物动态分配的玉米生物量模拟[J]. 应用生态学报,2016,27(07):2292-2300
         */
        new float[3]{10, 25, 30},   //苗期
        new float[3]{12, 30, 35},   //拔节期
        new float[3]{12, 30, 35},   //抽雄期
        new float[3]{15, 24, 30},   //粒期
        new float[3]{15, 24, 30}    //成熟期
    };

    /******玉米缺乏某种元素时颜色变化******/
    public static Color lackN_Color = new Color(255f / 255.0f, 209f / 255f, 102f / 255f);
    public static Color lackP_Color = new Color(204f / 255f, 123f / 255f, 191f / 255f);
    public static Color lackK_Color = new Color(255f / 255f, 149f / 255f, 0f / 255f); //Before(255, 187, 26);

    /******玉米各阶段作物系数******/
    public static double[] KC = new double[3] { 0.3, 1.2, 0.6 };

    /******土壤系数******/
    public const double FC = 0.36; //The water content at field capacity of clay
    public const double WP = 0.22; //The water content at wilting point of clay

    /******土壤水分含量******/
    public static List<double> WaterContents = new List<double>(new double[3] { 0.35, 0.28, 0.21 });

    /******Unity单位转换******/
    public const float SCALE = 0.1f;     //Unity中每一单位代表的实际长度(m)，如 0.1 表示Unity中每一个单位代表实际长度0.1m

    static MaizeParams()
    {
        int typeCount = Enum.GetNames(typeof(OrganType)).Length;    //获取枚举个数
        EXPANDS = new double[typeCount][];

        for (int i = 0; i < typeCount; i++ )
        {
            double a = 0, b = 0, m = 0;
            int maxAge = 0;

            GetExpandParams((OrganType)i, ref a, ref b, ref maxAge);

            EXPANDS[i] = new double[maxAge];

            for (int j = 1; j <= maxAge; j++)
            {
                EXPANDS[i][j - 1] = Expand(a, b, j, maxAge);
                m += EXPANDS[i][j - 1];
            }

            if (m == 0) continue;

            //normalize
            for (int j = 1; j <= maxAge; j++)
            {
                EXPANDS[i][j - 1] /= m;
            }
        }
    }

    private static void GetExpandParams(OrganType type, ref double a, ref double b, ref int maxAge)
    {
        switch (type)
        {
            case OrganType.Branch:
                a = STEM_A;
                b = STEM_B;
                maxAge = STEM_MAX_DEVELOPMENT_AGE;
                break;
            case OrganType.Leaf:
                a = LEAF_A;
                b = LEAF_B;
                maxAge = LEAF_MAX_DEVELOPMENT_AGE;
                break;
            case OrganType.Sheath:
                a = SHEATH_A;
                b = SHEATH_B;
                maxAge = SHEATH_MAX_DEVELOPMENT_AGE;
                break;
            case OrganType.Fruit:
                a = FEMALE_A;
                b = FEMALE_B;
                maxAge = FEMALE_MAX_DEVELOPMENT_AGE;
                break;
            default:
                a = 1;
                b = 1;
                maxAge = MALE_MAX_DEVELOPMENT_AGE;
                break;
        }
    }

    private static double Expand(double a, double b, int age, int maxAge)
    {
        return Math.Pow((age - 0.5) / maxAge, a - 1) * Math.Pow(1 - (age - 0.5) / maxAge, b - 1) * (1.0 / maxAge);
    }
}
