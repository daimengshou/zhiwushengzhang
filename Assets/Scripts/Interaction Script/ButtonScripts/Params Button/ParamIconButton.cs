using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RectTransform))]
public class ParamIconButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    IDragHandler
{
    [Serializable]
    public class ParamIconBtnEvent : SerializableCallback<BaseTree, bool> { }

    private Image image;
    [SerializeField]
    private RectTransform imageRect;
    private RectTransform imageContainerRect;
    private Transform imageTransform;

    [SerializeField]
    private ParamIconBtnEvent onEnterTree = null;

    [SerializeField]
    private Image tipImage;
    [SerializeField]
    private float duration = 0.25f;

    private ParamButtonsPlane container;

    private Vector3 defaultPos;

    public bool warning = false;
    [SerializeField]
    private Color labelColor = Color.black;
    [SerializeField]
    private Color warnLabelColor = Color.red;
    [SerializeField]
    private string label_tw;
    [SerializeField]
    private string label_en;
    [SerializeField]
    private string warnLabel_tw;
    [SerializeField]
    private string warnLabel_en;
    // Start is called before the first frame update
    void Start()
    {
        if (image == null)
            image = GetComponent<Image>();

        if (imageRect == null)
            imageRect = GetComponent<RectTransform>();

        if (imageTransform == null)
            imageTransform = imageRect.transform;

        if (imageContainerRect == null)
            imageContainerRect = imageTransform.parent.GetComponent<RectTransform>();

        if (container == null)
            container = transform.parent.GetComponent<ParamButtonsPlane>();

        defaultPos = imageRect.position;
    }

    private static Vector3 ENTER_SCALE = new Vector3(1.2f, 1.2f, 1.2f);
    private static Vector3 DEFAULT_SCALE = Vector3.one;

    public void OnPointerEnter(PointerEventData eventData)
    {
        imageTransform.localScale = ENTER_SCALE;
        container.Highlight(imageRect);

        SetTipActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        imageTransform.localScale = DEFAULT_SCALE;
        container.Unhighlight(imageRect);

        SetTipActive(false);
    }

    private Vector3 offset;
    public void OnPointerDown(PointerEventData eventData)
    {
        offset = Vector3.zero;

        if (imageContainerRect != null && RectTransformUtility.RectangleContainsScreenPoint(imageRect, eventData.position, eventData.enterEventCamera))
        {
            Vector3 mousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(imageRect, eventData.position, eventData.enterEventCamera, out mousePos))
                offset = mousePos - imageRect.position;

            LScene.GetInstance().IsSettingParmsUsingIcon = true;
            image.raycastTarget = false;

            SetTipActive(false);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        foreach (var treeGroup in LScene.GetInstance().TreeGroups)
        {
            if (treeGroup.Selected)
            {
                if (onEnterTree.target != null)
                    warning = !onEnterTree.Invoke(treeGroup);


                StartCoroutine(ShowLabel(eventData));
            }
        }

        imageRect.position = defaultPos;

        LScene.GetInstance().IsSettingParmsUsingIcon = false;
        GetComponent<Image>().raycastTarget = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (image.raycastTarget) return;

        Vector3 mousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(imageRect, eventData.position, eventData.enterEventCamera, out mousePos))
        {
            imageRect.position = mousePos - offset;
        }
    }

    private void SetTipActive(bool value)
    {
        if (tipImage == null) return;

        tipImage.gameObject.SetActive(value);
        if (value)
            StartCoroutine(EnterAnimationCoroutine());
    }

    private IEnumerator EnterAnimationCoroutine()
    {
        if (tipImage != null)
        {
            for (float i = 0; i < duration; i += Time.deltaTime)
            {
                Color tipColor = tipImage.color;
                tipColor.a = Mathf.SmoothStep(0, 1, i / duration);
                tipImage.color = tipColor;

                yield return null;
            }
        }
    }

    private Vector3 detlaMove = new Vector3(0, 1, 0);
    private IEnumerator ShowLabel(PointerEventData eventData)
    {
        var g = ObjectPool.Instance.GetGameObject();
        var label = g.GetComponent<Text>();

        label.color = 
            warning ? warnLabelColor : labelColor;

        if (label != null)
        {
            if (warning)
            {
                label.text =
                    LScene.GetInstance().Language == SystemLanguage.Chinese ?
                    warnLabel_tw : warnLabel_en;
            }
            else
            {
                label.text =
                    LScene.GetInstance().Language == SystemLanguage.Chinese ?
                    label_tw : label_en;
            }

            Vector3 mousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(label.rectTransform, eventData.position, eventData.enterEventCamera, out mousePos))
                label.rectTransform.position = mousePos;

            for (int i = 0; i < 40; i++)
            {
                yield return null;
                label.rectTransform.position += detlaMove;
            }
        }

        ObjectPool.Instance.RecycleGameObject(g);
    }
}
