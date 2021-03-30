using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cakeslice;

namespace PlantSim.Buttons
{
    [RequireComponent(typeof(Button))]
    public class ExperimentBtn : BaseLabelBtn
    {
        [SerializeField]
        protected string ReplaceText_Chinese;
        [SerializeField]
        protected string ReplaceText_English;

        private Button button;

        protected override void Start()
        {
            base.Start();

            if (button == null)
                button = GetComponent<Button>();
        }

        private void Update()
        {
            /*
              * 当仍处于动画中时
              * 禁止按钮可交互
              * 防止数据出现
              */
            if (TreeAnimator.IsPlaying() || CameraMove.IsPlay())
                button.interactable = false;
            else
                button.interactable = true;
        }
        
        public void Click()
        {
            base.LabelText_Chinese = ReplaceText_Chinese;
            base.LabelText_English = ReplaceText_English;
            UpdateLabel();

            StartCoroutine(NextWithDuration());
        }

        private IEnumerator NextWithDuration()
        {
            LScene scene = LScene.GetInstance();

            for (int i = 0; i < LScene.GetInstance().Duration; i++)
            {
                scene.NextDay();

                yield return new WaitWhile(TreeAnimator.IsPlaying);
            }

            OutlineEffect.Instance.UpdateOutlineControl();
        }
    }
}
