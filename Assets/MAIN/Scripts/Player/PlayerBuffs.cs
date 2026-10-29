using Citlali.Items;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Player
{
    public class PlayerBuffs : NetworkBehaviour
    {
        [SerializeField] float punchMultiplier = 2f;
        [SerializeField] float jumpMultiplier = 1.6f;

        public NetworkVariable<int> GlovesSecs = new();
        public NetworkVariable<int> JumpSecs = new();

        float glovesUntil, jumpUntil;

        public float PunchMult => Time.time < glovesUntil ? punchMultiplier : 1f;
        public float JumpMult => Time.time < jumpUntil ? jumpMultiplier : 1f;

        public void Give(PowerUpType type, float seconds)
        {
            if (!IsServer) return;
            if (type == PowerUpType.Gloves) glovesUntil = Time.time + seconds;
            else if (type == PowerUpType.Boots) jumpUntil = Time.time + seconds;
        }

        public void ClearAll() { glovesUntil = 0f; jumpUntil = 0f; }

        void Update()
        {
            if (!IsServer) return;
            SetSecs(GlovesSecs, glovesUntil);
            SetSecs(JumpSecs, jumpUntil);
        }

        static void SetSecs(NetworkVariable<int> v, float until)
        {
            int s = Mathf.Max(0, Mathf.CeilToInt(until - Time.time));
            if (v.Value != s) v.Value = s;
        }
    }
}
