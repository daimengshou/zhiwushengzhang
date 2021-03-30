using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class CellularTexMemory
{
    #region 静态函数
    private static CellularTexMemory instance = null;

    public static CellularTexMemory GetInstance()
    {
        if (instance == null)
            instance = new CellularTexMemory();

        return instance;
    }
    #endregion

    #region 动态函数

    #region 成员变量
    private List<CellularTexture> cellularTextures = new List<CellularTexture>();
    #endregion

    private CellularTexMemory()
    {
        cellularTextures.Add(
            new CellularTexture(System.Environment.CurrentDirectory + "\\Assets\\Resources\\Textures\\texture.png", 215489)
            );
    }

    public CellularTexture GetCellularTex(int index)
    {
        if (index >= cellularTextures.Count)
            index = cellularTextures.Count - 1;

        if (index < 0)
            index = 0;

        if (cellularTextures.Count == 0)
            throw new NullReferenceException("Have no cellular texture");

        return cellularTextures[index];
    }
    #endregion
}

