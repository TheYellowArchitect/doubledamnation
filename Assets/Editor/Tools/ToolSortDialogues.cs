using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

//TODO: Make a language.ToString() global, when you go serious for localization (spellboi)
//TODO: Dialogue System fetches the dialogue containers depending on language
//TODO: Error detection, if no sentences. (or at least, no sentences in the folder, which should check if any sentences inside the dialogues folder just in case too)
//      ^Because when this happens, unity freezes and thats fucking sad. (rip progress). For example, go sort dialogues on any language except english and see it yourself feelsbadman.

public enum DialogueCategory { Death, PowerUp, MidCutscenes, LevelCutscenes, InterruptionsByWarrior, InterruptionsByMage };

/// <summary>
/// Created to run the function which takes all the sentences, and sorts them automatically for the DialogueSystem to be used.
/// </summary>
public class ToolSortDialogues
{
    DialogueCategory currentCategory;

    [MenuItem("Tools/Sort Dialogues/English")]
    /// <summary>
    /// So, all the sentences are in their folder + appropriate subfolder (Death/PowerUp/MidCutscene)
    /// By using the metadata (dialogue index and line index), this function creates as many dialogues needed (inside the dialogue container).
    /// Then it puts inside those sentences inside the dialogues, which are contained in 3 dialogue containers. Editor-only ofc.
    /// </summary>
    /// idk why this isn't shown as a tooltip when clicked, obscure bug, if you read this, plz help to fix the bug :P
    public static void SortDialoguesEnglish()
    {
        SortAllDialogues("English");
    }

    [MenuItem("Tools/Sort Dialogues/AncientGreek")]
    public static void SortDialoguesAncientGreek()
    {
        SortAllDialogues("AncientGreek");
    }

    [MenuItem("Tools/Sort Dialogues/Japanese")]
    public static void SortDialoguesJapanese()
    {
        SortAllDialogues("Japanese");
    }

    [MenuItem("Tools/Sort Dialogues/German")]
    public static void SortDialoguesGerman()
    {
        SortAllDialogues("German");
    }

    [MenuItem("Tools/Sort Dialogues/Russian")]
    public static void SortDialoguesRussian()
    {
        SortAllDialogues("Russian");
    }

    [MenuItem("Tools/Sort Dialogues/SimplifiedChinese")]
    public static void SortDialoguesSimplifiedChinese()
    {
        SortAllDialogues("SimplifiedChinese");
    }

    [MenuItem("Tools/Sort Dialogues/Korean")]
    public static void SortDialoguesKorean()
    {
        SortAllDialogues("Korean");
    }

    [MenuItem("Tools/Sort Dialogues/French")]
    public static void SortDialoguesFrench()
    {
        SortAllDialogues("French");
    }

    [MenuItem("Tools/Sort Dialogues/Spanish")]
    public static void SortDialoguesSpanish()
    {
        SortAllDialogues("Spanish");
    }

    [MenuItem("Tools/Sort Dialogues/Italian")]
    public static void SortDialoguesItalian()
    {
        SortAllDialogues("Italian");
    }

    [MenuItem("Tools/Sort Dialogues/Greek")]
    public static void SortDialoguesGreek()
    {
        SortAllDialogues("Greek");
    }

    public static void SortAllDialogues(string language)
    {
        SortDialogues(language, DialogueCategory.Death);
        SortDialogues(language, DialogueCategory.PowerUp);
        SortDialogues(language, DialogueCategory.MidCutscenes);
        SortDialogues(language, DialogueCategory.LevelCutscenes);
        SortDialogues(language, DialogueCategory.InterruptionsByWarrior);
        SortDialogues(language, DialogueCategory.InterruptionsByMage);
    }

    public static void SortDialogues(string language, DialogueCategory category)
    {
        //This is the main folder Unity opens.
        //If your path is different (gotta change it manually lmao), note the final \ at the very end of the string !!!!!
        string mainFolderPath = @"C:\Users\" + System.Environment.UserName + @"\Desktop\Unity\DoubleDamnation\Co-Op Prototype\46e\";

        string targetPath = mainFolderPath + @"Assets\Co-Op Prototype\Sentences\" + language;
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
            Debug.LogError("Sort Dialogues failed. The dialogue category does not exist.");
            //Aktualye... Create new folders based on the paths...
            return;
        }

        //If directory doesnt exist (e.g. different pc)
        if (Directory.Exists(mainFolderPath) == false)
        {
            Debug.LogError("||Sort Dialogues - Path Error! Directory doesn't exist/is invalid! (DirectoryNotFoundException)\n" + mainFolderPath);

            return;
        }


        Debug.Log("||Sort Dialogues - Starting **" + categoryPath.Remove(0,1) + "** Dialogue Category!||");

        //===== First Step: Deleting all previous data. Aka, deleting all dialogues and dialogueContainers. =====
        //By creating dialogues+container, they are replaced.
        //====== End of First Step ==============================================================================


        //===== Second Step: Making the appropriate dialogue container (Death, PowerUp, MidCutscenes) =====

        Debug.Log("||Sort Dialogues - Starting Second Step!||");

        DialogueContainer newDialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();//Scriptable Object Constructor

        //HideFlags.DontUnloadUnusedAsset happens every play/exit, loading scenes or even random moments. Without it, this would reset to default every time I compiled...
        //newDialogueContainer.hideFlags = HideFlags.DontUnloadUnusedAsset; -> works, but it doesn't work when unity is reset/restarted.
        //^mandatory thanks to JoshuaMcKenzie for explaining the flag https://forum.unity.com/threads/question-about-the-data-storage-of-scriptableobject-at-runtime.477091/

