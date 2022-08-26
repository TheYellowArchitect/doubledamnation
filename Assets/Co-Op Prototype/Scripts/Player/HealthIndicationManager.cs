using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

//Placed onto Warrior, next to WarriorMovement and WarriorHealth
public class HealthIndicationManager : MonoBehaviour
{
    [Header("3 UI Text gameobjects")]

    /// <summary>
    /// This one shows "+ X value" aka when u have killed many enemies
    /// </summary>
    public TMP_Text tempHPIndication;

    /// <summary>
    /// This one shows when you get damaged, but you are not to enter danger levels.
    /// </summary>
    public TMP_Text normalDamageHPIndication;

    /// <summary>
    /// This one shows to player when 3 hp or below and stays above player's head.
    /// </summary>
    public TMP_Text dangerHPIndication;



    [Header("Color per level")]
    public Color level1Color = Color.white;

    public Color level2Color;

    public Color level3Color;



    [Header("Speed for when spawned")]
    public float speedX = 0.2f;

    public float speedY = 6;



    [Header("Position to spawn Indication")]

    public float offsetX = 3;

    public float offsetY = 6;



    [Header("Transparency values")]

    public float timeToStartTransparency = 0.8f;

    public float timeToEndTransparency = 2.4f;

    

    [Header("DangerIndication values")]

    [Tooltip("When hit, when should the value permanently stay above the player's head?")]
    public float HPDangerValue = 3;

    [Tooltip("In how many seconds after dangerHit, is the health indication at the furthest spot?")]
    public float durationAway = 0.5f;

    [Tooltip("In how many seconds after dangerHit, is the health indication back at the player's head?")]
    public float durationBack = 1.2f;

    [Tooltip("When player is hit on danger, this is the offsetX of the value from transform.position")]
    public int offsetXOnDangerHit = 17;

    [Tooltip("When player is hit on danger, this is the offsetY of the value from transform.position")]
    public int offsetYOnDangerHit = 9;

    [ReadOnly]
    [Tooltip("If its true, it means player is hit and indication is trying to go above the head, otherwise it is automatically set above it on Update()")]
    public bool dangerIsMoving = false;
    

    private WarriorMovement commonWarrior;//Cached for facing
    private Rigidbody2D commonRigidbody;
    private Color commonColor;
    private float LerpRate;

    private Vector3 dangerHPindicationTargetPosition;

    // Use this for initialization
    void Start ()
    {
        commonRigidbody = GetComponent<Rigidbody2D>();//to detect X velocity
        commonWarrior = GetComponent<WarriorMovement>();
        Reset();
	}

    private void Update()
    {
        //Does this every frame, aka to ensure its always above the head of player
        if (dangerIsMoving == false)
            SetIndicationPositionAbovePlayerHead(dangerHPIndication);
    }

    //Called by warrior health
    public void InitializeTempHPIndication(int normalMaxHealth)
    {
        //Set the actual text in this gameobject
        tempHPIndication.text = " + " + romanStringNumerals.romanStringNumeralGenerator(normalMaxHealth);

        FlyIndication(tempHPIndication);

        //Also, "hide" dangerHP!
        dangerHPIndication.text = "";
    }

    //Called by warrior health
    public void InitializeDamageIndication (int currentHealth, Vector2 directionHitNormalized)
    {
        if (currentHealth <= HPDangerValue)
            InitializeDangerIndication(currentHealth, directionHitNormalized);
        else
            InitializeNoDangerIndication(currentHealth, directionHitNormalized);
    }

    private void InitializeDangerIndication(int currentHealth, Vector2 directionHitNormalized)
    {
        //Set the actual text in this gameobject
        dangerHPIndication.text = romanStringNumerals.romanStringNumeralGenerator(currentHealth);

        //Make sure transparency is 0.
        dangerHPIndication.color = GetLevelColor();




        SetDangerHPIndicationTargetPosition(directionHitNormalized);

        StartCoroutine( MoveIndication(dangerHPindicationTargetPosition, dangerHPIndication) );
    }

    private void InitializeNoDangerIndication(int currentHealth, Vector2 directionHitNormalized)
    {
        //Set the actual text in this gameobject
        normalDamageHPIndication.text = romanStringNumerals.romanStringNumeralGenerator(currentHealth);

        FlyIndication(normalDamageHPIndication);
    }

    



