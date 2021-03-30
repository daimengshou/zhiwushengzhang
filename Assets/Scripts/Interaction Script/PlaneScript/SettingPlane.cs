using UnityEngine;
using UnityEngine.UI;

public class SettingPlane : MonoBehaviour {

    private Text label;
	// Use this for initialization
	void Start () {
        LanguageNotification ln = LanguageNotification.GetInstance();

        ln.AddListener(UpdateLabel);
        ln.Notification();
	}
	
    public void UpdateLabel()
    {
        if (label == null)
            label = transform.Find("Label").Find("Text").GetComponent<Text>();

        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "设置" : "Setting";
    }
}
