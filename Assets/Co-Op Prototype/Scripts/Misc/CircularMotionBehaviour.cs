using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMotionBehaviour : MonoBehaviour
{
    //Why public tho. Simply Get()....
    [Tooltip("Used by player or flying/harpy?")]
    public bool playerOwned;

    private float timeCounter;
    private Transform ownerTransform;

    [Tooltip("The speed of the circular motion")]
    public float speed;

    [Tooltip("The maximum width allowed")]
    public float width;

    [Tooltip("The maximum height allowed")]
    public float height;

    //[Tooltip("The offset of transform.position, after every calculation.")]
    //public float offsetX;

    //[Tooltip("The offset of transform.position, after every calculation.")]
    //public float offsetY;

    [Tooltip("The angle in radians, of this object in the ring")]
    public float angle;

    [Tooltip("The direction the ring does a full circle towards, -1 is clock-wise, and +1 is; you guessed it, counter-clock wise.")]
    public float direction;

    [Tooltip("How fast it rotates")]
    public float rotationSpeed;

    //Cache
    float x;
    float y;
    Vector3 startPos;
    //Vector3 offsetVector;
	
	void Start ()
    {
        //timeCounter = 0f;
        if (playerOwned)
            ownerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        //offsetVector = new Vector3(offsetX, offsetY);
        //startPos = ownerTransform.position;

        //startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //timeCounter += Time.deltaTime * speed;

        //Update warrior's position
        startPos = ownerTransform.position;// + offsetVector;// + transform.localPosition;

        /*
        x = startPos.x + (Mathf.Cos(timeCounter) * width);
        y = startPos.y + (Mathf.Sin(timeCounter) * height);
        */
        //Debug.Log(transform.localPosition);


        //Credits to aarober from this thread: https://www.reddit.com/r/Unity3D/comments/1zjn3h/having_trouble_with_implementing_circular_player/
        //Bless that guy, I was worried this feature would be half-ass implemented, but not today.//well it turns out it still is because it needs lerp when one goes missing but ah well

        //angle += Input.GetAxis("Horizontal") * Time.deltaTime * speed / width;
        if (playerOwned)
            angle += direction * Time.deltaTime * (speed + KillMeterManager.totalSpeedBoost);//  / width;
        else
            angle += direction * Time.deltaTime * speed;//  / width;

        x = startPos.x + Mathf.Cos(angle) * width;
        y = startPos.y + Mathf.Sin(angle) * height;



        //transform.position = center + (new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f)) * radius;

        //Convert this shit to swap the cos and sin, but yeah, use math to identify? since, it will be used for triangle enemy attack as well? idk.
        //x = startPos.x + (offsetVector.x) * (Mathf.Cos(timeCounter) * width);
        //y = startPos.y + (offsetVector.y) * (Mathf.Sin(timeCounter) * height);

        /* Works. But exclusively for player, and super slow. Tis just a test, and it's fail cuz it only works for 4 gameobjects. What if only 3 mana.
        //If you use this kind of variation, reset the transform.position at start.
        if (gameObject.name == "Right")
        {
            x = startPos.x + (Mathf.Cos(timeCounter) * width);
            y = startPos.y - (Mathf.Sin(timeCounter) * height);
        }
        else if (gameObject.name == "Left")
        {
            x = startPos.x - (Mathf.Cos(timeCounter) * width);
            y = startPos.y + (Mathf.Sin(timeCounter) * height);
        }
        else if (gameObject.name == "Up")
        {
            x = startPos.x + (Mathf.Sin(timeCounter) * width);
            y = startPos.y + (Mathf.Cos(timeCounter) * height);
        }
        else if (gameObject.name == "Down")
        {
            x = startPos.x - (Mathf.Sin(timeCounter) * width);
            y = startPos.y - (Mathf.Cos(timeCounter) * height);
        }
        */

        transform.position = new Vector3(x, y, 0f);

        //Rotate
        transform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
    }

    public void SetOwnerTransform(Transform value)
    {
        ownerTransform = value;
    }
}
