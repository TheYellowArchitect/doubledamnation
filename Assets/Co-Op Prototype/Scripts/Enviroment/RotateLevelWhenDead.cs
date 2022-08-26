using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// What this script does, is you put the enemies inside here, and when they are all dead, rotates the level by X degrees, in Y seconds!
/// </summary>
public class RotateLevelWhenDead : MonoBehaviour
{
    //[Tooltip("When rotate starts, which game object gets activated?\nParticleSystem holder ;)")]
    //public GameObject gameObjectToEnable;

    [Tooltip("In total, how many degrees should this object rotate?")]
    public float degreesToRotate;

    [Tooltip("How fast should the rotation happen?")]
    public float duration;

    [Tooltip("A list of the monsters (gameobject). When all of them die while player is alive, the rotation happens.")]
    public List<GameObject> monsters = new List<GameObject>();

    private WarriorHealth warriorHealth;

    private Quaternion startingQuaternion;
    private Quaternion targetQuaternion;

    private bool startRotating = false;
    private bool reachedRotation = true;

    [SerializeField]
    [ReadOnly]
    private float lerpTimeCounter = 0;

	//Why am i doing this again?
	void Start ()
    {
        startingQuaternion = transform.rotation;

        targetQuaternion = startingQuaternion * Quaternion.Euler(0, 0, degreesToRotate);//Multiplication, I think adds the quaternions.

        warriorHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorHealth>();
	}
	
	// Update is called once per frame
	void Update ()
    {

        if (startRotating == false)
            DetermineStartRotation();
        //if (Input.GetKeyDown(KeyCode.Space)) startRotating = true;

        if (startRotating)
        {
            lerpTimeCounter += Time.deltaTime;

            transform.rotation = Quaternion.Lerp(startingQuaternion, targetQuaternion, lerpTimeCounter / duration);

            //Debug.Log("Z is: " + transform.rotation.eulerAngles.z);

            //If the targetRotation is achieved, destroy this script, so it won't consume resources.
            if (transform.rotation.eulerAngles.z < 270.01f && transform.rotation.eulerAngles.z > 269.99f)
                FinishedRotating();
        }
	}

    /// <summary>
    /// Iterates through all the monsters (list) and sets startRotating = true; when all are dead!
    /// </summary>
    /// <returns></returns>
    public void DetermineStartRotation()
    {
        foreach (GameObject pickedGameobject in monsters)
            if (pickedGameobject.activeSelf == true)
                return;

        //gameObjectToEnable.SetActive(true);

        startRotating = true;
    }

    public void FinishedRotating()
    {
        GameObject.FindGameObjectWithTag("Mage").GetComponent<SpellDeath>().deathSpellwordEasterEggEnabled = true;
        Destroy(this);
    }

}
