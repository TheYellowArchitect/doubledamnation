using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debugScript : MonoBehaviour
{
    public bool debug;

    int i = 0;


	// Use this for initialization
	void Start ()
    {
        if (debug == false)
            Destroy(this);

        GetComponentInChildren<AudioSource>().Play();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (i > 60)
        {
            GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3(24.7f, -27.0f, 0);
            Destroy(this);
        }  
        else
            i++;
            
	}
}
