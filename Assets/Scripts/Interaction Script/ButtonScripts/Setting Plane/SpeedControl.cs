using UnityEngine;
using UnityEngine.UI;

namespace PlantSim.Buttons
{
    public class SpeedControl : BaseLabelBtn
    {
        [SerializeField]
        private Slider slider;
        [SerializeField]
        private Text speedLabel;

        private float[] speedLevel = { 0.25f, 0.5f, 1f, 1.5f, 2f };
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            if (slider == null)
                slider = GetComponent<Slider>();

            slider.value = (int)(LScene.GetInstance().PlaybackSpeed / 0.5f);
            UpdateSpeedLabel();
        }

        public void OnValueChange(float value)
        {
            if (value == 0)
                LScene.GetInstance().PlaybackSpeed = 0.25f;
            else
                LScene.GetInstance().PlaybackSpeed = value * 0.5f;

            UpdateSpeedLabel();
        }

        private void UpdateSpeedLabel()
        {
            speedLabel.text = "" + LScene.GetInstance().PlaybackSpeed.ToString("f2");
        }
    }
}
