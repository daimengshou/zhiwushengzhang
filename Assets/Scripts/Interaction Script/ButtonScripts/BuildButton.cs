using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuildButton : MonoBehaviour {

    private static BuildButton instance;

    public BuildButton Instance
    {
        get
        {
            if (instance == null)
            {
                var buttons = GameObject.FindObjectsOfType<Transform>().AsEnumerable().
                    Select(c => c.GetComponent<BuildButton>()).
                    Where(e => e != null);

                if (buttons == null || buttons.Count() == 0)
                    throw new NullReferenceException("Hava no build button");

                instance = buttons.First();
            }

            return instance;
        }
    }

    public GameObject PlaneSettings;

    private Text label;

    void Start()
    {
        LanguageNotification.GetInstance().AddListener(UpdateLabel);
    }

    public void Click()
    {
        LScene.GetInstance().LoadTrees();

        HidePlaneSettingButtons();
        SetLabel();
    }

    private void HidePlaneSettingButtons()
    {
        PlaneSettings.SetActive(false);
    }

    private void ShowPlaneSettingButtons()
    {
        PlaneSettings.SetActive(true);
    }

    private void SetLabel()
    {
        if (label == null)
            label = transform.Find("Text").GetComponent<Text>();

        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "重新开始" :
            "Restart";
    }

    public void UpdateLabel()
    {
        if (label == null)
            label = transform.Find("Text").GetComponent<Text>();

        if (label.text == "Seed" || label.text == "播种")
            label.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                "播种" : "Seed";
        else
            label.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                "重新开始" : "Restart";
    }
}
