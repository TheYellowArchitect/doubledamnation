using UnityEngine;
using TMPro;
using NaughtyAttributes;

//Shouldn't this be elsewhere? Like... The menu? Regardless, it is contained in the same namespace as everything else, so it is theoritically a global enum now.
public enum Language { English, SimplifiedChinese, Russian, Korean, German, Japanese, French, Greek, AncientGreek, Spanish};

[System.Serializable]
[CreateAssetMenu(fileName = "New Sentence", menuName = "Sentence", order = 0)]
public class Sentence : ScriptableObject
{
    [InfoBox("Want to translate this?\nIf so, copy-paste all Sentences(like this one) into the language folder you want to translate\n" +
        "Then, select them all, and change the language. And then, translate the strings for each one! Doing that, you finished like 90% of the translation!\n" +
        "Though, truth be told, replacing the strings is the hard part hahaha\nRegardless, do not hesitate to reach out to me if you want to do a translation!!!")]
    //then right-click extraction to me and ggwp, another language and fanbase is pleased :) Bless the translators!
    //For me, all I have to do after the extraction is simply run the "tool" I made! :) 
    //And ofc, if it doesn't work, the font is incompatible, and change it to something of that language :D

    [Header("Main Attributes")]

    [TextArea]
    [Tooltip("The string/text displayed in-game. If translating, change this.")]
    public string textToDisplay = "";

    [Tooltip("The \"Ancient Greek\" font only supports english characters.\nIf you are translating this, put a generic font, the best of your language, if not something resembling this.")]
    public TMP_FontAsset textFont;

    [Tooltip("Every voice clip matches the textToDisplay.\nAlso, shouldn't be tweaked on translation/localisation.")]//weeb voice-acting + ancient greek voice acting ftw!
    public AudioClip voiceClip;

    [Tooltip("This should be left alone regardless of translation/localization/language.")]
    public Color textColor = Color.black;

    [Tooltip("This is the animation that will play for P1, when saying it.\nWon't play any animation if the string is named \"None\"")]
    public string warriorAnimation = "None";

    [Tooltip("This is the animation that will play for P2, when saying it.\nWon't play any animation if the string is named \"None\"")]
    public string mageAnimation = "None";

    [Header("Metadata")]
    public Language selectedLanguage;

    //[MinValue(1)]
    [Tooltip("Which dialogue does it trigger? In which dialogue is it contained?")]
    public int dialogueIndex;

    [Tooltip("In the dialogue used inside, when does it play? In what order?")]
    public int lineIndex;

    /* Transferred onto ToolSentencesFontColor
    [Button("WarriorFont&Color")]
    public void SetWarriorFontColor()
    {
        textColor = Color.black;

        textFont = (TMP_FontAsset) UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets\\Co-Op Prototype\\Fonts\\WarriorFont(Dalek) SDF.asset") as TMP_FontAsset;
    }

    [Button("MageFont&Color")]
    public void SetMageFontColor()
    {
        textColor = Color.white;

        textFont = (TMP_FontAsset)UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets\\Co-Op Prototype\\Fonts\\MageFont(Elektra) SDF.asset") as TMP_FontAsset;
    }
    */
}
