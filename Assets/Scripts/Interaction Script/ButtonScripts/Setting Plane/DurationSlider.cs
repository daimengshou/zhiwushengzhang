using UnityEngine;
using UnityEngine.UI;

namespace PlantSim.Buttons
{
    [RequireComponent(typeof(Slider))]
    public class DurationSlider : BaseLabelBtn
    {
        [SerializeField]
        private Slider slider;
        [SerializeField]
        private Text dayLabel;
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            if (slider == null)
                slider = GetComponent<Slider>();

            slider.value = LScene.GetInstance().Duration / 10;
            UpdateDayLabel();
        }

        public void OnValueChange(float value)
        {
            if (value == 0)
                LScene.GetInstance().Duration = 1;
            else
                LScene.GetInstance().Duration = (int)value * 10;

            UpdateDayLabel();
        }

        private void UpdateDayLabel()
        {
            dayLabel.text = "" + LScene.GetInstance().Duration;
        }
    }
}

