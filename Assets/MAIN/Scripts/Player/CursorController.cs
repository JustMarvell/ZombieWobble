using Citlali.Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Citlali.Player
{
    public class CursorController : MonoBehaviour
    {
        public static bool Paused { get; set; }

        public static bool GameplayActive => Cursor.lockState == CursorLockMode.Locked;

        bool paused;

        void Update()
        {
            var nm = NetworkManager.Singleton;
            var gm = GameManager.Instance;
            bool playing = nm != null && nm.IsListening && gm != null && gm.State.Value == GameState.Playing;

            var kb = Keyboard.current;
            if (playing && kb != null && (kb.escapeKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame))
                Paused = !Paused;
            if (!playing) Paused = false;

            bool wantLock = playing && !Paused;
            var target = wantLock ? CursorLockMode.Locked : CursorLockMode.None;
            if (Cursor.lockState != target) Cursor.lockState = target;
            Cursor.visible = !wantLock;
        }
    }
}