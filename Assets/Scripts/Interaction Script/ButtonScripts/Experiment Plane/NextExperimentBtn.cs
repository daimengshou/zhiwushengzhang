using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlantSim.Buttons
{
    [RequireComponent(typeof(Button))]
    public class NextExperimentBtn :BaseLabelBtn
    {
        [SerializeField]
        private int NextSceneIndex;
        [SerializeField]
        private Button button;

        protected override void Start()
        {
            base.Start();

            if (button == null)
                button = GetComponent<Button>();
        }

        private void Update()
        {
            button.interactable = !LScene.GetInstance().IsPlaying();
        }

        public void Click()
        {
            SceneSwitch.SwitchTo(NextSceneIndex);
        }
    }
}
