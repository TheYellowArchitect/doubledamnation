using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using NaughtyAttributes;


//NOTIFICATION! Every enemy should start deactivated and with all components except pathfinder disabled.(and gameobject active ofc.) Right now, the reverse happens, everything is active, and deactivation is false.
//A simple change, no coding required, but damn, when 100+ enemies, do fucking notice this.
//[RequireComponent(typeof(EnemyBehaviour))]
public class EnemyPathfinder : MonoBehaviour
{
    [Tooltip("The corpse-type when this enemy/monster dies.")]
    public GameObject corpsePrefab;

    //[Required]<-Removed cuz it couldn't be prefab'd
    [Tooltip("What to \"Chase\"\nIf null, it will automatically search for Gameobject with tag: \"Player\"")]
    public static Transform warriorTransform;

    [MinValue(0f)]
    [Tooltip("How many times each second the \"Pathfinding\" is updated.")]
    public float currentRefreshRate;

    [MinValue(1)]
    [Tooltip("How often per second, the A.I. updates, when in sight range.")]
    public int activeRefreshRate;

    [MinValue(1)]
    [Tooltip("How often per second, the A.I. updates, when NOT in sight range.\nShould be bigger than active.")]
    public int passiveRefreshRate;

    [MinValue(0f)]
    [Tooltip("When deactivated, in how many seconds the \"Pathfinding\" is updated.")]
    public int DeactivationUpdateTime;

