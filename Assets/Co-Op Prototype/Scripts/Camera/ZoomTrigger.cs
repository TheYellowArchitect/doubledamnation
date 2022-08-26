using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

//To be attached on an object with collider2D
[RequireComponent(typeof(Collider2D))]
public class ZoomTrigger : MonoBehaviour
{
    [Tooltip("How much should the camera zoom in, once triggered")]
    public int zoomIn;

    [Tooltip("How fast to go from normal to zoomIn")]
    public float zoomStartDuration;

    [Tooltip("How fast to go from zoomIn to normal")]
    public float zoomEndDuration;

    [Tooltip("When triggered, in how many seconds does the zooming process start?")]
    public float waitFor;//soz for some reason i can't figure a better variable name, but i don't want to spend 5+ mins for a "useless" name :(

    [Tooltip("When triggered, after zooming, in how many seconds does the zooming process start the final zoomIn/Out?")]
    public float haltZoomingFor = 1f;//soz for some reason i can't figure a better variable name, but i don't want to spend 5+ mins for a "useless" name :(

    [Tooltip("Leave it null if its supposed to just zoom and not focus anything")]
    public Transform targetZoom;

    [Tooltip("Should it lose focus of everything, and exclusively focus this?")]
    public bool zoomExclusively = false;

    private bool triggeredOnce = false;

    private MultipleTargetCamera cameraScript;

    private void Start()
    {
        cameraScript = GameObject.FindGameObjectWithTag("CameraHolder").GetComponent<MultipleTargetCamera>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggeredOnce == false)
            StartCoroutine(StartZoom());
    }

    public IEnumerator StartZoom()//Since this runs once, is IEnumerator or Invoke() cheaper?...
    {
        yield return new WaitForSeconds(waitFor);

        triggeredOnce = true;

        cameraScript.ZoomBoth(-1 * zoomIn, zoomStartDuration, zoomEndDuration, true, haltZoomingFor, targetZoom, zoomExclusively);
    }

    [Button("Reset Trigger")]
    public void ResetTrigger()
    {
        triggeredOnce = false;
    }
}
