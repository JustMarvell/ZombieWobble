using Citlali.Common;
using Citlali.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Citlali.Multiplayer
{
    public class LobbyPlayerSpawner : MonoBehaviour
    {
        [SerializeField] NetworkObject playerPrefab;
        [SerializeField] Transform[] spawnPoints;
        int next;

        void Start()
        {
            var nm = NetworkManager.Singleton;
            if (!nm.IsServer) return;
            foreach (var id in nm.ConnectedClientsIds) Spawn(id);
            nm.OnClientConnectedCallback += Spawn;
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientConnectedCallback -= Spawn;
        }

        void Spawn(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            var sp = spawnPoints[next++ % spawnPoints.Length];
            var existing = nm.ConnectedClients[clientId].PlayerObject;

            if (existing == null)
                Instantiate(playerPrefab, sp.position, sp.rotation).SpawnAsPlayerObject(clientId);
            else
                ResetPlayer(existing, sp);   // coming back from a round
        }

        static void ResetPlayer(NetworkObject p, Transform sp)
        {
            p.GetComponent<PlayerGrab>().Release();
            p.GetComponent<Health>().ResetFull();
            p.GetComponent<PlayerLife>().ResetToAlive();

            var rb = p.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = sp.position;
            rb.rotation = sp.rotation;
            p.GetComponent<NetworkTransform>().Teleport(sp.position, sp.rotation, Vector3.one);
        }
    }
}