/*
 * 计算参考作物蒸散量和作物标准情况下的蒸散量
 * 用于后续水分胁迫因子的计算
 * 参考文献：http://www.fao.org/3/X0490E/x0490e08.htm#TopOfPage
 * 
 * 输入参数：
 * 日均最高、最低温度；
 * 日均最高、最低相对湿度；
 * 风速；
 * 冠层处的辐射量（非冠层接受的辐射量）；
 * 作物当前生长周期；
 * 
 * 输出参数：
 * 标准情况下作物的蒸散量；
 */

using System;
using System.Collections;
using UnityEngine;

public partial class EnvironmentEffect
{
    /// <summary>
    /// 饱和水气压曲线斜率(kPa/℃)
    /// </summary>
    /// <param name="temperature">日均温度(℃)</param>
    private static double SlopeOfSaturationVapourPressureTemperature(double temperature)
    {
        return 4098 * (SaturationVapourPressureAtTemperature(temperature)) 
            / Math.Pow(temperature + 237.3, 2);
    }

    /// <summary>
    /// 某温度下的饱和水气压
    /// </summary>
    /// <param name="temperature">温度(℃)</param>
    private static double SaturationVapourPressureAtTemperature(double temperature)
    {
        return 0.6108 * Math.Exp(17.27 * temperature / (temperature + 237.3));
    }

    /// <summary>
    /// 日均饱和水气压
    /// </summary>
    /// <param name="dailyMaxTemperature">日最高温度(℃)</param>
    /// <param name="dailyMinTemperature">日最低温度(℃)</param>
    private static double MeanSaturationVapourPressure(double dailyMaxTemperature, double dailyMinTemperature)
    {
        return (SaturationVapourPressureAtTemperature(dailyMinTemperature) +
                SaturationVapourPressureAtTemperature(dailyMaxTemperature)) /
                2.0;
    }

    /// <summary>
    /// 日均实际水气压
    /// </summary>
    /// <param name="dailyMaxTemperature">日最高温度(℃)</param>
    /// <param name="dailyMinTemperature">日最低温度(℃)</param>
    /// <param name="dailyMaxRelativeHumidity">日最大相对湿度</param>
    /// <param name="dailyMinRelativeHumidity">日最小相对湿度</param>
    private static double ActualVapourPressure(double dailyMaxTemperature, double dailyMinTemperature,
                                               double dailyMaxRelativeHumidity, double dailyMinRelativeHumidity)
    {
        return (SaturationVapourPressureAtTemperature(dailyMinTemperature) * dailyMaxRelativeHumidity / 100.0 +
                SaturationVapourPressureAtTemperature(dailyMaxTemperature) * dailyMinRelativeHumidity / 100.0) /
                2.0;
    }

    /// <summary>
    /// 入射长波辐射（MJ m^-2 day^-1）
    /// </summary>
    /// <param name="solarRadiation">到达冠层的辐射量（MJ m^-2 day^-1）</param>
    private static double NetShortwaveRadiation(double solarRadiation)
    {
        double a = 0.23; //冠层反射系数

        return (1.0 - a) * solarRadiation;
    }

    private const double STEFAN_BOLTZMANN_CONSTANT = 4.903E-9;  //玻尔兹曼常数

    /// <summary>
    /// 天空晴朗无云情况下的出射净长波辐射（MJ m^-2 day^-1）
    /// </summary>
    /// <param name="dailyMaxTemperature">日最高温度(℃)</param>
    /// <param name="dailyMinTemperature">日最低温度(℃)</param>
    /// <param name="actualVapourPressure">日均实际水气压</param>
    private static double NetLongwaveRadiation_ClearSky(double dailyMaxTemperature, double dailyMinTemperature,
                                                        double actualVapourPressure)
    {
        double maxAbsoluteTemperature = dailyMaxTemperature + 273.16;
        double minAbsoluteTemperature = dailyMinTemperature + 273.16;

        return STEFAN_BOLTZMANN_CONSTANT * ((Math.Pow(maxAbsoluteTemperature, 4) + Math.Pow(minAbsoluteTemperature, 4)) / 2) *
            (0.34 - 0.14 * Math.Sqrt(actualVapourPressure)) *
            (1.35 - 0.35);
    }

