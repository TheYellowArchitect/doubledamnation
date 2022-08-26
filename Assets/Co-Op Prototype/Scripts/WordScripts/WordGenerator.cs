using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordGenerator : MonoBehaviour
{
    //Insert dictionary here
    private static string[] wordList = { "word1", "word2", "kek", "haha!" };

    public static string GetRandomWord()
    {
        //Gets the location/index of a word, randomly from wordList
        int randomIndex = Random.Range(0, wordList.Length);

        //Gets the word affiliated with the randomIndex
        string randomWord = wordList[randomIndex];

        return randomWord;
    }
}
