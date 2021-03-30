using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


class StringValidate
{
    /// <summary>
    /// 判断字符串是否均为数字
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNumeric(string str)
    {
        if (str == null || str.Length == 0) //验证字符串是否为空
            return false;

        const string pattern = "^[0-9]*$";  //正则表达式
        Regex rx = new Regex(pattern);

        return rx.IsMatch(str);
    }
}

class GameObjectValidate
{
    /// <summary>
    /// 判断是否含有顶点
    /// </summary>
    public static bool HavaVertices(GameObject _Object)
    {
        if (_Object.GetComponent<MeshFilter>() == null)
            return false;
        else if (_Object.GetComponent<MeshFilter>().mesh == null)
            return false;
        else if (_Object.GetComponent<MeshFilter>().mesh.vertices.Length == 0)
            return false;
        else
            return true;
    }

    /// <summary>
    /// 判断是否含有包围盒
    /// </summary>
    public static bool HaveBounds(GameObject _Object)
    {
        return _Object.GetComponent<MeshRenderer>() != null;
    }

    /// <summary>
    /// 判断是否有纹理
    /// </summary>
    public static bool HaveTexture(GameObject _Object)
    {
        if (_Object == null)
            return false;
        if (_Object.GetComponent<MeshRenderer>() == null)
            return false;
        else if (_Object.GetComponent<MeshRenderer>().material == null)
            return false;
        else if (_Object.GetComponent<MeshRenderer>().material.mainTexture == null)
            return false;
        else
            return true;  
    }

    /// <summary>
    /// 判断两个物体是否性质相同且均含有顶点
    /// </summary>
    public static bool BothObjectsHaveVertices(GameObject FstObject, GameObject SndObject)
    {
        ValidateTagOfObject(FstObject, SndObject);

        switch (Convert.ToInt32(HavaVertices(FstObject)) + Convert.ToInt32(HavaVertices(SndObject)))
        {
            case 0: //两个物体均无顶点坐标    
                return false;
            case 1: //两个物体只有一个有顶点坐标
                throw new ArgumentException("One of Objects hava not Mesh.");
            case 2: //两个物体均有顶点坐标
                return true;
            default://未知错误
                throw new Exception("Unknown Exception");
        }
    }

    /// <summary>
    /// 判断两物体是否具有相同的Tag
    /// </summary>
    public static bool IsTheSameTag(GameObject FstObject, GameObject SndObject)
    {
        if (!FstObject.tag.Equals(SndObject.tag))
            return false;
        else
            return true;
    }

    /// <summary>
    /// 判断两物体是否具有相同的Tag
    /// </summary>
    public static void ValidateTagOfObject(GameObject FstObject, GameObject SndObject)
    {
        if (!IsTheSameTag(FstObject, SndObject))
            throw new ArgumentException("The tags of the two objects are different.");
    }
}

