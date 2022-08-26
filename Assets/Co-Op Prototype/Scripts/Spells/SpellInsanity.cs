using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// By default de-activated ;)
/// It makes the mode more accessible but does destroy the hype of finding what "End" does ;)
/// Hence if you want to add it (should be some variation idk), just add it in the spellstrings
/// </summary>
public class SpellInsanity : Spell
{

    //[Header("Unique Spell Values")]//This header should be a standard for every spell!

    public override void Cast()
    {
        GameObject.FindGameObjectWithTag("BackgroundManager").GetComponent<InsanityToggle>().ToggleInsanity();
    }
}
