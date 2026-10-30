using System.Collections.Generic;
using System.Collections;
using Citlali.Common;
using Citlali.Enemies;
using Citlali.Items;
using Citlali.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Citlali.Level;

namespace Citlali.Game
{
    public enum GameState { Lobby, Countdown, Playing, Won, Lost }

    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] NetworkObject carryPrefab;
        [SerializeField] Transform carrySpawn;
        [SerializeField] Transform[] playerSpawns;
        [SerializeField] NetworkObject medkitPickupPrefab;
        [SerializeField] Transform[] medkitSpawns;
        [SerializeField] NetworkObject[] powerUpPrefabs;
        [SerializeField] Transform[] powerUpSpawns;
        [SerializeField] int powerUpCount = 3;
        [SerializeField] int countdownSeconds = 3;
        [SerializeField] LevelGenerator level;
        
        public NetworkVariable<GameState> State = new NetworkVariable<GameState>(GameState.Lobby);
        public NetworkVariable<int> LevelSeed = new();
        public NetworkVariable<int> Countdown = new();

        NetworkObject carry;
        Health carryHealth;

        void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            LevelSeed.OnValueChanged += OnSeedChanged;

            if (IsServer)
            {
                State.Value = GameState.Lobby;
                Countdown.Value = 0;
                LevelSeed.Value = Random.Range(1, int.MaxValue);
            }
            else
            {
                level.Build(LevelSeed.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            LevelSeed.OnValueChanged -= OnSeedChanged;
        }

        void OnSeedChanged(int previous, int next) => level.Build(next);

        public void StartRound()
        {
            if (!IsServer) return;
            StopAllCoroutines();
            LevelSeed.Value = Random.Range(int.MinValue, int.MaxValue);
            ResetPlayers();
            ClearCarry();
            ClearMedkits();
            ClearEnemies();
            ClearPowerUps();
            SpawnCarry();
            SpawnMedkits();
            SpawnPowerUps();

            StartCoroutine(CountdownRoutine());
        }

        public void ReportGoalReached()
        {
            if (IsServer && State.Value == GameState.Playing) State.Value = GameState.Won;
        }

        void OnCarryDied()
        {
            if (State.Value == GameState.Playing) State.Value = GameState.Lost;
        }

        void Update()
        {
            if (!IsServer || State.Value != GameState.Playing) return;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var life = client.PlayerObject ? client.PlayerObject.GetComponent<PlayerLife>() : null;
                if (life != null && life.IsAlive) return;
            }
            State.Value = GameState.Lost;
        }

        void ResetPlayers()
        {
            int i = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var p = client.PlayerObject;
                if (p == null) continue;
                
                p.GetComponent<PlayerGrab>().Release();
                p.GetComponent<PlayerLife>().ResetToAlive();
                p.GetComponent<PlayerBuffs>().ClearAll();

                var rb = p.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                var sp = level.PlayerSpawns[i++ % level.PlayerSpawns.Count];
                p.GetComponent<NetworkTransform>().Teleport(sp.position, sp.rotation, Vector3.one);
            }
        }

        void ClearCarry()
        {
            if (carryHealth != null) carryHealth.Died -= OnCarryDied;
            if (carry != null && carry.IsSpawned) carry.Despawn(true);
            carry = null;
            carryHealth = null;
        }

        void SpawnCarry()
        {
            carry = Instantiate(carryPrefab, level.CarrySpawn.position, level.CarrySpawn.rotation);
            carry.Spawn();
            carryHealth = carry.GetComponent<Health>();
            carryHealth.Died += OnCarryDied;
        }

        void ClearMedkits()
        {
            foreach (var m in new List<MedkitPickup>(MedkitPickup.All))
            {
                if (m.NetworkObject.IsSpawned) m.NetworkObject.Despawn(true);
            }
        }

        void ClearPowerUps()
        {
            foreach (var p in new List<PowerUpPickup>(PowerUpPickup.All))
                if (p.NetworkObject.IsSpawned) p.NetworkObject.Despawn(true);
        }

        void SpawnMedkits()
        {
            var pool = new List<Transform>(level.MedkitSpawns);
            int count = Mathf.Min(Random.Range(1, 3), pool.Count);
            for (int i = 0; i < count; i++)
            {
                int k = Random.Range(0, pool.Count);
                Instantiate(medkitPickupPrefab, pool[k].position, Quaternion.identity).Spawn();
                pool.RemoveAt(k);
            }
        }

        void SpawnPowerUps()
        {
            var pool = new List<Transform>(level.PowerUpSpawns);
            int count = Mathf.Min(powerUpCount, pool.Count);
            for (int i = 0; i < count; i++)
            {
                int k = Random.Range(0, pool.Count);
                var prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
                Instantiate(prefab, pool[k].position, Quaternion.identity).Spawn();
                pool.RemoveAt(k);
            }
        }

        void ClearEnemies()
        {
            foreach (var e in new List<EnemyAI>(EnemyAI.All))
                if (e.NetworkObject.IsSpawned) e.NetworkObject.Despawn(true);
        }

        IEnumerator CountdownRoutine()
        {
            State.Value = GameState.Countdown;
            for (int i = countdownSeconds; i > 0; i--)
            {
                Countdown.Value = i;
                yield return new WaitForSeconds(1f);
            }
            Countdown.Value = 0;
            State.Value = GameState.Playing;
        }
    }
}
