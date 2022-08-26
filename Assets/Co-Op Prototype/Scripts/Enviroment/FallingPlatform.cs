using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Tooltip("Once touched, in how much time does it fall?")]
    public float timeToFall = 0.5f;
    [Tooltip("Once falling, what is the speed(Y axis) going downwards?")]
    public float fallingSpeedY;
    [Tooltip("Once falling, what is the speed(X axis)?")]
    public float fallingSpeedX = 0;

    //Cacheboi
    private Rigidbody2D commonRigidbody;
    private Vector3 OriginPos;

    private bool toggled = false;

    private Collider2D commonPlatformCollider;
    //When falling, hitting the head, it shouldn't float -_-
    private Collider2D enemyCollider;//I should make it list but w/e, super rare to bug, and consumes more memory ;P

    private short pillarsRegistered = 0;

    private void Start()
    {
        commonRigidbody = GetComponent<Rigidbody2D>();
        OriginPos = transform.position;

        commonPlatformCollider = GetComponent<PlatformBehaviour>().platformCollider;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SpellPillar"))
            pillarsRegistered++;

        //When encountering lava, platform goes +5 on Z-depth, so it hides from camera, as if lava consumed it!
        //Level manager on reset, gets its Z to 0, by this class's ResetPlatform()
        //if (collision.gameObject.CompareTag("Lava"))
            //transform.position = new Vector3(transform.position.x, transform.position.y, -15);

        //Z depth trick didnt work since the collider stayed there. And i dont want toggling shit. So just take it far far away, beyond the winds of oblivion~
        if (collision.gameObject.CompareTag("Lava"))
        {
            //B E G O N E
            transform.position = new Vector3(transform.position.x, transform.position.y - 500, transform.position.z);//You should make a YBOTTOM_LEVEL variable instead of yolo - 500. Then do -200 that and ggwp.

            //No speed, so it wont consume computational resources via velocity bs
            commonRigidbody.velocity = Vector2.zero;
        }
            


        if (collision.gameObject.CompareTag("Enemy"))
            if (collision.gameObject.GetComponent<EnemyBehaviour>().monsterType == 4)//Harpy
                return;

        //If level editor and paused, do not fall, but act as normal platform.
        if (LevelManager.currentLevel == 7 && LevelEditorMenu.isPlayMode == false)
            return;

        if (toggled == false)
            StartCoroutine(FallTimer());
        else if (collision.gameObject.CompareTag("Enemy") && collision.contacts[0].normal.y > 0)
        {
            enemyCollider = collision.gameObject.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(commonPlatformCollider, enemyCollider, true);
        }


    }

    IEnumerator FallTimer()
    {
        toggled = true;

        yield return new WaitForSeconds(timeToFall);

        commonRigidbody.velocity = new Vector2(fallingSpeedX, -fallingSpeedY);//- so it goes downwards
    }

    public void ResetPlatform()
    {
        if (toggled)
        {
            //No speed
            commonRigidbody.velocity = Vector2.zero;

            //Relocated to original position
            transform.position = OriginPos;

            //Resets the flag
            toggled = false;
        }

        if (enemyCollider != null)
        {
            Physics2D.IgnoreCollision(commonPlatformCollider, enemyCollider, false);
            enemyCollider = null;
        }
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SpellPillar"))
        {
            pillarsRegistered--;

            if (pillarsRegistered == 0)
                commonRigidbody.velocity = new Vector2(fallingSpeedX, -fallingSpeedY);//- so it goes downwards

            //commonRigidbody.velocity = new Vector2(fallingSpeedX, -fallingSpeedY);//- so it goes downwards

            Debug.Log("Pillar exited from platform");
        }
            
            
    }
}
