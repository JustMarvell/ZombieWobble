using System.Collections;
using System.Collections.Generic;
using Citlali.Carry;
using Citlali.Common;
using Citlali.Game;
using Citlali.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Citlali.Enemies
{
    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public class EnemyAI : NetworkBehaviour
    {
        public static readonly List<EnemyAI> All = new();

        [SerializeField] float moveSpeed = 3.5f;
        [SerializeField] float acceleration = 12f;
        [SerializeField] float responsiveness = 4f;
        [SerializeField] float contactDamage = 10f;
        [SerializeField] float attackCooldown = 1f;
        [SerializeField] RagdollController ragdoll;

        Rigidbody rb;
        Health health;
        float nextAttackTime;
        bool dead;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            health = GetComponent<Health>();
            ragdoll.SetRagdoll(false);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) { enabled = false; return; }
            All.Add(this);
            health.Died += OnDied;
        }

        public override void OnNetworkDespawn()
        {
            All.Remove(this);
            if (health != null) health.Died -= OnDied;
        }

        void OnDied()
        {
            if (dead) return;
            dead = true;
            All.Remove(this);

            GetComponent<Collider>().enabled = false;
            GetComponent<NetworkRigidbody>().SetIsKinematic(true);

            DieRpc();
            StartCoroutine(DespawnLater(2.5f));
        }

        [Rpc(SendTo.ClientsAndHost)]
        void DieRpc() => ragdoll.SetRagdoll(true);

        IEnumerator DespawnLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (NetworkObject.IsSpawned) NetworkObject.Despawn(true);
        }

        void FixedUpdate()
        {
            if (dead) return;

            rb.angularVelocity = Vector3.zero;

            if (GameManager.Instance.State.Value != GameState.Playing || CarryObject.All.Count == 0) return;

            Vector3 to = CarryObject.All[0].transform.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.01f) return;
            Vector3 dir = to.normalized;

            Vector3 v = rb.linearVelocity;
            if (new Vector3(v.x, 0f, v.z).magnitude > moveSpeed * 1.3f) return;

            Vector3 want = dir * moveSpeed;
            Vector3 diff = new Vector3(want.x - v.x, 0f, want.z - v.z);
            rb.AddForce(Vector3.ClampMagnitude(diff * responsiveness, acceleration), ForceMode.Acceleration);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(dir), 8f * Time.fixedDeltaTime));
        }

        void OnCollisionStay(Collision c)
        {
            if (dead) return;

            if (!IsServer || Time.time < nextAttackTime) return;
            if (GameManager.Instance.State.Value != GameState.Playing) return;

            var other = c.collider.attachedRigidbody;
            if (other == null || other.GetComponent<EnemyAI>() != null) return;
            if (!other.TryGetComponent(out Health target)) return;

            nextAttackTime = Time.time + attackCooldown;
            target.TakeDamage(contactDamage);
            if (other.TryGetComponent(out PlayerLife victim)) victim.Knockdown(0.6f);
        }
    }
}