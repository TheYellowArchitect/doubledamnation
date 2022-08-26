using UnityEngine;

/// <summary>
/// Changes the background color for both clay+fill, for every level load
/// </summary>
public class BackgroundLevelChange : MonoBehaviour
{
    [Header("Plug the Background here")]
    public SpriteRenderer clay;
    public SpriteRenderer flatColor;

    [Header("Level1")]
    public Color clay1;
    public Color flatColor1;

    [Header("Level2")]
    public Color clay2;
    public Color flatColor2;

    [Header("Level3")]
    public Color clay3;
    public Color flatColor3;

    [Header("Level4")]
    public Color clay4;
    public Color flatColor4;

    [Header("Level5")]
    public Color clay5;
    public Color flatColor5;

    public void OnLevelLoad()
    {
        //You could use an index/forloop for the below, retard.

        if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
        {
            clay.color = clay1;
            flatColor.color = flatColor1;
        }
        else if (LevelManager.currentLevel == 2)
        {
            clay.color = clay2;
            flatColor.color = flatColor2;
        }
        else if (LevelManager.currentLevel == 3)//equals 2, so it won't re-do it for every level after it. I should do this in other scripts as well..
        {
            clay.color = clay3;
            flatColor.color = flatColor3;
        }
        else if (LevelManager.currentLevel == 4)//equals 2, so it won't re-do it for every level after it. I should do this in other scripts as well..
        {
            clay.color = clay4;
            flatColor.color = flatColor4;
        }
        else if (LevelManager.currentLevel == 5)//equals 2, so it won't re-do it for every level after it. I should do this in other scripts as well..
        {
            clay.color = clay5;
            flatColor.color = flatColor5;
        }
    }
}
