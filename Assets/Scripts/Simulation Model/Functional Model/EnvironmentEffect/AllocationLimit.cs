/*
 * 计算分配时的限制因子
 * 参考文献：平晓燕, 周广胜, 孙敬松, 等. 基于功能平衡假说的玉米光合产物分配动态模拟[J]. 应用生态学报, 2010,21(01):129-135.
 */

using System;
using System.Collections;
using UnityEngine;

public partial class EnvironmentEffect
{
    /// <summary>
    /// 有效水分比例
    /// </summary>
    /// <param name="WC">土壤当前平均体积持水量(mm)</param>
    private static double AvailableWaterPercentage(double WC)
    {
        if (WC <= WP)
            return 0;
        else
            return (WC - WP) / (FC - WP);
    }

    /// <summary>
    /// 水分限制因子
    /// </summary>
    /// <param name="WC">土壤当前平均体积持水量(mm)</param>
    /// <returns></returns>
    private static double WaterLimitingFactor(double WC)
    {
        double W = AvailableWaterPercentage(WC);

        return 1.0 / (1 + 6.63 * Math.Exp(-5.69 * W));
    }

    /// <summary>
    /// 温度限制因子
    /// </summary>
    /// <param name="temperature">日均气温(℃)</param>
    private static double TemperatureLimitFactor(double temperature)
    {
        return Math.Pow(2, (temperature - 30) / 10);
    }

    public static double AllocationLimitFactor(TreeModel treeModel)
    {
        EnvironmentParams envirParams = treeModel.EnvironmentParams;

        return AllocationLimitFactor(envirParams.WaterContent, (envirParams.DailyMaxTemperature + envirParams.DailyMinTemperature) / 2.0);
    }

    /// <summary>
    /// 生物量分配时的限制因子
    /// </summary>
    /// <param name="WC">土壤当前平均体积持水量(mm)</param>
    /// <param name="temperature">5cm处土壤温度(℃)</param>
    public static double AllocationLimitFactor(double WC, double temperature)
    {
        double waterFactor = WaterLimitingFactor(WC);
        double temperatureFactor = TemperatureLimitFactor(temperature);

        return Math.Min(waterFactor, temperatureFactor);
    }

    public static double AvailabilityOfLight(TreeModel treeModel)
    {
        double LAI = treeModel.LAI;

        return Math.Exp(-0.5 * LAI);
    }
}

