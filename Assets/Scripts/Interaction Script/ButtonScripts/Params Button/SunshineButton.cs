
namespace PlantSim.Buttons
{
    public enum SunshineType
    {
        High,
        Normal,
        Low
    }

    public class SunshineButton : BaseParamEventBtn
    {
        /// <summary>
        /// 在光照实验中进入植物调用的函数
        /// </summary>
        /// <param name="baseTree">进入的植物</param>
        /// <returns>是否调用成功</returns>

        public SunshineType SunshineType;
        public override bool OnEnterTree(BaseTree baseTree)
        {
            EnvironmentParams envir = baseTree.EnvironmentParams;

            switch (SunshineType)
            {
                case SunshineType.High:
                    envir.SunshineFactor = 1.5f;
              
                    break;
                case SunshineType.Normal:
                    envir.SunshineFactor = 1.0f;
                    break;
                case SunshineType.Low:
                    envir.SunshineFactor = 0.5f;
                    break;
            }
            envir.SunshineType = SunshineType;
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
