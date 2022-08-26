using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

//Somehow change the font per language. Probably 2 new parameters.
public class ToolSentencesFontColor
{
    [MenuItem("Tools/Font&ColorSentences/English")]
    public static void ChangeSentencesFontColorEnglish()
    {
        ChangeSentencesFontColor("English", DialogueCategory.Death);
        ChangeSentencesFontColor("English", DialogueCategory.PowerUp);
        ChangeSentencesFontColor("English", DialogueCategory.LevelCutscenes);
        ChangeSentencesFontColor("English", DialogueCategory.MidCutscenes);
        ChangeSentencesFontColor("English", DialogueCategory.InterruptionsByWarrior);
        ChangeSentencesFontColor("English", DialogueCategory.InterruptionsByMage);
    }

    /// <summary>
    /// Having freshly made the sentences, I have to manually change the font + color for each one. But doing it automatically is faster.
    /// This script detects if the voiceClip inside is P1 or P2, and does the appropriate change.
    /// </summary>
    /// idk why this isn't shown as a tooltip when clicked, obscure bug, if you read this, plz help to fix the bug :P
    public static void ChangeSentencesFontColor(string language, DialogueCategory category)
    {
        string categoryPath;
        if (category == DialogueCategory.Death)
            categoryPath = "\\Death";
        else if (category == DialogueCategory.PowerUp)
            categoryPath = "\\PowerUp";
        else if (category == DialogueCategory.MidCutscenes)
            categoryPath = "\\MidCutscenes";
        else if (category == DialogueCategory.LevelCutscenes)
            categoryPath = "\\LevelCutscenes";
        else if (category == DialogueCategory.InterruptionsByWarrior)
            categoryPath = "\\InterruptionsByWarrior";
        else if (category == DialogueCategory.InterruptionsByMage)
            categoryPath = "\\InterruptionsByMage";
        else
        {
            Debug.LogError("ChangeSentencesFontColor failed. The dialogue category does not exist.");
            //Aktualye... Create new folders based on the paths...
            return;
        }

        //The below isn't confirmed to work. If it doesn't check the path name again. There is no other reason it shouldn't work.
        DialogueContainer pickedDialogueContainer = (DialogueContainer) AssetDatabase.LoadMainAssetAtPath(@"Assets\Co-Op Prototype\Sentences\" + language + categoryPath + "Dialogues.asset") as DialogueContainer;

        //With this, all changes made are about to be permanent, aka written on the disk. It has started asset editting, and it finishes when invoked again at the end
        AssetDatabase.StartAssetEditing();

        //Setting dirty, while AssetDatabase is importing/processing, means that this will be written on the disk.
        EditorUtility.SetDirty(pickedDialogueContainer);

        //With the below 2 forloops, we iterate through every sentence.
        for (int tempDialogueIndex = 0; tempDialogueIndex < pickedDialogueContainer.totalDialogues.Count; tempDialogueIndex++)
        {

            for (int tempSentenceIndex = 0; tempSentenceIndex < pickedDialogueContainer.totalDialogues[tempDialogueIndex].sentencesInside.Count; tempSentenceIndex++)
            {
                //Here, we find the voice clip and check its string (comparing if it contains "P1" or "P2")

                //P1
                if (pickedDialogueContainer.totalDialogues[tempDialogueIndex].sentencesInside[tempSentenceIndex].voiceClip.ToString().Contains("P1"))
                    SetSentenceToWarriorFontColor( pickedDialogueContainer.totalDialogues[tempDialogueIndex].sentencesInside[tempSentenceIndex] );
                //P2
                else if (pickedDialogueContainer.totalDialogues[tempDialogueIndex].sentencesInside[tempSentenceIndex].voiceClip.ToString().Contains("P2"))
                    SetSentenceToMageFontColor(pickedDialogueContainer.totalDialogues[tempDialogueIndex].sentencesInside[tempSentenceIndex]);
                //Neither P1 or P2
                else
                {
                    Debug.Log("Voice Clip " + pickedDialogueContainer.totalDialogues[tempDialogueIndex].sentencesInside[tempSentenceIndex].voiceClip.ToString() + " had neither P1 or P2 in its name. Skipping it...");
                    continue;
                }
            }
        }

        //Now, stop the asset import/processing, aka everything is in some kind of buffer, ready to be put on the disk.
        AssetDatabase.StopAssetEditing();

        //Saves all assets that are AssetDatabase.editted :P To the disk permanently they go!
        AssetDatabase.SaveAssets();

        Debug.Log("Setting Font&Color on the sentences is done!");
    }

    public static void SetSentenceToWarriorFontColor(Sentence pickedSentence)
    {
        EditorUtility.SetDirty(pickedSentence);

        pickedSentence.textColor = Color.black;

        pickedSentence.textFont = (TMP_FontAsset)AssetDatabase.LoadMainAssetAtPath("Assets\\Co-Op Prototype\\Fonts\\WarriorFont(Dalek) SDF.asset") as TMP_FontAsset;
    }

    public static void SetSentenceToMageFontColor(Sentence pickedSentence)
    {
        EditorUtility.SetDirty(pickedSentence);

        pickedSentence.textColor = Color.white;

        pickedSentence.textFont = (TMP_FontAsset)AssetDatabase.LoadMainAssetAtPath("Assets\\Co-Op Prototype\\Fonts\\MageFont(Elektra) SDF.asset") as TMP_FontAsset;
    }

}
