using NaughtyAttributes;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Invoked from MageBehaviour!
public class SpellAid : Spell
{
    [Header("Unique Spell Values")]//This header should be a standard for every spell!

    [MinValue(0f)]
    [Tooltip("When the Help spell spawns spellwords (TMP Text gameobjects), how far above player's head should they spawn?")]
    public float offsetYFromWarriorHead;//10

    [Tooltip("When the Help spell spawns spellwords (TMP Text gameobjects), how fast should 1 mana spells be in X axis?")]
    public float spell1ManaVelocityX = 10;
    [Tooltip("When the Help spell spawns spellwords (TMP Text gameobjects), how fast should 1 mana spells be in Y axis?")]
    public float spell1ManaVelocityY = 10;
    [Tooltip("When the Help spell spawns spellwords (TMP Text gameobjects), how fast should 2 mana spells be in X axis?")]
    public float spell2ManaVelocityX = 10;
    [Tooltip("When the Help spell spawns spellwords (TMP Text gameobjects), how fast should 2 mana spells be in Y axis?")]
    public float spell2ManaVelocityY = 10;
    [Tooltip("When the Help spell spawns spellwords (TMP Text gameobjects), how fast should 4 mana spells be in X axis?")]
    public float spell4ManaVelocityX = 10;
    [Tooltip("When the Help spell spawns spellwords (TMP Text gameobjects), how fast should 4 mana spells be in Y axis?")]
    public float spell4ManaVelocityY = 10;

    [Tooltip("The TMP_Text Gameobject to spawn!")]
    public GameObject prefabHelpText;

    private Rigidbody2D warriorRigidbody;
    private WordManager wordManager;
    private List<string> spellStringsFromWordManager;

    private GameObject tempGameObject;
    private Rigidbody2D tempRigidbody2D;
    private int degreesOffset;//0 to 180, how much apart should each gameobject be?
    private int currentDegreesOffset;//Think of it as i, but for degreesOffset lel
    private float tempVelocityX;//The value between min and max, for Y axis, each text should have.
    private float tempVelocityY;//The value between min and max, for Y axis, each text should have.

    // Use this for initialization
    void Start ()
    {
        wordManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<WordManager>();
        warriorRigidbody = transform.parent.parent.GetComponent<Rigidbody2D>();
    }

    public override void Cast()
    {
        //Get the spell strings from word manager, so as you know how many they are and what strings the spells are!
        spellStringsFromWordManager = new List<string>(wordManager.GetSpellStrings());
        //This is a by reference, not by value, so it is commented out just in case you go spagghettiman in the future and fuck the original values
        //spellStringsFromWordManager = wordManager.GetSpellStrings();

        //Get the degrees each spellword should have from each other, so they go from 0 to 180.
        degreesOffset = 180 / spellStringsFromWordManager.Count;

        //And I will add the above for each gameobject ;)
        currentDegreesOffset = degreesOffset / 2;
        //used to be 0

        //Create a gameobject and give it speed and stuff, for every spell there is
        for (int i = 0; i < spellStringsFromWordManager.Count; i++)
        {
            //Create the GameObject
            tempGameObject = Instantiate(prefabHelpText);

            //Give the GameObject the spellstring deserving of its text
            tempGameObject.GetComponent<TMP_Text>().text = spellStringsFromWordManager[i];

            //Cache the rigidbody so you dont spam getcomponent
            tempRigidbody2D = tempGameObject.GetComponent<Rigidbody2D>();

            //Set position above warrior's head
            tempGameObject.transform.position = warriorRigidbody.transform.position + new Vector3(0, offsetYFromWarriorHead, 0);

            //Set velocity
            //This works and looks clean, but is actually bad, because its not automatic or linked.
            //In simpler words, if you want to add another spell, you gotta update this and a bunch of other shit
            //Instead of having it all referenced in one place (spell, string, manacost etc etc)
            if (spellStringsFromWordManager[i] == "aid" || spellStringsFromWordManager[i] == "observe" || spellStringsFromWordManager[i] == "death" || spellStringsFromWordManager[i] == "health")
            {
                tempVelocityX = spell1ManaVelocityX;
                tempVelocityY = spell1ManaVelocityY;
            }
            else if (spellStringsFromWordManager[i] == "push" || spellStringsFromWordManager[i] == "fire" || spellStringsFromWordManager[i] == "up")
            {
                tempVelocityX = spell2ManaVelocityX;
                tempVelocityY = spell2ManaVelocityY;
            }
            else// if (spellStringsFromWordManager[i] == "rewind")//tmw no level 3 cuz of complexity.
            {
                tempVelocityX = spell4ManaVelocityX;
                tempVelocityY = spell4ManaVelocityY;
            }

            //tempVelocityX = Mathf.Lerp(velocityXMin, velocityXMax, Random.Range(0f, 1f));
            //tempVelocityY = Mathf.Lerp(velocityYMin, velocityYMax, Random.Range(0f, 1f));

            //Reminder we want them to go towards 180 degrees, aka from left up to the right.
            tempRigidbody2D.velocity = new Vector2(Mathf.Cos( (currentDegreesOffset + 0) * Mathf.Deg2Rad) * tempVelocityX, Mathf.Sin((currentDegreesOffset + 0) * Mathf.Deg2Rad) * tempVelocityY);//Mathf.Tan(currentDegreesOffset) * velocityMin);

            //I don't want the spells to go downwards, so we revert the velocity of the negativeY ones
            if (tempRigidbody2D.velocity.y < 0)
                tempRigidbody2D.velocity = new Vector2(tempRigidbody2D.velocity.x, tempRigidbody2D.velocity.y * -1 * 0.75f);

            Debug.Log("Degrees: " + (currentDegreesOffset + 0));
            Debug.Log("DegreesRadians: " + currentDegreesOffset * Mathf.Deg2Rad);
            Debug.Log("Cos(Degrees): " + (Mathf.Cos((currentDegreesOffset + 0) * Mathf.Deg2Rad)));
            Debug.Log("Spellname: " + spellStringsFromWordManager[i]);

            //Gotta have them rotated somewhat.
            tempGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, currentDegreesOffset - 90));//-90

            //Increase so offset is good, think of i++; at the end of a loop ;)
            currentDegreesOffset = currentDegreesOffset + degreesOffset;
        }

        //SFX?
    }
}
