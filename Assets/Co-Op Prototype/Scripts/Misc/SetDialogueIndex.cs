using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Made as a hack to not trigger the very first levelcutscene index (1)
//So it goes from 0 to 2, without hardcoding anything onto class DialogueManager (if dialogueIndex == 1, then return;)
//Could be extended to be paired with TriggerLevelGateCutscene (so TriggerLevelCutscene scans GetComponentInChildren<SetDialogueIndex>)
//and a good example is using it for level editor.
public class SetDialogueIndex : MonoBehaviour
{
	public int targetIndex = 2;

	public bool triggeredOnce = false;

	//Sadly isn't triggered by parent gameobject
	private void OnTriggerEnter2D(Collider2D player)
    {
        if (triggeredOnce)
            return;

        if (triggeredOnce == false)
		{
			triggeredOnce = true;

			LevelManager.levelCutsceneDialogueIndex = targetIndex;
		}

    }
}
