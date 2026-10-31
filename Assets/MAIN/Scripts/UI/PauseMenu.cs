using Citlali.Config;
using Citlali.Networking;
using Citlali.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Citlali.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Button resumeButton, leaveButton;
        [SerializeField] Slider volumeSlider;

        void Start()
        {
            resumeButton.onClick.AddListener(() => CursorController.Paused = false);
            leaveButton.onClick.AddListener(OnLeave);

            volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            volumeSlider.onValueChanged.AddListener(v =>
            {
                AudioListener.volume = v;
                PlayerPrefs.SetFloat("Volume", v);
            });
        }

        void Update()
        {
            panel.SetActive(CursorController.Paused);
        }

        async void OnLeave()
        {
            CursorController.Paused = false;
            await SessionManager.Instance.Leave();
            if (NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(GameConfig.MenuScene);
        }
    }
}
