using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempHPBehaviour : MonoBehaviour
{

	public void Die(float timeToDie)
    {
        GetComponent<ParticleSystem>().emissionRate = 0;

        Invoke("Death", timeToDie);
    }

    void Death()
    {
        Destroy(gameObject);
    }
}
