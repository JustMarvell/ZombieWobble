using Citlali.Audio;
using Citlali.Common;
using Citlali.Game;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Player
{
    public class PlayerAudio : NetworkBehaviour
    {
        [SerializeField] float stepDistance = 1.4f;        // meters between footsteps
        [SerializeField] float minStepSpeed = 1.5f;
        [SerializeField] float minLandAirTime = 0.25f;     // ignore tiny hops
        [SerializeField] float groundRadius = 0.3f;
        [SerializeField] float groundDistance = 0.9f;      // from the capsule centre

        Health health;
        PlayerLife life;
        PlayerInventory inventory;
        int groundMask;
        Vector3 lastPos;
        bool wasGrounded = true;
        float airTime, fallSpeed, stepAccum, lastHurt;

        public override void OnNetworkSpawn()
        {
            health = GetComponent<Health>();
            life = GetComponent<PlayerLife>();
            inventory = GetComponent<PlayerInventory>();
            groundMask = ~LayerMask.GetMask("Player", "Ragdoll");
            lastPos = transform.position;

            health.Hp.OnValueChanged += OnHp;
            life.State.OnValueChanged += OnState;
            life.KnockedDown.OnValueChanged += OnKnock;
            inventory.HasMedkit.OnValueChanged += OnMedkit;
            inventory.UseProgress.OnValueChanged += OnUseProgress;
        }

        public override void OnNetworkDespawn()
        {
            if (health != null) health.Hp.OnValueChanged -= OnHp;
            if (life != null)
            {
                life.State.OnValueChanged -= OnState;
                life.KnockedDown.OnValueChanged -= OnKnock;
            }
            if (inventory != null)
            {
                inventory.HasMedkit.OnValueChanged -= OnMedkit;
                inventory.UseProgress.OnValueChanged -= OnUseProgress;
            }
        }

        bool Grounded() => Physics.SphereCast(transform.position, groundRadius, Vector3.down,
            out _, groundDistance, groundMask, QueryTriggerInteraction.Ignore);

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            Vector3 vel = (transform.position - lastPos) / dt;
            lastPos = transform.position;

            bool grounded = Grounded();
            if (vel.magnitude > 30f)                       // teleport (restart, lobby): reset, no sounds
            {
                wasGrounded = grounded; airTime = 0f; fallSpeed = 0f; stepAccum = 0f;
                return;
            }

            bool canMove = life.IsAlive && !life.KnockedDown.Value;

            if (!grounded)
            {
                airTime += dt;
                fallSpeed = Mathf.Min(fallSpeed, vel.y);
                if (wasGrounded && vel.y > 2f && canMove)
                    Sfx.Play(SfxId.Jump, transform.position);
            }
            else
            {
                if (!wasGrounded && airTime > minLandAirTime && canMove)
                    Sfx.Play(SfxId.Land, transform.position, Mathf.Clamp01(0.4f - fallSpeed / 15f));
                airTime = 0f;
                fallSpeed = 0f;

                float horizontal = new Vector2(vel.x, vel.z).magnitude;
                if (canMove && horizontal > minStepSpeed)
                {
                    stepAccum += horizontal * dt;
                    if (stepAccum >= stepDistance)
                    {
                        stepAccum = 0f;
                        Sfx.Play(SfxId.Footstep, transform.position);
                    }
                }
            }
            wasGrounded = grounded;
        }

        void OnMedkit(bool previous, bool next)
        {
            if (next && !previous) Sfx.Play(SfxId.MedkitPickup, transform.position);
        }

        void OnUseProgress(float previous, float next)
        {
            if (previous <= 0f && next > 0f) Sfx.Play(SfxId.MedkitUse, transform.position);   // starts when you begin healing
        }

        void OnHp(float previous, float next)
        {
            if (next >= previous || next <= 0f || Time.time < lastHurt + 0.15f) return;   // 0 HP = Downed sound instead
            lastHurt = Time.time;
            Sfx.Play(SfxId.Hurt, transform.position);
        }

        void OnState(PlayerState previous, PlayerState next)
        {
            if (next == PlayerState.Downed) Sfx.Play(SfxId.Downed, transform.position);
            else if (next == PlayerState.Dead) Sfx.Play(SfxId.Death, transform.position);
            else if (next == PlayerState.Alive && previous == PlayerState.Downed &&
                     GameManager.Instance != null && GameManager.Instance.State.Value == GameState.Playing)
                Sfx.Play(SfxId.Revive, transform.position);   // not on restart or lobby resets
        }

        void OnKnock(bool previous, bool next)
        {
            if (next) Sfx.Play(SfxId.Knockdown, transform.position);
        }

        [Rpc(SendTo.ClientsAndHost)] public void PowerUpSfxRpc() => Sfx.Play(SfxId.PowerUpPickup, transform.position);
        [Rpc(SendTo.ClientsAndHost)] public void FallSfxRpc() => Sfx.Play(SfxId.Fall, transform.position);
    }
}