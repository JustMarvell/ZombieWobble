using Citlali.Common;
using Citlali.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Citlali.Game
{
    public enum GameState { Lobby, Playing, Won, Lost }

    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] NetworkObject carryPrefab;
        [SerializeField] Transform carrySpawn;
        [SerializeField] Transform[] playerSpawns;
        
        public NetworkVariable<GameState> State = new NetworkVariable<GameState>(GameState.Lobby);
        public NetworkVariable<int> LevelSeed = new();

        NetworkObject carry;
        Health carryHealth;

        void Awake()
        {
            Instance = this;
        }

        public void StartRound()
        {
            if (!IsServer) return;
            LevelSeed.Value = Random.Range(int.MinValue, int.MaxValue);
            ResetPlayers();
            ClearCarry();
            SpawnCarry();
            State.Value = GameState.Playing;
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
                var h = client.PlayerObject ? client.PlayerObject.GetComponent<Health>() : null;
                if (h != null && !h.IsDead) return; 
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
                p.GetComponent<Health>().ResetFull();

                var rb = p.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                var sp = playerSpawns[i++ % playerSpawns.Length];
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
            carry = Instantiate(carryPrefab, carrySpawn.position, carrySpawn.rotation);
            carry.Spawn();
            carryHealth = carry.GetComponent<Health>();
            carryHealth.Died += OnCarryDied;
        }
    }
}
