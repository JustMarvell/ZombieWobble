using Citlali.Audio;
using Citlali.Common;
using Citlali.Game;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Enemies
{
    public class EnemyAudio : NetworkBehaviour
    {
        [SerializeField] Vector2 groanInterval = new(3f, 7f);

        Health health;
        float nextGroan;

        public override void OnNetworkSpawn()
        {
            health = GetComponent<Health>();
            health.Hp.OnValueChanged += OnHp;
            nextGroan = Time.time + Random.Range(groanInterval.x, groanInterval.y);
        }

        public override void OnNetworkDespawn()
        {
            if (health != null) health.Hp.OnValueChanged -= OnHp;
        }

        void Update()
        {
            if (health == null || health.Hp.Value <= 0f || Time.time < nextGroan) return;
            nextGroan = Time.time + Random.Range(groanInterval.x, groanInterval.y);

            var gm = GameManager.Instance;
            if (gm != null && gm.State.Value == GameState.Playing)
                Sfx.Play(SfxId.EnemyGroan, transform.position);
        }

        void OnHp(float previous, float next)
        {
            if (next >= previous) return;
            Sfx.Play(next <= 0f ? SfxId.EnemyDeath : SfxId.EnemyHurt, transform.position);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void AttackRpc() => Sfx.Play(SfxId.EnemyAttack, transform.position);
    }
}