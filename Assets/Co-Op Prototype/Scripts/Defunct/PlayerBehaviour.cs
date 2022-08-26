using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour, IDamageable
{

    [SerializeField]
    private int maxHealth;
    public int MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = value; }
    }

    [SerializeField]
    private int currentHealth;
    public int CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = value; }
    }

    // Use this for initialization
    void Start ()
    {
        SetStartingHealth();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [ContextMenu("Take 1 Damage")]
    public void ETakeDamage()
    {
        TakeDamage(1, Vector2.zero);
    }

    public void TakeDamage(int damageTaken, Vector3 damageOrigin, int knockbackPowerX = 0, int knockbackPowerY = 0, float hitstun = 0f, bool restrictY = true, bool hitByHazard = false)
    {
        CurrentHealth = CurrentHealth - damageTaken;

        //TODO: VFX of damage here

        /* Axis + CalculateForces
        //Sets the direction towards damage
        Vector2 directionToDamage = (damageOrigin - transform.position).normalized;//Cache dis

        if (directionToDamage != vector2.zero)
        {
            //Shows the direction towards player
            Debug.DrawRay(transform.position, directionToDamage, Color.blue, 1);

            if (directionToDamage.x > 0)
                CalculateForces(Xaxis, knockbackPower)
            else
                CalculateForces(Xaxis, -knockbackPower)

            //&& midair? idk, just check so if an attack launches upwards, it doesn't fuck up the animation states, otherwise apply the below.
            if (directionToDamage.y > some small Value like -0.3~0.3)
                CalculateForces for Y
        }

        Debug.Log(damageOrigin);
        Debug.Log(knockbackPower);
        Debug.Break();
        */

        Debug.Log("Took damage");

        //Dead
        if (CurrentHealth < 1)
        {
            Die();
        }
    }

    [ContextMenu("Omae wa mo... Shindeiru.")]
    public void Die()
    {
        Debug.Log("Died");
        Debug.Break();
    }

    public void SetStartingHealth()
    {
        CurrentHealth = MaxHealth;
    }
}
