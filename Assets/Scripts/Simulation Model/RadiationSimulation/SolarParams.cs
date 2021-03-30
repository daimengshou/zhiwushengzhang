/*
 * 该类主要用于模拟太阳，如太阳与地球的距离、太阳赤纬角等一系列参数
 * 根据这些参数可以计算出太阳到达地球时的辐射能、太阳光线的入射角度等信息
 * 可用于光线跟踪、辐射度等计算，确定植物某一瞬间或某一时间内接受的光辐射量或光分布情况
 * 
 * 该类中各参数计算的参考文献：
 * 王炳忠. 太阳辐射计算讲座 第一讲 太阳能中天文参数的计算[J]. 太阳能, 1999(2):8-10.
 * 王炳忠. 太阳辐射计算讲座 第二讲 相对于斜面的太阳位置计算[J]. 太阳能, 1999(3):8-9.
 */


using System;
using System.Collections.Generic;
using UnityEngine;


public class SolarSim
{
    /// <summary>
    /// 积日
    /// </summary>
    public static int DayNumber(int Year, int Month, int Day)
    {
        return new DateTime(Year, Month, Day).DayOfYear;
    }

    /// <summary>
    /// 太阳日角（弧度制）
    /// </summary>
    public static double DayAngle(int Year, int Month, int Day, int Hour, int Min, double Longitude)
    {
        double N0 = 79.6764 + 0.2422 * (Year - 1985) - (Year - 1985) / 4;
        double K = 2 * Math.PI / 365.2422;

        return (DayNumberCorrection(Year, Month, Day, Hour, Min, Longitude) - N0) * K;
    }

    public static double LongitudeCorrection(int Longitude_Degree, int Longitude_Minute)
    {
        return LongitudeCorrection(Longitude_Degree + Longitude_Minute / 60.0);
    }

    public static double LongitudeCorrection(double Longitude)
    {
        return Longitude / 15;
    }

    public static double HourCorrection(int Hour, int Min)
    {
        return Hour - 8 + Min / 60;
    }

    public static double DayNumberCorrection(int Year, int Month, int Day, int Hour, int Min, double Longitude)
    {
        return DayNumber(Year, Month, Day) + (HourCorrection(Hour, Min) - LongitudeCorrection(Longitude)) / 24;
    }

    /// <summary>
    /// 计算日地距离与日地平均距离比值的平方，即(r0/r)^2
    /// </summary>
    public static double DistanceBetweenSunAndEarth(int Year, int Month, int Day, int Hour, int Min, double Longitude)
    {
        return DistanceBetweenSunAndEarth(DayAngle(Year, Month, Day, Hour, Min, Longitude));
    }

    /// <param name="_DayAngle">太阳日角</param>
    public static double DistanceBetweenSunAndEarth(double DayAngle_Rad)
    {
        return 1.000423 + 0.032359 * Math.Sin(DayAngle_Rad) + 0.000086 * Math.Sin(2 * DayAngle_Rad) - 0.008349 * Math.Cos(DayAngle_Rad) + 0.000115 * Math.Cos(2 * DayAngle_Rad);
    }

    /// <summary>
    /// 计算太阳赤纬角
    /// </summary>
    public static double Declination(int Year, int Month, int Day, int Hour, int Min, double Longitude)
    {
        return Declination(DayAngle(Year, Month, Day, Hour, Min, Longitude));
    }

    /// <param name="DayAngle_Rad">太阳日角</param>
    public static double Declination(double DayAngle_Rad)
    {
        return 0.3723 + 23.2567 * Math.Sin(DayAngle_Rad) + 0.1149 * Math.Sin(2 * DayAngle_Rad) - 0.1712 * Math.Sin(3 * DayAngle_Rad) - 0.758 * Math.Cos(DayAngle_Rad)
               + 0.3656 * Math.Cos(2 * DayAngle_Rad) + 0.0201 * Math.Cos(3 * DayAngle_Rad);
    }

    /// <summary>
    /// 计算真太阳时与平太阳时（24小时）之间的时差
    /// </summary>
    public static double HourDifference(int Year, int Month, int Day, int Hour, int Min, double Longitude)
    {
        return HourDifference(DayAngle(Year, Month, Day, Hour, Min, Longitude));
    }

    /// <param name="DayAngle_Rad">太阳日角</param>
    public static double HourDifference(double DayAngle_Rad)
    {
        return 0.0028 - 1.9857 * Math.Sin(DayAngle_Rad) + 9.9059 * Math.Sin(2 * DayAngle_Rad) - 7.0924 * Math.Cos(DayAngle_Rad) - 0.6882 * Math.Cos(2 * DayAngle_Rad);
    }

    /// <summary>
    /// 真太阳时
    /// </summary>
    public static double ApparentSolarTime(int Year, int Month, int Day, int Hour, int Min, double Longitude)
    {
        return ApparentSolarTime(Hour, Min, Longitude, DayAngle(Year, Month, Day, Hour, Min, Longitude));
    }

