using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Next1 : MonoBehaviour
{
    public int Scene=1;
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("Next Button").GetComponent<Button>().onClick.AddListener(test);
    }

    void test()
    {
        SceneManager.LoadScene(Scene);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
