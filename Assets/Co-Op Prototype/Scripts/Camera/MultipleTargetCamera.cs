using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//You better remake all of this code. What a spagghetti you have made. 
//But at least you know how it should approximately work and the extent of the features and how they work together
//tl;dr: Move the zooming in/out bs on its own class + fuck the async bs, and go for Update() logic.
//^Should work with all 3 "inputs" (Observe spellword, InfluenceX/Momentum, Cutscene)
public class MultipleTargetCamera : MonoBehaviour
{
    public List<Transform> focusTargets;

    public Vector3 offset;

    public float smoothTime;//0.2f

    public float defaultMinZoom;//45f
    public float defaultMaxZoom;//34f

    /// <summary>
    /// defaultMaxZoom cannot exceed this value, no matter what.
    /// </summary>
    public float maxZoomLimiter = 90f;
    public float misunderstoodZoomLimiter;//75f

    private Vector3 artificialVelocity;
    private Camera mainCamera;

    //Cache
    private int i;
    private Vector3 centerPoint;
    private Vector3 newPosition;
    private Bounds focusBounds;

    [Header("Read Only")]
    public float finalMaxZoom;
    public float currentMaxZoom;
    public List<ZoomData> zoomDataSequences = new List<ZoomData>();
    public bool momentumZoomedOutTwice = false;
    public bool momentumZoomedOutOnce = false;
    public bool allowFocusingMonsters = true;//When zooming out in speed, this is a thing ;)

    private bool zoomOutCoroutineRunning = false;
    private bool warnNoFocusTargets = true;//For when there are no focus targets, to not spam debug

    
    private ZoomData newZoomData;
    private float finalZoomOut;//To store to be the entry point, to the next sequence since all previous data is deleted and the lerp's 1.0 value must be stored.
    private float finalZoomIn;

    private WarriorMovement warriorBehaviour;
    private float warriorInfluenceX;
    

    private void Awake()
    {
        //Gets camera component
        mainCamera = GetComponentInChildren<Camera>();
        finalMaxZoom = defaultMaxZoom;
        currentMaxZoom = defaultMaxZoom;
    }

    private void Start()
    {
        warriorBehaviour = GameObject.FindGameObjectWithTag("Player").GetComponent<WarriorMovement>();

        //Finds player.transform and puts it in focusTargets, so it won't be added publicly
        focusTargets.Add(GameObject.FindGameObjectWithTag("Player").transform);

        if (focusTargets.Count == 0)
            Debug.LogError("Player not found by camera.");

        //Creates bounds, to at least exist, before tweaking :P
        focusBounds = new Bounds(focusTargets[0].position, Vector3.zero);
    }

    private void LateUpdate()
    {

        if (focusTargets.Count == 0 || focusTargets[0] == null)
        {
            if (warnNoFocusTargets)
            {
                warnNoFocusTargets = false;
                Debug.LogWarning("No Focus Units for camera");
            }
            return;
        }

        //Zoom In/Out depending on speed
        DetermineWarriorMomentumZoom();

        //Updates the (focus)Bounds
        SetEncapsulatingBounds();

        //Moves camera depending on the focus
        Move();

        //Zooms depending on distance
        Zoom();
    }

    //Find what the fucking bug is. It has sth to do with zoomin/reset!!!
    private void DetermineWarriorMomentumZoom()
    {
        //Having absolute value helps tremendously, so I dont check with negatives all the time.
        warriorInfluenceX = Mathf.Abs(warriorBehaviour.GetTotalInfluenceX());

        if (warriorInfluenceX > 150 && momentumZoomedOutTwice == false && momentumZoomedOutOnce == true)
        {
            momentumZoomedOutTwice = true;
            MaximumZoomOutMomentum();

            allowFocusingMonsters = false;
            ClearFocusTargetsExceptPlayer();
        }
        else if (warriorInfluenceX > 100 && momentumZoomedOutOnce == false && momentumZoomedOutTwice == false)
        {
            momentumZoomedOutOnce = true;
            HugeZoomOutMomentum();

            allowFocusingMonsters = false;
            ClearFocusTargetsExceptPlayer();
        }
        else if (warriorInfluenceX < 150 && momentumZoomedOutTwice == true && momentumZoomedOutOnce == true)
        {
            momentumZoomedOutTwice = false;
            momentumZoomedOutOnce = false;
            ResetZoomToDefault();

            allowFocusingMonsters = true;
        }
        else if (warriorInfluenceX < 100 && momentumZoomedOutOnce == true)
        {
            momentumZoomedOutTwice = false;
            momentumZoomedOutOnce = false;
            ResetZoomToDefault();

            allowFocusingMonsters = true;
        }

    }