    [MinValue(0f)]
    [Tooltip("How far away the player must be, so this monster/enemy, deactivates/sleeps for a while")]
    public float DeactivationRange;

    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("What is considered a Ground Layer?\nSo the raycasting from enemy to player will see if obstructed or not")]
    public LayerMask WhatIsGround;

    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("What is considered a Platform Layer?\nSo the raycasting from enemy to player will see if player is standing on a platform")]
    public LayerMask WhatIsPlatform;

    [Tooltip("What is considered a Hazardous Layer? So as with Raycasting->Avoid this layer/gameobject")]
    public LayerMask WhatIsHazard;

    [ValidateInput("IsLayerMaskEmpty")]
    [Tooltip("What Layermask is the player in?")]
    public LayerMask WhatIsPlayer;

    //Caching
    private EnemyBehaviour commonBehaviour;
    private float commonSightRange;
    private GameObject groundDetection;


    // Direction to the player(normalized)
    private Vector3 directionToPlayer;

    // The distance of player and enemy
    private float distanceToPlayer;

    // The distance of enemy and originPosition
    private float distanceToOriginPos;

    //For pathfinding/raycasting towards originPos
    private Vector3 originPosition;

    // The raycasting towards player
    private RaycastHit2D hit;

    //From raycasting
    private bool isPlayerObstructed;
    private bool isPathfindingHazardous;
    private bool isPlayerPlatformed;
    private bool isOriginPositionObstructed;

    //To determine deactivation(deactivation, not deathfreeze, aka all things enabled=false, except this script and gameobject.)
    private bool deactivated;

    //The simple and most elegant way of checking if a coroutine is running/active.
    private bool coroutineRunning;

    //So OnEnable() won't hijack Start() and everything will be smooth :D
    [ReadOnly]
    public bool hasInitialized = false;

    //Stores corpse Instance
    private GameObject corpse;

    //FLYING EXCLUSIVE//Object-Oriented programming in 2018? We don't do that here. This is so sad. looming deadline denying design time -> Sadness seeing 2018 as a "deadline" here ngl
    //Store Collider2D
    private BoxCollider2D commonCollider;
    private Vector3 flyingTempVector;
    private bool canFly;
    public Action rotationTrigger;
    Vector3 tempVector;

    [Header("Flying-Exclusive")]
    [Tooltip("On raycasts, how much percent should be added on top of 100% on the X side, aka the width. So the pathfinding over 100% is accurate.")]
    public float colliderPathfindingXPercent = 0.1f;
    [Tooltip("On raycasts, how much percent should be added on top of 100% on the Y side, aka the height. So the pathfinding over 100% is accurate.")]
    public float colliderPathfindingYPercent = 0.05f;

    //Initialization for itself
    private void Awake()
    {
        commonBehaviour = GetComponent<EnemyBehaviour>();

        commonSightRange = commonBehaviour.SightRange + 2f;

        //If deactivated starts false, enemy should be with active components. Optimal is starting true, with all components shut down.
        deactivated = true;

        coroutineRunning = false;

        originPosition = transform.position;

        canFly = commonBehaviour.canFly;
        if (canFly == false)
            groundDetection = transform.Find("GroundCollision").gameObject;
        else
            commonCollider = GetComponent<BoxCollider2D>();
    }

    //Initialization for others
    public void Start()
    {
        //Create new corpse
        corpse = Instantiate(corpsePrefab);

        //If level editor, cache the corpse not yolo in activegamescene, but in enemytilemanager
        if (LevelManager.currentLevel == 7)
            MasterGridManager.globalInstance.enemyTileManager.AddCorpse(corpse);

        //Disable new corpse
        corpse.SetActive(false);

        commonBehaviour.died += DeathFreeze;

        hasInitialized = true;

        //If in level manager and paused
        /*if (MasterGridManager.globalInstance != null && LevelEditorMenu.globalInstance.isPlayMode == false)
        {
            DisableEnemy(false, false);
            return;
        }*/

        if (!coroutineRunning)
            //Updates the path every currentRefreshRate
            StartCoroutine(UpdatePath());        
    }

    public void LevelEditorPlay()
    {
        if (!coroutineRunning)
            //Updates the path every currentRefreshRate
            StartCoroutine(UpdatePath());
    }

    public void LevelEditorPause()
    {
        //See OnEnable and make sure this fits perfectly with it. I guess see the enemy behaviour?
        coroutineRunning = false;
        StopCoroutine(UpdatePath());
        DisableEnemy();
    }

    IEnumerator UpdatePath()
    {
        //Super rare, or plainly impossible?
        if (Time.frameCount == 0)
        {
            Debug.Break();
            //Debug.Log("Got in here");
        }

        coroutineRunning = true;

        //Loops so it restarts after 1/currentRefreshRate seconds :D
        while (coroutineRunning)
        {
            //Debug.Log("Distance to player is: " + distanceToPlayer);
            //Debug.Log("Deactivated? " + deactivated);

            //If player/warrior isn't cached.
            if (warriorTransform == null)
            {
                //Find player
                Debug.Log("Searching for player/warrior.");
                warriorTransform = WarriorMovement.globalInstance.gameObject.transform;
                if (warriorTransform == null)
                {
                    warriorTransform = GameObject.FindGameObjectWithTag("Player").transform;
                    if (warriorTransform == null)
                    {
                        Debug.LogError("Cannot find Player/Warrior.");
                        yield return null;
                    }
                }
            }

            //Sets the distance to player
            distanceToPlayer = Vector3.Distance(transform.position, warriorTransform.position);
            if (distanceToPlayer < commonSightRange)
            {
                //Does it more accurately here, via raycasting. Horizontal accuracy was ok, but vertical was really not.
                hit = Physics2D.Raycast(transform.position, directionToPlayer, commonSightRange, WhatIsPlayer);
                if (hit.collider != null)
                    distanceToPlayer = hit.distance;
            }

            //So it will sleep if player is far away, but still relatively close.
            while (distanceToPlayer > DeactivationRange)
            {
                //Deactivate the enemy, if he isn't already deactivated ;)
                if (deactivated == false)
                    DisableEnemy();

                //Sets the distance to player
                distanceToPlayer = Vector3.Distance(transform.position, warriorTransform.position);

                //TODO: Check this out, so spawning is smooth, but also performant. This is in how many seconds it checks again for activation. Related with the range ofc, so trial&error range + this :D
                if (distanceToPlayer > DeactivationRange * 4)
                    yield return new WaitForSeconds(DeactivationUpdateTime * 3);
                else if (distanceToPlayer > DeactivationRange * 2)
                    yield return new WaitForSeconds(DeactivationUpdateTime * 2);
                else
                    yield return new WaitForSeconds(DeactivationUpdateTime);
            }

            if (deactivated)
                EnableEnemy();
            
                


            //Update rotation if flying
            if (canFly && rotationTrigger != null)
                rotationTrigger.Invoke();

            //TODO: Check CommonBehaviour boolean that toggles when inRange, instead!
            //Adjust refresh rate for pathfinding.
            if (distanceToPlayer > commonSightRange)
                currentRefreshRate = passiveRefreshRate;
            else
                currentRefreshRate = activeRefreshRate;

            //Sets the direction to player (Normalized it so that it has a length of 1 world unit)
            directionToPlayer = (warriorTransform.position - transform.position).normalized;

            //Shows the direction towards player
            Debug.DrawRay(transform.position, directionToPlayer, Color.red, 1 / currentRefreshRate);
            //Debug.DrawLine(transform.position, warriorTransform, Color.red, 1 / currentRefreshRate);

            //Pathfinding here
            ResetPathfindingFlags();

            //Origin Position Pathfinding
            if (distanceToPlayer > commonSightRange && commonBehaviour.isOnOriginPosition == false)
            {
                distanceToOriginPos = (originPosition - transform.position).magnitude;

                if (canFly == false)
                {
                    hit = Physics2D.Raycast(transform.position, (originPosition - transform.position), distanceToOriginPos, WhatIsGround | WhatIsHazard | WhatIsPlatform);
                    if (hit.collider == null)
                        isOriginPositionObstructed = false;
                    else
                        isOriginPositionObstructed = true;
                }
                else//Raycasting from all 4 corners! //This is so sad. Can we get 50 likes and kill my CPU?
                {
                    //OriginPos
                    Debug.DrawRay(transform.position, (originPosition - transform.position).normalized, Color.magenta, 1 / currentRefreshRate);
                    hit = Physics2D.Raycast(transform.position, (originPosition - transform.position).normalized, distanceToOriginPos, WhatIsGround | WhatIsHazard);
                    if (hit.collider != null)//Shoulda be a function/method btw.
                        isOriginPositionObstructed = true;

                    /*
                    Debug.DrawRay(commonCollider.bounds.max + new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0), (originPosition - commonCollider.bounds.max).normalized, Color.magenta, 1 / currentRefreshRate);
                    Debug.DrawRay(commonCollider.bounds.min - new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0), (originPosition - commonCollider.bounds.min).normalized, Color.magenta, 1 / currentRefreshRate);
                    flyingTempVector = new Vector3(commonCollider.bounds.extents.x * -1, commonCollider.bounds.extents.y, commonCollider.bounds.extents.z);
                    Debug.DrawRay(transform.position + flyingTempVector + new Vector3(commonCollider.bounds.extents.x * -colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0), (originPosition - (transform.position + flyingTempVector)).normalized, Color.magenta, 1 / currentRefreshRate);
                    Debug.DrawRay(transform.position - flyingTempVector + new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * -colliderPathfindingYPercent, 0), (originPosition - (transform.position - flyingTempVector)).normalized, Color.magenta, 1 / currentRefreshRate);

                    //Top-Right
                    tempVector = commonCollider.bounds.max + new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0);
                    hit = Physics2D.Raycast(tempVector, originPosition - commonCollider.bounds.max, distanceToOriginPos, WhatIsGround | WhatIsHazard);
                    if (hit.collider != null)
                        isOriginPositionObstructed = true;

                    //Bottom-Left
                    tempVector = commonCollider.bounds.min - new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0);
                    hit = Physics2D.Raycast(tempVector, originPosition - commonCollider.bounds.min, distanceToOriginPos, WhatIsGround | WhatIsHazard);
                    if (hit.collider != null)
                        isOriginPositionObstructed = true;


                    flyingTempVector = new Vector3(commonCollider.bounds.extents.x * -1, commonCollider.bounds.extents.y, commonCollider.bounds.extents.z);

                    //Top-Left
                    tempVector = transform.position + flyingTempVector + new Vector3(commonCollider.bounds.extents.x * -colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0);
                    hit = Physics2D.Raycast(tempVector, originPosition - (transform.position + flyingTempVector), distanceToOriginPos, WhatIsGround | WhatIsHazard);
                    if (hit.collider != null)
                        isOriginPositionObstructed = true;

                    //Bottom-Right
                    tempVector = transform.position - flyingTempVector + new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * -colliderPathfindingYPercent, 0);
                    hit = Physics2D.Raycast(tempVector, originPosition - (transform.position - flyingTempVector), distanceToOriginPos, WhatIsGround | WhatIsHazard);
                    if (hit.collider != null)
                        isOriginPositionObstructed = true;
                        */
                }


            }

            //Player Pathfinding
            //Raycasting to see if player is obstructed by terrain, and check obstacle
            if (canFly == false)
                DetermineDefaultRaycasts();
            else
            {
                Debug.DrawRay(transform.position, (warriorTransform.position - transform.position).normalized, Color.blue, 1 / currentRefreshRate);
                CalculateRaycastLayerMasked(WhatIsGround, ref isPlayerObstructed);
                CalculateRaycastLayerMasked(WhatIsHazard, ref isPathfindingHazardous);
                //Commented the below cuz it needs some math remake. I fucked up, it works on mini-harpy perfectly, but on the rescaling, it doesnt work, and it would take a day to get it to work... so..... feelsbadman. Another thing sacrificed.... In the altar of time....
                #region needs math remake to work properly
                /*
                //Does the above, but 4 times instead of 1, because of 4 corners. aka Raycast SPAM for flying!

                //PlayerPos

                //Debug.DrawRay(commonCollider.bounds.max, Vector3.up, Color.gray);
                Debug.DrawRay(commonCollider.bounds.max + new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0), (warriorTransform.position - commonCollider.bounds.max).normalized, Color.blue, 1 / currentRefreshRate);
                Debug.DrawRay(commonCollider.bounds.min - new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0), (warriorTransform.position - commonCollider.bounds.min).normalized, Color.blue, 1 / currentRefreshRate);
                flyingTempVector = new Vector3(commonCollider.bounds.extents.x * -1, commonCollider.bounds.extents.y, commonCollider.bounds.extents.z);
                Debug.DrawRay(transform.position + flyingTempVector + new Vector3(commonCollider.bounds.extents.x * -colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0), (warriorTransform.position - (transform.position + flyingTempVector)).normalized, Color.blue, 1 / currentRefreshRate);
                Debug.DrawRay(transform.position - flyingTempVector + new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * -colliderPathfindingYPercent, 0), (warriorTransform.position - (transform.position - flyingTempVector)).normalized, Color.blue, 1 / currentRefreshRate);

                //Top-Right
                tempVector = commonCollider.bounds.max + new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0);
                hit = Physics2D.Raycast(tempVector, warriorTransform.position - commonCollider.bounds.max, distanceToPlayer, WhatIsGround | WhatIsHazard);
                if (hit.collider != null)//Shoulda be a function/method btw.
                {
                    if ((WhatIsGround & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        isPlayerObstructed = true;
                    else if ((WhatIsHazard & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        isPathfindingHazardous = true;
                }

                //Bottom-Left
                tempVector = commonCollider.bounds.min - new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent);
                hit = Physics2D.Raycast(tempVector, warriorTransform.position - commonCollider.bounds.min, distanceToPlayer, WhatIsGround | WhatIsHazard);
                if (hit.collider != null)
                {
                    if ((WhatIsGround & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        isPlayerObstructed = true;
                    else if ((WhatIsHazard & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        isPathfindingHazardous = true;
                }

                flyingTempVector = new Vector3(commonCollider.bounds.extents.x * -1, commonCollider.bounds.extents.y, commonCollider.bounds.extents.z);

                //Top-Left
                tempVector = transform.position + flyingTempVector + new Vector3(commonCollider.bounds.extents.x * -colliderPathfindingXPercent, commonCollider.bounds.extents.y * colliderPathfindingYPercent, 0);
                hit = Physics2D.Raycast(tempVector, warriorTransform.position - (transform.position + flyingTempVector), distanceToPlayer, WhatIsGround | WhatIsHazard);
                if (hit.collider != null)
                {
                    if ((WhatIsGround & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        isPlayerObstructed = true;
                    else if ((WhatIsHazard & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        isPathfindingHazardous = true;
                }

                //Bottom-Right
                tempVector = transform.position - flyingTempVector + new Vector3(commonCollider.bounds.extents.x * colliderPathfindingXPercent, commonCollider.bounds.extents.y * -colliderPathfindingYPercent, 0);
                hit = Physics2D.Raycast(tempVector, warriorTransform.position - (transform.position - flyingTempVector), distanceToPlayer, WhatIsGround | WhatIsHazard);
                if (hit.collider != null)
                {
                    if ((WhatIsGround & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        isPlayerObstructed = true;
                    else if ((WhatIsHazard & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        isPathfindingHazardous = true;
                }



                */
                #endregion
            }

            //Normally needs all 4 corners but ah well
            if (canFly && isPlayerObstructed)//to confirm the monster/enemy won't fly straight into player.
            {
                hit = Physics2D.Raycast(transform.position, commonBehaviour.GetVelocity().normalized, distanceToPlayer, WhatIsGround | WhatIsHazard | WhatIsPlatform);
                if (hit.collider != null)
                {
                    if ((WhatIsHazard & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
                        commonBehaviour.RotateFlyingDirection();
                }
            }

            commonBehaviour.RefreshBehaviour(directionToPlayer, distanceToPlayer, isPlayerObstructed, isPathfindingHazardous, isPlayerPlatformed, isOriginPositionObstructed);

            //Wait some seconds before re-updating the path
            yield return new WaitForSeconds(1 / currentRefreshRate);
        }
    }

    public void ResetPathfindingFlags()
    {
        isPlayerObstructed = false;
        isPathfindingHazardous = false;
        isPlayerPlatformed = false;
        isOriginPositionObstructed = false;
    }

    public void DetermineDefaultRaycasts()
    {
        CalculateRaycastLayerMasked(WhatIsGround, ref isPlayerObstructed);
        CalculateRaycastLayerMasked(WhatIsHazard, ref isPathfindingHazardous);
        CalculateRaycastLayerMasked(WhatIsPlatform, ref isPlayerPlatformed);
    }

    public void CalculateRaycastLayerMasked(LayerMask WhatIsCast, ref bool PathfindingBoolean)
    {
        hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, WhatIsCast);
        if (hit.collider != null) //&& ((WhatIsCast & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer))
            PathfindingBoolean = true;
    }

    public void DisableEnemy(bool disableSpriteRenderer = true, bool disableCollision = true)
    {
        //Debug.Log("Sleeping time! at frame: " + Time.frameCount);

        //Has deactivated(so it will check for re-activation)
        deactivated = true;

        //Disable sprite renderer
        if (disableSpriteRenderer)
            GetComponentInChildren<SpriteRenderer>().enabled = false;
            //GetComponent<SpriteRenderer>().enabled = false;

        if (disableCollision)
        {
            //Sleep groundDetection's rigidbody.
            //groundDetection.GetComponent<Rigidbody2D>().Sleep();

            //Disable Ground Detection
            if (canFly == false)
                groundDetection.SetActive(false);

            //Sleep rigidbody2D
            GetComponent<Rigidbody2D>().Sleep();

            //Disable polygonal collider2D
            GetComponent<Collider2D>().enabled = false;
        }

        //Disable animator
        commonBehaviour.animatorController.enabled = false;

        //Disable all attack scripts
        foreach (BaseEnemyAttack pickedAttack in commonBehaviour.possibleAttacks)
        {
            if (pickedAttack.enabled == true)
                pickedAttack.enabled = false;
        }

        //Disable jump, if it exists
        if (commonBehaviour.jumpAction != null && commonBehaviour.jumpAction.enabled == true)
        {
            //Disable jumpContact attack, if it exists.
            if (commonBehaviour.jumpAction.commonJumpContact != null && commonBehaviour.jumpAction.commonJumpContact.enabled == true)
                commonBehaviour.jumpAction.commonJumpContact.enabled = false;

            //Disable jumpStompAttack
            if (GetComponent<EnemyJumpStompAttack>() != null && GetComponent<EnemyJumpStompAttack>().enabled == true)
                GetComponent<EnemyJumpStompAttack>().enabled = false;

            commonBehaviour.jumpAction.enabled = false;
        }

        //Disable EnemyBehaviour (it has OnDisabled, to stop it's coroutine)
        commonBehaviour.enabled = false;
    }

    //TODO: Cache the GetComponent.
    public void EnableEnemy()
    {
        //Debug.Log("Awake time! at frame: " + Time.frameCount);

        //Has not deactivated (so it will check for deactivation)
        deactivated = false;

        //Re-enable sprite renderer
        GetComponentInChildren<SpriteRenderer>().enabled = true;
        //GetComponent<SpriteRenderer>().enabled = true;        

        //Enable Ground Detection
        if (canFly == false)
            groundDetection.SetActive(true);

        //Wake up groundDetection's rigidbody.
        //groundDetection.GetComponent<Rigidbody2D>().WakeUp();

        //Awake rigidbody2D
        GetComponent<Rigidbody2D>().WakeUp();

        //Enable polygonal collider2D
        GetComponent<Collider2D>().enabled = true;

        //Re-enable animator
        commonBehaviour.animatorController.enabled = true;

        //Enable all attack scripts
        foreach (BaseEnemyAttack pickedAttack in commonBehaviour.possibleAttacks)
        {
            if (pickedAttack.enabled == false)
                pickedAttack.enabled = true;
        }

        //Re-enable jump, if it exists
        if (commonBehaviour.jumpAction != null && commonBehaviour.jumpAction.enabled == false)
        {
            //Re-enable jumpContact attack, if it exists.
            if (commonBehaviour.jumpAction.commonJumpContact != null && commonBehaviour.jumpAction.commonJumpContact.enabled == false)
                commonBehaviour.jumpAction.commonJumpContact.enabled = true;

            //Re-enable jumpStompAttack
            if (GetComponent<EnemyJumpStompAttack>() != null && GetComponent<EnemyJumpStompAttack>().enabled == false)
                GetComponent<EnemyJumpStompAttack>().enabled = true;

            commonBehaviour.jumpAction.enabled = true;
        }

        //Re-enable EnemyBehaviour (it has OnEnabled, to restart its coroutine)
        commonBehaviour.enabled = true;
    }

    //Called by Action from EnemyBehaviour, when monster died (and finished animation)
    public void DeathFreeze()
    {
        DisableEnemy();
        coroutineRunning = false;
        StopCoroutine(UpdatePath());

        //Corpse activation
        corpse.SetActive(true);
        corpse.GetComponent<CorpseBehaviour>().SetCorpseSprite(false);
        //Debug.Log("isGrounded: " + commonBehaviour.isGrounded);//to be used above, but i fucked up and its always false. Ah well.
        corpse.transform.position = new Vector3(transform.position.x, transform.position.y, 1);

        this.enabled = false;
        this.gameObject.SetActive(false);
    }

    //Called when "Revived" aka player died and level restarts.
    private void OnEnable()
    {
        if (!hasInitialized)
            return;

        //Re-starts all the scripts/methods
        EnableEnemy();

        if (!coroutineRunning)
            //Restarts the pathfinding
            StartCoroutine(UpdatePath());

        commonBehaviour.Revive();
    }


    public GameObject GetCorpse()
    {
        return corpse;
    }
    /*
     * //If you didn't notice, the implementation of LevelManager instancing and setting the corpses was retarded. It was fixed, OOF.
    //Via LevelManager.
    public void SetCorpse(GameObject _corpse)
    {
        corpse = _corpse;
    }
    */

    //Shows the radius
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //Draw attack range
        Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.back, DeactivationRange);
    }
    #endif

    //To validate Input, so game won't run with layermask(WhatIsPlayer) = Nothing
    protected bool IsLayerMaskEmpty(LayerMask layermask)
    {
        return layermask.value != 0;
    }
}