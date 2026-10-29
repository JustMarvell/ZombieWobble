using System;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Common
{
    public class Health : NetworkBehaviour
    {
        [SerializeField] float maxHp = 100f;

        public NetworkVariable<float> Hp = new NetworkVariable<float>(100f);
        public NetworkVariable<int> PaddingSecs = new();
        public float MaxHp => maxHp;
        public bool IsDead => Hp.Value <= 0f;

        public event Action<float> Damaged;
        public event Action Died;

        float paddingUtil;
        float paddingMultiplier = 1f;

        public override void OnNetworkSpawn()
        {
            if (IsServer) Hp.Value = maxHp;
        }

        public void TakeDamage(float amount)
        {
            if (!IsServer || IsDead || amount <= 0f) return;

            if (Time.time < paddingUtil) amount *= paddingMultiplier;

            Hp.Value = Mathf.Max(0f, Hp.Value - amount);
            Damaged?.Invoke(amount);
            if (Hp.Value <= 0f) Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (!IsServer || IsDead) return;
            Hp.Value = Mathf.Min(maxHp, Hp.Value + amount);
        }

        public void ResetFull()
        {
            if (IsServer) Hp.Value = maxHp;
        }

        public void SetHp(float value)
        {
            if (IsServer) Hp.Value = Mathf.Clamp(value, 0f, maxHp);
        }

        public void ApplyPadding(float multiplier, float seconds)
        {
            if (!IsServer) return;
            paddingMultiplier = multiplier;
            paddingUtil = Time.time + seconds;
        }

        void Update()
        {
            if (!IsServer) return;
            int s = Mathf.Max(0, Mathf.CeilToInt(paddingUtil - Time.time));
            if (PaddingSecs.Value != s) PaddingSecs.Value = s;
        }
    }
}
