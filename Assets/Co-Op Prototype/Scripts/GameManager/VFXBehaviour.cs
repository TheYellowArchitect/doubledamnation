using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXBehaviour : MonoBehaviour
{
    [Tooltip("When it dies after being spawned")]
    public float deathTime;

	// Use this for initialization
	void Start ()
    {
        Invoke("Death", deathTime);
	}

    //Destroys gameobject with everything inside, aka this script as well
    void Death()
    {
        Destroy(gameObject);
    }
}