    public void FlyIndication(TMP_Text flyingIndication)
    {
        //Position the text properly
        SetIndicationPositionToDefault(flyingIndication);

        //Get proper facing/offset/facingdirection
        if ((commonWarrior.GetFacingLeft() == false && offsetX > 0) || (commonWarrior.GetFacingLeft() == true && offsetX < 0))
            offsetX *= -1;

        //Set velocity to the text
        if (commonRigidbody.velocity.y < 0)
            flyingIndication.GetComponent<Rigidbody2D>().velocity = new Vector2(-1 * commonRigidbody.velocity.x * speedX, speedY);
        else
            flyingIndication.GetComponent<Rigidbody2D>().velocity = new Vector2(-1 * commonRigidbody.velocity.x * speedX, commonRigidbody.velocity.y + speedY);

        StartCoroutine(BecomeTransparent(flyingIndication));
    }
    

    public IEnumerator BecomeTransparent(TMP_Text transparentIndication)
    {
            //Starting, sets the color to default/white and turns the coroutine flag on
            commonColor = GetLevelColor();

            transparentIndication.color = commonColor;

            //When spellcast is done, starts transparency
            yield return new WaitForSeconds(timeToStartTransparency);

            LerpRate = 0;

            //Cycle here
            while (LerpRate < 1)//commonColor.a > 0)
            {
                //Decrease the transparency
                LerpRate += Time.deltaTime / timeToEndTransparency;
                commonColor.a = Mathf.Lerp(1, 0, LerpRate);//notice the 1->0 and not the reverse.

                transparentIndication.color = commonColor;

                yield return null;
            }

            yield break;

    }


    //This code was copy-pasted from https://answers.unity.com/questions/12859/Move-an-object-through-a-set-of-positions.html
    //and like every random code, gotta love 0 documentation and weird naming (what is i?!)
    //Second code with movement() getnextwaypoint() and the below should be sweet. Good enough to be used on every unity project!
    /// <summary>
    /// The TMP_Text inside, will move to targetVector over time, and then when it reached that, back to above the player's head!
    /// </summary>
    /// <param name="targetVector"></param>
    /// <param name="indicationToMove"></param>
    /// <returns></returns>
    public IEnumerator MoveIndication(Vector3 targetVector, TMP_Text indicationToMove)
    {
        #region Original copy-pasta tweaked to work for many waypoints, but too complicated/complex for just 1 waypoint and back!
        /* 
        //Array to hold waypoint locations
        List<Transform> waypoints = new List<Transform>();

        //The array index of the current target waypoint
        int targetWaypointIndex;

        //The time taken to travel between points
        float duration = 1;

        Vector3 startPoint;
        Vector3 endPoint;
        float startTime;

        bool endCoroutine = false;

        
        //Start
        startPoint = transform.position;
        startTime = Time.time;
        if (waypoints.Count == 0)
        {
            Debug.Log("No waypoints found");
            yield break;
        }
        targetWaypointIndex = 0;
        endPoint = waypoints[targetWaypointIndex].position;
        


        //Loop movement!
        while(endCoroutine == false)
        {
            float timePassed = (Time.time - startTime) / duration;
            transform.position = Vector3.Lerp(startPoint, endPoint, timePassed);
            if (timePassed > 1)
            {

                //If no more waypoints
                if (targetWaypointIndex + 1 >= waypoints.Count)
                    endCoroutine = true;
                else
                {
                    //Increase the waypoint index by one
                    targetWaypointIndex++;

                    //Resets the time properly.
                    startTime = Time.time;

                    //Assign the new lerp waypoints
                    startPoint = endPoint;
                    endPoint = waypoints[targetWaypointIndex].position;
                }

                
            }

            yield return new WaitForEndOfFrame();
        }
        */
        #endregion

        Vector3 startPoint;
        Vector3 endPoint;
        float startTime;

        float timePassed;
        bool endLoop = false;


        dangerIsMoving = true;

        //Set Lerp Values
            //Sets the start of the lerp
            startPoint = GetPositionAbovePlayerHead();

            //Sets the end of the lerp
            endPoint = targetVector;

            //Resets the time properly.
            startTime = Time.time;

            

        //Loop going away
            while (endLoop == false)
            {
                timePassed = (Time.time - startTime) / durationAway;
                indicationToMove.transform.position = Vector3.Lerp(startPoint, endPoint, timePassed);
                if (timePassed > 1)
                    endLoop = true;

                yield return new WaitForEndOfFrame();
            }


        //Set Lerp Values
            //Sets the start of the lerp
            startPoint = endPoint;

            //Resets the time properly.
            startTime = Time.time;


        endLoop = false;

        //Loop going back to default
            while (endLoop == false)
            {
                timePassed = (Time.time - startTime) / durationBack;

                //Sets the end of the lerp, inside the while loop, cuz it MUST be updated, otherwise it will go "above player head" back when he was hit, not when he is currently moving!
                endPoint = GetPositionAbovePlayerHead();

                indicationToMove.transform.position = Vector3.Lerp(startPoint, endPoint, timePassed);

                if (timePassed > 1)
                    endLoop = true;

                yield return new WaitForEndOfFrame();
            }


        dangerIsMoving = false;
    }


