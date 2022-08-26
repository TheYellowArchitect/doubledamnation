using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTimer : MonoBehaviour
{
    public void DieInSeconds(float timeToDie)
    {
        StartCoroutine(Death(timeToDie));
    }

    public IEnumerator Death(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(this.gameObject);
    }
	
}