    public static double ApparentSolarTime(int Hour, int Min, double Longitude, double _DayAngle)
    {
        return BeiJingTimeToLocalTime(Hour, Min, Longitude) + HourDifference(_DayAngle) / 60.0;
    }

    /// <summary>
    /// 太阳时角（角度制）
    /// </summary>
    public static double HourAngle(int Year, int Month, int Day, int Hour, int Min, double Longitude)
    {
        return (ApparentSolarTime(Year, Month, Day, Hour, Min, Longitude) - 12.0) * 15.0;
    }

    public static double HourAngle(int Hour, int Min, double Longitude, double DayAngle_Rad)
    {
        return (ApparentSolarTime(Hour, Min, Longitude, DayAngle_Rad) - 12.0) * 15.0;
    }

    /// <summary>
    /// 太阳高度角（角度制）
    /// </summary>
    public static double SolarAltitude(int Year, int Month, int Day, int Hour, int Min, double Longitude, double Latitude)
    {
        return SolarAltitude(Declination(Year, Month, Day, Hour, Min, Longitude), HourAngle(Year, Month, Day, Hour, Min, Longitude), Latitude);
    }

    /// <param name="SolarDeclination_Rad">太阳赤纬角（角度制）</param>
    /// <param name="SolarHourAngle_Rad">太阳时角（角度制）</param>
    /// <param name="Latitude_Rad">纬度（角度制）</param>
    public static double SolarAltitude(double SolarDeclination_Deg, double SolarHourAngle_Deg, double Latitude_Deg)
    {
        double SolarDeclination_Rad = DegToRad(SolarDeclination_Deg);
        double SolarHourAngle_Rad = DegToRad(SolarHourAngle_Deg);
        double Latitude_Rad = DegToRad(Latitude_Deg);

        return RadToDeg(Math.Asin(Math.Sin(SolarDeclination_Rad) * Math.Sin(Latitude_Rad) + Math.Cos(SolarDeclination_Rad) * Math.Cos(Latitude_Rad) * Math.Cos(SolarHourAngle_Rad)));
    }

    /// <summary>
    /// 太阳方位角（角度制）
    /// </summary>
    public static double SolarAzimuth(int Year, int Month, int Day, int Hour, int Min, double Longitude, double Latitude)
    {
        double SolarHourAngle_Deg = HourAngle(Year, Month, Day, Hour, Min, Longitude);
        double SolarDeclination_Deg = Declination(Year, Month, Day, Hour, Min, Longitude);
        double SolarAltitude_Deg = SolarAltitude(Year, Month, Day, Hour, Min, Longitude, Latitude);

        return SolarAzimuth(SolarDeclination_Deg, SolarAltitude_Deg, Latitude, SolarHourAngle_Deg);
    }

    public static double SolarAzimuth(double SolarDeclination_Deg, double SolarAltitude_Deg, double Latitude_Deg, double SolarHourAngle_Deg)
    {
        double SolarDeclination_Rad = DegToRad(SolarDeclination_Deg);
        double SolarAltitude_Rad = DegToRad(SolarAltitude_Deg);
        double Latitude_Rad = DegToRad(Latitude_Deg);

        double Cos_Azimuth = (Math.Sin(SolarAltitude_Rad) * Math.Sin(Latitude_Rad) - Math.Sin(SolarDeclination_Rad)) / (Math.Cos(SolarAltitude_Rad) * Math.Cos(Latitude_Rad));
        double Azimuth = RadToDeg(Math.Acos(Cos_Azimuth));

        /* 根据太阳的时角判断当前的时间
         * 当时角小于0时，则为午后，方位角不变
         * 当时角大于0时，则为午前，方位角要变换
         */
        if (SolarHourAngle_Deg < 0)
            return Azimuth;
        else
            return 360.0 - Azimuth;
    }

    private const int       THE_SOLAR_CONSTANT_W    = 1367;     //太阳常数（W * m^-2）
    private const double    THE_SOLAR_CONSTANT_KJ   = 4921.2;   //太阳常数（kJ * h^-1 * m^-2）
    private const double    W_TO_KJ_PER_HOUR        = 3.6;      //将单位从瓦转换到千焦每小时的转换系数

    /// <summary>
    /// 计算在日出和日落之间某一时间段内的地外辐射（W /m^2）
    /// </summary>
    public static double ExtraterrestrialIrradianceOfLight(double DayAngle_Rad)
    {
        return THE_SOLAR_CONSTANT_W * DistanceBetweenSunAndEarth(DayAngle_Rad);
    }

