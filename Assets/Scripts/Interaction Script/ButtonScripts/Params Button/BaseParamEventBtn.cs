using UnityEngine;

namespace PlantSim.Buttons
{
    public abstract class BaseParamEventBtn : MonoBehaviour
    {
        public abstract bool OnEnterTree(BaseTree baseTree);

        public abstract void ChangeDayUpdate(EnvironmentParams envir);

        public virtual bool OnEnterTreeInOtherExp(BaseTree baseTree)
        {
            return false;
        }
    }
}
