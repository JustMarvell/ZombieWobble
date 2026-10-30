using Citlali.Config;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Citlali.Networking
{
    public class ConnectionUI : MonoBehaviour
    {
        string code = "";
        string playerName;

        void Start()
        {
            PlayerPrefs.GetString("PlayerName", "");
        }

        void OnGUI()
        {
            var sm = SessionManager.Instance;
            if (sm == null) return;

            GUILayout.BeginArea(new Rect(Screen.width - 260f, 20f, 240f, 220f));

            if (sm.InSession)
            {
                GUILayout.Label($"Join code: {sm.JoinCode}");
                if (GUILayout.Button("Copy code")) GUIUtility.systemCopyBuffer = sm.JoinCode;
                if (GUILayout.Button("Leave")) sm.Leave();
            }
            else
            {
                GUI.enabled = !sm.IsBusy;
                GUILayout.Label("Name");
                playerName = GUILayout.TextField(playerName, 14);
                if (GUILayout.Button("Host")) { PlayerPrefs.SetString("PlayerName", playerName); sm.Host(); }
                code = GUILayout.TextField(code, 6).ToUpperInvariant();
                if (GUILayout.Button("Join")) sm.Join(code);
                GUI.enabled = true;
            }

            if (!string.IsNullOrEmpty(sm.Status)) GUILayout.Label(sm.Status);
            GUILayout.EndArea();
        }
    }
}