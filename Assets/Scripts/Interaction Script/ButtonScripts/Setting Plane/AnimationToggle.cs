using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlantSim.Buttons
{
    public class AnimationToggle : BaseLabelBtn
    {
        [SerializeField]
        private List<GameObject> bindingGameObjects;
        [SerializeField]
        private List<Text> bindingLabels;

        protected override void Start()
        {
            base.Start();

            GetComponent<Toggle>().isOn = LScene.GetInstance().HaveAnimator;
        }

        private Color interactableColor = new Color(50.0f / 255, 50.0f / 255, 50.0f / 255, 1);
        private Color uninteractableColor = Color.gray;

        public void OnValueChange(bool value)
        {
            LScene.GetInstance().HaveAnimator = value;

            foreach(var g in bindingGameObjects)
            {
                g.GetComponent<Selectable>().interactable = value;
            }

            foreach(var label in bindingLabels)
            {
                label.color = value ?
                    interactableColor : uninteractableColor;
            }
        }
    }
}

//public class AnimationToggle : MonoBehaviour {

//    private Text label;
//	// Use this for initialization
//	void Start () {
//        label = transform.Find("Label").GetComponent<Text>();

//        LanguageNotification.GetInstance().AddListener(UpdateLabel);

//        GetComponent<Toggle>().isOn = LScene.GetInstance().HaveAnimator;
//	}

//    public void ValueChange(bool value)
//    {
//        LScene.GetInstance().HaveAnimator = value;

//        GameObject cameraAdaptionObject = GameObject.Find("Camera Adaption");
//        cameraAdaptionObject.GetComponent<Toggle>().interactable = value;

//        //设置相机自适应文字的颜色（无法选择时偏灰）
//        Text text = cameraAdaptionObject.transform.Find("Label").GetComponent<Text>();

//        if (value)
//            text.color = new Color(50.0f / 255, 50.0f / 255, 50.0f / 255, 1);
//        else
//            text.color = Color.gray;
            
//    }

//    public void UpdateLabel()
//    {
//        label.text =
//            LScene.GetInstance().Language == SystemLanguage.Chinese ?
//            "动画" :
//            "Animation";
//    }
//}
