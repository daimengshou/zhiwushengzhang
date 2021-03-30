using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousButton : MonoBehaviour 
{
    public GameObject disappear;
    public GameObject appear;

    public void Click()
    {
        if (disappear.activeInHierarchy == true)
        {
            StartCoroutine(GameObjectSwitch());
        }
    }

    const int AnimatorCount = 10;

    IEnumerator GameObjectSwitch()
    {
        appear.SetActive(true);

        RectTransform rtDisappear = disappear.GetComponent<RectTransform>();
        RectTransform rtAppear = appear.GetComponent<RectTransform>();

        Vector2 posDetla = new Vector2((rtDisappear.rect.width + rtAppear.rect.width) / (2.0f * AnimatorCount), 0);

        for (int i = 1; i <= AnimatorCount; i++)
        {
            rtDisappear.anchoredPosition += posDetla;
            rtAppear.anchoredPosition += posDetla;

            yield return null;
        }

        disappear.SetActive(false);
    }
}
