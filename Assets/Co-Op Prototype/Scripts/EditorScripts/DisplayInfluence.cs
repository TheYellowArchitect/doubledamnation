using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayInfluence : MonoBehaviour
{
    private WarriorMovement warriorBehaviour;
    private TextMeshProUGUI influenceUI;

	// Use this for initialization
	void Start ()
    {
        warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();

        influenceUI = GetComponent<TextMeshProUGUI>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        influenceUI.text = "InfluenceX || " + warriorBehaviour.GetTotalInfluenceX().ToString("0") + "\nInfluenceY || " + warriorBehaviour.GetInfluenceY().ToString("0");
    }
}
