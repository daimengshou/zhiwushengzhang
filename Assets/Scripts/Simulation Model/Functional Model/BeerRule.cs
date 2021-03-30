 /*
 * Beer模型
 * 用于计算正常情况、缺氮、缺磷、缺钾情况下的光合速率
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BeerRule
{
    public static double BiomassCal(TreeModel treeModel)
    {
        double biomass = Normal(treeModel);

        LightResponseType type = treeModel.EnvironmentParams.NutrientType;

        switch (type)
        {
            case LightResponseType.N_LACK: biomass *= 0.8; break;
            case LightResponseType.P_LACK: biomass *= 0.75; break;
            case LightResponseType.K_LACK: biomass *= 0.94; break;
            case LightResponseType.ALL_LACK: biomass *= 0.65; break;
        }

        if (treeModel.EnvironmentParams.HaveInsects)
        {
            biomass = BiomassUnderInsected(treeModel, biomass);
        }

        return biomass;
    }

    private static double Normal(TreeModel treeModel)
    {
        List<OrganIndex> indexes = treeModel.OrganIndexes;

        double leafArea = 0;
        foreach (OrganIndex index in indexes)
        {
            if (index.Type != OrganType.Leaf) continue;

            if (index.Age > 12) continue;

            leafArea += (index as LeafIndex).LeafArea;
        }

        double SP = 3600;
        double RP = 312.608969;
        double KP = 1.170887;

        leafArea *= 10000;  //将单位转换成cm^2

        return 11 * SP / (RP * KP) * (1 - Math.Exp(-KP / SP * leafArea)) * 1.27;
    }

    private static double BiomassUnderInsected(TreeModel treeModel, double biomass)
    {
        int count_Insected = 0;
        double insectedRatio = 0;

        foreach(var index in treeModel.OrganIndexes)
        {
            if (index.Type != OrganType.Leaf) continue;
            if (index.Age > 12) continue;

            count_Insected++;
            insectedRatio += (index as LeafIndex).InsectedRatio;
        }

        return biomass * (insectedRatio / count_Insected);
    }
}
