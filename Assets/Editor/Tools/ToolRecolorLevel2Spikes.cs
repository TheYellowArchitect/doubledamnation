using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Made this hacky script, to change the color of all the spikes in level 2, at once, instead of clicking each like an braindead monkey.
/// </summary>
public class ToolRecolorLevel2Spikes : MonoBehaviour
{
    [MenuItem("Tools/ColorChange")]
    public static void ChangeSpikes()
    {
        GameObject spikeParent = GameObject.Find("Spikes");

        //If the spikes exist
        if (spikeParent != null)
        {
            //When to end the loop, depending on how many children/spikes there are
            int loopLimit = spikeParent.transform.childCount;

            for (int i = 0; i < loopLimit; i++)
            {
                //Has a sprite renderer, aka isnt a holder/parent empty gameobject
                if (spikeParent.transform.GetChild(i).GetComponent<SpriteRenderer>() != null)
                {
                    Debug.Log(spikeParent.transform.GetChild(i).GetComponent<SpriteRenderer>().color);

                    if (spikeParent.transform.GetChild(i).GetComponent<SpriteRenderer>().color != Color.white)
                        spikeParent.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                }                
                    
            }
        }
    }
}