    /// <summary>
    /// 计算大气直射透明系数
    /// 参考文献： 陈刚, 李鑫. 基于虚拟现实的杉木形态建模及冠层辐射模拟[J]. 福建电脑, 2013, 29(6).
    /// </summary>
    public static double AtmosphericDirectTransparency(double Elevation, double SolarAltitude_Deg)
    {
        double SolarAltitude_Rad = DegToRad(SolarAltitude_Deg);

        double AtmosCorreCoffi = Math.Pow((288.0 - 0.0065 * Elevation) / 288.0, 5.2526);    //大气压修正系数
        double SeaAirVolum = Math.Sqrt(1229.0 + Math.Pow(614.0 * Math.Sin(SolarAltitude_Rad), 2.0)) - 614.0 * Math.Sin(SolarAltitude_Rad);  //海平面大气量
        double AirVolume = SeaAirVolum * AtmosCorreCoffi;   //一定地形高度下的大气量

        return 0.56 * (Math.Exp(-0.56 * AirVolume) + Math.Exp(-0.095 * AirVolume));
    }

    /// <summary>
    /// 计算大气散射透明系数
    /// </summary>
    /// <param name="AtmosDirTransparency">大气直射透明系数</param>
    public static double AtmosphericScatterTransparency(double AtmosDirTransparency)
    {
        return 0.271 - 0.294 * AtmosDirTransparency;
    }

    /// <summary>
    /// 计算某一高程某一时间段的地表直射辐射（W / m^2）
    /// </summary>
    public static double DirectIrradiance(double DayAngle_Rad, double SolarAltitude_Deg, double Elevation)
    {
        return ExtraterrestrialIrradianceOfLight(DayAngle_Rad) * AtmosphericDirectTransparency(Elevation, SolarAltitude_Deg);
    }

    /// <summary>
    /// 计算整日的地表直射辐射（MJ m^-2 d^-1）
    /// </summary>
    /// <param name="Time"></param>
    /// <param name="Longitude"></param>
    /// <param name="Latitude"></param>
    /// <returns></returns>
    public static double DailyDirectIrradiance(DateTime Time, double Longitude, double Latitude)
    {
        double dayAngle = DayAngle(Time.Year, Time.Month, Time.Day, 12, 0, Longitude); //一天的平均日角

        double riseTime = RiseTime(dayAngle, Longitude, Latitude);    //日出时间
        double fallTime = FallTime(dayAngle, Longitude, Latitude);    //日落时间

        double irradiance = 0.0;

        for (double startHour = riseTime; startHour < fallTime; startHour = startHour + 1) //以一个小时为步长计算辐射量
        {
            double endHour = startHour + 1;
            if (endHour > fallTime) endHour = fallTime;   //当结束的时间超过日落时间时，则将日落时间作为结束时间

            double midHour = (startHour + endHour) / 2;     //中间时间点
            int midHour_Int = HourToHour(midHour);          //中间时间点（小时）
            int midMin_Int = HourToMin(midHour);            //中间时间点（分钟）

            double solarDayAngle_Rad = DayAngle(Time.Year, Time.Month, Time.Day, midHour_Int, midMin_Int, Longitude);   //这段时间内的平均日角
            double solarHourAngle = HourAngle(midHour_Int, midMin_Int, Longitude, solarDayAngle_Rad);                //这段时间内的平均时角
            double solarDelication = Declination(solarDayAngle_Rad);                                                  //这段时间内的平均赤纬角

            double solarAltitude_Deg = SolarAltitude(solarDelication, solarHourAngle, Latitude);                        //这段时间内的平均太阳高度角
            double solarAzimuth_Deg = SolarAzimuth(solarDelication, solarAltitude_Deg, Latitude, solarHourAngle);       //这段时间内的平均太阳方位角

            double tempIrradiance = DirectIrradiance(solarDayAngle_Rad, solarAltitude_Deg, 2);
            irradiance += tempIrradiance * (endHour - startHour) * 60 * 60;
        }

        return irradiance / 1000000.0;
    }

    /// <summary>
    /// 返回一天的平均直射辐射（W/m^2）
    /// </summary>
    /// <param name="Time"></param>
    /// <param name="Longitude"></param>
    /// <param name="Latitude"></param>
    /// <returns></returns>
    public static double DailyDirectIrradiance_Average(DateTime Time, double Longitude, double Latitude)
    {
        double dayAngle = DayAngle(Time.Year, Time.Month, Time.Day, 12, 0, Longitude); //一天的平均日角

        double riseTime = RiseTime(dayAngle, Longitude, Latitude);    //日出时间
        double fallTime = FallTime(dayAngle, Longitude, Latitude);    //日落时间

        double wholeSeconds = (fallTime - riseTime) * 60 * 60;

        double irradiance = DailyDirectIrradiance(Time, Longitude, Latitude) / wholeSeconds;

        return irradiance * 1000000;
    }

