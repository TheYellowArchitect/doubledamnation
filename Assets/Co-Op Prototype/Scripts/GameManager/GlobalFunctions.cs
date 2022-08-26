using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalFunctions : MonoBehaviour
{
    


    //Remake Towards0 via Event, so it is frame-dependent.


    //Flawed, since it has to be called, and is frame dependant. See IEnumerator/Coroutine: 
	public float Towards0(float number, float distance)
    {
        if (number == 0)
            return number;
        else if (number > 0)
            return number - distance;
        else
            return number + distance;
    }
}
