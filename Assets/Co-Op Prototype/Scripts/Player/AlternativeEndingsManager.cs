using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternativeEndingsManager : MonoBehaviour
{
    [Header("Prefabs to spawn")]
    [Tooltip("The warrior in taunt pose, simply a sprite in a gameobject")]
    public GameObject warriorTauntSpriteObject;

    [Tooltip("The warrior in crouch pose, simply a sprite in a gameobject")]
    public GameObject warriorCrouchSpriteObject;

    [Tooltip("The mage in death pose, simply a sprite in a gameobject")]
    public GameObject mageDeathSpriteObject;

    [Tooltip("The mage in health pose, simply a sprite in a gameobject")]
    public GameObject mageHealthSpriteObject;

    [Header("Ending Flags")]//tmw split from insanity and default endings, you are definitely not insane
    public bool hasActivatedDeathlessEnding = false;
    public bool hasActivatedPerfectEnding = false;



    /// <summary>
    /// Invoked by FinalBattleManager to determine if it should play the normal cutscene and then hollow fighting or not
    /// </summary>
    /// <returns></returns>
    public bool IsOnAlternativeEnding()
    {
        //Deathless
        if (PlayerStatsManager.globalInstance.GetTotalDeaths() == 0)
        {
            hasActivatedDeathlessEnding = true;

            //Perfect
            if (PlayerStatsManager.globalInstance.GetTotalKills() == 0)
            {
                hasActivatedPerfectEnding = true;
                hasActivatedDeathlessEnding = false;
            }
                

            return true;
        }

        return false;
    }

    /// <summary>
    /// Invoked by FinalBattleManager
    /// </summary>
    public void DetermineAlternativeEnding()
    {
        if (hasActivatedDeathlessEnding || hasActivatedPerfectEnding)
            CleanupDefaultEnding();

        if (hasActivatedDeathlessEnding)
            StartCoroutine(DeathlessEnding());
        else if (hasActivatedPerfectEnding)
            StartCoroutine(PerfectEnding());
    }

    //Standing at the right edge ((rightly edgy)), with the taunt pose where the hand at the waist and spear at the ground
    //Talking without P2 being visible
    public IEnumerator DeathlessEnding()
    {
        //Set the ending, for the JSON
        PlayerStatsManager.globalInstance.SetEnding(Endings.Deathless);

        //Write the ending on the drive, aka save the .json on the pc
        PlayerStatsManager.globalInstance.SaveCoreStats();

        //Get the sprite to spawn
        Instantiate(warriorTauntSpriteObject);

        //Camera focus at tree and zoom out slowly
        GameObject.FindGameObjectWithTag("FinalCamera").GetComponent<Animator>().Play("DeathlessEnding");

        //Notify dialogue manager to play the dialogue
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(12);

        //===

        //This must sync to saying "Forward!"
        yield return new WaitForSeconds(62.7f);

        //Get the mage sprite to spawn
        GameObject tempGameObject = Instantiate(mageHealthSpriteObject);

        //Rotate it 270 degrees
        tempGameObject.transform.rotation = Quaternion.Euler(0, 0, 270);

        //Place it on proper position
        tempGameObject.transform.position = new Vector3(57.79f, 3.28f, 0);

        yield return new WaitForSeconds(17.551f);
        //====

        //Wait time of dialogue above
        //yield return new WaitForSeconds(80.251f);

        //Fade-in to gray
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<FadeUIManager>().FadeInGray();

        //The time to fade in^
        yield return new WaitForSeconds(1.5f);

        //Lava Sizzle SFX
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.LavaSizzle);

        //Play Death SFX (this is so sad, can we taste lava?)
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Death);

        //So I guess it won't crash when loading new scene lel
        yield return new WaitForSeconds(3);

        //Endloop!
        GetComponent<FinalBattleManager>().TheEndloop();
    }

    public IEnumerator PerfectEnding()
    {
        //Set the ending, for the JSON
        PlayerStatsManager.globalInstance.SetEnding(Endings.Perfect);

        //Write the ending on the drive, aka save the .json on the pc
        PlayerStatsManager.globalInstance.SaveCoreStats();

        //Get the sprites to spawn at the tree (agony and death)
        Instantiate(warriorCrouchSpriteObject);
        Instantiate(mageDeathSpriteObject);

        //Camera focus at tree and zoom out slowly
        GameObject.FindGameObjectWithTag("FinalCamera").GetComponent<Animator>().Play("PerfectEnding");

        //Notify dialogue manager to play the dialogue
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(13);

        //Wait time of dialogue above
        yield return new WaitForSeconds(149);//149.734f

        //Fade-in to gray
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<FadeUIManager>().FadeInGray();

        //The time to fade in^
        yield return new WaitForSeconds(1);

        //Play Death SFX (this is so sad, can we flip the screen again so tears go back in the eyes?)
        PlayerSoundManager.globalInstance.PlayAudioSource(PlayerSoundManager.AudioSourceName.Death);

        //So I guess it won't crash when loading new scene lel
        yield return new WaitForSeconds(3);

        //Endloop!
        GetComponent<FinalBattleManager>().TheEndloop();
    }


    //Something all alternative cutscenes do in finalScene, aka clean all dependencies not needed like finalOpponent1/2 ;)
    public void CleanupDefaultEnding()
    {
        //String-wise cuz lazy and this is final anyway lel

        //Delete the players, and hence, FinalCharacterBehaviour.cs
        Destroy(GameObject.Find("FinalOpponent1"));
        Destroy(GameObject.Find("FinalOpponent2"));

        //Delete the lava bounds
        Destroy(GameObject.Find("BoundsHitbox"));

        //Notify pause menu, cant have pausing in final cutscene lmao
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<PauseMenu>().InLevelCutscene();
    }
}
