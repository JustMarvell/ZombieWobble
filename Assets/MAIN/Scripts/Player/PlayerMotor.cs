using Unity.Netcode;
using UnityEngine;

namespace Citlali.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMotor : NetworkBehaviour
    {
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float acceleration = 25f;
        [SerializeField] float responsiveness = 5f;
        [SerializeField] float jumpSpeed = 7f;
        [SerializeField] float turnSpeed = 10f;

        Rigidbody rb;
        Vector3 moveDir;
        float lastInputTime;

        void Awake() => rb = GetComponent<Rigidbody>();

        public override void OnNetworkSpawn()
        {
            if (!IsServer) { enabled = false; return; }
            transform.position = new Vector3(Random.Range(-2f, 2f), 1.5f, Random.Range(-2f, 2f));
        }

        public void SetMoveDir(Vector3 dir)
        {
            moveDir = Vector3.ClampMagnitude(dir, 1f);
            lastInputTime = Time.time;
        }

        public void Jump()
        {
            if (Physics.Raycast(transform.position, Vector3.down, 1.1f))
                rb.AddForce(Vector3.up * jumpSpeed, ForceMode.VelocityChange);
        }

        void FixedUpdate()
        {
            if (Time.time - lastInputTime > 0.25f) moveDir = Vector3.zero;

            Vector3 v = rb.linearVelocity;
            Vector3 want = moveDir * moveSpeed;
            Vector3 diff = new Vector3(want.x - v.x, 0f, want.z - v.z);
            rb.AddForce(Vector3.ClampMagnitude(diff * responsiveness, acceleration), ForceMode.Acceleration);

            if (moveDir.sqrMagnitude > 0.01f)
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDir), turnSpeed * Time.fixedDeltaTime));
        }
    }
}