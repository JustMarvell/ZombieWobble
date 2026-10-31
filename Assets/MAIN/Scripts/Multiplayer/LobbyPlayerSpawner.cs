using Unity.Netcode;
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
            if (nm.ConnectedClients[clientId].PlayerObject != null) return;
            var sp = spawnPoints[next++ % spawnPoints.Length];
            Instantiate(playerPrefab, sp.position, sp.rotation).SpawnAsPlayerObject(clientId);
        }
    }
}