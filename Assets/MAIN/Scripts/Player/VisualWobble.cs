using UnityEngine;

namespace Citlali.Player
{
    public class VisualWobble : MonoBehaviour
    {
        [SerializeField] Transform owner;      // the root that moves (player or enemy root)
        [SerializeField] PlayerLife life;      // optional; leave empty on enemies
        [SerializeField] float runLean = 8f;           // degrees of forward lean at full speed
        [SerializeField] float runSpeed = 6f;
        [SerializeField] float accelLean = 0.6f;       // degrees per m/s² of acceleration
        [SerializeField] float stiffness = 120f;       // higher = snappier return
        [SerializeField] float damping = 8f;           // lower = more wobble
        [SerializeField] float maxAngle = 25f;

        Quaternion baseRot;
        Vector3 lastPos, lastVel;
        float pitch, roll, pitchVel, rollVel;

        void Start()
        {
            baseRot = transform.localRotation;
            lastPos = owner.position;
        }

        // Short shove, for example when punching. Positive pitch leans forward.
        public void Kick(float pitchImpulse, float rollImpulse = 0f)
        {
            pitchVel += pitchImpulse;
            rollVel += rollImpulse;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 vel = (owner.position - lastPos) / dt;
            vel.y = 0f;
            lastPos = owner.position;

            Vector3 acc = (vel - lastVel) / dt;
            lastVel = vel;
            if (vel.magnitude > 20f || acc.magnitude > 60f) acc = Vector3.zero;   // teleports and glitches

            Vector3 localAcc = owner.InverseTransformDirection(acc);
            Vector3 localVel = owner.InverseTransformDirection(vel);

            float targetPitch = runLean * Mathf.Clamp01(localVel.z / runSpeed) - accelLean * localAcc.z;
            float targetRoll = accelLean * localAcc.x;

            if (life != null && !life.IsAlive) { targetPitch = 0f; targetRoll = 0f; }

            pitchVel += (stiffness * (targetPitch - pitch) - damping * pitchVel) * dt;
            rollVel += (stiffness * (targetRoll - roll) - damping * rollVel) * dt;
            pitch = Mathf.Clamp(pitch + pitchVel * dt, -maxAngle, maxAngle);
            roll = Mathf.Clamp(roll + rollVel * dt, -maxAngle, maxAngle);

            transform.localRotation = baseRot * Quaternion.Euler(pitch, 0f, roll);
        }
    }
}
