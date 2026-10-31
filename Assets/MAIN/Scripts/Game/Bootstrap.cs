using Citlali.Config;
using Citlali.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Citlali.Game
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] GameObject[] presistent;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            foreach (var go in presistent) DontDestroyOnLoad(go);
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
            SceneManager.LoadScene(GameConfig.MenuScene);
        }

        void OnDisconnected(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm.IsServer || clientId != nm.LocalClientId) return;
            _ = SessionManager.Instance.Leave();
            if (SceneManager.GetActiveScene().name != GameConfig.MenuScene)
                SceneManager.LoadScene(GameConfig.MenuScene);
        }
    }
}