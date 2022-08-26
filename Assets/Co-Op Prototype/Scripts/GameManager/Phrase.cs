using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Data-Driven programming when. tfw ECS will not be pure and stable/functional for at least 4 years... smh!
[Serializable]
public class Phrase
{
    //Shoulda use properties. Ah well. At least it's not spagghetti.

    public string sentence;

    public Color mainColor;

    public TMP_FontAsset font;
    //public Font font;

    public float timeToNextPhrase;

    //At what death should this play?
    public int deathCount;

    //Which cutscene should this start playing?
    public int cutsceneIndex;

    //Voice clip for voice acting? :P

    public Phrase()
    {
        sentence = "";
        mainColor = Color.black;
        timeToNextPhrase = 1f;
    }

    public void SetSentence(string _sentence)
    {
        sentence = _sentence;
    }

    public void SetMainColor(Color _mainColor)
    {
        mainColor = _mainColor;
    }

    public void SetFont(TMP_FontAsset _font)
    {
        font = _font;
    }

    public void SetTime(float _Delay)
    {
        timeToNextPhrase = _Delay;
    }

    public void SetDeathCount(int _DeathCount)
    {
        deathCount = _DeathCount;
    }

    public void SetCutsceneIndex(int i)
    {
        cutsceneIndex = i;
    }

    public override string ToString()
    {
        return sentence;
    }

    public Color GetMainColor()
    {
        return mainColor;
    }

    public TMP_FontAsset GetFont()
    {
        return font;
    }

    public float GetTime()
    {
        return timeToNextPhrase;
    }

    public int GetDeathCount()
    {
        return deathCount;
    }

    public int GetCutsceneIndex()
    {
        return cutsceneIndex;
    }
}
