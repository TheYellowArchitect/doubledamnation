using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WordManager : MonoBehaviour
{
    public static WordManager globalInstance;

    //We store here the list of all possible "spells"
    public List<Word> spells;

    public List<string> spellStrings;

    public WordDisplay wordDisplay;

    //Active word so player won't type letters and be "fulfilling" more than 2 words at the same time.
    private bool hasActiveWord;
    private Word activeWord;

    //Timers
    Timer expiredWordTimer;

    //To notify warriorInput
    public Action<string> spellActivated;
    public Action<string> incompleteSpellWritten;

    private void Start()
    {
        globalInstance = this;

        TimerManager timerManager = GetComponent<TimerManager>();
        expiredWordTimer = timerManager.CreateTimer(expiredWordTimer, 1, 1f, false, true);
        expiredWordTimer.TriggerOnEnd += ExpireWord;

        AddSpells();        
    }

    public void AddSpells()
    {
        foreach (string pickedString in spellStrings)
        {
            spells.Add(new Word(pickedString));
        }
    }

    //Add a spell in the possible list, so as to be able to type it
    public void AddSpellWord(string spellToAdd)
    {
        spellStrings.Add(spellToAdd);

        spells.Add(new Word(spellToAdd));
    }

    public void RemoveSpellWord(string spellToRemove)
    {
        spellStrings.Remove(spellToRemove);

        for (int i = 0; i < spells.Count; i++)
        {
            if (spells[i].thisWord == spellToRemove)
                spells.RemoveAt(i);
        }
    }

    //Used for TASbot -_-
    public bool IsStringSpellWord(string spellword)
    {
        for (int i = 0; i < spells.Count; i++)
        {
            if (spells[i].ToString() == spellword)
                return true;
        }

        return false;
    }

    public void TypeLetter (char letter)
    {
        //Debug.Log("Has Active Word: " + hasActiveWord);

        //In case player(s) have caps lock active.
        letter = Char.ToLower(letter);

        if (hasActiveWord)
        {
            //Check If the letter we typed was the next letter to the sequence
            if (activeWord.GetNextLetter() == letter)
            {
                //Notifies UI display, and increases letter index by 1
                wordDisplay.CompleteLetter(activeWord.ToString(), activeWord.typeIndex);
                activeWord.typeIndex++;

                expiredWordTimer.ResetTime();

                if (incompleteSpellWritten != null)
                    incompleteSpellWritten(activeWord.ToString().Substring(0, activeWord.typeIndex));
            }
            else//No need to check if shift/space since it doesnt get inside TypeLette
                ClearActiveWord();//Negative/Red VFX

        }
        else
        {
            //Check if the letter we typed is the beginning letter of any of the words at screen
            foreach (Word pickedWord in spells)
            {
                if (pickedWord.GetNextLetter() == letter)
                {
                    activeWord = pickedWord;
                    hasActiveWord = true;

                    wordDisplay.SetWord(pickedWord.ToString());

                    expiredWordTimer.Activate();                    

                    //Notifies UI display, and increases letter index by 1
                    wordDisplay.CompleteLetter(activeWord.ToString(), activeWord.typeIndex);
                    activeWord.typeIndex++;

                    expiredWordTimer.ResetTime();

                    if (incompleteSpellWritten != null)
                        incompleteSpellWritten(activeWord.ToString().Substring(0, activeWord.typeIndex));

                    break;
                }
            }
        }

        //Finished an word
        if (hasActiveWord && activeWord.IsTypedEntirely())
        {
            //Sends the string to warriorInput
            if (spellActivated != null)
                spellActivated.Invoke(activeWord.ToString());

            //Positive VFX
            ClearActiveWord();


            //Determine what happens nao
        }
    }

    public void ClearActiveWord()
    {
        hasActiveWord = false;

        if (activeWord != null)
        {
            activeWord.typeIndex = 0;
            activeWord = null;
        }

        wordDisplay.Reset();        
    }

    //Activated via timer
    public void ExpireWord()
    {
        Debug.Log("Word Expired");

        //If there is still an active word, delete it.
        if (activeWord != null)
            ClearActiveWord();

        //With neutral vfx ofc
    }

    //Useless but you never know.
    /// <summary>
    /// Returns how many spells the player/mage can cast.
    /// </summary>
    /// <returns></returns>
    public int GetSpellStringsCount()
    {
        return spellStrings.Count;
    }

    //Used by MageBehaviour, for "Help" spell, to know which strings to spawn, and how many they are
    public List<string> GetSpellStrings()
    {
        return spellStrings;
    }

}