    private void SetEncapsulatingBounds()
    {
        focusBounds = new Bounds(focusTargets[0].position, Vector3.zero);

        for (i = 0; i < focusTargets.Count; i++)
            if (focusTargets[i] != null)//This if could be gone, if i somehow notified really fast, right after the levelLoad. (rn, it waits 0.1f, look at scenemanager)
                focusBounds.Encapsulate(focusTargets[i].position);
    }

    private void Move()
    {
        //Gets the center of the focusTargets, by using Bounds class (encapsulates them, and gets the center)
        centerPoint = focusBounds.center;

        //Uses the offset, so it will zoom out for the scene
        newPosition = centerPoint + offset;

        //Smoothing movement
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref artificialVelocity, smoothTime);
    }

    private void Zoom()
    {
        float newZoom = Mathf.Lerp(finalMaxZoom, defaultMinZoom, GetGreatestDistance() / misunderstoodZoomLimiter);

        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, newZoom, Time.deltaTime);
    }

    

    private float GetGreatestDistance()
    {
        return focusBounds.size.magnitude;
        //Debug.Log("Magnitude: " + focusBounds.size.magnitude + "max: " + Mathf.Max(focusBounds.size.x, focusBounds.size.y));

        //I think this doesn't count the diagonal, but ah well.
        //return Mathf.Max(focusBounds.size.x, focusBounds.size.y);

        //return focusBounds.size.x;
    }


    public void AddFocusTarget(GameObject targetToAdd)
    {
        AddFocusTarget(targetToAdd.transform);
    }

    //Called by CameraTargetDetermination.cs
    public void AddFocusTarget(Transform targetToAdd)
    {
        if (allowFocusingMonsters || targetToAdd.gameObject.CompareTag("Player"))
            focusTargets.Add(targetToAdd);
        
        if (warnNoFocusTargets == false)
            warnNoFocusTargets = true;
    }

    public void RemoveFocusTarget(Transform targetToRemove)
    {
        focusTargets.Remove(targetToRemove);
    }

    public void ClearFocusTargetsExceptPlayer()
    {
        focusTargets.Clear();
        if (GameObject.FindGameObjectWithTag("Player") != null)
            focusTargets.Add(GameObject.FindGameObjectWithTag("Player").transform);
    }

    //When new level, so no bugs happen.
    public void OnLevelLoad()
    {
        ClearFocusTargetsExceptPlayer();

        if (LevelManager.currentLevel == 3)
            //While destroying **this** would be optimal, there may be bugs. A little performance hit from camera isn't as great as fixing the upcoming bugs ;)
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraTargetDetermination>().DestroyThis();
    }

    public float GetLowestBoundPoint()
    {
        return focusBounds.min.y;
    }
    
    //This is a function used when u want to zoom in/out, and then back again to the original zoom.
    //Wrapper of a wrapper btw.
    public void ZoomBoth(int valueToZoom, float startDuration, float endDuration, bool zoomOutFirst = true, float haltZoomFor = 0f, Transform zoomTarget = null, bool zoomExclusively = false)
    {
        StartZoom(valueToZoom, startDuration, haltZoomFor, zoomTarget, zoomExclusively, zoomOutFirst);
        StartZoom(valueToZoom, endDuration, haltZoomFor, zoomTarget, zoomExclusively, !zoomOutFirst);
    }

    /* 
    //This was used for testing
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            HugeZoomOutMomentum();

        if (Input.GetKeyDown(KeyCode.G))
            MaximumZoomOutMomentum();

        if (Input.GetKeyDown(KeyCode.Q))
            CutsceneZoomIn();

        if (Input.GetKeyDown(KeyCode.W))
            ResetZoomToDefault();
    }
    */

    //To be called when high momentum (influenceX)
    public void HugeZoomOutMomentum()
    {
        StartZoom(19, 0.2f);
    }

    public void MaximumZoomOutMomentum()
    {
        StartZoom(35, 0.1f);
    }

    public void CutsceneZoomIn(float timeToZoomIn = 0.1f)
    {
        //Tweaking this variable is a hack. But hey... if it works...
        //^You tech debt monster! Your spagghetti really is endless, isn't it?
        currentMaxZoom = 12;
        StartZoom(0, timeToZoomIn, 0, null, false, false);
    }

    public void ResetZoomToDefault(float timeToReset = 0.2f)
    {
        //Camera is currently zoomed out
        if (finalMaxZoom > defaultMaxZoom)
        {
            //The value to zoom towards, aka to reach the default value.
            currentMaxZoom = defaultMaxZoom;

            //Zoom In!
            StartZoom(currentMaxZoom, timeToReset, 0, null, false, false);
        }
        else
        {
            //The value to zoom towards, aka to reach the default value.
            currentMaxZoom = defaultMaxZoom;

            //Zoom Out!
            StartZoom(0, 1);
        }
        //If finalMaxZoom smaller than defaultMaxZoom blabla, else blabla -> not needed

        //finalMaxZoom = defaultMaxZoom;
    }

    //This is a wrapper.
    public void StartZoom(float valueToZoom, float zoomDuration, float haltZoomFor = 0f, Transform zoomTarget = null, bool zoomExclusively = false, bool zoomOut = true)
    {
        //Initialize and set the new ZoomData
            newZoomData = new ZoomData();
            newZoomData.zoomValue = valueToZoom;
            newZoomData.zoomDuration = zoomDuration;
            newZoomData.intervalBetweenNextZoom = haltZoomFor;
            newZoomData.zoomTarget = zoomTarget;
            newZoomData.clearFocusTargetsBeforeZoom = zoomExclusively;
            newZoomData.zoomOut = zoomOut;

        //Ensure no invalid data is put
            //Limit the zoom.
            if (newZoomData.zoomOut && currentMaxZoom + newZoomData.zoomValue > maxZoomLimiter)
                newZoomData.zoomValue = maxZoomLimiter - currentMaxZoom;
            else if (newZoomData.zoomOut == false && currentMaxZoom + newZoomData.zoomValue < 12)
                newZoomData.zoomValue = 12 - currentMaxZoom;

        //Add the new zoomData to the sequences, where zoom logic processes by them.
        zoomDataSequences.Add(newZoomData);

        

        StartCoroutine(ZoomOutCoroutine());

        MergeZoomSequences();
    }

    public IEnumerator ZoomOutCoroutine()
    {
        //Do not run another coroutine, on top of this one!
        //If MergeZoomSequences() was ran above this coroutine, this wouldnt work.
        if (zoomDataSequences.Count > 1)
            yield break;

        while (zoomDataSequences.Count != 0)
        {
            //The start/initialization/setup
                float timePassed = 0f;

                

                //TODO: This must be refactored for when zooming in is a thing.
                if (zoomDataSequences[0].zoomOut)
                    finalZoomIn = currentMaxZoom;
                else//ZoomIn
                {
                    Debug.Log("Final Zoom Out is: " + finalZoomOut);

                    //If not zoomed out before.
                    if (finalZoomOut == 0)
                    {
                        Debug.Log("Nani?");
                        finalZoomOut = defaultMaxZoom;
                    }
                        
                }
                

                //if (finalZoomOut > 90f)
                    //Debug.Log("Hmmmmm.... zoomValue is: " + zoomDataSequences[0].zoomValue + " and finalZoom is:  " + finalZoomOut + " and lerp timethingy: " + (timePassed / zoomDataSequences[0].zoomDuration));

                if (zoomDataSequences[0].clearFocusTargetsBeforeZoom)
                    focusTargets.Clear();

                //Zoom at target
                if (zoomDataSequences[0].zoomTarget != null)
                    AddFocusTarget(zoomDataSequences[0].zoomTarget);

            //The actual zoom logic.
            while (timePassed < zoomDataSequences[0].zoomDuration)
            {
                //This should be 0.03, not Time.deltaTime
                timePassed += Time.deltaTime;

                //Zoom In/Zoom Out
                //finalMaxZoom is used every frame, in Zoom() ;)
                if (zoomDataSequences[0].zoomOut)//ZoomOut
                    finalMaxZoom = Mathf.Lerp(finalZoomIn, currentMaxZoom + zoomDataSequences[0].zoomValue, timePassed / zoomDataSequences[0].zoomDuration);
                else//ZoomIn
                    finalMaxZoom = Mathf.Lerp(finalZoomOut, currentMaxZoom, timePassed / zoomDataSequences[0].zoomDuration);

                if (finalMaxZoom > 90f)
                    Debug.Log("Right below where currentMaxZoom is set and > 90, zoomValue is: " + zoomDataSequences[0].zoomValue + " and finalZoom is:  " + finalZoomOut + " and lerp timethingy: " + (timePassed / zoomDataSequences[0].zoomDuration));

                yield return new WaitForSeconds(0.03f);
            }

            //The "zoom ended" clean-up part
                //To keep the zoom for a certain time.
                yield return new WaitForSeconds(zoomDataSequences[0].intervalBetweenNextZoom);


                //Gets zoom to the zoom it should be if no more zooms
                if (zoomDataSequences[0].zoomOut == true)
                {
                    finalZoomOut = currentMaxZoom + zoomDataSequences[0].zoomValue;

                    //if (finalZoomOut > 90f)
                        //Debug.Log("FinalZoomLogic: zoomValue is: " + zoomDataSequences[0].zoomValue + " and finalZoom is:  " + finalZoomOut + " and lerp timethingy: " + (timePassed / zoomDataSequences[0].zoomDuration));
                } 
                else
                    finalZoomIn = currentMaxZoom;

                //If no more sequences, reset these so they wont apply to next zooming stuff.
                /*if (zoomDataSequences.Count == 1)
                {
                    finalZoomIn = 0;
                    finalZoomOut = 0;
                }*/
                currentMaxZoom = defaultMaxZoom;
                    

                //Now, put back the player
                if (zoomDataSequences[0].zoomTarget != null)
                    RemoveFocusTarget(zoomDataSequences[0].zoomTarget);

                if (zoomDataSequences[0].clearFocusTargetsBeforeZoom)
                    AddFocusTarget(GameObject.FindGameObjectWithTag("Player").transform);

                //if (zoomDataSequences.Count == 0)
                    //Debug.Log("Before" + zoomDataSequences.Count);

                //When all of the logic ends, to keep this while loop properly.
                //So after this, it goes at the next [0]
                zoomDataSequences.RemoveAt(0);

                //if (zoomDataSequences.Count == 0)
                    //Debug.Log("After" + zoomDataSequences.Count);
                //if (currentMaxZoom > 90f)
                    //Debug.Log("FinalBlock! zoomValue is: " + zoomDataSequences[0].zoomValue + " and finalZoom is:  " + finalZoomOut + " and lerp timethingy: " + (timePassed / zoomDataSequences[0].zoomDuration));


        }

        //Debug.Log("Exited");


    }

    public void MergeZoomSequences()
    {
        if (zoomDataSequences.Count < 2)
            return;

        //Do the algorithm on paper (for and if condition nothing else) with .count = 4, and true,false,true,false.
        //The code seems counter-intuitive, but if u understand it in paper, it all makes sense ez.
        int i = 0;
        int j = 1;

        //Search for any zoom data in the sequence that has same zoomIn/Out direction!
        for (; i < zoomDataSequences.Count - 1; j++)
        {
            if (zoomDataSequences[j].zoomOut == zoomDataSequences[i].zoomOut)
            {
                //Create new struct cuz u cannot manipulate struct data in a list (for some reason)
                ZoomData currentZoomData = zoomDataSequences[i];

                //currentZoomData.zoomDuration = currentZoomData.zoomDuration + zoomDataSequences[j].zoomDuration;
                //currentZoomData.intervalBetweenNextZoom = currentZoomData.intervalBetweenNextZoom + zoomDataSequences[j].intervalBetweenNextZoom;
                currentZoomData.zoomValue = currentZoomData.zoomValue + zoomDataSequences[j].zoomValue;
                

                if (zoomDataSequences[j].zoomTarget != null)
                    currentZoomData.zoomTarget = zoomDataSequences[j].zoomTarget;

                if (zoomDataSequences[j].clearFocusTargetsBeforeZoom == true)
                    currentZoomData.clearFocusTargetsBeforeZoom = zoomDataSequences[j].clearFocusTargetsBeforeZoom;

                //Limit the zoom.
                if (currentZoomData.zoomOut && currentMaxZoom + currentZoomData.zoomValue > maxZoomLimiter)
                {
                    currentZoomData.zoomValue = maxZoomLimiter - currentMaxZoom;

                    Debug.Log("The value is: " + (currentMaxZoom + currentZoomData.zoomValue));
                }
                    

                //Replace [i] with currentZoomData, that is the "updated" [i] with [i]
                zoomDataSequences[i] = currentZoomData;

                //Now remove [j] from zoomDataSequences
                zoomDataSequences.RemoveAt(j);

                //And start from beginning this forloop, because by removing this, you skip a slot, and there will be weird iteration behaviour.
                i = 0;
                j = 0;//This will be incremented to 1.

                //Error-catching, in case this is the last sequence ([0])
                if (zoomDataSequences.Count < 2)
                    return;
            }

            if (j == zoomDataSequences.Count - 1)
            {
                i++;
                j = i;//It gets incremented automatically to +1
            }
        }

    }

    /// <summary>
    /// Used to be CutsceneZoomIn(0.1f) but gotta put mage, and remove monsters!
    /// (Also perhaps put camera a little upwards?)
    /// </summary>
    public void StartCutsceneZoomIn()
    {
        //Remove everything, monsters DESTROYED.
        ClearFocusTargetsExceptPlayer();

        //Add Mage
        AddFocusTarget(GameObject.FindGameObjectWithTag("Mage"));

        //Don't allow nearby monsters to break the focus!
        allowFocusingMonsters = false;

        //Do the classic zoom in
        CutsceneZoomIn(0.1f);
    }

    public void EndCutsceneZoom()
    {
        //Remove everything, mage included
        ClearFocusTargetsExceptPlayer();

        //Don't allow nearby monsters to break the focus!
        allowFocusingMonsters = true;

        ResetZoomToDefault();
    }
}
