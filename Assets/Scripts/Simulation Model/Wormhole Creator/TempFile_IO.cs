using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class CellularTexture
{
    public static List<KeyValuePair<int, int>> LoadAccumulationCountFile(string path, Texture2D tex = null)
    {
        /*
         * 当该文件不存在
         * 自动生成
         */
        if (!System.IO.File.Exists(path))
        {
            SaveAccumulationCountFile(path, tex);
        }

        List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>();

        FileStream stream = new FileStream(path, FileMode.Open);
        StreamReader reader = new StreamReader(stream);

        /*
         * 逐行读取数据
         * 每行的格式为 Key Value
         * 故如果读取并分离后的字符串个数不为2
         * 则存在错误
         */
        while (!reader.EndOfStream)
        {
            string[] keyValue = reader.ReadLine().Split(' ');

            if (keyValue == null || keyValue.Length != 2)
                throw new IOException("File stored information error.");

            list.Add(new KeyValuePair<int, int>(Convert.ToInt32(keyValue[0]), Convert.ToInt32(keyValue[1])));
        }

        reader.Close();
        stream.Close();

        return list;
    }

    public static void SaveAccumulationCountFile(string path, Texture2D tex)
    {
        /*
         * 纹理为空
         * 无法计算
         * 故抛出异常
         */
        if (tex == null)
            throw new ArgumentNullException("Texture is null");

        /*
         * 用键对的方式记录纹理中所有的像素
         * 其中
         * key 为 颜色十进制
         * value 为 该颜色的个数
         */
        SortedList<int, int> maps = new SortedList<int, int>();

        /*
         * 获取每个像素的颜色，并转换成十进制
         * 根据对应的键值累加其值
         */
        foreach (Color pixel in tex.GetPixels())
        {
            if (pixel.a == 0) continue;

            //转换成十进制
            int DEC = Color2DEC(pixel);

            if (maps.ContainsKey(DEC))
            {
                maps[DEC]++;
            }
            else
            {
                maps.Add(DEC, 1);
            }
        }

        /*
         * 累加键的值
         * 如有键对 1：10、2：5、3:7
         * 累加后为 1：10、2：15、3：22
         */
        for (int i = 1; i < maps.Count; i++ )
        {
            maps[maps.Keys[i]] += maps.Values[i - 1];
        }

        SaveFile(path, maps);
    }

    private static void SaveFile(string path, SortedList<int, int> maps)
    {
        string directoryPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        FileStream stream = new FileStream(path, FileMode.Create);
        StreamWriter writer = new StreamWriter(stream);

        foreach (var map in maps)
        {
            writer.WriteLine(map.Key + " " + map.Value);
        }

        writer.Close();
        stream.Close();
    }
}