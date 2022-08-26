using System;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class Camera2DFollow : MonoBehaviour
    {
        public Transform target;
        public float damping = 1;//How smooth the camera moves
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0;
        public float lookAheadMoveThreshold = 0.1f;
        public Vector3 offset = Vector2.zero;

        private float m_OffsetZ;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_CurrentVelocity;
        private Vector3 m_LookAheadPos;

        // Use this for initialization
        private void Start()
        {
            m_LastTargetPosition = target.position;
            m_OffsetZ = (transform.position - target.position).z;
            //transform.parent = null;
        }


        private void LateUpdate()
        {
            // only update lookahead pos if accelerating or changed direction
            float xMoveDelta = (target.position - m_LastTargetPosition).x;

            bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

            if (updateLookAheadTarget)
            {
                m_LookAheadPos = lookAheadFactor*Vector3.right*Mathf.Sign(xMoveDelta);
            }
            else
            {
                m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime*lookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward*m_OffsetZ + offset;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

            transform.position = newPos;

            m_LastTargetPosition = target.position;
        }
    }
}
/*
 Compute shake angle and offset:
 angle = maxAngle * shake * GetPerlinNoise (seed,time,...)
 offsetX = maxOffset * shake * GetPerlinNoise (seed+1,time,...)
 offsetY = maxOffset * shake * GetPerlinNoise (seed+2,time,...)

then add it to the camera for that frame (preserve the base camera)
shakyCamera.angle = camera.Angle + angle;
shakyCamera.center = camera.center + Vec2(offsetX,offsetY);

camera shouldnt change if ur standing still and simply jump, to the same platform/position. (aka 100% vertical jump)

    Use cubic hermite curves for following?
    sin/cos?
    x = x + (target-x) * 0.1f; -> Each frame we move 10% towards the target. Also, pause or timescale = fail.
    x = (0.9f*x) + (0.1f*target); ->Each frame, we take a 90/10 blend of ourselves and our target. (ourselves = enemy, ty)
    0.01f -> nice&slow (at 60 fps)
    0.1f -> fast

Main point of focus towards enemy, and player plz.
 */