    /// <summary>
    /// 天空晴朗无云情况下的净辐射（MJ m^-2 day^-1）
    /// </summary>
    /// <param name="solarRadiation">到达冠层的辐射量（MJ m^-2 day^-1）</param>
    /// <param name="dailyMaxTemperature">日最高温度(℃)</param>
    /// <param name="dailyMinTemperature">日最低温度(℃)</param>
    /// <param name="actualVapourPressure">日均实际水气压</param>
    private static double NetRadiation_ClearSky(double solarRadiation,
                                       double dailyMaxTemperature, double dailyMinTemperature,
                                       double actualVapourPressure)
    {
        return NetShortwaveRadiation(solarRadiation) - NetLongwaveRadiation_ClearSky(dailyMaxTemperature, dailyMinTemperature, actualVapourPressure);
    }

    /// <summary>
    /// 参考作物蒸散量(mm day^-1)
    /// </summary>
    /// <param name="dailyMaxTemperature">日最高温度(℃)</param>
    /// <param name="dailyMinTemperature">日最低温度(℃)</param>
    /// <param name="dailyMaxRelativeHumidity">日最大相对湿度</param>
    /// <param name="dailyMinRelativeHumidity">日最小相对湿度</param>
    /// <param name="windSpeed">风速(m s^-1)</param>
    /// <param name="solarRadiation">到达冠层的辐射量（MJ m^-2 day^-1）</param>
    public static double ReferenceCropEvapotranspiration(double dailyMaxTemperature, double dailyMinTemperature,
                                                         double dailyMaxRelativeHumidity, double dailyMinRelativeHumidity,
                                                         double windSpeed, double solarRadiation)
    {
        double dailyAverageTemperature = (dailyMaxTemperature + dailyMinTemperature) / 2.0;
        //double dailyAverageRelativeHumidity = (dailyMaxRelativeHumidity + dailyMinRelativeHumidity) / 2.0;

        double G = 0;           //土壤热流通量，时间跨度为整日或更长时为0
        double y = 0.665E-3;    //干湿表常数

        double k = SlopeOfSaturationVapourPressureTemperature(dailyAverageTemperature);         //饱和水气压曲线斜率
        double es = MeanSaturationVapourPressure(dailyMaxTemperature, dailyMinTemperature);     //日均饱和水气压
        double ea = ActualVapourPressure(dailyMaxTemperature, dailyMinTemperature, dailyMaxRelativeHumidity, dailyMinRelativeHumidity); //日均实际水气压

        double Rn = NetRadiation_ClearSky(solarRadiation, dailyMaxTemperature, dailyMinTemperature, ea);    //净辐射

        return (0.408 * k * (Rn - G) + y * (900 / (dailyAverageTemperature + 273)) * windSpeed * (es - ea)) /
               (k + y * (1 + 0.34 * windSpeed));
    }

    /// <summary>
    /// 标准情况下的作物蒸散量
    /// </summary>
    /// <param name="dailyMaxTemperature">日最高温度(℃)</param>
    /// <param name="dailyMinTemperature">日最低温度(℃)</param>
    /// <param name="dailyMaxRelativeHumidity">日最大相对湿度</param>
    /// <param name="dailyMinRelativeHumidity">日最小相对湿度</param>
    /// <param name="windSpeed">风速(m s^-1)</param>
    /// <param name="solarRadiation">到达冠层的辐射量（MJ m^-2 day^-1）</param>
    /// <param name="period">当前生长阶段</param>
    /// <returns></returns>
    public static double CropEvapotranspirationUnderStandardConditions(double dailyMaxTemperature, double dailyMinTemperature,
                                                                       double dailyMaxRelativeHumidity, double dailyMinRelativeHumidity,
                                                                       double windSpeed, double solarRadiation,
                                                                       int GC, GrowthPeriod period)
    {
        double ET0 = ReferenceCropEvapotranspiration(dailyMaxTemperature, dailyMinTemperature, dailyMaxRelativeHumidity, dailyMinRelativeHumidity, windSpeed, solarRadiation);

        double KC = GetCropCoefficient(GC, period);

        return ET0 * KC;
    }

    /// <summary>
    /// 获取作物系数
    /// </summary>
    private static double GetCropCoefficient(int GC, GrowthPeriod period)
    {
        double[] KC = MaizeParams.KC;   //作物系数

        switch (period)
        {
            case GrowthPeriod.SEEDING_STAGE:
                return KC[0];
            case GrowthPeriod.JOINTING_STAGE:
                return (GC - 8) * (KC[1] - KC[0]) / (22.0 - 8.0) + KC[0];
            case GrowthPeriod.TASSELING_STAGE:
                return KC[1];
            case GrowthPeriod.GRAIN_STAGE:
                return (GC - 21) * (KC[2] - KC[1]) / (34.0 - 21.0) + KC[1];
            default:
                return KC[2];
        }
    }
}

