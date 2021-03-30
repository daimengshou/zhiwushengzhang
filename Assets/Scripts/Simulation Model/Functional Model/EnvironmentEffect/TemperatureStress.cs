/*
 * 计算温度胁迫因子
 * 参考文献：麻雪艳, 周广胜. 基于光合产物动态分配的玉米生物量模拟[J]. 应用生态学报, 2016,27(07):2292-2300.
 * 
 * 输入参数：
 * 日均温度或
 * 日最高、最低温度
 * 
 * 输出参数：
 * 温度胁迫因子
 */

using System;
using System.Collections;
using UnityEngine;

public partial class EnvironmentEffect
{
    public static double TemperatureStressFactor(TreeModel treeModel)
    {
        EnvironmentParams envirParams = treeModel.EnvironmentParams;

        return TemperatureStressFactor(FunctionSim.GrowthPeriodJudgment(treeModel), 
            envirParams.DailyMaxTemperature, envirParams.DailyMinTemperature);
    }

    /// <summary>
    /// 温度胁迫因子
    /// </summary>
    /// <param name="dailyMaxTemperature">日最高温度(℃)</param>
    /// <param name="dailyMinTemperature">日最低温度(℃)</param>
    public static double TemperatureStressFactor(GrowthPeriod period, double dailyMaxTemperature, double dailyMinTemperature)
    {
        return TemperatureStressFactor(period, (dailyMaxTemperature + dailyMinTemperature) / 2.0);
    }

    /// <summary>
    /// 温度胁迫因子
    /// </summary>
    /// <param name="temperature">日均温度(℃)</param>
    public static double TemperatureStressFactor(GrowthPeriod period, double temperature)
    {
        int periodIndex = (int)period;

        float lowestTemperature = MaizeParams.MAIZE_DEVELOPMENT_TEMPERATURE[periodIndex][0];     //最低温度
        float optimumTemperature = MaizeParams.MAIZE_DEVELOPMENT_TEMPERATURE[periodIndex][1];    //最适温度
        float maximumTemperature = MaizeParams.MAIZE_DEVELOPMENT_TEMPERATURE[periodIndex][2];    //最高温度

        /*
         * 当温度低于最低温度或高于最高温度时
         * 温度胁迫因子为0
         */
        if (temperature < lowestTemperature ||
            temperature > maximumTemperature)
            return 0;

        double q = 0.25;

        return 
            Math.Pow((temperature - lowestTemperature) / (optimumTemperature - lowestTemperature), 1 + q) *
            Math.Pow((maximumTemperature - temperature) / (maximumTemperature - optimumTemperature), 1 - q);
    }
}