        //The container is to be made on \Sentences\English\
        AssetDatabase.CreateAsset(newDialogueContainer, targetPath.Replace(mainFolderPath, "") + categoryPath + "Dialogues.asset");

        //With this, all changes made are about to be permanent, aka written on the disk. It has started asset editting, and it finishes when invoked again (Ctrl+f .stop)
        AssetDatabase.StartAssetEditing();

        //Setting dirty, while AssetDatabase is importing/processing, means that this will be written on the disk.
        EditorUtility.SetDirty(newDialogueContainer);

        Debug.Log("||Sort Dialogues - Succesfully finished Second Step!||");
        //====================================================================================


        //===== Third Step (Hard/Complex): Sorting sentences and putting them into newly created Dialogues =====

        Debug.Log("||Sort Dialogues - Starting Third Step!||");

        if (Directory.Exists(targetPath + categoryPath) == false)
        {
            Debug.Log("||Sort Dialogues - Path Error! Directory doesn't exist/is invalid! (DirectoryNotFoundException)\n" + targetPath + categoryPath);

            //Now, stop the asset import/processing, aka everything is in some kind of buffer, ready to be put on the disk.
            AssetDatabase.StopAssetEditing();

            //Saves all assets that are AssetDatabase.editted :P To the disk permanently they go!
            AssetDatabase.SaveAssets();
            return;
        }

        //This gets all the sentences in the folder
        string[] filePathsArray = Directory.GetFiles(targetPath + categoryPath, "*.asset");//This will throw an error if the filepath doesn't exist (categorypath most often)
        List<string> pathfiles = new List<string>(filePathsArray);

        //No idea why the below if condition doesn't work, but there is a bug if no sentences in the categoryPath folder.

        //Debug.Log(pathfiles.Count + " Assets are found within" + targetPath + categoryPath);
        if (pathfiles.Count == 0)
        {
            Debug.LogError("No files/sentences inside the path.");
            return;
        }

        Sentence pickedSentence;
        int dialogueIndex = 1;//Aka what dialogue
        int lineIndex;//Aka in what line/order in the dialogue


        //Iterates the dialogue Index, exits when it reaches the end of the loop with both dialogue+line index being 0.
        while (true)
        {

            //First of all create the dialogue to go to the dialogue index.
            Dialogue createdDialogue = ScriptableObject.CreateInstance<Dialogue>();//ScriptableObject Constructor

            //HideFlags.DontUnloadUnusedAsset happens every play/exit, loading scenes or even random moments. Without it, this would reset to default every time I compiled...
            //createdDialogue.hideFlags = HideFlags.DontUnloadUnusedAsset; -> works, but it doesn't work when unity is reset/restarted.

            //Used so you won't drop an macaroni into that parameter below.
            string tempPath = targetPath.Replace(mainFolderPath, "");

            //Create the dialogue as an asset on \English\categoryPath\Dialogues
            AssetDatabase.CreateAsset(createdDialogue, (tempPath + categoryPath + @"\Dialogues\" + dialogueIndex.ToString() + ".asset"));//the dialogue index is used to indicate the name/index

            //Setting dirty, while AssetDatabase is importing/processing, means that this will be written on the disk.
            EditorUtility.SetDirty(createdDialogue);

            //Link it to the DialogueContainer
            newDialogueContainer.totalDialogues.Add(createdDialogue);

            lineIndex = 0;

            //Loops through all filepaths
            for (int i = 0; i < pathfiles.Count; i++)
            {
                //Loads the sentence (the replace is to remove the unrecognized pathname, cuz AssetDatabase starts from \Assets)
                pickedSentence = (Sentence)AssetDatabase.LoadMainAssetAtPath(pathfiles[i].Replace(mainFolderPath, "")) as Sentence;
                //^double casting to sentence cuz unity.

                //Found it, put it in the dialogues!
                if (pickedSentence.dialogueIndex == dialogueIndex && pickedSentence.lineIndex == lineIndex)
                {
                    //Add it to dialogue
                    createdDialogue.sentencesInside.Add(pickedSentence);


                    //if (dialogueIndex != 0)
                    //^this condition causes a bug for the dialogue 0 which I cannot track the true source problem (dialogue 0 gets only the first sentence)
                    //^fixed? (the below removal for re-processing)

                    //Remove so it won't be re-processed
                    pathfiles.RemoveAt(i);

                    //Increase the line index
                    lineIndex++;

                    //Get i to reset again, so it loops again from the start!
                    i = -1;//-1 instead of 0, cuz it will get i++ right after here, and the first element will always be ignored...
                }

            }

            //If in the above forloop, it didn't find any sentence, that means it doesn't exist. In other words, we are done here.
            if (lineIndex == 0)
            {
                //The final dialogue made was empty since it found nothing, but it is useless so lets delete it.

                //Firstly from the dialogueContainer
                newDialogueContainer.totalDialogues.RemoveAt(newDialogueContainer.totalDialogues.Count - 1);

                //Then deleting the dialogue itself
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(createdDialogue));

                break;
            }

            dialogueIndex++;
        }

        Debug.Log("||Sort Dialogues - Succesfully finished Third Step!||");

        //======================================================================================================

        //Now, stop the asset import/processing, aka everything is in some kind of buffer, ready to be put on the disk.
        AssetDatabase.StopAssetEditing();

        //Saves all assets that are AssetDatabase.editted :P To the disk permanently they go!
        AssetDatabase.SaveAssets();

        Debug.Log("||The task of SortingDialogues is done!||");

        //for (int i = 0; i < newDialogueContainer.totalDialogues.Count; i++)
        //Debug.Log("Text Inside: " + newDialogueContainer.totalDialogues[i].sentencesInside[0].textToDisplay);

    }

}
