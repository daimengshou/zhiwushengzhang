
namespace PlantSim.Buttons
{
    public enum TemperatureType
    {
        High,
        Normal,
        Low
    }

    public class TemperatureButton : BaseParamEventBtn
    {
        public TemperatureType TemperatureType;
        /// <summary>
        /// 在温度实验中进入植物调用的函数
        /// </summary>
        /// <param name="baseTree">进入的植物</param>
        /// <returns>是否调用成功</returns>
        public override bool OnEnterTree(BaseTree baseTree)
        {
            EnvironmentParams envir = baseTree.EnvironmentParams;

            switch(TemperatureType)
            {
                case TemperatureType.High:
                    envir.DailyMaxTemperature = 30;
                    envir.DailyMinTemperature = 30;
                    break;
                case TemperatureType.Normal:
                    envir.DailyMaxTemperature = 25;
                    envir.DailyMinTemperature = 25;
                    break;
                case TemperatureType.Low:
                    envir.DailyMaxTemperature = 20;
                    envir.DailyMinTemperature = 20;
                    break;
            }
            envir.TemperatureType = TemperatureType;
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
