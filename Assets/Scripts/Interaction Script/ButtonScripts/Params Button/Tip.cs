using UnityEngine;
using UnityEngine.UI;

public class Tip : MonoBehaviour
{
    public string label_tw;
    public string label_en;

    [SerializeField]
    private Text label;


    private void OnEnable()
    {
        if (label == null)
            label = transform.GetComponentInChildren<Text>();

        label.text =
            LScene.GetInstance().Language == SystemLanguage.Chinese ?
            label_tw : label_en;
    }
}
