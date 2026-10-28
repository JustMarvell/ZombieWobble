using Citlali.Game;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.UI
{
    public class GameFlowUI : MonoBehaviour
    {
        void OnGUI()
        {
            var nm = NetworkManager.Singleton;
            var gm = GameManager.Instance;
            if (gm == null || !nm.IsListening) return;

            var state = gm.State.Value;
            GUI.Label(new Rect(20, 180, 300, 25), $"State: {state}");

            if (state == GameState.Won || state == GameState.Lost)
            {
                var big = new GUIStyle(GUI.skin.label) { fontSize = 48, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(0, Screen.height * 0.3f, Screen.width, 80), state == GameState.Won ? "YOU WIN" : "YOU LOST", big);
            }

            if (!nm.IsServer) return;
            bool canStart = state == GameState.Lobby || state == GameState.Won || state == GameState.Lost;
            string label = state == GameState.Lobby ? "Start" : "Restart";
            if (canStart && GUI.Button(new Rect(20, 210, 160, 30), label)) gm.StartRound();
        }
    }
}