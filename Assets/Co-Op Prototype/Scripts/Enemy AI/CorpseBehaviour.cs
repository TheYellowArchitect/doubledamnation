using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Just applies "Gravity".
public class CorpseBehaviour : MonoBehaviour
{
    const int Hollow = 0, Centaur = 1, Satyr = 2, Minotaur = 3, Harpy = 4, Cyclops = 5, Unknown = 6, Hollow2 = 7, Centaur2 = 8, Satyr2 = 9, Minotaur2 = 10, Harpy2 = 11, Cyclops2 = 12, Unknown2 = 13;

    public int monsterType;

    //[Tooltip("How fast this corpse falls into the ground")]
    //public int fallingSpeed;

    [Tooltip("The sprite when it touches the ground")]
    public Sprite groundSprite;

    [Tooltip("The sprite to play while falling")]
    public Sprite fallingSprite;

    private Rigidbody2D commonRigidbody;
    private SpriteRenderer commonRenderer;

    //Cacheboi
    private Vector2 tempCollisionPoint;


	void Awake ()
    {
        commonRigidbody = GetComponent<Rigidbody2D>();
        commonRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        tempCollisionPoint = collision.contacts[0].normal;

        if (tempCollisionPoint.y > 0.95f)
        {
            //commonRigidbody.velocity = Vector2.zero;

            //commonRigidbody.Sleep();

            commonRenderer.sprite = groundSprite;
        }
    }

    private void OnEnable()
    {
        //if (commonRigidbody.IsSleeping())
            //commonRigidbody.WakeUp();

        //commonRigidbody.velocity = Vector2.down * fallingSpeed;
    }

    //Order is Awake->OnEnable->Start

    /// <summary>
    /// Determines which sprite will be "initialized", because if falling, it should be different (aka not flat lul)
    /// Also, this happens always before any collision.
    /// </summary>
    /// <param name="isGrounded"></param>
    public void SetCorpseSprite(bool isGrounded)
    {
        if (isGrounded)
            commonRenderer.sprite = groundSprite;
        else
            commonRenderer.sprite = fallingSprite;
    }
}
