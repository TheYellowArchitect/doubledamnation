using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayBoss : MonoBehaviour
{
    //TODO: Delete this variable in non-debugging.
    public bool activated = false;

    [Tooltip("The big text that will be displayed in the middle of the screen for the boss name")]
    public string bossName;

    [Tooltip("The color of the string")]
    public Color nameColor;

    [Tooltip("The text that will be displayed right below the name.")]
    public string bossTitle;

    [Tooltip("The color of the string")]
    public Color titleColor;

    [Tooltip("The font to display on boss and title.")]
    public TMP_FontAsset font;

    [Tooltip("For how long it stays onscreen, before start fading")]
    public float timeToDisplayActive;

    private TextMeshProUGUI nameTextAreaUI;
    private TextMeshProUGUI titleTextAreaUI;

    private void Start()
    {
        nameTextAreaUI = GameObject.FindGameObjectWithTag("BossName").GetComponent<TextMeshProUGUI>();
        titleTextAreaUI = GameObject.FindGameObjectWithTag("BossTitle").GetComponent<TextMeshProUGUI>();
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
        //TODO: Don't forget the zoom!

        nameTextAreaUI.text = bossName;
        nameTextAreaUI.color = nameColor;//naming inconsistency already, smh.

        titleTextAreaUI.text = bossTitle;
        titleTextAreaUI.color = titleColor;

        //Shared font
        titleTextAreaUI.font = font;
        nameTextAreaUI.font = font;

        yield return new WaitForSecondsRealtime(timeToDisplayActive);

        for (float i = 1; i > 0; i -= 0.01f)
        {
            nameTextAreaUI.color = new Color(nameTextAreaUI.color.r, nameTextAreaUI.color.g, nameTextAreaUI.color.b, i);
            titleTextAreaUI.color = new Color(titleTextAreaUI.color.r, titleTextAreaUI.color.g, titleTextAreaUI.color.b, i);
            yield return null;//Framerate dependent, but who cares.
        }

    }
}
