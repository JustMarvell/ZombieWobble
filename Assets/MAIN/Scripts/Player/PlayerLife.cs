using Citlali.Common;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Citlali.Player
{
    public enum PlayerState { Alive, Downed, Dead }

    public class PlayerLife : NetworkBehaviour
    {
        [SerializeField] float bleedOutTime = 30f;
        [SerializeField] float reviveHpFraction = 0.3f;
        [SerializeField] Transform visual;
        [SerializeField] Collider bodyCollider;
        [SerializeField] RagdollController ragdoll;

        public NetworkVariable<PlayerState> State = new(PlayerState.Alive);
        public NetworkVariable<int> BleedOut = new();   // whole seconds left, for the UI
        public NetworkVariable<bool> KnockedDown = new();

        public bool IsAlive => State.Value == PlayerState.Alive;
        public bool IsDowned => State.Value == PlayerState.Downed;

        Health health;
        Rigidbody rb;
        NetworkRigidbody netRb;
        PlayerGrab grab;
        PlayerInventory inventory;
        Vector3 visualStartPos;
        float bleedTimer;
        float knockdownEnd;

        public override void OnNetworkSpawn()
        {
            health = GetComponent<Health>();
            rb = GetComponent<Rigidbody>();
            netRb = GetComponent<NetworkRigidbody>();
            grab = GetComponent<PlayerGrab>();
            inventory = GetComponent<PlayerInventory>();
            visualStartPos = visual.localPosition;

            State.OnValueChanged += OnStateChanged;
            KnockedDown.OnValueChanged += OnKnockedDownChanged;
            RefreshRagdoll();

            if (IsServer) health.Died += GoDown;
        }

        public override void OnNetworkDespawn()
        {
            State.OnValueChanged -= OnStateChanged;
            KnockedDown.OnValueChanged -= OnKnockedDownChanged;
            if (IsServer && health != null) health.Died -= GoDown;
        }

        void OnStateChanged(PlayerState previous, PlayerState next) => RefreshRagdoll();
        void OnKnockedDownChanged(bool previous, bool next) => RefreshRagdoll();

        void RefreshRagdoll()
        {
            bool down = State.Value != PlayerState.Alive;
            ragdoll.SetRagdoll(down || KnockedDown.Value, follow: !down);
        }


        void ApplyVisual(PlayerState s)
        {
            bool down = s != PlayerState.Alive;
            visual.localRotation = down ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
            visual.localPosition = down ? visualStartPos + Vector3.down * 0.5f : visualStartPos;
        }

        public void Knockdown(float seconds)
        {
            if (!IsServer || !IsAlive) return;
            grab.Release();
            inventory.SetUsing(false);
            knockdownEnd = Time.time + seconds;
            KnockedDown.Value = true;
        }

        void SetDownedPhysics(bool downed)
        {
            if (downed)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            netRb.SetIsKinematic(downed);
            bodyCollider.enabled = !downed;
        }

        // Server only
        void GoDown()
        {
            if (!IsAlive) return;
            grab.Release();
            inventory.DropMedkit();
            bleedTimer = bleedOutTime;
            BleedOut.Value = Mathf.CeilToInt(bleedOutTime);
            State.Value = PlayerState.Downed;
            SetDownedPhysics(true);
        }

        public void Revive()
        {
            if (!IsServer || !IsDowned) return;
            health.SetHp(health.MaxHp * reviveHpFraction);
            State.Value = PlayerState.Alive;
            SetDownedPhysics(false);
        }

        public void ResetToAlive()
        {
            if (!IsServer) return;
            health.ResetFull();
            inventory.ClearMedkit();
            State.Value = PlayerState.Alive;
            KnockedDown.Value = false;
            SetDownedPhysics(false);
        }

        void Update()
        {
            if (!IsServer) return;
            if (KnockedDown.Value && Time.time >= knockdownEnd) KnockedDown.Value = false;
            if (!IsDowned) return;

            bleedTimer -= Time.deltaTime;
            int secs = Mathf.Max(0, Mathf.CeilToInt(bleedTimer));
            if (secs != BleedOut.Value) BleedOut.Value = secs;
            if (bleedTimer <= 0f) State.Value = PlayerState.Dead;
        }

        public void Kill()
        {
            if (!IsServer || State.Value == PlayerState.Dead) return;

            grab.Release();
            inventory.ClearMedkit();
            health.SetHp(0f);
            State.Value = PlayerState.Dead;
            SetDownedPhysics(true);
        }
    }
}
