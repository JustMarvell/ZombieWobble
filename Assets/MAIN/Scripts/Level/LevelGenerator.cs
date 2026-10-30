using System.Collections.Generic;
using UnityEngine;

namespace Citlali.Level
{
    public struct SeededRandom
    {
        uint state;

        public SeededRandom(int seed)
        {
            state = unchecked((uint)seed * 2654435761u + 0x9E3779B9u);
            if (state == 0) state = 1;
        }

        public int Next(int max)   // 0 to max-1, same result on every machine
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (int)(state % (uint)max);
        }
    }

    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] LevelChunk startChunk;
        [SerializeField] LevelChunk[] middleChunks;
        [SerializeField] LevelChunk finishChunk;
        [SerializeField, Min(0)] int middleCount = 3;

        public List<Transform> PlayerSpawns { get; } = new();
        public Transform CarrySpawn { get; private set; }
        public List<Transform> MedkitSpawns { get; } = new();
        public List<Transform> PowerUpSpawns { get; } = new();
        public List<Transform> EnemySpawns { get; } = new();

        GameObject root;

        public void Build(int seed)
        {
            if (root != null) Destroy(root);
            root = new GameObject("Level");

            PlayerSpawns.Clear(); MedkitSpawns.Clear(); PowerUpSpawns.Clear(); EnemySpawns.Clear();
            CarrySpawn = null;

            var rng = new SeededRandom(seed);
            LevelChunk previous = Place(startChunk, null);

            int last = -1;
            for (int i = 0; i < middleCount; i++)
            {
                int index = (last < 0 || middleChunks.Length == 1)
                    ? rng.Next(middleChunks.Length)
                    : (last + 1 + rng.Next(middleChunks.Length - 1)) % middleChunks.Length;   // never the same twice in a row
                last = index;
                previous = Place(middleChunks[index], previous);
            }

            Place(finishChunk, previous);
            Physics.SyncTransforms();   // colliders are in place immediately
        }

        LevelChunk Place(LevelChunk prefab, LevelChunk previous)
        {
            var chunk = Instantiate(prefab, root.transform);
            chunk.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (previous != null)
            {
                // line up this chunk's entry with the previous chunk's exit
                Quaternion rot = previous.exit.rotation * Quaternion.Inverse(chunk.entry.rotation);
                Vector3 pos = previous.exit.position - rot * chunk.entry.position;
                chunk.transform.SetPositionAndRotation(pos, rot);
            }

            PlayerSpawns.AddRange(chunk.playerSpawns);
            MedkitSpawns.AddRange(chunk.medkitSpawns);
            PowerUpSpawns.AddRange(chunk.powerUpSpawns);
            EnemySpawns.AddRange(chunk.enemySpawns);
            if (chunk.carrySpawn != null) CarrySpawn = chunk.carrySpawn;
            return chunk;
        }
    }
}
