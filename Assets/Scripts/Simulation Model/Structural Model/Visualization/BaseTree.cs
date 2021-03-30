/*
 * 文件名： BaseTree.cs
 * 描述：由L-系统生成的植物模型的基类
 */
using System;
using UnityEngine;

[Serializable]
/*
 * 植物模型基类
 * 
 * @version: 1.0
 */
public abstract class BaseTree : MonoBehaviour, ISelected, IEnvirParams, ITreeParams
{
    public bool Selected { get; set; }

    [SerializeField]
    private EnvironmentParams m_EnvironmentParams;
    public EnvironmentParams EnvironmentParams
    {
        get { return m_EnvironmentParams; }
        set { m_EnvironmentParams = value; }
    }

    public void EnvirParamsDepthCopy(EnvironmentParams envirParams)
    {
        EnvironmentParams.DepthCopy(envirParams);
    }

    public virtual int GrowthCycle { get; set; }

    public virtual double Biomass { get; set; }

    public virtual double AbovegroundBiomass { get; set; }

    public virtual double Height { get; set; }

    public virtual double LeafArea { get; set; }
}
