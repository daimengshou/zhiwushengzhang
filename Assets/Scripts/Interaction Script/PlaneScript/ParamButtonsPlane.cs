using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamButtonsPlane : MonoBehaviour
{
    [SerializeField]
    private List<RectTransform> buttonRects = new List<RectTransform>();

    private static Vector3 deltaMove = new Vector3(5, 0);
    public void Highlight(RectTransform highlightRect)
    {
        if (!buttonRects.Contains(highlightRect)) return;

        deltaMove *= -1.0f;

        foreach(var buttonRect in buttonRects)
        {
            if (buttonRect == highlightRect)
            {
                deltaMove *= -1.0f;
                continue;
            }

            buttonRect.position += deltaMove;
        }
    }

    public void Unhighlight(RectTransform unhighlightRect)
    {
        if (!buttonRects.Contains(unhighlightRect)) return;

        foreach(var buttonRect in buttonRects)
        {
            if (buttonRect == unhighlightRect)
            {
                deltaMove *= -1.0f;
                continue;
            }

            buttonRect.position += deltaMove;
        }

        deltaMove *= -1.0f;
    }
}
