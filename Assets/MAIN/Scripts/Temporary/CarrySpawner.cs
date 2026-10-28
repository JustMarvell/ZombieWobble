using Unity.Netcode;
using UnityEngine;

namespace Citlali.Temporary
{
    public class CarrySpawner : MonoBehaviour
    {
        [SerializeField] NetworkObject carryPrefab;
        [SerializeField] Transform spawnPoint;

        void Start() => NetworkManager.Singleton.OnServerStarted += Spawn;

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnServerStarted -= Spawn;
        }

        void Spawn()
        {
            var obj = Instantiate(carryPrefab, spawnPoint.position, spawnPoint.rotation);
            obj.Spawn();
        }
    }
}