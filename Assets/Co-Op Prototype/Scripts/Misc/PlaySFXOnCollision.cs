using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFXOnCollision : MonoBehaviour
{
    public PlayerSoundManager.AudioSourceName sfxName;

    [Tooltip("When it should play, how many times to ignore instead?\nUsually used to skip the first time.")]
    public int timesToIgnore = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("IntroBoatPlayer"))
            return;

        //If timestoignore > 0, then it should be ignored
        //galaxybrain documentation above...
        if (timesToIgnore > 0)
        {
            //Decrease it by one so it won't loop forever lmao
            timesToIgnore--;
            return;
        }

        if (PlayerSoundManager.globalInstance != null)
            PlayerSoundManager.globalInstance.PlayAudioSource(sfxName);

        Debug.Log("Waveslam DETECTED");

        if (transform.childCount > 0 && transform.GetChild(0).GetComponent<ParticleSystem>() != null)
            transform.GetChild(0).GetComponent<ParticleSystem>().Play();
    }
}
