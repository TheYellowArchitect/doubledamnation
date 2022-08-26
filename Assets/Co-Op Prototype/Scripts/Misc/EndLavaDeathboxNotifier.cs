using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLavaDeathboxNotifier : MonoBehaviour
{
    public bool activated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated == false)
        {
            activated = true;

            //Notify FinalBattleManager
            FinalBattleManager.globalInstance.InitiateCombatEnding(collision.gameObject);
        }
    }
}
