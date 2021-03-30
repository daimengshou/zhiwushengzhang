using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour {

    private Text label;

    void Start()
    {
        UpdateLabel();
        LanguageNotification.GetInstance().AddListener(UpdateLabel);
    }

    void Update()
    {
        KeyDown();
    }

    public void BackToGame()
    {
        transform.parent.gameObject.SetActive(false);
        PointerExit();
    }

    public void BackToWindows()
    {
        LScene.Quit();
    }

    public void PointerEnter()
    {
        transform.Find("Text").GetComponent<Text>().fontSize = 22;
    }

    public void PointerExit()
    {
        transform.Find("Text").GetComponent<Text>().fontSize = 20;
    }

    public void KeyDown()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            BackToGame();
    }

    public void UpdateLabel()
    {
        if (label == null)
            label = transform.Find("Text").GetComponent<Text>();

        if (transform.name == "Return Button")
            label.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                "返回" : "Back To Game";
        else
            label.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                "回到桌面" : "Back To Windows";
    }
}
