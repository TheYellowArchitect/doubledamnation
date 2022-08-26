using System.Collections.Generic;
using UnityEngine;

//When this and Dialogue.cs should be structs, but Unity doesn't allow you to create them on the project folder via CreateAsset...

/// <summary>
/// This holds all the dialogues. So you can access them easier, via index/queue-to-be-played
/// One DialogueContainer exists for "Death", one for "PowerUp", and one for "MidCutscenes"
/// </summary>
//[System.Serializable]
[CreateAssetMenu(fileName = "New DialogueContainer", menuName = "DialogueContainer", order = 2)]

public class DialogueContainer : ScriptableObject
{
    public List<Dialogue> totalDialogues = new List<Dialogue>();
	
}
