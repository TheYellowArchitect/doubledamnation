using UnityEngine;

[RequireComponent(typeof(EnemyBehaviour))]
public class FlyingRotation : MonoBehaviour
{
    //public Transform spriteVisible -> nvm cuz lets rotate the collider as well.

    private Rigidbody2D commonRigidbody;
    private EnemyBehaviour commonBehaviour;
    private float angle;

	// Use this for initialization
	void Start ()
    {
        commonRigidbody = GetComponent<Rigidbody2D>();
        commonBehaviour = GetComponent<EnemyBehaviour>();

        GetComponent<EnemyPathfinder>().rotationTrigger += UpdateRotation;
	}
	
	// Update is called per behaviour update.
	void UpdateRotation()
    {
        angle = Mathf.Atan2(commonRigidbody.velocity.y, commonRigidbody.velocity.x) * Mathf.Rad2Deg;
        if (commonBehaviour.GetFacingRight() == false)
            angle = angle - 180;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        //if (transform.localEulerAngles.z == 180 || transform.localEulerAngles.z == -180)
        //Debug.log("Flip bug detected");

        if (transform.localEulerAngles.z == 180 || transform.localEulerAngles.z == -180)
            //transform.rotation = Quaternion.Euler(Vector3.zero);//if the 180 flip bug at spawn happens again, just activate dis. Although, it is exactly the same, which means, that the bug should originate from somewhere else.
            transform.rotation = Quaternion.identity;
    }
}
