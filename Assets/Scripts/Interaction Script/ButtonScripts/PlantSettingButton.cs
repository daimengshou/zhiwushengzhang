using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlantSettingButton : MonoBehaviour
{
    private static int count = 0;

    Transform buttonTransform;
    Transform arrowTransform;
    SpriteRenderer buttonSpriteRenderer;

    public GameObject PlantSettingPlane;    //对应的界面
    public TreeGroup TreeGroup;

	// Use this for initialization
	void Start () {
        count++;

        buttonTransform = transform.Find("Button");
        arrowTransform = transform.Find("Arrow");

        buttonSpriteRenderer = buttonTransform.GetComponent<SpriteRenderer>();

	}
	
	// Update is called once per frame
	void Update () {
        LookAtCamera();
        Flash();
	}

    private Vector3 normalScale = new Vector3(1.5f, 1.5f, 1.5f);
    private Vector3 highLightScale = new Vector3(1.8f, 1.8f, 1.8f);

    public void OnMouseEnter()
    {
        buttonTransform.localScale = highLightScale;
    }

    public void OnMouseExit()
    {
        buttonTransform.localScale = normalScale;
    }

    public void OnMouseDown()
    {
        if (PlantSettingPlane == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        //重置所有输入
        Input.ResetInputAxes();

        /*
         * 设置植物属性面板的值
         * 并显示
         */
        PlantSettingPlane.GetComponent<PlantSettingPlane>().EnvironmentParams = TreeGroup.EnvironmentParams;
        PlantSettingPlane.SetActive(true);
    }

    /// <summary>
    /// 调用该函数，使该按钮始终对向显示的相机
    /// </summary>
    void LookAtCamera()
    {
        transform.LookAt(Camera.main.transform);

        Transform arrowTransform = transform.Find("Arrow");
        Vector3 arrowRotation = arrowTransform.rotation.eulerAngles;
        arrowRotation.x = 0;
        arrowTransform.rotation = Quaternion.Euler(arrowRotation);
    }

    float alphaDetla = 0.02f;   //透明值变化量，用于改变其闪烁的频率
    void Flash()
    {
        buttonSpriteRenderer.color -= new Color(0, 0, 0, alphaDetla);

        if (buttonSpriteRenderer.color.a <= 0 || buttonSpriteRenderer.color.a >= 1)
            alphaDetla *= -1;
    }
}
