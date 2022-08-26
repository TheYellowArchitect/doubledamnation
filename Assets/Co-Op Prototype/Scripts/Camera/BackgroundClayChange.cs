using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Changes the clay background (via spriterenderer.sprite) to have the alternatives, for every death.
/// </summary>
public class BackgroundClayChange : MonoBehaviour
{

    [Header("Plug the Background here")]
    public SpriteRenderer clay;

    [Header("Alternative Clay Sprites")]
    public List<Sprite> clayList = new List<Sprite>();

    private void Start()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>().startedRevive += ChangeClayBackground;
    }

    public void ChangeClayBackground()
    {
        clay.sprite = clayList.GetRandomListIndexElementExcept(clay.sprite);
    }

}
