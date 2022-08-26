using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WarriorInputData
{
    //Both below, range from [-1,+1] in both x and y.
    public SerializableVector2 movementInputDirection;//Movement direction
    public SerializableVector2 combatInputDirection;//Attack direction
}