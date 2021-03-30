/*
 * Part of: SmartChart
 * Class respresenting single data set for SmartChart.
 */

using UnityEngine;

namespace ToucanSystems
{
    public struct EnvironmentMarker
    {
        public int index;
        public string value 
        {
            get
            {
                if (LScene.GetInstance().Language == SystemLanguage.Chinese)
                    return value_tw;
                else
                    return value_en;
            }
        }

        private string value_en;
        private string value_tw;

        public EnvironmentMarker(int _index, string _value_en, string _value_tw)
        {
            index = _index;

            value_en = _value_en;
            value_tw = _value_tw;
        }
    }

    /// <summary>
    /// Class respresenting single data set for the chart.
    /// </summary>
    [System.Serializable]
    public class SmartChartData
    {

        /// <summary>
        /// Color used to display data line.
        /// </summary>
        public Color32 dataLineColor = Color.black;
        /// <summary>
        /// Material used to display data line.
        /// </summary>
        public Sprite dataLineSprite;
        /// <summary>
        /// Color used to fill area under the chart line.
        /// </summary>
        public Color32 dataFillColor = Color.black;
        /// <summary>
        /// Texture used to fill area under the chart line.
        /// </summary>
        public Texture dataFillTexture;
        /// <summary>
        /// Sprite that will be displayed as marker.
        /// </summary>
        public Sprite markerSprite;
        /// <summary>
        /// Dimensions of markers.
        /// </summary>
        public Vector2 markerSize;
        /// <summary>
        /// Color tint of markers.
        /// </summary>
        public Color markerColor;
        /// <summary>
        /// Thickness of line that will represent data.
        /// </summary>
        [Range(0, 100)]
        public float dataLineWidth = 1;
        /// <summary>
        /// Actual data, stored in array of Vector2s.
        /// </summary>
        public Vector2[] data;

        /// <summary>
        /// 环境因子marker
        /// </summary>
        public EnvironmentMarker[] envirMarker = new EnvironmentMarker[0];
    }
}
