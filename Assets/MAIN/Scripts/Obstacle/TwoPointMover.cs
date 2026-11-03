using System.Collections;
using Citlali.Carry;
using Citlali.Game;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Obstacle
{
    [RequireComponent(typeof(Rigidbody))]
    public class TwoPointMover : NetworkBehaviour
    {
        [SerializeField] float moveSpeed = 4f;
        [SerializeField] float acceleration = 12f;
        [SerializeField] float responsiveness = 4f;
        [SerializeField] float cooldown = 1.5f;

        [SerializeField] Transform pointA;
        [SerializeField] Transform pointB;
        
        Transform target;
        Rigidbody rb;
        bool isSwitching;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            target = pointA;
            if (pointA == null || pointB == null) Debug.LogWarning("There's no point a and/or point b assinged in here", this);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) { enabled = false; return; }
        }

        void FixedUpdate()
        {
            rb.angularVelocity = Vector3.zero;

            // if (GameManager.Instance.State.Value != GameState.Playing || CarryObject.All.Count == 0) return;

            float dist = (target.position - transform.position).magnitude;
            
            if (dist <= 0.2f)
                StartCoroutine(SwitchTarget());

            Vector3 to = target.position - transform.position;

            if (to.sqrMagnitude < 0.01f) return;
            Vector3 dir = to.normalized;

            Vector3 v = rb.linearVelocity;
            if (new Vector3(v.x, v.y, v.z).magnitude > moveSpeed * 1.3f) return;

            Vector3 want = dir * moveSpeed;
            Vector3 diff = new Vector3(want.x - v.x, want.y - v.y, want.z - v.z);
            if (!isSwitching) rb.AddForce(Vector3.ClampMagnitude(diff * responsiveness, acceleration), ForceMode.Acceleration);            
        }

        IEnumerator SwitchTarget()
        {
            isSwitching = true;
            yield return new WaitForSeconds(cooldown);
            isSwitching = false;
            target = target == pointA ? pointB : pointA;
        }
    }
}