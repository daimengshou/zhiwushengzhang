using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChartTypeButton : MonoBehaviour {

    public ChartType type;
	
    public void Click()
    {
        Chart chart = Chart.GetInstance();

        chart.SetChartType(type);
        chart.UpdateSmartChart();
    }
}
