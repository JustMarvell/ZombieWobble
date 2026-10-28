using Citlali.Common;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Player
{
    public class PlayerInventory : NetworkBehaviour
    {
        [SerializeField] NetworkObject medkitPickupPrefab;
        [SerializeField] float useTime = 2f;
        [SerializeField] float useRange = 2.5f;
        [SerializeField] float healAmount = 60f;

        public NetworkVariable<bool> HasMedkit = new();
        public NetworkVariable<float> UseProgress = new();   // 0 to 1, for the UI

        PlayerLife life;
        Health health;
        PlayerLife target;
        bool isUsing;
        float useTimer;

        public override void OnNetworkSpawn()
        {
            life = GetComponent<PlayerLife>();
            health = GetComponent<Health>();
            if (IsServer) health.Damaged += OnHit;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && health != null) health.Damaged -= OnHit;
        }

        void OnHit(float amount) => CancelUse();   // getting hit interrupts

        // ---- Server only below ----

        public bool TryPickup()
        {
            if (!IsServer || HasMedkit.Value || !life.IsAlive) return false;
            HasMedkit.Value = true;
            return true;
        }

        public void DropMedkit()
        {
            if (!IsServer || !HasMedkit.Value) return;
            HasMedkit.Value = false;
            CancelUse();

            Vector3 pos = transform.position + transform.forward;
            if (Physics.Raycast(pos + Vector3.up, Vector3.down, out var hit, 6f))
                pos = hit.point + Vector3.up * 0.3f;
            Instantiate(medkitPickupPrefab, pos, Quaternion.identity).Spawn();
        }

        public void ClearMedkit()
        {
            if (!IsServer) return;
            HasMedkit.Value = false;
            CancelUse();
        }

        public void SetUsing(bool holding)
        {
            if (!IsServer) return;
            if (!holding) { CancelUse(); return; }
            if (isUsing || !life.IsAlive || !HasMedkit.Value) return;

            target = FindTarget();
            if (target == null) return;
            isUsing = true;
            useTimer = 0f;
        }

        PlayerLife FindTarget()
        {
            PlayerLife best = null;
            float bestScore = float.MaxValue;
            foreach (var c in NetworkManager.Singleton.ConnectedClientsList)
            {
                var pl = c.PlayerObject ? c.PlayerObject.GetComponent<PlayerLife>() : null;
                if (pl == null || pl.State.Value == PlayerState.Dead) continue;

                var h = pl.GetComponent<Health>();
                if (!pl.IsDowned && h.Hp.Value >= h.MaxHp) continue;   // nothing to heal

                float d = Vector3.Distance(transform.position, pl.transform.position);
                if (d > useRange) continue;

                float score = d + (pl.IsDowned ? 0f : 100f);           // downed players first
                if (score < bestScore) { bestScore = score; best = pl; }
            }
            return best;
        }

        void Update()
        {
            if (!IsServer || !isUsing) return;

            if (!life.IsAlive || target == null || target.State.Value == PlayerState.Dead ||
                Vector3.Distance(transform.position, target.transform.position) > useRange + 0.5f)
            {
                CancelUse();
                return;
            }

            useTimer += Time.deltaTime;
            UseProgress.Value = Mathf.Clamp01(useTimer / useTime);
            if (useTimer < useTime) return;

            if (target.IsDowned) target.Revive();
            else target.GetComponent<Health>().Heal(healAmount);

            HasMedkit.Value = false;
            CancelUse();
        }

        void CancelUse()
        {
            isUsing = false;
            target = null;
            useTimer = 0f;
            if (IsServer && UseProgress.Value != 0f) UseProgress.Value = 0f;
        }
    }
}