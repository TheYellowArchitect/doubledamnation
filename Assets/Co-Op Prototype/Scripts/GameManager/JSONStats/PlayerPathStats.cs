using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerPathStats
{
    /// <summary>
    /// Used to identify, for which run this is used.
    /// So there wont be only 1 savefile that will be overwritten
    /// but each new run will have its own playerStats file.
    /// Should match the one with PlayerCoreStats obviously.
    /// </summary>
    public int playerStatsID;

    //All variables below are initialized by PlayerStatsManager to `new List<>()`

    //DeathPositions (aka where player died)
    public List<Vector2> level1DeathPositions;
    public List<Vector2> level2DeathPositions;
    public List<Vector2> level3DeathPositions;
    public List<Vector2> level4DeathPositions;

    //PlayerTrackingPositionsData
    //  You can use the below to make a map of most players' way of navigating the levels!
    public List<List<PlayerPositionTrackingData>> level0Positions;
    public List<List<PlayerPositionTrackingData>> level1Positions;
    public List<List<PlayerPositionTrackingData>> level2Positions;
    public List<List<PlayerPositionTrackingData>> level3Positions;
    public List<List<PlayerPositionTrackingData>> level4Positions;

    //Player(s)InputsData (initialized
    public List<List<InputTrackingData>> level0Inputs;
    public List<List<InputTrackingData>> level1Inputs;
    public List<List<InputTrackingData>> level2Inputs;
    public List<List<InputTrackingData>> level3Inputs;
    public List<List<InputTrackingData>> level4Inputs;
}