    /// <summary>
    /// 返回一天的平均直射辐射（umol/s * m^2）
    /// </summary>
    public static double DailyDirectIrradiance_Average_PPDF(DateTime Time, double Longitude, double Latitude)
    {
        return IrradianceToPPFD(DailyDirectIrradiance_Average(Time, Longitude, Latitude));
    }

    /// <param name="ExtraterrestrialIrradianceOfLight">某一时间段内的地外辐射</param>
    /// <param name="AtmosDirTransparency">大气直射透明系数</param>
    public static double DirectIrradiance(double ExtraterrestrialIrradianceOfLight, double AtmosDirTransparency)
    {
        return ExtraterrestrialIrradianceOfLight * AtmosDirTransparency;
    }

    /// <summary>
    /// 计算某一高程某一时间段的地表散射辐射(W / m^2)
    /// </summary>
    public static double ScatterIrradiance(double DayAngle_Rad, double SolarAltitude_Deg, double Elevation)
    {
        return ExtraterrestrialIrradianceOfLight(DayAngle_Rad) * AtmosphericScatterTransparency(AtmosphericDirectTransparency(Elevation, SolarAltitude_Deg));
    }

    /// <param name="ExtraterrestrialIrradianceOfLight">某一时间段内的地外辐射</param>
    /// <param name="AtmosDirTransparency">大气直射透明系数</param>
    public static double ScatterIrradiance(double ExtraterrestrialIrradianceOfLight, double AtmosDirTransparency)
    {
        return ExtraterrestrialIrradianceOfLight * AtmosphericScatterTransparency(AtmosDirTransparency);
    }

    /// <summary>
    /// 日出时间（北京真太阳时）
    /// </summary>
    public static double RiseTime(int Year, int Month, int Day, int Hour, int Min, double Longitude, double Latitude)
    {
        return RiseTime(DayAngle(Year, Month, Day, Hour, Min, Longitude), Longitude, Latitude);
    }

    public static double RiseTime(double DayAngle_Rad, double Longitude_Deg, double Latitude_Deg)
    {
        double Latitude_Rad = DegToRad(Latitude_Deg);
        double SolarDeclination_Rad = DegToRad(Declination(DayAngle_Rad));

        double SolarRiseAngle_Deg = RadToDeg(-Math.Acos(-Math.Tan(Latitude_Rad) * Math.Tan(SolarDeclination_Rad)));

        return LocalTimeToBeiJingTime(12.0 - (Math.Abs(SolarRiseAngle_Deg)) / 15.0, Longitude_Deg);
    }

    /// <summary>
    /// 日落时间（北京真太阳时）
    /// </summary>
    public static double FallTime(int Year, int Month, int Day, int Hour, int Min, double Longitude, double Latitude)
    {
        return FallTime(DayAngle(Year, Month, Day, Hour, Min, Longitude), Longitude, Latitude);
    }

    public static double FallTime(double DayAngle_Rad, double Longitude_Deg, double Latitude_Deg)
    {
        double Latitude_Rad = DegToRad(Latitude_Deg);
        double SolarDeclination_Rad = DegToRad(Declination(DayAngle_Rad));

        double SolarFallAngle_Deg = RadToDeg(Math.Acos(-Math.Tan(Latitude_Rad) * Math.Tan(SolarDeclination_Rad)));

        return LocalTimeToBeiJingTime(12.0 + (Math.Abs(SolarFallAngle_Deg)) / 15.0, Longitude_Deg);
    }

    /// <summary>
    /// 计算单条太阳光线
    /// </summary>
    /// <param name="TopPoint">假设标杆的顶点位置</param>
    public static Ray LightRay(int Year, int Month, int Day, int Hour, int Min, double Longitude, double Latitude, Vector3 TopPoint)
    {
        double SolarDayAngle = DayAngle(Year, Month, Day, Hour, Min, Longitude);    //太阳日角
        double SolarHourAngle = HourAngle(Hour, Min, Longitude, SolarDayAngle);     //太阳时角
        double SolarDeclination = Declination(SolarDayAngle);                       //太阳赤纬角

        double SolarAltitude_Deg = SolarAltitude(SolarDeclination, SolarHourAngle, Latitude);   //太阳高度角（角度制）

        double SolarAzimuth_Deg = SolarAzimuth(SolarDeclination, SolarAltitude_Deg, Latitude, SolarHourAngle);  //太阳方位角（角度制）

        return LightRay(SolarAltitude_Deg, SolarAzimuth_Deg, TopPoint);
    }

