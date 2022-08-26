using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Would make it an interface, but Unity doesn't play well with them, sadly.
//Mainly, the variables demand to be member'd which is bloat, and the UI also hates them, aka doesn't display them at all.
//If you can somehow fix the above 2 issues, do rethink converting to interface. 
//Though, look at warriorHealth which insists changing interface to class even if above issues weren't a thing.
/// <summary>
/// This is the father class of all spells. It contains:\n
/// int manaCost
/// string spellName
/// ??? animationToPlay
/// Cast()
/// </summary>
public abstract class Spell : MonoBehaviour
{
    [Header("Mandatory Values")]
    [Tooltip("How much mana should this spell consume to activate?")]
    public int manaCost;
    [Tooltip("The string name to be detected from MageBehaviour")]
    public string spellName;
    //??? animationToPlay
    //bool activated (by default On)

    public abstract void Cast();
	
}
