using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Citlali
{
    public class ConnectionUI : MonoBehaviour
    {
        string ip = "127.0.0.1";

        void Start()
        {
            var nm = NetworkManager.Singleton;
            nm.NetworkConfig.ConnectionApproval = true;
            nm.ConnectionApprovalCallback = (req, res) =>
            {
                res.Approved = nm.ConnectedClientsIds.Count < GameConfig.MaxPlayer;
                res.CreatePlayerObject = true;
                res.Pending = false;
            };
        }

        void OnGUI()
        {
            var nm = NetworkManager.Singleton;
            if (nm.IsClient || nm.IsServer) return;

            var utp = nm.GetComponent<UnityTransport>();
            GUILayout.BeginArea(new Rect(20, 20, 220, 150));
            ip = GUILayout.TextField(ip);
            if (GUILayout.Button("Host"))
            {
                utp.SetConnectionData("127.0.0.1", GameConfig.Port, "0.0.0.0");
                nm.StartHost();
            }
            if (GUILayout.Button("Join"))
            {
                utp.SetConnectionData(ip, GameConfig.Port);
                nm.StartClient();
            }
            GUILayout.EndArea();
        }
    }
}