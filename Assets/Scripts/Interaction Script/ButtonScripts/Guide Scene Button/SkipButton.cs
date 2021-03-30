using UnityEngine;
using UnityEngine.UI;

public class SkipButton : MonoBehaviour 
{
    public int SceneIndex = 0;
    private Text label;
    
    void Start()
    {
        label = transform.Find("Text").GetComponent<Text>();
    }

    public void PointerEnter()
    {
        label.fontSize = 35;
    }

    public void PointerExit()
    {
        label.fontSize = 30;
    }

    public void Click()
    {
        SceneSwitch.SwitchTo(SceneIndex);
    }
}
