using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunbeamDialogueNotifier : MonoBehaviour
{
    void Start()
    {
        SunbeamDialogueManager.globalInstance.ActivateForThisLevel();
    }
}
