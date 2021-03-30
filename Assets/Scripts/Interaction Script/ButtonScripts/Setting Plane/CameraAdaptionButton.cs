using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraAdaptionButton : MonoBehaviour {

    private CameraMove mainCameraMove;
    private Text label;

	// Use this for initialization
	void Start () {
        mainCameraMove = GameObject.Find("Main Camera").GetComponent<Camera>().GetComponent<CameraMove>();

        mainCameraMove.Adaptive = true;

        label = transform.Find("Label").GetComponent<Text>();

        LanguageNotification.GetInstance().AddListener(UpdateLabel);
	}

    public void ValueChange(bool value)
    {
        mainCameraMove.Adaptive = value;
    }

    public void UpdateLabel()
    {
        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "相机自适应" :
            "Camera Adaption";
    }
}
