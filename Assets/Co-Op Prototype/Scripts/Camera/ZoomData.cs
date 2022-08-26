using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ZoomData
{
    /// <summary>
    /// How much to zoom In/Out?
    /// </summary>
    public float zoomValue;

    /// <summary>
    /// In how much time should the zoom be finished?
    /// </summary>
    public float zoomDuration;

    /// <summary>
    /// In how much time will the next zoom (if any) happen?<para />
    /// Default: 0
    /// </summary>
    public float intervalBetweenNextZoom;

    /// <summary>
    /// Which transform should the camera zoom onto?<para />
    /// Default: null
    /// </summary>
    public Transform zoomTarget;

    /// <summary>
    /// Name is self-explanatory, and often used to focus on a specific transform, aka using it paired with zoomTarget.<para />
    /// Default: false
    /// </summary>
    public bool clearFocusTargetsBeforeZoom;

    /// <summary>
    /// This data is for Zooming Out?<para />
    /// </summary>
    public bool zoomOut;
}
