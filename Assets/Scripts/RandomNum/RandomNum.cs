using System;
using System.Collections;
using System.Collections.Generic;

public class RandomNumer
{
    /// <summary>
    /// 返回整型
    /// </summary>
    /// <returns></returns>
    public static int Int()
    {
        return GetRandom().Next();
    }

    public static int Int(int maxValue)
    {
        return GetRandom().Next(maxValue);
    }

    public static int Int(int minValue, int maxValue)
    {
        return GetRandom().Next(minValue, maxValue);
    }

    public static int PossionVariable_Int(double lambda, int minValue, int maxValue)
    {
        int result = PossionVariable_Int(lambda);

        while (result < minValue || result > maxValue)  //循环至给出符合条件的数值
        {
            result = PossionVariable_Int(lambda);
        }

        return result;
    }

    /// <summary>
    /// 产生符合泊松分布的随机数
    /// 参考代码：https://en.wikipedia.org/wiki/Poisson_distribution
    /// </summary>
    public static int PossionVariable_Int(double lambda)
    {
        const double STEP = 500;

        double lambdaLeft = lambda;
        int k = 0;
        double p = 1;

        do 
        {
            k++;
            p = p * Double();
            while (p < 1 && lambdaLeft > 0)
            {
                if (lambdaLeft > STEP)
                {
                    p *= Math.Exp(STEP);
                    lambdaLeft -= 500;
                }
                else
                {
                    p *= Math.Exp(lambdaLeft);
                    lambdaLeft = 0;
                }
            }
        } while (p > 1);

        return k - 1;
    }

    /// <summary>
    /// 符合一元二次分布规律的随机点
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    public static int ParabolaVariable_Int(int minValue, int maxValue)
    {
        decimal denominator = (((decimal)Math.Pow(maxValue, 3) - (decimal)Math.Pow(minValue, 3)) / 3 - (maxValue + minValue) * ((decimal)Math.Pow(maxValue, 2) - (decimal)Math.Pow(minValue, 2)) / 2 + (decimal)Math.Pow(maxValue + minValue, 2) * (maxValue - minValue)) / 4;
        decimal a = 1M / (decimal)denominator;
        decimal b = -a * (minValue + maxValue);
        decimal c = (decimal)Math.Pow(maxValue + minValue, 2) * a / 4;

        return ParabolaVariable_Int(a, b, c, minValue, maxValue);
    }

    /// <summary>
    /// 一元二次方程组的微分方程
    /// </summary>
    private static decimal Differential_Parabola(decimal a, decimal b, decimal c, int x)
    {
        return a * (decimal)Math.Pow(x, 3) / 3 + b * (decimal)Math.Pow(x, 2) / 2 + c * (decimal)x;
    }

    private static int ParabolaVariable_Int(decimal a, decimal b, decimal c, int minValue, int maxValue)
    {
        decimal p = (decimal)Double();

        decimal min_DifferentialValue = Differential_Parabola(a, b, c, minValue);   //minValue的微分值

        int leftValue = minValue;
        int rightValue = maxValue;

        while((rightValue - leftValue) > 1)
        {
            int midValue = (leftValue + rightValue) / 2;
            decimal mid_DifferentialValue = Differential_Parabola(a, b, c, midValue);

            if ((mid_DifferentialValue - min_DifferentialValue) > p)
            {
                rightValue = midValue;
            }
            else
            {
                leftValue = midValue;
            }
        }

        return rightValue;
    }

    public enum DistributionFunction
    {
        Uniform, Possion, Parabola
    }

    //生成无重复的多个整型
    public static int[] Ints_Unrepeating(int count, int minValue, int maxValue, DistributionFunction _Distribution = DistributionFunction.Uniform)
    {
        int[] ints = new int[count];    //输出的整型数组

        int[] ranges = new int[maxValue - minValue + 1];    //将取值范围内所有的数组都排列出来
        for (int i = minValue; i <= maxValue; i++)
        {
            ranges[i - minValue] = i;
        }

        double lambda = 0;
        if (_Distribution == DistributionFunction.Possion)
            lambda = (minValue + maxValue) / 2.0;

        for (int i = 0; i < count; i++ )
        {
            int index = Int(minValue, maxValue) - minValue;    //取值范围内的下标

            if (_Distribution == DistributionFunction.Uniform)
                index = Int(minValue, maxValue) - minValue;    //取值范围内的下标
            else if (_Distribution == DistributionFunction.Possion)
            {
                index = PossionVariable_Int(lambda, minValue, maxValue);
            }
            else
            {
                index = ParabolaVariable_Int(minValue, maxValue);
            }

            ints[i] = ranges[index];    //赋值给输出的数组

            ranges[index] = ranges[maxValue - minValue];    //替换当前位置的值，防止重复
            maxValue--;
        }

        return ints;
    }

    /// <summary>
    /// 获取多个随机整型
    /// </summary>
    /// <param name="count">整型个数</param>
    /// <param name="minValue">最大值</param>
    /// <param name="maxValue">最小值</param>
    /// <param name="_Distribution">分布规律</param>
    public static int[] Ints(int count, int minValue, int maxValue, DistributionFunction _Distribution = DistributionFunction.Uniform)
    {
        int[] ints = new int[count];

        decimal a = 0, b = 0, c = 0;

        if (_Distribution == DistributionFunction.Parabola)
        {
            decimal d = ((decimal)Math.Pow(maxValue, 3) - (decimal)Math.Pow(minValue, 3)) / 3 - (maxValue + minValue) * ((decimal)Math.Pow(maxValue, 2) - (decimal)Math.Pow(minValue, 2)) / 2 + (decimal)Math.Pow(maxValue + minValue, 2) * (maxValue - minValue) / 4;
            a = 1 / d;
            b = -a * (minValue + maxValue);
            c = a * (decimal)Math.Pow(minValue + maxValue, 2) / 4;
        }
        

        for (int i = 0; i < count; i++ )
        {
            switch (_Distribution)
            {
                case DistributionFunction.Uniform :
                    ints[i] = Int(minValue, maxValue + 1); break;
                case DistributionFunction.Possion:
                    ints[i] = PossionVariable_Int((maxValue + minValue) / 2.0, minValue, maxValue);break;
                case DistributionFunction.Parabola:
                    ints[i] = ParabolaVariable_Int(a, b, c, minValue, maxValue);break;
            }
        }

        return ints;
    }

    public static float Single()
    {
        return Convert.ToSingle(Double());
    }

    public static double Double()
    {

        return GetRandom().NextDouble();
    }

    private static Random GetRandom()
    {
        //产生随机数  参考代码：https://www.cnblogs.com/xiaowie/p/8759837.html
        byte[] randomBytes = new byte[4];
        System.Security.Cryptography.RNGCryptoServiceProvider rngServiceProivider = new System.Security.Cryptography.RNGCryptoServiceProvider();
        rngServiceProivider.GetBytes(randomBytes);
        int iSeed = BitConverter.ToInt32(randomBytes, 0);

        return new Random(iSeed);
    }
}
