using UnityEngine;

namespace PlantSim.Buttons
{
    public class InsertButton : BaseParamEventBtn
    {
        /// <summary>
        /// 在虫害实验中进入植物调用的函数
        /// </summary>
        /// <param name="baseTree">进入的植物</param>
        /// <returns>是否调用成功</returns>
        public override bool OnEnterTree(BaseTree baseTree)
        {
            EnvironmentParams envir = baseTree.EnvironmentParams;

            if (envir.HaveInsects)
                return false;
            else
            {
                envir.HaveInsects = true;
                ChangeDayUpdate(envir);
                return true;
            }
        }

        public override void ChangeDayUpdate(EnvironmentParams envir)
        {
            envir.ChangeDay =
                LScene.GetInstance().Day < 1 ? 1 : LScene.GetInstance().Day;
        }
    }
}
