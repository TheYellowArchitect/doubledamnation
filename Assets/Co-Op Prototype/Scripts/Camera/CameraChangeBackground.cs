using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraChangeBackground : MonoBehaviour
{

    private void Start()
    {
        if (GameManager.testing == false)
            Destroy(this);
    }

    // Update is called once per frame
    void Update ()
    {
        #region bugged af, report this to unity.
        //if (Input.GetKeyDown(KeyCode.Insert))
            //GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().backgroundColor = new Color(128, 128, 128, 20);
        #endregion
    }
}