    public void SetDangerHPIndicationTargetPosition(Vector2 directionHitNormalized)
    {
        //If hit Vertically
        if (directionHitNormalized.y > 0.87f || directionHitNormalized.y < -0.87f)
        {
            //Hit from upwards, so u go down, and the text should also go down
            if (directionHitNormalized.normalized.y > 0.87f)
                dangerHPindicationTargetPosition = transform.position + new Vector3(0, offsetYOnDangerHit * 3, 0);

            //else, got hit downwards, so text and character should go upwards
            else
                dangerHPindicationTargetPosition = transform.position + new Vector3(0, -offsetYOnDangerHit * 2, 0);
        }
        //else hit normally, aka any attack that isnt some 100% vertical bs.
        else
        {
            //Get proper facing/offset/facingdirection//Commented out cuz hacky and doesnt use the precious local data given.
            //if ((commonWarrior.GetFacingLeft() == false && offsetXOnDangerHit > 0) || (commonWarrior.GetFacingLeft() == true && offsetXOnDangerHit < 0))
            //offsetXOnDangerHit *= -1;


            //Get proper facing/offset/facingdirection
            if ( (directionHitNormalized.x < 0 && offsetXOnDangerHit > 0) || (directionHitNormalized.x > 0 && offsetXOnDangerHit < 0) )
                offsetXOnDangerHit *= -1;

            dangerHPindicationTargetPosition = transform.position + new Vector3(offsetXOnDangerHit, offsetYOnDangerHit, 0);
        }
    }

    /// <summary>
    /// What this does, is take the parameter, and always set it to the default/standard position, above player's head!
    /// </summary>
    /// <param name="IndicationToPosition"></param>
    public void SetIndicationPositionToDefault(TMP_Text IndicationToPosition)
    {
        IndicationToPosition.transform.position = transform.position + new Vector3(offsetX, offsetY, 0);
    }

    //Literally the above, but without the X offset.
    public void SetIndicationPositionAbovePlayerHead(TMP_Text IndicationToPosition)
    {
        IndicationToPosition.transform.position = GetPositionAbovePlayerHead();
    }

    public Vector3 GetPositionAbovePlayerHead()
    {
        return transform.position + new Vector3(0, offsetY, 0);
    }

    public Color GetLevelColor()
    {
        if (LevelManager.currentLevel < 2 || LevelManager.currentLevel == 7)
            return level1Color;
        else if (LevelManager.currentLevel == 2)
            return level2Color;
        else//if (LevelManager.currentLevel > 2)
            return level3Color;
    }

    //Called on LevelLoad() of warrior!
    public void Reset()
    {
        ResetNumeralString();
        ResetColor();
    }

    public void ResetNumeralString()
    {
        dangerHPIndication.text = "";
        tempHPIndication.text = "";
        normalDamageHPIndication.text = "";
    }

    public void ResetColor()
    {
        dangerHPIndication.color = GetLevelColor();
        tempHPIndication.color = GetLevelColor();
        normalDamageHPIndication.color = GetLevelColor();
    }
}
