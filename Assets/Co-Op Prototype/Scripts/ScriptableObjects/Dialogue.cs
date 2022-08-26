using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// In a way, Dialogue simply holds all the sentences within in a list.
/// So, for example when dialogue/cutscene of number X happens, all the sentences of that dialogue are played one by one.
/// </summary>
//[System.Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 1)]
public class Dialogue : ScriptableObject
{
    public List<Sentence> sentencesInside = new List<Sentence>();
}
