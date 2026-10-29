using Citlali.Enemies;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Common
{
    [RequireComponent(typeof(Health))]
    public class ImpactDamage : NetworkBehaviour
    {
        [SerializeField] float threshold = 25f;
        [SerializeField] float damagePerImpulse = 1f;
        [SerializeField] float maxDamage = 60f;
        [SerializeField] float hitCooldown = 0.15f;
        [SerializeField] bool logImpacts = true;

        Health health;
        float nextHitTime;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        void OnCollisionEnter(Collision c)
        {
            if (!IsServer || Time.time < nextHitTime) return;

            if (c.rigidbody != null && c.rigidbody.TryGetComponent(out EnemyAI _) && !TryGetComponent(out EnemyAI _)) return;

            float impulse = c.impulse.magnitude;
            if (logImpacts && impulse > 5f) Debug.Log($"{name} impact {impulse:F1}");
            if (impulse <= threshold) return;

            nextHitTime = Time.time + hitCooldown;
            health.TakeDamage(Mathf.Min((impulse - threshold) * damagePerImpulse, maxDamage));
        }
    }
}
