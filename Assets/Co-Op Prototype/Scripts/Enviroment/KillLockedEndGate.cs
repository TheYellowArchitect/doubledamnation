using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriggerLevelCutscene))]
public class KillLockedEndGate : MonoBehaviour
{
    public List<EnemyBehaviour> killedEnemiesToUnlock;

    private Action enemyDiedAction;

    private TriggerLevelCutscene endGateCutscene;

    private int currentDeaths;
    private int deathsNeededToUnlock;

	// Use this for initialization
	void Start ()
    {
        endGateCutscene = GetComponent<TriggerLevelCutscene>();
        endGateCutscene.lockedGate = true;

        deathsNeededToUnlock = killedEnemiesToUnlock.Count;

        for (int i = 0; i < deathsNeededToUnlock; i++)
            killedEnemiesToUnlock[i].died += OnDeath;
	}
	
	public void OnDeath()
    {
        currentDeaths++;

        if (currentDeaths >= deathsNeededToUnlock)
            UnlockEndGate();
    }

    public void UnlockEndGate()
    {
        endGateCutscene.lockedGate = false;

        GetComponent<SunbeamToggle>().Toggle();
    }
}
