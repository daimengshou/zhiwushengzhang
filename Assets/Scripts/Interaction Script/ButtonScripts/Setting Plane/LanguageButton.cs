using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Dropdown>().value =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            0 : 1;

        LanguageNotification.GetInstance().AddListener(UpdateLabel);
	}

    public void ValueChange(int value)
    {
        LScene.GetInstance().Language =
            value == 0 ?
            SystemLanguage.Chinese :
            SystemLanguage.English;

        LanguageNotification.GetInstance().Notification();
    }

    public void UpdateLabel()
    {
        transform.parent.GetComponent<Text>().text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            "语言" : "Language";
    }


}

/// <summary>
/// 消息机制
/// 增强代码耦合性
/// 通过添加监听事件绑定语言切换时需要处理的对象
/// 当发生语言切换或必要时刻时
/// 通知各监听者执行对应的程序
/// </summary>
public class LanguageNotification
{
    private static LanguageNotification instance = null;

    public static LanguageNotification GetInstance()
    {
        if (instance == null)
            instance = new LanguageNotification();

        return instance;
    }

    public static void Destroy()
    {
        instance = null;
    }

    public delegate void UpdateDelegate();
    private UpdateDelegate Update = null;

    public void AddListener(UpdateDelegate listener)
    {
        if (Update == null)
            Update = new UpdateDelegate(listener);
        else
            Update += listener;
    }

    public void RemoveListener(UpdateDelegate listener)
    {
        if (Update != null)
            Update -= listener;
    }

    public void Notification()
    {
        Update();
    }
}
