using System;
using Citlali.Config;
using Citlali.Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Citlali.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] TMP_InputField nameInput, codeInput;
        [SerializeField] Button hostButton, joinButton, quitButton;
        [SerializeField] TMP_Text statusText;
        [SerializeField] CanvasGroup panel;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            nameInput.text = PlayerPrefs.GetString("PlayerName", "");
            hostButton.onClick.AddListener(OnHost);
            joinButton.onClick.AddListener(OnJoin);
            quitButton.onClick.AddListener(Application.Quit);
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
        }

        void OnDisconnected(ulong _) => SetBusy(false, "Connection Lost.");

        void SetBusy(bool busy, string msg = "")
        {
            panel.interactable = !busy;
            statusText.text = msg;
        }

        void SaveName() => PlayerPrefs.SetString("PlayerName", nameInput.text.Trim());

        async void OnHost()
        {
            SaveName();
            SetBusy(true, "Creating Room");
            try
            {
                await SessionManager.Instance.Host();
                NetworkManager.Singleton.SceneManager.LoadScene(GameConfig.LobbyScene, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SetBusy(false, "Couldn't Create the Room");
            }
        }

        async void OnJoin()
        {
            var code = codeInput.text.Trim().ToUpper();
            if (code.Length == 0) { statusText.text = "Enter a Room Code."; return; }

            SaveName();
            SetBusy(true, "Joining...");
            try
            {
                await SessionManager.Instance.Join(code);
                statusText.text = "Joined, Loading Lobby....";
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SetBusy(false, "Couldn't join, Check the Code.");
            }
        }
    }
}
