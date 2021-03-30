using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool: MonoBehaviour
{
    private static ObjectPool instance;
    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectsOfType<Transform>().AsEnumerable()
                    .Select(c => c.GetComponent<ObjectPool>())
                    .First(e => e != null);
            }

            if (instance == null)
                throw new NullReferenceException("Have no Object Pool GameObject");

            return instance;
        }
    }


    [SerializeField]
    private int minCount = 3;
    [SerializeField]
    private GameObject baseObject;

    private List<GameObject> objects = new List<GameObject>();

    void Start()
    {
        if (baseObject == null) return;

        for (int i = 0; i < minCount; i++)
        {
            Instantiate();
        }          
    }

    public GameObject GetGameObject()
    {
        //寻找是否有未被使用的对象
        foreach(var item in objects)
        {
            if (!item.activeSelf)
            {
                item.SetActive(true);
                return item;
            }
        }

        //对象池内的对象均被使用，则重新生成一个
        var cloneItem = Instantiate();
        cloneItem.SetActive(true);

        return cloneItem;
    }

    public void RecycleGameObject(GameObject item)
    {
        int index = objects.IndexOf(item);

        if (index < 0) return;
        else if (index < minCount)
        {
            item.SetActive(false);
        }
        else
        {
            objects.RemoveAt(index);
            GameObject.Destroy(item);
        }
    }

    private GameObject Instantiate()
    {
        var cloneItem = GameObject.Instantiate<GameObject>(baseObject);
        cloneItem.SetActive(false);
        cloneItem.transform.SetParent(baseObject.transform.parent);
        cloneItem.transform.localScale = baseObject.transform.localScale;

        objects.Add(cloneItem);

        return cloneItem;
    }
}
