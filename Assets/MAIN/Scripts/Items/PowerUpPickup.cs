using System.Collections.Generic;
using Citlali.Carry;
using Citlali.Common;
using Citlali.Player;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Items
{
    public enum PowerUpType { Gloves, Boots, Padding}
    
    public class PowerUpPickup : NetworkBehaviour
    {
        public static readonly List<PowerUpPickup> All = new();

        [SerializeField] PowerUpType type;
        [SerializeField] float duration = 15f;
        [SerializeField] float paddingMultiplier = 0.3f;

        public override void OnNetworkSpawn()
        {
            All.Add(this);
        }

        public override void OnNetworkDespawn()
        {
            All.Remove(this);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            var body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out PlayerLife life) || !life.IsAlive) return;

            if (type == PowerUpType.Padding)
            {
                if (CarryObject.All.Count == 0) return;
                CarryObject.All[0].GetComponent<Health>().ApplyPadding(paddingMultiplier, duration);
            }
            else
            {
                body.GetComponent<PlayerBuffs>().Give(type, duration);
            }

            if (body.TryGetComponent(out PlayerAudio pa)) pa.PowerUpSfxRpc();

            NetworkObject.Despawn(true);
        }
    }
}
