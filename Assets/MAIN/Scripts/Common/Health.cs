using System;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Common
{
    public class Health : NetworkBehaviour
    {
        [SerializeField] float maxHp = 100f;

        public NetworkVariable<float> Hp = new NetworkVariable<float>(100f);
        public float MaxHp => maxHp;
        public bool IsDead => Hp.Value <= 0f;

        public event Action<float> Damaged;
        public event Action Died;

        public override void OnNetworkSpawn()
        {
            if (IsServer) Hp.Value = maxHp;
        }

        public void TakeDamage(float amount)
        {
            if (!IsServer || IsDead || amount <= 0f) return;

            Hp.Value = Mathf.Max(0f, Hp.Value - amount);
            Damaged?.Invoke(amount);
            if (Hp.Value <= 0f) Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (!IsServer || IsDead) return;
            Hp.Value = Mathf.Min(maxHp, Hp.Value + amount);
        }
    }
}
