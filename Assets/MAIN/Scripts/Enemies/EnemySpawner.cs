using System.Collections.Generic;
using Citlali.Carry;
using Citlali.Game;
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

            if (EnemyAI.All.Count >= maxAlive || CarryObject.All.Count == 0) return;
            Vector3 center = CarryObject.All[0].transform.position;
            
            var candidates = new List<Transform>();
            foreach (var p in spawnPoints)
            {
                float d = Vector3.Distance(p.position, center);
                if (d >= minDistance && d <= maxDistance) candidates.Add(p);
            }
            if (candidates.Count == 0) return;

            var sp = candidates[Random.Range(0, candidates.Count)];
            Instantiate(enemyPrefab, sp.position + Vector3.up, sp.rotation).Spawn();
        }
    }
}
