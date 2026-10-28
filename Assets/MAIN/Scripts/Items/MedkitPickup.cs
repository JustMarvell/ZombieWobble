using System.Collections.Generic;
using Citlali.Player;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Items
{
    public class MedkitPickup : NetworkBehaviour
    {
        public static readonly List<MedkitPickup> All = new();

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
            var rbody = other.attachedRigidbody;
            var inv = rbody ? rbody.GetComponent<PlayerInventory>() : null;
            if (inv != null && inv.TryPickup()) NetworkObject.Despawn(true);
        }
    }
}