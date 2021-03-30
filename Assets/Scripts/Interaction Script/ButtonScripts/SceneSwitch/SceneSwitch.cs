using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch
{
    public static void SwitchTo(int index)
    {
        LScene.Destroy();
        LanguageNotification.Destroy();
        MeshResource.Destroy();

        SceneManager.LoadScene(index);
    }

    public static IEnumerator SwitchTo_Coroutine(int index)
    {
        yield return new WaitForEndOfFrame();

        SwitchTo(index);
    }
}
