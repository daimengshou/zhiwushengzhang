/*
 * 计算水分胁迫因子
 * 参考文献：http://www.fao.org/3/X0490E/x0490e0e.htm#TopOfPage
 * 
 * 输入参数：
 * 日均最高、最低温度；
 * 日均最高、最低相对湿度；
 * 风速；
 * 冠层处的辐射量（非冠层接受的辐射量）；
 * 作物当前生长周期；
 * 土壤当前平均体积持水量；
 * 根的深度（不影响水分胁迫因子的计算结果）
 * 
 * 输出参数：
 * 水分胁迫因子
 */

using System;
using System.Collections;
using UnityEngine;

public partial class EnvironmentEffect
{
    /// <summary>
    /// 可速效水分百分比
    /// </summary>
    /// <param name="ETc">标准情况下的作物蒸散量</param>
    private static double AverageFractionOfTotalAvailableSoilWater(double ETc)
    {
        double p0 = 0.55;

        return p0 + 0.04 * (5 - ETc);
    }

    private const double FC = MaizeParams.FC;     //The water content at field capacity of clay
    private const double WP = MaizeParams.WP;     //The water content at wilting point of clay

    /// <summary>
    /// 土壤有效水分含量(mm)
    /// </summary>
    /// <param name="rootDepth">根的深度</param>
    private static double TotalAvailableWater(double rootDepth = 1)
    {
        return 1000.0 * (FC - WP) * rootDepth;
    }

    /// <summary>
    /// 土壤可速效水分含量(mm)
    /// </summary>
    /// <param name="TAW">土壤有效水分含量(mm)</param>
    /// <param name="ETc">标准情况下的作物蒸散量</param>
    private static double ReadilyAvailableWater(double TAW, double ETc)
    {
        return AverageFractionOfTotalAvailableSoilWater(ETc) * TAW;
    }

    /// <summary>
    /// 土壤水分消耗量(mm)
    /// </summary>
    /// <param name="WC">土壤当前平均体积持水量(mm)</param>
    /// <param name="rootDepth">根的深度</param>
    private static double RootZoneDepletion(double WC, double rootDepth = 1)
    {
        return 1000.0 * (FC - WC) * rootDepth;
    }

    /// <summary>
    /// 水分胁迫因子
    /// </summary>
    /// <param name="dailyMaxTemperature">日最高温度(℃)</param>
    /// <param name="dailyMinTemperature">日最低温度(℃)</param>
    /// <param name="dailyMaxRelativeHumidity">日最大相对湿度</param>
    /// <param name="dailyMinRelativeHumidity">日最小相对湿度</param>
    /// <param name="windSpeed">风速(m s^-1)</param>
    /// <param name="solarRadiation">到达冠层的辐射量（MJ m^-2 day^-1）</param>
    /// <param name="period">当前生长阶段</param>
    /// <param name="WC">土壤当前平均体积持水量(mm)</param>
    /// <param name="rootDepth">根的深度</param>
    public static double WaterStressFactor(double dailyMaxTemperature, double dailyMinTemperature,
                                           double dailyMaxRelativeHumidity, double dailyMinRelativeHumidity,
                                           double windSpeed, double solarRadiation, int GC, GrowthPeriod period,
                                           double WC, double rootDepth = 1)
    {
        //标准情况下作物的蒸散量
        double ETc = CropEvapotranspirationUnderStandardConditions(
            dailyMaxTemperature, dailyMinTemperature, 
            dailyMaxRelativeHumidity, dailyMinRelativeHumidity, 
            windSpeed, solarRadiation, 
            GC, period);

        //土壤有效水分含量
        double TAW = TotalAvailableWater(rootDepth);

        //土壤可速效水分含量（在此区间内无水分胁迫）
        double RAW = ReadilyAvailableWater(TAW, ETc);

        //土壤水分消耗量
        double Dr = RootZoneDepletion(WC, rootDepth);

        if (Dr < 0)
        {
            //未验证数据
            Dr = -Dr;
            if (Dr >= TAW)
                return 0;
            else
                return 1 - Dr / TAW;
        }
        else if (Dr <= RAW)
        {
            return 1;
        }
        else
        {
            return (TAW - Dr) / (TAW - RAW);
        }
    }

    public static double WaterStressFactor(TreeModel treeModel)
    {
        EnvironmentParams envirParams = treeModel.EnvironmentParams;

        return WaterStressFactor(envirParams.DailyMaxTemperature, envirParams.DailyMinTemperature,
                                 envirParams.DailyMaxRelativeHumidity, envirParams.DailyMinRelativeHumidity,
                                 envirParams.WindSpeed, SolarSim.DailyDirectIrradiance(DateTime.Now, 119, 26),
                                 treeModel.ComputeGrowthCycle(), FunctionSim.GrowthPeriodJudgment(treeModel),
                                 envirParams.WaterContent);
    }
}

