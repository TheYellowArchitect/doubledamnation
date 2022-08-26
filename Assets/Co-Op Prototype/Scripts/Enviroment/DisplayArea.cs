using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayArea : MonoBehaviour
{
    //TODO: Delete this variable in non-debugging.
    public bool activated = false;

    [Tooltip("The text that will be displayed in the middle of the screen")]
    public string areaName;

    [Tooltip("The font to use")]
    public TMP_FontAsset font;

    //idk
    public Color color;

    [Tooltip("For how long it stays onscreen, before start fading")]
    public float timeToDisplayActive;

    [Tooltip("If player(s) don't move, when should this show?\n-1 means never, which is the default for a good reason, this is used only for final level lmao")]
    public float timeUntilAutomaticallyDisplayed = -1;

    private TextMeshProUGUI textAreaUI;

    private void Start()
    {
        textAreaUI = GameObject.FindGameObjectWithTag("NewArea").GetComponent<TextMeshProUGUI>();

        if (timeUntilAutomaticallyDisplayed != -1)
            Invoke("TimerInvocation", timeUntilAutomaticallyDisplayed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated == false)
        {
            activated = true;
            StartCoroutine(DisplayOverTime());
        }
    }

    public IEnumerator DisplayOverTime()
    {
        textAreaUI.text = areaName;

        textAreaUI.color = color;

        textAreaUI.font = font;

        yield return new WaitForSecondsRealtime(timeToDisplayActive);

        for (float i = 1; i > 0; i -= 0.01f)
        {
            textAreaUI.color = new Color(textAreaUI.color.r, textAreaUI.color.g, textAreaUI.color.b, i);
            yield return null;//Framerate dependent, but who cares.
        }
            
    }

    /// <summary>
    /// Called to display area, if timeUntilAutomaticallyDisplayed is different than -1 aka a timer should run.
    /// </summary>
    public void TimerInvocation()
    {
        if (activated == false)
        {
            activated = true;
            StartCoroutine(DisplayOverTime());
        }
    }
}
