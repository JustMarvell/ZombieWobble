using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace Citlali
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarryObject : NetworkBehaviour
    {
        public static readonly List<CarryObject> All = new();
        const ulong Empty = ulong.MaxValue;

        [SerializeField] Transform[] handles;

        public NetworkList<ulong> Occupants = new();
        public Rigidbody Rb { get; private set; }

        void Awake()
        {
            Rb = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            All.Add(this);
            if (IsServer)
                for (int i = 0; i < handles.Length; i++) Occupants.Add(Empty);
        }

        public override void OnNetworkDespawn()
        {
            All.Remove(this);
        }

        public bool TryClaim(Vector3 from, float range, ulong playerId, out int index, out Transform handle)
        {
            index = -1; handle = null;
            float best = range;
            for (int i = 0; i < handles.Length && i < Occupants.Count; i++)
            {
                if (Occupants[i] != Empty) continue;
                float d = Vector3.Distance(from, handles[i].position);
                if (d <= best) { best = d; index = i; }
            }
            if (index < 0) return false;

            Occupants[index] = playerId;
            handle = handles[index];
            return true;
        }

        public void Release(int index)
        {
            if (IsServer && index >= 0 && index < Occupants.Count) Occupants[index] = Empty;
        }
    }
}
