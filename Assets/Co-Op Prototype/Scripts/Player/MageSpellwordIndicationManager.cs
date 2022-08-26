using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageSpellwordIndicationManager : MonoBehaviour
{
    [Header("The UI Text Gameobject")]
    public GameObject spellwordIndication;

    [Header("Position to spawn Indication")]
    public float offsetX = 0;
    public float offsetY = 6;

    private Rigidbody2D warriorRigidbody;

    private void Start()
    {
        warriorRigidbody = transform.parent.parent.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update ()
    {
        spellwordIndication.transform.position = warriorRigidbody.transform.position + new Vector3(0, offsetY, 0);
    }
}
