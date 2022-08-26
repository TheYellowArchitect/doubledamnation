using UnityEngine;

//I wish unity supported interfaces more, they are really useful still, but half as useful **because** variables are fucked up. Only method contracts REEEE
//This is horrible. The interface "support" was already bad, but adding my retarded mistake of not making this an actual class + child, so I won't fuck up the player/monster categorization.... super horrible spagghetti.
public interface IDamageable
{
    int MaxHealth { set; get; }

    int CurrentHealth { set; get; }

    void TakeDamage(int damageAmount, Vector3 damageOrigin, int knockbackPowerX, int knockbackPowerY, float hitstun, bool restrictedY = true, bool hitByHazard = false);

    void Die();

    void SetStartingHealth();
}