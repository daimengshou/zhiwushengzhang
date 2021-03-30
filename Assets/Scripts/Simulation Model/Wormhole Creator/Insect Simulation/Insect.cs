using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectSim
{
    private int initCount = 0;

    /*
     * 病虫各阶段各年龄级的数量
     * 动态模拟
     */
    private double[][] insects = new double[10][];

    /*
     * 该变量用于存储病虫第二天数量的增量
     * 主要防止数据叠加后造成的错误
     */
    private double[][] insects_detla = new double[10][];

    /*
     * 病虫各阶段的数量总和
     * 用于计算总的病虫啃食的数量
     */
    private double[] insects_stage = new double[10];

    /*
     * 用于记录各阶段病虫的积温
     * 用于后续计算数量变化
     */
    private double[] accumulaterTemperatures = new double[10];

    //各阶段个体向前推进的年龄级数的整数部分
    private int[] Ms = new int[10];

    //各阶段个体向前推进的年龄级数的小数部分
    private double[] Gs = new double[10];

    //天敌影响后的存活率
    private double[] SPs = new double[10];

    //自然存活率
    private double[] SNs = new double[10];

    private TreeModel treeModel;

    public InsectSim(int num)
    {
        for (int i = 0; i < 10; i++)
        {
            insects[i] = new double[InsectParams.AGE_SERIES[i]];
            insects_detla[i] = new double[InsectParams.AGE_SERIES[i]];
        }

        initCount = num;
    }

    public void NextDay(double temperature, double relativeHumidity)
    {
        if (accumulaterTemperatures[0] == 0)
        {
            insects[0][0] = initCount;
            insects_stage[0] = initCount;
        }
        else
            QuantityChange(relativeHumidity);

        TemperatureAccumulation(temperature);
    }

    private void TemperatureAccumulation(double temperature)
    {
        for (int i = 0; i < 10; i++)
        {
            /*
             * 如果该阶段的病虫个数为0
             * 并且其积温为0
             * 说明病虫还未发育到该阶段及后续阶段
             * 因此不累加其有效温度
             */
            if (insects_stage[i] <= 0 && accumulaterTemperatures[i] <= 0) break;

            accumulaterTemperatures[i] = InsectParams.DailyAccumulatedTemperature(temperature, i);
        }
    }

    private void QuantityChange(double relativeHumidity)
    {
        /*
         * 对每日的数据进行初始化
         * 防止数据重复
         */
        DailyDataInit(relativeHumidity);

        /*
         * 不模拟最后一个阶段
         * 即已经变成成虫的阶段
         */
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < insects[i].Length; j++)
            {
                SetCount(i, j);
            }
        }

        /*
         * 根据每个阶段每个年龄级数的增量
         * 动态变化病虫群落的数量
         */
        for (int i = 0; i < 10; i++)
        {
            insects_stage[i] = 0;

            for (int j = 0; j < insects[i].Length; j++)
            {
                insects[i][j] += insects_detla[i][j];
                insects_stage[i] += insects_detla[i][j];
            }
        }
    }

    private void SetCount(int i, int j)
    {
        if (insects[i][j] == 0) return;

        /*
         * 将病虫划分为两部分
         * 一部分的年龄增长M+1
         * 另一部分的年龄增长M
         */
        double detla0 = Gs[i] * insects[i][j];
        double detla1 = insects[i][j] - detla0;

        //增长后的年龄
        int age0 = j + Ms[i] + 1;
        int age1 = j + Ms[i];

        //当前阶段的年龄级数
        int ageSeries = InsectParams.AGE_SERIES[i];

        /*
         * 当病虫增长后的年龄超过了当前阶段的上限
         * 则阶段向前进一
         * 当阶段发生改变时，有自然存活率SN影响
         * 无论阶段有无发生变化，均有天敌SP影响
         */
        if (age0 >= ageSeries)
            insects_detla[i + 1][age0 - ageSeries] += SPs[i] * SNs[i] * detla0;
        else
            insects_detla[i][age0] += SPs[i] * detla0;

        if (age1 >= ageSeries)
            insects_detla[i + 1][age1 - ageSeries] += SPs[i] * SNs[i] * detla1;
        else
            insects_detla[i][age1] += SPs[i] * detla1;

        insects[i][j] = 0;
    }

    private void DailyDataInit(double relativeHumidity)
    {
        InsectDetlaInit();
        EffectFactorInit(relativeHumidity);
    }

    private void InsectDetlaInit()
    {
        for (int i = 0; i < insects_detla.Length; i++)
            for (int j = 0; j < insects_detla[i].Length; j++)
                insects_detla[i][j] = 0;
    }

    private void EffectFactorInit(double relativeHumidity)
    {
        for (int i = 0; i < insects.Length; i++)
        {
            double developmentRate = InsectParams.DevelopmentRate(i, accumulaterTemperatures[i]);
            double ageAdvance = InsectParams.AGE_SERIES[i] * developmentRate;

            Ms[i] = (int)ageAdvance;
            Gs[i] = ageAdvance - Ms[i];

            SPs[i] = InsectParams.SurvivalRate_Enemies(i, developmentRate);
            SNs[i] = InsectParams.SurvicalRate_Natural(relativeHumidity, i);
        }
    }

    public double TotalIntakes()
    {
        double intakes = 0;

        for (int i = 0; i < 10; i++)
            intakes += (insects_stage[i] * InsectParams.INTAKES[i]);

        return intakes;
    }

    public override string ToString()
    {
        string str = "";

        str += "------------ POPULATION DETAILS -------------\n";

        for (int i = 0; i < insects.Length; i++)
        {
            switch (i)
            {
                case 0: str += "SPAWN       "; break;
                case 1: str += "1ST INSTAR  "; break;
                case 2: str += "2ND INSTAR  "; break;
                case 3: str += "3RD INSTAR  "; break;
                case 4: str += "4TH INSTAR  "; break;
                case 5: str += "5TH INSTAR  "; break;
                case 6: str += "6TH INSTAR  "; break;
                case 7: str += "PREPUPA     "; break;
                case 8: str += "PUPA        "; break;
                case 9: str += "ADULT       "; break;
            }

            for (int j = 0; j < insects[i].Length; j++)
                str += insects[i][j].ToString("f2") + "\t";

            str += "\n";
        }

        str += "----------- POPULATION INFORMATION ----------\n";

        for (int i = 0; i < 10; i++)
        {
            switch (i)
            {
                case 0: str += "SPAWN\t\t"; break;
                case 1: str += "1ST INSTAR\t"; break;
                case 2: str += "2ND INSTAR\t"; break;
                case 3: str += "3RD INSTAR\t"; break;
                case 4: str += "4TH INSTAR\t"; break;
                case 5: str += "5TH INSTAR\t"; break;
                case 6: str += "6TH INSTAR\t"; break;
                case 7: str += "PREPUPA\t\t"; break;
                case 8: str += "PUPA\t\t"; break;
                case 9: str += "ADULT\t\t"; break;
            }

            str += insects_stage[i].ToString("f2") + "\n";
        }

        str += "INTAKES\t\t" + TotalIntakes() + " CM^2\n";

        str += "-------------------- END --------------------";

        return str;
    }
}
