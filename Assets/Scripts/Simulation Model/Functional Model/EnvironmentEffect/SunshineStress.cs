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

public static double SunshineStress(TreeModel tree)
    {
        //获取tree的environmentParams
        EnvironmentParams envirParams = tree.EnvironmentParams;
        //计算Sunshine Factor
        
        //  高光照 1.5
        //  正常 1.0
        //  低光照 0.6

        // 返回回去
        return  envirParams.SunshineFactor;
    }
}



