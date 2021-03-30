using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeImage : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(Fade());
	}
	
    public int AnimatorCount = 100;

    IEnumerator Fade()
    {
        RawImage rawImage = GetComponent<RawImage>();

        float alphaDetla = 1.0f / AnimatorCount;
        Color colorDetla = new Color(0, 0, 0, alphaDetla);

        for (int i = 1; i <= AnimatorCount; i++)
        {
            rawImage.color -= colorDetla;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
