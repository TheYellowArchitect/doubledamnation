using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLevelClear : MonoBehaviour 
{
	public bool triggeredOnce = false;

	private void OnTriggerEnter2D(Collider2D player)
    {
        if (triggeredOnce == false && LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode)
            Trigger();
    }
	
	public void Trigger()
	{
		triggeredOnce = true;

		GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().PlayVictoryPose();

		//TODO: MusicSoundManager.globalInstance. play victory fanfare
		//MusicSoundManager.globalInstance.MusicFadeOut();

		//Notify SpeedrunManager, that the current level is finished/done!
        //GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>().ClearedLevel();

		LevelEditorMenu.globalInstance.TogglePlayMode();
	}
}
