using UnityEngine;

/// <summary>
/// Contains a Vector2 Position (of the player), the deltaTime from last time a struct like this was registered, and the currentRun/TotalDeaths.
/// This is simply a timestamp, of the player position, so his path can be recreated (deltaTime helps with accuracy)
/// </summary>
[System.Serializable]
public struct PlayerPositionTrackingData
{
    /// <summary>
    /// Position in the world space of the level (X,Y, cuz Z is always 0)
    /// </summary>
    public Vector2 position;

    /// <summary>
    /// Using this, an accurate movement path can be made.
    /// </summary>
    public float deltaTimeFromLastRegister;

    /// <summary>
    /// This is always the same with the amount of totalDeaths ;)
    /// </summary>
    public short currentRun;
}

/// Reminder that to get the order of the playerPositions, you dont need to get a "currentRunIndex" variable.
/// Because, level 1 -> level 2 -> level 3, and in each level list, they are added in order ;)
/// Hence, it should be possible to get the order of the positions. When the position is around the start area, then... it is a new run.
/// 
/// However, I dont want to spend time on it, since it seems prone to bugs, and should have some pretty good logic to not fuck up.
/// And that's why, it also has the variable: currentRun.
/// Which is actually the same as the amount of total deaths.