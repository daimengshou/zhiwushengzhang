
namespace PlantSim.Buttons
{
    public class WateringButton : BaseParamEventBtn
    {
        /// <summary>
        /// 在水分实验中进入植物调用的函数
        /// </summary>
        /// <param name="baseTree">进入的植物</param>
        /// <returns>是否调用成功</returns>
        public override bool OnEnterTree(BaseTree baseTree)
        {
            EnvironmentParams envir = baseTree.EnvironmentParams;

            //int index = MaizeParams.WaterContents.IndexOf(envir.WaterContent);

            //if (index <= 0)
            //    return false;
            //else
            //{
            //    envir.WaterContent = MaizeParams.WaterContents[index - 1];
            //    ChangeDayUpdate(envir);
            //    return true;
            //}
            envir.WaterContent += 0.07;
        
            ChangeDayUpdate(envir);
            return true;
        }

        public override void ChangeDayUpdate(EnvironmentParams envir)
        {
            envir.ChangeDay =
                LScene.GetInstance().Day < 1 ? 1 : LScene.GetInstance().Day;
        }
    }
}