    public static Ray LightRay(double SolarAltitude_Deg, double SolarAzimuth_Deg, Vector3 TopPoint)
    {
        double SolarAltitude_Rad = DegToRad(SolarAltitude_Deg); //太阳高度角（弧度制）
        double SolarAzimuth_Rad = DegToRad(SolarAzimuth_Deg);   //太阳方位角（弧度制）

        /* 思路说明：
         * 假设地面上有一根顶点在TopPoint处，且与地面垂直的标杆
         * 当太阳照射时，该标杆在水平的地面上产生阴影
         * 则从标杆的顶端到阴影的顶端的向量就是太阳光的入射方向
         * 由于太阳的真实位置在距离物体极远的地方，不便于计算
         * 因此假定与标杆顶端相距10个逆射线距离的点为光线的起始位置
         */
        Vector3 EndPoint = SolarAltitude_Deg > 0 ? TopPoint : new Vector3(TopPoint.x, -TopPoint.y, TopPoint.z);
        Vector3 StartPoint = new Vector3((float)(-(TopPoint.y / Math.Tan(SolarAltitude_Rad)) * Math.Sin(Math.PI - SolarAzimuth_Rad)) + TopPoint.x, 0, (float)(-(TopPoint.y / Math.Tan(SolarAltitude_Rad)) * Math.Cos(Math.PI - SolarAzimuth_Rad)) + TopPoint.z);

        Vector3 Direction = StartPoint - EndPoint;
        Vector3 Origin = EndPoint - 10.0f * Vector3.Normalize(Direction);

        if (SolarAltitude_Deg < 0) return new Ray();    //如果太阳高度角小于0，说明还没到日出或已经日落

        return new Ray(Origin, Direction);
    }

    /// <summary>
    /// 获取所有可能会与物体相交的直射光线
    /// </summary>
    /// <param name="d">光线之间的间隔，即光线密度</param>
    public static Light[] DirectLights(int Year, int Month, int Day, int Hour, int Min, double Longitude, double Latitude, Octree _Octree, float d)
    {
        double SolarDayAngle = DayAngle(Year, Month, Day, Hour, Min, Longitude);    //太阳日角
        double SolarHourAngle = HourAngle(Hour, Min, Longitude, SolarDayAngle);     //太阳时角
        double SolarDeclination = Declination(SolarDayAngle);                       //太阳赤纬角

        double SolarAltitude_Deg = SolarAltitude(SolarDeclination, SolarHourAngle, Latitude);   //太阳高度角（角度制）
        double SolarAzimuth_Deg = SolarAzimuth(SolarDeclination, SolarAltitude_Deg, Latitude, SolarHourAngle);  //太阳方位角（角度制）

        return DirectLights(SolarDayAngle, SolarAltitude_Deg, SolarAzimuth_Deg, _Octree, d);
    }

    public static Light[] DirectLights(double SolarDayAngle_Rad, double SolarAltitude_Deg, double SolarAzimuth_Deg, Octree _Octree, float d)
    {
        Ray CenterRay = LightRay(SolarAltitude_Deg, SolarAzimuth_Deg, _Octree == null ? Vector3.up * 5 : _Octree.Root.Bounds.center);   //向中心发射的光线
        if (CenterRay.direction.Equals(Vector3.zero)) return new Light[1] { new Light(CenterRay) };

        LightCastPlane LightPlane = new LightCastPlane(new Plane(CenterRay.direction, CenterRay.origin));   //初始化投射光线的平面
        LightPlane.SetRectBounds(_Octree == null ? new Bounds(Vector3.up * 5.0f, Vector3.one * 10.0f) : _Octree.Root.Bounds);   //设置投射光线的大小

        Vector3 Detla_Horizontal =
            (LightPlane.RightBottom - LightPlane.LeftBottom).magnitude / d < 100 ?
            (LightPlane.RightBottom - LightPlane.LeftBottom) / 100f :
            (LightPlane.RightBottom - LightPlane.LeftBottom).normalized * d; //平面水平方向的增量
        Vector3 Detla_Vertical =
            (LightPlane.LeftTop - LightPlane.LeftBottom).magnitude / d < 100 ?
            (LightPlane.LeftTop - LightPlane.LeftBottom) / 100f :
            (LightPlane.LeftTop - LightPlane.LeftBottom).normalized * d;       //平面垂直方向的增量

        /*
         * 每条边上光线的个数
         * 这条边的总长度 / 增量的长度
         */
        int CountOfHorizontal = (int)((LightPlane.RightBottom - LightPlane.LeftBottom).magnitude / Detla_Horizontal.magnitude) + 1;
        int CountOfVertical = (int)((LightPlane.LeftTop - LightPlane.LeftBottom).magnitude / Detla_Vertical.magnitude) + 1;

        Light[] Lights = new Light[CountOfHorizontal * CountOfVertical];

        /*
         * 每条光线的能量
         * 由于光线的分布是均匀的，因此能量也均分
         * 计算出投射的光平面的总能量，再将能量均分到每条光线上
         */
        float LightEnergy = LightPlane.Energy(SolarDayAngle_Rad, SolarAltitude_Deg) / Lights.Length;

        for (int i = 0; i < CountOfHorizontal; i++)
        {
            Vector3 Distance_Horizontal = i * Detla_Horizontal;

            for (int j = 0; j < CountOfVertical; j++)
            {
                Vector3 Distance_Vertical = j * Detla_Vertical;

                Lights[i * CountOfVertical + j] = new Light(LightPlane.LeftBottom + Distance_Horizontal + Distance_Vertical, LightPlane.Normal, LightEnergy);
            }
        }

        return Lights;
    }

