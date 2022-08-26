using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunbeamToggle : MonoBehaviour
{
    public GameObject yellowSunbeams;
    public GameObject blaccSunbeams;

    public Color yellowColor;
    public Color blaccColor;

    private float lerpRate = 0;

    /// <summary>
    /// Wrapper to call coroutine that changes transparency
    /// </summary>
    public void Toggle()
    {
        StartCoroutine(ChangeTransparency());
    }
	
	private IEnumerator ChangeTransparency()
    {
        //Cycle here
        while (lerpRate < 1)//commonColor.a > 0)
        {
            //Progress the linear interpolation
            lerpRate += Time.deltaTime / 2;

            //Increase the transparency for yellow, decrease for blacc
            yellowColor.a = Mathf.Lerp(0, 0.95f, lerpRate);//0.95f and 0.85f is the maximum transparency for each color.
            blaccColor.a  = Mathf.Lerp(0.85f, 0, lerpRate);

            //Apply the color changes
            ApplyColor(yellowColor, blaccColor);

            //Wait a frame
            yield return null;
        }

        //Transparency is finished, so delet dis.
        Destroy(this);
    }

    private void ApplyColor(Color yellowColor, Color blaccColor)
    {
        //Increase transparency for yellow color
        for (int i = 0; i < yellowSunbeams.transform.childCount; i++)
            yellowSunbeams.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = yellowColor;

        //Decrease transparency for blacc color
        for (int i = 0; i < blaccSunbeams.transform.childCount; i++)
            blaccSunbeams.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = blaccColor;
    }
}
