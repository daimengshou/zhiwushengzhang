using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundToggle : MonoBehaviour {

    public GameObject soil;
    public GameObject soil_white;

    private Text label;

	// Use this for initialization
	void Start () {
        label = transform.Find("Label").GetComponent<Text>();

        LanguageNotification.GetInstance().AddListener(UpdateLabel);

        GetComponent<Toggle>().isOn = LScene.GetInstance().HaveBackground;
	}


    public void ValueChange(bool value)
    {
        LScene.GetInstance().HaveBackground = value;

        Color fontColor = value ? Color.white : Color.black;

        GameObject.Find("Day Label").GetComponent<Text>().color = fontColor;

        soil.SetActive(value);
        soil_white.SetActive(!value);

        GameObject.Find("Main Camera").GetComponent<Camera>().clearFlags = value ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
    }

    public void UpdateLabel()
    {
        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "背景" :
            "Background";
    }
}
