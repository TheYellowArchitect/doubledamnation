using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXLoopBoatRowing : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        StartCoroutine(PlaySFX());
    }
	
	public IEnumerator PlaySFX()
    {
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.BoatRow);

        yield return new WaitForSeconds(11.8f);

        PlayerSoundManager.globalInstance.Stop(PlayerSoundManager.AudioSourceName.BoatRow);
    }
}
