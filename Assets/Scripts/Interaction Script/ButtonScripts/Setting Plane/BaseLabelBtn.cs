using UnityEngine;
using UnityEngine.UI;

namespace PlantSim.Buttons
{
    public class BaseLabelBtn : MonoBehaviour
    {
        [SerializeField]
        protected Text Label;
        [SerializeField]
        protected string LabelText_Chinese;
        [SerializeField]
        protected string LabelText_English;
        // Start is called before the first frame update
        protected virtual void Start()
        {
            if (Label != null)
                LanguageNotification.GetInstance().AddListener(UpdateLabel);
        }

        public virtual void UpdateLabel()
        {
            Label.text =
                LScene.GetInstance().Language == SystemLanguage.Chinese ?
                LabelText_Chinese : LabelText_English;
        }
    }
}
