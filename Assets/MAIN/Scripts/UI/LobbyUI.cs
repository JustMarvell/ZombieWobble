using Citlali.Config;
using Citlali.Networking;
using Citlali.Player;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Citlali.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] TMP_Text codeText, countText, hintText;
        [SerializeField] Button copyButton, startButton, leaveButton;

        void Start()
        {
            var nm = NetworkManager.Singleton;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            codeText.text = SessionManager.Instance.JoinCode;
            copyButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = SessionManager.Instance.JoinCode);
            startButton.gameObject.SetActive(nm.IsHost);
            hintText.gameObject.SetActive(!nm.IsHost);
            startButton.onClick.AddListener(OnStart);
            leaveButton.onClick.AddListener(OnLeave);
            InvokeRepeating(nameof(RefreshCount), 0f, 0.5f);
        }

        void RefreshCount()
        {
            int n = FindObjectsByType<PlayerLife>(FindObjectsSortMode.None).Length;
            countText.text = $"Players {n}/{GameConfig.MaxPlayer}";
        }

        async void OnStart()
        {
            startButton.interactable = false;
            await SessionManager.Instance.SetLocked(true);
            NetworkManager.Singleton.SceneManager.LoadScene(GameConfig.GameScene, LoadSceneMode.Single);
        }

        async void OnLeave()
        {
            await SessionManager.Instance.Leave();
            if (NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(GameConfig.MenuScene);
        }
    }
}