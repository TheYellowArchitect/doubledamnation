using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Don't judge.
//My hand hurts, but I gotta finish this shit, sleep nor hunger can stop me. (nor logic lul ME GO FACE, ME GO FINISH)

//This exists once in EarthLevel and once in FireLevel
//Runs by itself
//Tbh, it could have no public variables at all.
//The color by getting their color
//The transparency same way.
public class SunbeamDialogueManager : MonoBehaviour
{
    public static SunbeamDialogueManager globalInstance;

    public float timeToEndTransparency = 3f;

    [Header("Below are read-only")]

    //flag to determine shit
    public bool hasPlayerSeenSunbeamTop = false;

    

    public GameObject[] sunbeamParents;
    public GameObject[] sunbeamTriggers;

    public Color levelColor;
    public float levelTransparency;//0.53 for level1, 0.85 for level2

    private float lerpRate = 0;
    

    void Start()
    {
        globalInstance = this;
    }

    //Invoked by SunbeamDialogueManagerNotifier s
    //Since not every level has lighting, and i dont want to bloat LevelManager
    //There are Notifiers in every level that has them which exist only to notify dis boi
    public void ActivateForThisLevel()
    {
        sunbeamParents = GameObject.FindGameObjectsWithTag("SunbeamParents");
        sunbeamTriggers = GameObject.FindGameObjectsWithTag("SunbeamTopTriggers");

        levelColor = sunbeamParents[0].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color;
        levelTransparency = levelColor.a;

        DetermineSunbeamRemoval();
    }

    public void WitnessedSunbeamTop()
    {
        if (hasPlayerSeenSunbeamTop == true)
            return;


        hasPlayerSeenSunbeamTop = true;

        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DialogueManager>().PlayerEnteredMidCutscene(15);

        DestroyAllSunbeamCollisionTriggers();

        StartCoroutine(DelayedLightRayRemoval());
    }
    
    public IEnumerator DelayedLightRayRemoval()
    {
        yield return new WaitForSeconds(2);

        ToggleSunbeamTransparency();
    }

    //Invoked on each level load
    public void DetermineSunbeamRemoval()
    {
        if (hasPlayerSeenSunbeamTop == false)
            return;

        DestroyAllSunbeamCollisionTriggers();
        ToggleSunbeamTransparency();
    }

    public void DestroyAllSunbeamCollisionTriggers()
    {
        for (int i = 0; i < sunbeamTriggers.Length; i++)
            Destroy(sunbeamTriggers[i]);
    }

    //==========================
    //    Changing Color
    //==========================



    /// <summary>
    /// Wrapper to call coroutine that changes transparency
    /// </summary>
    public void ToggleSunbeamTransparency()
    {
        lerpRate = 0;

        StartCoroutine(ChangeTransparency());
    }

    

    private IEnumerator ChangeTransparency()
    {
        //Cycle here
        while (lerpRate < 1)//commonColor.a > 0)
        {
            //Progress the linear interpolation
            lerpRate += Time.deltaTime / timeToEndTransparency;

            levelColor.a = Mathf.Lerp(levelTransparency, 0, lerpRate);

            //Apply the color changes
            ApplyColor(levelColor);

            //Wait a frame
            yield return null;
        }

        //Transparency is finished, so delet the sunbeams and triggers?
    }

    private void ApplyColor(Color currentColor)
    {
        for (int i = 0; i < sunbeamParents.Length; i++)
        {
            //Increase transparency for yellow color
            for (int j = 0; j < sunbeamParents[i].transform.childCount; j++)
                sunbeamParents[i].transform.GetChild(j).gameObject.GetComponent<SpriteRenderer>().color = currentColor;
        }
        
    }
}
