using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCursor : MonoBehaviour
{
	
	// Update is called once per frame
	void Update ()
    {
        //If you want performance, cache the main camera
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = pos;
	}
}
