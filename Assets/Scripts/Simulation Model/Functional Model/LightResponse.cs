 /*
 * 光合作用光响应模型
 * 用于计算正常情况、缺氮、缺磷、缺钾情况下的光合速率
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LightResponseType
{
    NORMAL, N_LACK, P_LACK, K_LACK, ALL_LACK
}

public class LightResponse
{
    /// <summary>
    /// 计算一段时间内的光合速率（umol·m^-2·s^-1）
    /// </summary>
    /// <param name="type">环境因素（正常 or 缺氮 or 缺磷 or 缺钾）</param>
    public static double PhotosyntheticRate(TreeModel treeModel, LightResponseType type = LightResponseType.NORMAL)
    {
        double PAR = FunctionSim.PARRecption_Mean(treeModel);

        return PhotosyntheticRate(PAR, type);
    }
    /// <summary>
    /// 计算光合速率（umol·m^-2·s^-1）
    /// </summary>
    /// <param name="PAR">光合有效辐射（umol·m^-2·s^-1）</param>
    /// <param name="type">环境因素（正常 or 缺氮 or 缺磷 or 缺钾）</param>
    /// <returns>光合速率（umol·m^-2·s^-1）</returns>
    public static double PhotosyntheticRate(double PAR, LightResponseType type = LightResponseType.NORMAL)
    {
        switch (type)
        {
            case LightResponseType.NORMAL: return PhotosyntheticRate_Normal(PAR);
            case LightResponseType.N_LACK: return PhotosyntheticRate_LackOfN(PAR);
            case LightResponseType.P_LACK: return PhotosyntheticRate_LackOfP(PAR);
            case LightResponseType.K_LACK: return PhotosyntheticRate_LackOfK(PAR);
            default: return PhotosyntheticRate_Normal(PAR);
        }
    }

    /// <summary>
    /// 正常情况下（不缺氮磷钾）的光合速率
    /// </summary>
    /// <param name="PAR">光合有效辐射</param>
    /// <returns>光合速率</returns>
    private static double PhotosyntheticRate_Normal(double PAR)
    {
        double a = 0.032996;
        double b = 0.000290272;
        double y = -0.0000201070390225866;
        double Rd = 0.797187623;

        return PhotosyntheticRate_RectangularHyperbolicModel(a, b, y, Rd, PAR);
    }

    /// <summary>
    /// 缺氮情况下的光合速率
    /// </summary>
    private static double PhotosyntheticRate_LackOfN(double PAR)
    {
        return PhotosyntheticRate_Normal(PAR) * 0.9364949;
    }

    /// <summary>
    /// 缺磷情况下的光合速率
    /// </summary>
    private static double PhotosyntheticRate_LackOfP(double PAR)
    {
        return PhotosyntheticRate_Normal(PAR) * 0.88574;
    }

    /// <summary>
    /// 缺钾情况下的光合速率
    /// </summary>
    private static double PhotosyntheticRate_LackOfK(double PAR)
    {
        return PhotosyntheticRate_Normal(PAR) * 0.98;
    }

    /// <summary>
    /// 采用直角双曲线修正模型计算光合速率
    /// Pn = a * (1 - b * PAR) / (1 + y * PAR) * PAR - Rd
    /// </summary>
    /// <param name="a">参数，初始量子效率</param>
    /// <param name="b">参数，光抑制项</param>
    /// <param name="y">参数，光饱和项</param>
    /// <param name="Rd">暗呼吸速率</param>
    /// <param name="PAR">光合有效辐射（umol·m^-2·s^-1）</param>
    /// <returns>光合速率（umol·m^-2·s^-1）</returns>
    private static double PhotosyntheticRate_RectangularHyperbolicModel(double a, double b, double y, double Rd, double PAR)
    {
        return a * (1 - b * PAR) * PAR / (1 + y * PAR) - Rd;
    }
}
