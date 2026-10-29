using System.Collections;
using System.Collections.Generic;
using Citlali.Common;
using Citlali.Enemies;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Player
{
    public class PlayerCombat : NetworkBehaviour
    {
        [SerializeField] Transform visual;
        [SerializeField] float cooldown = 0.6f;
        [SerializeField] float range = 1.2f;
        [SerializeField] float radius = 0.8f;
        [SerializeField] float force = 12f;
        [SerializeField] float enemyDamage = 10f;
        [SerializeField] PunchArmIK punchIK;
        [SerializeField] VisualWobble wobble;
        [SerializeField] float knockdownTime = 1.5f;

        float nextPunchTime;
        readonly HashSet<Rigidbody> hitThisPunch = new();
        PlayerBuffs buffs;

        void Awake()
        {
            buffs = GetComponent<PlayerBuffs>();
        }

        public void TryPunch()
        {
            if (!IsServer || Time.time < nextPunchTime) return;
            nextPunchTime = Time.time + cooldown;

            hitThisPunch.Clear();
            Vector3 center = transform.position + transform.forward * range;
            foreach (var col in Physics.OverlapSphere(center, radius, ~(1 << LayerMask.NameToLayer("Ragdoll"))))
            {
                var target = col.attachedRigidbody;
                if (target == null || target.gameObject == gameObject) continue;
                if (!hitThisPunch.Add(target)) continue;   // one hit per rigidbody

                Vector3 dir = (target.position - transform.position).normalized + Vector3.up * 0.3f;
                target.AddForce(dir.normalized * force * buffs.PunchMult, ForceMode.Impulse);

                if (target.TryGetComponent(out EnemyAI _) && target.TryGetComponent(out Health enemyHealth))
                {
                    enemyHealth.TakeDamage(enemyDamage * buffs.PunchMult);
                    if (target.TryGetComponent(out PlayerLife victim)) victim.Knockdown(knockdownTime);
                }
            }
            PunchVisualRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        void PunchVisualRpc()
        {
            punchIK.Play();
            wobble.Kick(150f);
            StartCoroutine(Lunge());
        }

        IEnumerator Lunge()
        {
            Vector3 start = visual.localPosition;
            for (float t = 0f; t < 0.15f; t += Time.deltaTime)
            {
                visual.localPosition = start + Vector3.forward * 0.5f * Mathf.Sin(t / 0.15f * Mathf.PI);
                yield return null;
            }
            visual.localPosition = start;
        }
    }
}
