using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    [SerializeField]
    private int SceneIndex;
    [SerializeField]
    private Button button;

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    private void Update()
    {
        if (LScene.GetInstance().IsPlaying())
            button.interactable = false;
        else
            button.interactable = true;
    }

    public void Click()
    {
        //SceneSwitch.SwitchTo(SceneIndex);

        StartCoroutine(SceneSwitch.SwitchTo_Coroutine(SceneIndex));
    }

    public void PointerEnter()
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }

    public void PointerExit()
    {
        transform.localScale = Vector3.one;
    }
}
