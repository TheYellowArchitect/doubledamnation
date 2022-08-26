using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This behaviour, when attached to a gameobject, it makes it transparent within [variable] seconds, and then kills it.
/// </summary>
public class MakeTransparentAndKillTMP : MonoBehaviour//Should make a sprite version too lel
{
    [Tooltip("Within how many seconds should this gameobject go from default color to fully transparent?")]
    public float timeToEndTransparency;

    private TMP_Text commonTMPText;

    private Color commonColor;

    private float lerpRate;
    private float startingColorTransparency;


    // Use this for initialization
    void Start ()
    {
        //Get the starting color
        commonTMPText = GetComponent<TMP_Text>();
        commonColor = commonTMPText.color;
        startingColorTransparency = commonColor.a;//Should be 1 in most cases

        lerpRate = 0;
    }
	
	//Update is called once per frame
	void Update ()
    {
        while (lerpRate < 1)
        {
            //Decrease the transparency
            lerpRate += Time.deltaTime / timeToEndTransparency;
            commonColor.a = Mathf.Lerp(startingColorTransparency, 0, lerpRate);//notice the 1->0 and not the reverse.

            commonTMPText.color = commonColor;

            return;
        }

        //D E L E T D I S
        Destroy(this.gameObject);
    }

}
