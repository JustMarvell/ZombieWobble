using Citlali.Carry;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Player
{
    public class PlayerGrab : NetworkBehaviour
    {
        [SerializeField] float grabRange = 2.5f;
        [SerializeField] Vector3 handAnchor = new Vector3(0f, 0.3f, 0.6f);
        [SerializeField] float spring = 60f;
        [SerializeField] float damper = 8f;
        [SerializeField] float maxForce = 80f;
        [SerializeField] float slipDistance = 3f;

        CarryObject carried;
        int handleIndex = -1;
        Rigidbody rb;
        Transform handleT;

        public bool IsHolding => carried != null;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void ToggleGrab()
        {
            if (!IsServer) return;
            if (IsHolding) Release(); else TryGrab();
        }

        void TryGrab()
        {
            foreach (var obj in CarryObject.All)
            {
                if (!obj.TryClaim(transform.position, grabRange, NetworkObjectId, out int idx, out Transform handle))
                    continue;

                carried = obj;
                handleIndex = idx;

                handleT = handle;
                return;
            }
        }

        void FixedUpdate()
        {
            if (!IsServer || carried == null) return;

            Vector3  hand = transform.TransformPoint(handAnchor);
            Vector3 target = handleT.position;

            if (Vector3.Distance(hand, target) > slipDistance) { Release(); return; }

            Vector3 relVel = rb.GetPointVelocity(hand) - carried.Rb.GetPointVelocity(target);
            Vector3 f = (hand - target) * spring + relVel * damper;
            f = Vector3.ClampMagnitude(f, maxForce);

            carried.Rb.AddForceAtPosition(f, target);
            rb.AddForce(-f);
        }

        public void Release()
        {
            if (!IsServer) return;
            if (carried) carried.Release(handleIndex);
            carried = null;
            handleT = null;
            handleIndex = -1;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) Release();
        }
    }
}