using Citlali.Game;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Citlali.UI
{
    public class GameHud : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject lobbyPanel;
        [SerializeField] GameObject hudPanel;
        [SerializeField] GameObject resultPanel;

        [Header("Lobby")]
        [SerializeField] Button startButton;
        [SerializeField] GameObject lobbyWaitingText;

        [Header("Countdown (PopAnimator on the same object as the label)")]
        [SerializeField] TMP_Text countdownLabel;
        [SerializeField] PopAnimator countdownPop;
        [SerializeField] float goShowTime = 0.8f;

        [Header("Result")]
        [SerializeField] TMP_Text resultTitle;
        [SerializeField] PopAnimator resultPop;
        [SerializeField] Button restartButton;
        [SerializeField] Button lobbyButton;
        [SerializeField] GameObject resultWaitingText;

        float hideCountdownAt;

        void Start()
        {
            var gm = GameManager.Instance;
            gm.State.OnValueChanged += OnStateChanged;
            gm.Countdown.OnValueChanged += OnCountdownChanged;
            startButton.onClick.AddListener(() => gm.StartRound());
            restartButton.onClick.AddListener(() => gm.StartRound());
            lobbyButton.onClick.AddListener(() => gm.ReturnToLobby());
            countdownLabel.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.State.OnValueChanged -= OnStateChanged;
            gm.Countdown.OnValueChanged -= OnCountdownChanged;
        }

        void Update()
        {
            var nm = NetworkManager.Singleton;
            bool online = nm != null && nm.IsListening;
            bool host = online && nm.IsServer;
            var s = GameManager.Instance.State.Value;

            lobbyPanel.SetActive(online && s == GameState.Lobby);
            hudPanel.SetActive(online && s != GameState.Lobby);
            resultPanel.SetActive(online && (s == GameState.Won || s == GameState.Lost));

            startButton.gameObject.SetActive(host);
            lobbyWaitingText.SetActive(!host);
            restartButton.gameObject.SetActive(host);
            lobbyButton.gameObject.SetActive(host);
            resultWaitingText.SetActive(!host);

            if (hideCountdownAt > 0f && Time.unscaledTime >= hideCountdownAt)
            {
                hideCountdownAt = 0f;
                countdownLabel.gameObject.SetActive(false);
            }
        }

        void OnStateChanged(GameState previous, GameState next)
        {
            if (previous == GameState.Countdown && next == GameState.Playing)
                ShowCountdown("GO!", goShowTime);

            if (next == GameState.Won || next == GameState.Lost)
            {
                resultTitle.text = next == GameState.Won ? "YOU WIN!" : "YOU LOST!";
                resultPanel.SetActive(true);   // must be active before the animation starts
                resultPop.Play();
            }
        }

        void OnCountdownChanged(int previous, int next)
        {
            if (next > 0) ShowCountdown(next.ToString(), 0f);
        }

        void ShowCountdown(string text, float hideAfter)
        {
            countdownLabel.text = text;
            countdownLabel.gameObject.SetActive(true);
            countdownPop.Play();
            hideCountdownAt = hideAfter > 0f ? Time.unscaledTime + hideAfter : 0f;
        }
    }
}