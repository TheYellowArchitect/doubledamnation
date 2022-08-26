using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// It takes the dialogueContainers inside the language folder mentioned, and feeds them nonstop to dialogueManager, to get them to play.
/// </summary>
public class ToolPlayDialogues
{
    [MenuItem("Tools/Play Dialogues/English/Death #&1")]
    public static void PlayDeath()
    {
        if (IsFundamentalSceneActive())
        {
            DialogueContainer tempDialogueContainer = GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().DeathDialogues;

            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayDebugDialogue(tempDialogueContainer);
        }
        else
            Debug.LogError("Cannot play the dialogues since fundamentalScene must be open, so as to access DialogueManager.");
    }

    [MenuItem("Tools/Play Dialogues/English/PowerUp #&2")]
    public static void PlayPowerUp()
    {
        if (IsFundamentalSceneActive())
        {
            DialogueContainer tempDialogueContainer = GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PowerUpDialogues;

            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayDebugDialogue(tempDialogueContainer);
        }
        else
            Debug.LogError("Cannot play the dialogues since fundamentalScene must be open, so as to access DialogueManager.");
    }

    [MenuItem("Tools/Play Dialogues/English/MidCutscenes")]
    public static void PlayMidCutscenes()
    {
        if (IsFundamentalSceneActive())
        {
            DialogueContainer tempDialogueContainer = GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().MidCutsceneDialogues;

            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayDebugDialogue(tempDialogueContainer);
        }
        else
            Debug.LogError("Cannot play the dialogues since fundamentalScene must be open, so as to access DialogueManager.");
    }

    [MenuItem("Tools/Play Dialogues/English/LevelCutscenes")]
    public static void PlayLevelCutscenes()
    {
        if (IsFundamentalSceneActive())
        {
            DialogueContainer tempDialogueContainer = GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().LevelCutsceneDialogues;

            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayDebugDialogue(tempDialogueContainer);
        }
        else
            Debug.LogError("Cannot play the dialogues since fundamentalScene must be open, so as to access DialogueManager.");
    }

    [MenuItem("Tools/Play Dialogues/English/InterruptionsByMage")]
    public static void PlayInterruptionByMageCutscenes()
    {
        if (IsFundamentalSceneActive())
        {
            DialogueContainer tempDialogueContainer = GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().InterruptionByMageDialogues;

            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayDebugDialogue(tempDialogueContainer);
        }
        else
            Debug.LogError("Cannot play the dialogues since fundamentalScene must be open, so as to access DialogueManager.");
    }

    [MenuItem("Tools/Play Dialogues/English/InterruptionsByWarrior")]
    public static void PlayInterruptionByWarriorCutscenes()
    {
        if (IsFundamentalSceneActive())
        {
            DialogueContainer tempDialogueContainer = GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().InterruptionByWarriorDialogues;

            GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayDebugDialogue(tempDialogueContainer);
        }
        else
            Debug.LogError("Cannot play the dialogues since fundamentalScene must be open, so as to access DialogueManager.");
    }

    /*
    public static void PlayDialogueContainer(DialogueContainer chosenDialogueContainer)
    {
        //This could be copy-pasta from DialogueManager's PlayDebugDialogue();
    }
    */

    public static bool IsFundamentalSceneActive()
    {

        //Looping through the scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "FundamentalScene")
                return true;
        }

        return false;
    }
}
