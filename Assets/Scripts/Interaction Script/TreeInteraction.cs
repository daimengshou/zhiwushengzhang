using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInteraction : MonoBehaviour
{
    public GameObject PropertyPlane;
    public GameObject PlantSettingPlane;

    public BaseTree BaseTree;

    public void ShowParamsPlane()
    {
        PropertyPlane.SetActive(false);

        PlantSettingPlane.GetComponent<PlantSettingPlane>().EnvironmentParams = BaseTree.EnvironmentParams;
        PlantSettingPlane.SetActive(true);
    }

    public void ShowPropertyPlane()
    {
        if (PropertyPlane.activeSelf || LScene.GetInstance().IsSettingParmsUsingIcon) return;

        PropertyPlane component = PropertyPlane.GetComponent<PropertyPlane>();

        component.GrowthCycle = BaseTree.GrowthCycle;
        component.Biomass = BaseTree.Biomass;
        component.Height = BaseTree.Height;
        component.LeafArea = BaseTree.LeafArea;

        component.WaterContent = BaseTree.EnvironmentParams.WaterContent;
        component.NutrientType = BaseTree.EnvironmentParams.NutrientType;
        component.HaveInsects = BaseTree.EnvironmentParams.HaveInsects;

        component.TemperatureType = BaseTree.EnvironmentParams.TemperatureType;
        component.SunshineType = BaseTree.EnvironmentParams.SunshineType;

        PropertyPlane.SetActive(true);
    }

    public void UnshowPropertyPlane()
    {
        PropertyPlane.SetActive(false);
    }
}