    /// <summary>
    /// 模拟整日的太阳辐射，并返回这段时间内累积的生物量(单位 g)
    /// </summary>
    /// <param name="Time">时间</param>
    /// <param name="Longitude">经度</param>
    /// <param name="Latitude">纬度</param>
    /// <param name="_Octree">由植物构建的八叉树模型</param>
    /// <param name="d">光线之间的间隔，即光线密度</param>
    /// <param name="LatitudeNum">纬度方向的剖分个数</param>
    /// <param name="LongtitudeNum">经度度方向的剖分个数</param>
    /// <returns>累积的生物量（单位 g）</returns>
    public static double DailySunShineSimluation(TreeModel treeModel,
        DateTime Time, double Longitude, double Latitude, Octree _Octree, float d, int LatitudeNum, int LongtitudeNum)
    {
        double _DayAngle = DayAngle(Time.Year, Time.Month, Time.Day, 12, 0, Longitude); //一天的平均日角

        double _RiseTime = RiseTime(_DayAngle, Longitude, Latitude);    //日出时间
        double _FallTime = FallTime(_DayAngle, Longitude, Latitude);    //日落时间

        double biomass = 0.0;

        for (double StartHour = _RiseTime; StartHour < _FallTime; StartHour = StartHour + 1 ) //以一个小时为步长计算辐射量
        {
            DataInitialization(treeModel);   //初始化数据，防止出错

            double EndHour = StartHour + 1;
            if (EndHour > _FallTime) EndHour = _FallTime;   //当结束的时间超过日落时间时，则将日落时间作为结束时间

            double MidHour = (StartHour + EndHour) / 2;     //中间时间点
            int MidHour_Int = HourToHour(MidHour);          //中间时间点（小时）
            int MidMin_Int = HourToMin(MidHour);            //中间时间点（分钟）

            double SolarDayAngle_Rad = DayAngle(Time.Year, Time.Month, Time.Day, MidHour_Int, MidMin_Int, Longitude);   //这段时间内的平均日角
            double SolarHourAngle    = HourAngle(MidHour_Int, MidMin_Int, Longitude, SolarDayAngle_Rad);                //这段时间内的平均时角
            double SolarDelication   = Declination(SolarDayAngle_Rad);                                                  //这段时间内的平均赤纬角

            double SolarAltitude_Deg = SolarAltitude(SolarDelication, SolarHourAngle, Latitude);                        //这段时间内的平均太阳高度角
            double SolarAzimuth_Deg = SolarAzimuth(SolarDelication, SolarAltitude_Deg, Latitude, SolarHourAngle);       //这段时间内的平均太阳方位角

            DirectionLightSimulation(SolarDayAngle_Rad, SolarAltitude_Deg, SolarAzimuth_Deg, StartHour, EndHour, _Octree, d);       //模拟这段时间内的直射光照
            //ScatterLightSimulation(SolarDayAngle_Rad, SolarAltitude_Deg, StartHour, EndHour, _Octree, LatitudeNum, LongtitudeNum);  //模拟这段时间内的散射光照

            biomass += FunctionSim.CumulativeBiomass(treeModel, 
                (int)((EndHour - StartHour) * 60 * 60), treeModel.EnvironmentParams.NutrientType);   //计算这段时间累积的生物量
        }

        //测试
        biomass *= 4.75078125;

        return biomass / 1000000.0;
    }

    public static double SunShineSim(TreeModel treeModel, PhotosyntheticModels type)
    {
        double biomass = 0.0;
        switch (type)
        {
            case PhotosyntheticModels.LightResponse :
                Octree octree = Octree.Build(treeModel);
                biomass = 
                    octree == null?     //无模型，说明未出苗
                    MaizeParams.SEED_BIOMASS : DailySunShineSimluation(treeModel, DateTime.Now, 119, 26, octree, 0.1f, 100, 100);
                if (octree != null) octree.Clear();
                break;
            default:
                GameObject go = treeModel.TreeModelInstance;

                if (go != null)  Mesh.AddBoxColliderInParent(go);

                biomass = 
                    treeModel.GetLeafIndexes().Count != 0 ?     //无叶片，说明未出苗
                    BeerRule.BiomassCal(treeModel) / FunctionSim.ComputeDaysInGC(treeModel) : MaizeParams.SEED_BIOMASS;
                break;
        }

        if (biomass != MaizeParams.SEED_BIOMASS)
            biomass *= EnvironmentEffect.TemperatureStressFactor(treeModel) * EnvironmentEffect.WaterStressFactor(treeModel) * EnvironmentEffect.SunshineStress(treeModel);

        return biomass;
    }

