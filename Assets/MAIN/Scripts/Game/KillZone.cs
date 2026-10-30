using Citlali.Carry;
using Citlali.Common;
using Citlali.Enemies;
using Citlali.Player;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Game
{
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        public Transform playerRespawnPoint;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer) return;

            var body = other.attachedRigidbody;
            if (body == null) return;

            if (body.TryGetComponent(out PlayerLife life))
            {
                if (playerRespawnPoint != null) life.Respawn(playerRespawnPoint);
                else life.Kill();
            }
            else if (body.TryGetComponent(out CarryObject _) && body.TryGetComponent(out Health carryHealth)) carryHealth.Kill();
            else if (body.TryGetComponent(out EnemyAI _) && body.TryGetComponent(out Health enemyHealth)) enemyHealth.Kill();  
        }
    }
}