using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImageButton : MonoBehaviour
{

    public GameObject panel;

    public void Click()
    {
        panel.SetActive(true);
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
