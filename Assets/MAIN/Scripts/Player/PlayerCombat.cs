using System.Collections;
using System.Collections.Generic;
using Citlali.Carry;
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
        [SerializeField] float hitDelay = 0.16f;
        [SerializeField] float hitStopTime = 0.08f;
        [SerializeField] float shakeAmount = 0.35f;

        float nextPunchTime;
        readonly HashSet<Rigidbody> hitThisPunch = new();
        PlayerBuffs buffs;
        PlayerLife life;

        void Awake()
        {
            buffs = GetComponent<PlayerBuffs>();
            life = GetComponent<PlayerLife>();
        }

        public void TryPunch()
        {
            if (!IsServer || Time.time < nextPunchTime) return;
            nextPunchTime = Time.time + cooldown;
            PunchVisualRpc();             // the swing starts now
            StartCoroutine(ApplyHit());   // the hit lands when the fist arrives
        }

        IEnumerator ApplyHit()
        {
            yield return new WaitForSeconds(hitDelay);
            if (!life.IsAlive || life.KnockedDown.Value) yield break;   // downed or knocked down mid-swing

            hitThisPunch.Clear();
            Vector3 center = transform.position + transform.forward * range;
            foreach (var col in Physics.OverlapSphere(center, radius, ~(1 << LayerMask.NameToLayer("Ragdoll"))))
            {
                var target = col.attachedRigidbody;
                if (target == null || target.gameObject == gameObject) continue;
                if (!hitThisPunch.Add(target)) continue;

                Vector3 dir = ((target.position - transform.position).normalized + Vector3.up * 0.3f).normalized;
                target.AddForce(dir * force * buffs.PunchMult, ForceMode.Impulse);

                if (target.TryGetComponent(out EnemyAI _) && target.TryGetComponent(out Health enemyHealth))
                    enemyHealth.TakeDamage(enemyDamage * buffs.PunchMult);
                if (target.TryGetComponent(out PlayerLife victim)) victim.Knockdown(knockdownTime);

                float strength = (target.TryGetComponent(out CarryObject _) ? 0.5f : 1f) * buffs.PunchMult;
                Vector3 point = Vector3.Lerp(center, target.worldCenterOfMass, 0.5f);
                var netObj = target.GetComponent<NetworkObject>();
                HitFeedbackRpc(point, dir, strength, netObj != null ? new NetworkObjectReference(netObj) : default);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void HitFeedbackRpc(Vector3 point, Vector3 dir, float strength, NetworkObjectReference target)
        {
            if (ImpactFX.Instance != null) ImpactFX.Instance.PlayHit(point, strength);

            punchIK.HitStop(hitStopTime * Mathf.Clamp(strength, 0.5f, 1.5f));

            var rig = PlayerCameraRig.Local;
            if (rig != null)
            {
                float distance = Vector3.Distance(rig.transform.position, point);
                rig.AddTrauma(shakeAmount * strength * Mathf.Clamp01(1f - distance / 15f));
            }

            if (target.TryGet(out NetworkObject victim))
            {
                var w = victim.GetComponentInChildren<VisualWobble>();
                if (w != null)
                {
                    Vector3 local = victim.transform.InverseTransformDirection(dir);
                    w.Kick(-local.z * 250f * strength, local.x * 250f * strength);   // lean away from the punch
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void PunchVisualRpc()
        {
            punchIK.Play();
            wobble.Kick(150f);
            if (ImpactFX.Instance != null) ImpactFX.Instance.PlayWhoosh(transform.position);
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
