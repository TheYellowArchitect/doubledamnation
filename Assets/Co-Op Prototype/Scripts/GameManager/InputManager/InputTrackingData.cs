using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores warrior+mage data, for every frame! Aka on every Update(), doesnt miss any!
/// Alongside them, it also stores the current Run (TotalDeaths) and the deltaTime from last register for accurate replaying ;)
/// </summary>
[System.Serializable]
public struct InputTrackingData
{
    //the below 2 are literally what u see, pretty self-explanatory.
    public WarriorInputData warriorInputData;
    public MageInputData mageInputData;

    /// <summary>
    /// Using this, an accurate movement path can be made.
    /// </summary>
    public float deltaTimeFromLastRegister;
}
