using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//In the future, please use an event-based system instead of per-frame, because this will be used in the future with many many monsters in a normal-sized level!!!
public class TitleFollowOffset : MonoBehaviour
{
    [Tooltip("So as to calculate the offset from this and the transform")]
    public Transform targetTransform;

    [Tooltip("Caches this, so as to know when it gets disabled/title holder dies.")]
    public EnemyPathfinder commonPathfinder;

    private Vector3 offset;
    
    private MeshRenderer commonMeshRenderer;

	void Start ()
    {
        offset = targetTransform.position - transform.position;

        commonMeshRenderer = GetComponent<MeshRenderer>();
	}
	

	void LateUpdate ()
    {
        transform.position = targetTransform.position - offset;

        //tfw no event/trigger, but check every frame smh.
        if (commonMeshRenderer.enabled == true && commonPathfinder.enabled == false)
            commonMeshRenderer.enabled = false;
        else if (commonMeshRenderer.enabled == false && commonPathfinder.enabled == true)
            commonMeshRenderer.enabled = true;
	}
}
