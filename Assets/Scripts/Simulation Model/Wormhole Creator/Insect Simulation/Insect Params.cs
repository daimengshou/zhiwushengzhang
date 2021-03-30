using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectParams
{
    //天敌影响后的存活率
    private static double[] SURVIVAL_RATES_ENEMIES = 
        new double[10] { 0.781, 0.772, 0.773, 0.572, 0.678, 0.684, 0.457, 1.0, 0.898, 1.0 };

    //自然存活率
    private  static double[] SURVIVAL_RATES_NATURAL =
        new double[10] { 0.957, 0, 0, 0.839, 0.751, 0.841, 0.908, 1.0, 0.951, 1.0 };

    //发育所需的最低温度
    public static double[] DEVELOPMENT_TEMPERATURE_MIN =
        new double[10] { 13.1, 7.7, 7.7, 7.7, 7.7, 7.7, 7.7, 7.7, 12.4, 9.0 };

    //发育所需的积温
    public static double[] DEVELOPMENT_ACCUMULATED_TEMPERATURE =
        new double[10] { 45.3, 58.5, 44.3, 41.8, 43.5, 50.6, 114.8, 38.5, 122.4, 230 };

    //各阶段的年龄级数
    public static int[] AGE_SERIES =
        new int[10] { 9, 12, 9, 8, 9, 10, 23, 8, 24, 46 };

    public static double[] INTAKES =
        new double[10] { 0, 0, 1.71, 3.735, 40.64, 322.59, 605.63, 0, 0, 0 };

    public static double SurvivalRate_Enemies(int index, double developmentRate)
    {
        return Math.Pow(SURVIVAL_RATES_ENEMIES[index], developmentRate);
    }

    public static double SurvicalRate_Natural(double relativeHumidity, int index)
    {
        switch (index)
        {
            case 1:
                return (1.811 * relativeHumidity - 68.635) * 0.01;
            case 2:
                return 1.4056 * Math.Exp(-33.19 / relativeHumidity);
            default:
                return SURVIVAL_RATES_NATURAL[index];
        }
    }

    public static double DailyAccumulatedTemperature(double temperature, int index)
    {
        /*
         * 当当日的温度低于发育所需的最低温度时
         * 当日的积温为0
         * 否则则为与最低发育温度之差
         */
        return temperature <= DEVELOPMENT_TEMPERATURE_MIN[index] ? 
            0 : temperature - DEVELOPMENT_TEMPERATURE_MIN[index];
    }

    public static double DevelopmentRate(int index, double accumulatedTemperature)
    {
        return  accumulatedTemperature <= 0 ? 0 :
                accumulatedTemperature >= DEVELOPMENT_ACCUMULATED_TEMPERATURE[index] ? 1 : 
                accumulatedTemperature / DEVELOPMENT_ACCUMULATED_TEMPERATURE[index];
    }
}