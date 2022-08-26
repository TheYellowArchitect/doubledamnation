using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Useless REEEE
public class LoadSceneTrigger : MonoBehaviour
{
    [Tooltip("The name of the scene/level to Load (on top of current)")]
    public string LoadName;
    [Tooltip("The name of the scene/level to Unload")]
    public string UnloadName;

    public bool runOnce = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        LoadScene();
    }

    public void LoadScene()
    {
        if (runOnce == false)
            return;

        runOnce = false;

        if (LoadName != "")
            SceneManagerScript.globalInstance.Load(LoadName);

        if (UnloadName != "")
            StartCoroutine("UnloadScene");//Coroutine cuz rumours of it freezing, cant take a huge logic risk rn.
    }

    IEnumerator UnloadScene()
    {
        yield return new WaitForSeconds(.1f);
        SceneManagerScript.globalInstance.Unload(UnloadName);
    }
}
