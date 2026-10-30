using System.Collections.Generic;
using Citlali.Carry;
using Citlali.Game;
using Citlali.Level;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] NetworkObject enemyPrefab;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] float firstDelay = 10f;
        [SerializeField] Vector2 interval = new Vector2(6f, 10f);
        [SerializeField] int maxAlive = 4;
        [SerializeField] float minDistance = 15f;
        [SerializeField] float maxDistance = 45f;
        [SerializeField] LevelGenerator level;

        float timer;

        void Update()
        {
            var nm = NetworkManager.Singleton;
            var gm = GameManager.Instance;
            if (nm == null || !nm.IsServer || gm == null) return;

            if (gm.State.Value != GameState.Playing) { timer = firstDelay; return; }

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = Random.Range(interval.x, interval.y);

            if (EnemyAI.All.Count >= maxAlive || CarryObject.All.Count == 0)
            {
                Debug.Log($"Spawner skipped: alive {EnemyAI.All.Count}/{maxAlive}, carry objects {CarryObject.All.Count}");
                return;
            }
            Vector3 center = CarryObject.All[0].transform.position;

            var candidates = new List<Transform>();
            foreach (var p in level.EnemySpawns)
            {
                float d = Vector3.Distance(p.position, center);
                if (d >= minDistance && d <= maxDistance) candidates.Add(p);
            }
            if (candidates.Count == 0)
            {
                Debug.LogWarning($"Spawner: no spawn point between {minDistance} and {maxDistance} of the object at {center}");
                return;
            }

            var sp = candidates[Random.Range(0, candidates.Count)];
            Instantiate(enemyPrefab, sp.position + Vector3.up, sp.rotation).Spawn();
        }

        // void OnDrawGizmosSelected()
        // {
        //     Gizmos.color = Color.blue;
        //     foreach (var p in spawnPoints)
        //     {
        //         Gizmos.DrawWireSphere(p.position, minDistance);
        //     }

        //     Gizmos.color = Color.red;
        //     foreach (var p in spawnPoints)
        //     {
        //         Gizmos.DrawWireSphere(p.position, maxDistance);
        //     }
        // }
    }
}
