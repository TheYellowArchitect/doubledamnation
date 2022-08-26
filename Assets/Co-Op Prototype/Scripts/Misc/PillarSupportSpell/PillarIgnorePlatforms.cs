using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarIgnorePlatforms : MonoBehaviour
{
    [Tooltip("Pillar's Collider (ground layer), not this trigger's")]
    public Collider2D pillarCollider;

    private PillarBehaviour commonPillarBehaviour;

	// Use this for initialization
	void Start ()
    {
        commonPillarBehaviour = GetComponent<PillarBehaviour>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //So it won't stop on platforms.
        if (collision.gameObject.CompareTag("Platform"))
            Physics2D.IgnoreCollision(pillarCollider, collision);
    }

}
