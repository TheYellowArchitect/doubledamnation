using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script has one job.
/// Send the position data to PlayerStatsManager.
/// Should be attached by default to Warrior.
/// </summary>
public class SpeedrunPositionTracker : MonoBehaviour
{
    [Tooltip("Per how many frames, to Track/Register this gameobject's position?" +
        "\n(The less this number, the more frequent the tracking/registering is)")]
    public int updateRate = 10;

    private int currentFramesCounted = 0;
    private float deltaTimeFromLastUpdate;
    private float timeFromLastUpdate;

    private SpeedrunManager speedrunManager;

	// Use this for initialization
	void Start ()
    {
        //Cache it.
        speedrunManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<SpeedrunManager>();

        //Error-handling just in case. (inb4 cosmic ray bitflips the above and makes this if condition be actually useful kappa)
        if (speedrunManager == null)
            Destroy(this);

        //Not much to say, it stores the earliest time.
        //Has to be cached here instead of variable declaration, otherwise error happens.
        timeFromLastUpdate = Time.unscaledTime;
    }
	
	// Update is called once per frame
	void Update ()
    {
        currentFramesCounted++;

        if (currentFramesCounted == updateRate)
        {
            //Reset how many frames are counted for next update
            currentFramesCounted = 0;

            //Update what was the difference in time, between last update and current one
            deltaTimeFromLastUpdate = Time.unscaledTime- timeFromLastUpdate;

            //Create current update's positionData we want to store, and update it with current stats
                //Create
                PlayerPositionTrackingData currentPlayerPositionTrackingData = new PlayerPositionTrackingData();

                //Update
                currentPlayerPositionTrackingData.position = transform.position;
                currentPlayerPositionTrackingData.deltaTimeFromLastRegister = deltaTimeFromLastUpdate;
                currentPlayerPositionTrackingData.currentRun = (short) PlayerStatsManager.globalInstance.GetTotalDeaths();

            //Send this data to PlayerStatsManager!
            //  Passed by value, but couldnt this be passed by reference? I mean, its as easy as using 'ref'
            //  but perhaps garbage collector will have issues, cuz it will be replaced here,
            //  hence the original reference will be lost.
            PlayerStatsManager.globalInstance.UpdatePlayerPositionTrackingData(currentPlayerPositionTrackingData);

            //Remember what was the time of this update, so the difference of next update is easy to calculate.
            timeFromLastUpdate = Time.unscaledTime;
        }
	}
}