    /// <summary>
    /// 模拟天空直射
    /// </summary>
    /// <param name="SolarDayAngle_Rad">太阳日角</param>
    /// <param name="SolarAltitude_Deg">太阳高度角</param>
    /// <param name="SolarAzimuth_Deg">太阳方位角</param>
    /// <param name="StartHour">开始的时间</param>
    /// <param name="EndHour">结束时间</param>
    /// <param name="_Octree">八叉树</param>
    /// <param name="d">光线之间的间隔，即光线密度</param>
    public static void DirectionLightSimulation(double SolarDayAngle_Rad, double SolarAltitude_Deg, double SolarAzimuth_Deg, double StartHour, double EndHour, Octree _Octree, float d)
    {
        Light[] Lights = DirectLights(SolarDayAngle_Rad, SolarAltitude_Deg, SolarAzimuth_Deg, _Octree, d);  //获取所有直射光线

        foreach (Light _Light in Lights)
        {
            Triangle HitTriangle;
            if (!RayTracing.CollisionDection(_Light, _Octree, out HitTriangle)) continue;   //如果该光线没有与三角面片相交，则遍历下一条光线

            if (HitTriangle.Type != OrganType.Leaf) continue;       //如果相交三角面片不为叶子，则遍历下一条光线

            LeafIndex _LeafIndex = HitTriangle.Index as LeafIndex;
            _LeafIndex.DirectionEnergy += _Light.Energy;            //累加叶子的直射能量

        }
    }

    /// <summary>
    /// 模拟天空散射
    /// </summary>
    /// <param name="SolarDayAngle_Deg">太阳日角</param>
    /// <param name="SolarAltitude_Deg">太阳高度角</param>
    /// <param name="StartHour">开始的时间</param>
    /// <param name="EndHour">结束的时间</param>
    /// <param name="_Octree">八叉树</param>
    /// <param name="LatitudeNum">纬度方向的剖分个数</param>
    /// <param name="LongtitudeNum">经度度方向的剖分个数</param>
    public static void ScatterLightSimulation(TreeModel treeModel, double SolarDayAngle_Rad, double SolarAltitude_Deg, double StartHour, double EndHour,Octree _Octree, int LatitudeNum, int LongtitudeNum)
    {
        Triangle[] TreeTriangles = GameObjectOperation.GetTreeTriangles(treeModel).ToArray();  //获取所有的三角面片

        double _ScatterIrradiance = ScatterIrradiance(SolarDayAngle_Rad, SolarAltitude_Deg, _Octree.Root.Bounds.center.y);   //总散射辐射度

        LightCastHemisphere LightHemisphere = null;

        for (int i = 0; i < TreeTriangles.Length; i++ )
        {
            if (TreeTriangles[i].Type != OrganType.Leaf) continue;  //非叶片，无需计算散射辐射

            /*
             * 优化
             * 限定天穹半球的大小
             * 每次三角面片，天穹半球跟随三角面片移动，无需重新计算
             */
            if (LightHemisphere == null) LightHemisphere = new LightCastHemisphere(TreeTriangles[i].Center, _Octree.Root.Bounds);
            else                         LightHemisphere.MoveTo(TreeTriangles[i].Center);

            /*
             * 通过天穹半球计算出天空散射透过率
             * 该面片上的平均散射辐射度（W / m^2） = 天空散射透过率 * 总散射辐射度
             * 该面片上的总辐射度(W) = 平均散射辐射度 * 该面片的面积
             * 注意：因为存在透明的纹理，因此需要再得到整个叶片模型的散射能量的基础上，再乘以非透明的比例，才能得到真正的散射能量
             */
            LeafIndex _LeafIndex = TreeTriangles[i].Index as LeafIndex;

            _LeafIndex.ModelScatterEnergy += (float)(LightHemisphere.GetSkyTransmissivity(LongtitudeNum, LatitudeNum, TreeTriangles, i) * _ScatterIrradiance * TreeTriangles[i].Area());
        }
    }

    /// <summary>
    /// 初始化数据
    /// 为防止数据重复以及影响精度
    /// </summary>
    private static void DataInitialization(TreeModel treeModel)
    {
        List<OrganIndex> indexes = treeModel.OrganIndexes;

        foreach (OrganIndex index in indexes)
        {
            if (index.Type != OrganType.Leaf) continue;

            ((LeafIndex)index).ClearEnergyData();
        }
    }

    /*
     * 辐照度转换成PAR（光合有效辐射）的转换系数
     * 参考文献： 马金玉, 刘晶淼, 李世奎, 等. 基于试验观测的光合有效辐射特征分析[J]. 自然资源学报, 2007,22(5):673-682. 转换系数 u = 0.41
     * 参考文献： 黄秉维. 现代自然地理学[M]. 北京: 科学出版社, 1999. 转换系数 u = 0.47
     */
    const double IRRADIANCE2PAR_CONVERSION_COEFFICIENT = 0.47;

