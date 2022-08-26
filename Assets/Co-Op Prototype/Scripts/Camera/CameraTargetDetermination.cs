using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CameraTargetDetermination : MonoBehaviour
{
    [ValidateInput("IsLayerMaskEmpty")]
    public LayerMask WhatIsFocusable;

    //private Camera commonCamera;
    private MultipleTargetCamera commonMultipleTargetCamera;

    private void Awake()
    {
        //Gets camera component
        //commonCamera = GetComponent<Camera>();
        commonMultipleTargetCamera = GetComponentInParent<MultipleTargetCamera>();
    }

    //Uses the boxcollider2D, to "register" the focusTargets.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((WhatIsFocusable & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
                commonMultipleTargetCamera.AddFocusTarget(collision.transform);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((WhatIsFocusable & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer)
            commonMultipleTargetCamera.RemoveFocusTarget(collision.transform);
    }

    //To validate Input, so game won't run with layermask(WhatIsPlayer) = Nothing
    protected bool IsLayerMaskEmpty(LayerMask layermask)
    {
        return layermask.value != 0;
    }

    public void DestroyThis()
    {
        Destroy(this);
    }
}
