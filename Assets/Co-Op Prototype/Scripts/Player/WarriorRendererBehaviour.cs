using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WarriorRendererBehaviour : MonoBehaviour
{
    public Vector3 revivePosition = new Vector3(4.9f, 2.3f);

    private float tempAngle;

    /// <summary>
    /// Used to re-locate the SpriteRenderer to normal, cuz revive animation relocates the transform
    /// </summary>
    public void ResetLocalPosition()
    {
        transform.localPosition = Vector3.zero;
    }

    public void SetRevivePosition()
    {
        transform.localPosition = revivePosition;
    }

    /// <summary>
    /// Currently used to reset death's
    /// </summary>
    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
    }

    //Make an ACTUAL rotation, a global extension plz?
    public void RotateVertically(bool RotatePositive, bool facingLeft)
    {
        if (RotatePositive)
            transform.rotation = Quaternion.Euler(0, 0, 270);
        else
            transform.rotation = Quaternion.Euler(0, 0, 90);

        if (facingLeft)
            transform.rotation = Quaternion.EulerAngles(0,0, -transform.rotation.eulerAngles.z);

        Debug.Log("Transform.rotationZ is: " + transform.rotation.eulerAngles.z);
        /* Has minor bugs, so i went for easy way out
        tempAngle = Mathf.Atan2(directionHit.y, directionHit.x) * Mathf.Rad2Deg;
        Debug.Log("Atan2" + tempAngle);

        transform.rotation = Quaternion.AngleAxis(tempAngle, Vector3.forward);
        */

    }
}