    /*
     * PAR（光合有效辐射 单位：W/m^2 或 J/s * m^2）转换成PPFD（光量子通量密度 单位：umol/s * m^2）的转换系数
     * 参考文献： 马金玉, 刘晶淼, 李世奎, 等. 基于试验观测的光合有效辐射特征分析[J]. 自然资源学报, 2007,22(5):673-682.
     */
    const double PAR2PPFD_CONVERSION_COEFFICIENT = 4.6;

    /// <summary>
    /// 采用最简单的方式将辐照度转换成PAR（光合有效辐射）
    /// </summary>
    /// <param name="Irradiance">总太阳辐照度</param>
    public static double IrradianceToPAR(double Irradiance)
    {
        return Irradiance * IRRADIANCE2PAR_CONVERSION_COEFFICIENT;
    }

    /// <summary>
    /// 采用最简单的方式将辐照度转换成PPFD（光量子通量密度）
    /// </summary>
    /// <param name="Irradiance"></param>
    /// <returns></returns>
    public static double IrradianceToPPFD(double Irradiance)
    {
        return IrradianceToPAR(Irradiance) * PAR2PPFD_CONVERSION_COEFFICIENT;
    }

    /// <summary>
    /// 采用较为精确的方式将辐射度转换成PPFD（光量子通量密度）
    /// 参考文献： González J A, Calbó J. Modelled and measured ratio of PAR to global radiation under cloudless skies[J]. Agricultural and Forest Meteorology, 2002,110(4):319-325.
    /// </summary>
    /// <param name="Irradiance">辐照度</param>
    /// <param name="AirMass"></param>
    /// <param name="PrecipitableWater"></param>
    public static double IrradianceToPPFD(double Irradiance, double AirMass, double PrecipitableWater)
    {
        double a = 1.834 - 0.0361 * AirMass - 0.0175 * Math.Pow(AirMass, 2) + 0.00354 * Math.Pow(AirMass, 3) - 0.000269 * Math.Pow(AirMass, 4) + 0.00000763 * Math.Pow(AirMass, 5);
        double b = 0.111 + 0.1023 * AirMass - 0.0225 * Math.Pow(AirMass, 2) + 0.00297 * Math.Pow(AirMass, 3) - 0.000206 * Math.Pow(AirMass, 4) + 0.00000579 * Math.Pow(AirMass, 5);
        double c = 0.365 - 0.0661 * AirMass + 0.0182 * Math.Pow(AirMass, 2) - 0.00271 * Math.Pow(AirMass, 3) + 0.000204 * Math.Pow(AirMass, 4) + 0.00000602 * Math.Pow(AirMass, 5);
        double ratio = 0.991 * (a + b * Math.Pow(PrecipitableWater, c));

        return Irradiance * ratio;
    }

    public static double PAR2PPFD(double PAR)
    {
        return PAR * PAR2PPFD_CONVERSION_COEFFICIENT;
    }

    public static double PPFD2PAR(double PPFD)
    {
        return PPFD / PAR2PPFD_CONVERSION_COEFFICIENT;
    }

    /// <summary>
    /// 角度制转弧度制
    /// </summary>
    private static double DegToRad(double Degree)
    {
        return Degree * Math.PI / 180.0;
    }

    /// <summary>
    /// 弧度制转角度制
    /// </summary>
    private static double RadToDeg(double Radians)
    {
        return Radians * 180.0 / Math.PI;
    }

    /// <summary>
    /// 将小时（double）拆分成小时（int）
    /// </summary>
    public static int HourToHour(double Hour)
    {
        return ((int)(Hour * 10.0)) / 10;
    }

    /// <summary>
    /// 将小时（double）拆分成分钟（int）
    /// </summary>
    public static int HourToMin(double Hour)
    {
        return (int)((Hour - HourToHour(Hour)) * 60);
    }

    public static double BeiJingTimeToLocalTime(int Hour, int Min, double Longitude)
    {
        return Hour + (Min + (Longitude - 120.0) * 4.0) / 60.0;
    }

    public static double BeiJingTimeToLocalTime(double Hour, double Longitude)
    {
        return BeiJingTimeToLocalTime(HourToHour(Hour), HourToMin(Hour), Longitude);
    }

    public static double LocalTimeToBeiJingTime(int Hour, int Min, double Longitude)
    {
        return Hour + (Min - (Longitude - 120.0) * 4.0) / 60.0;
    }

    public static double LocalTimeToBeiJingTime(double Hour, double Longitude)
    {
        return LocalTimeToBeiJingTime(HourToHour(Hour), HourToMin(Hour), Longitude);
    }
}

