using Citlali.Common;
using Citlali.Player;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.UI
{
    public class PlayerStatusUI : MonoBehaviour
    {
        void OnGUI()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening) return;

            float y = 260f;
            foreach (var life in FindObjectsByType<PlayerLife>(FindObjectsSortMode.None))
            {
                var h = life.GetComponent<Health>();
                var inv = life.GetComponent<PlayerInventory>();

                string s = $"P{life.OwnerClientId}{(life.IsOwner ? " (you)" : "")}  HP {h.Hp.Value:F0}  {life.State.Value}";
                if (life.IsDowned) s += $" {life.BleedOut.Value}s";
                if (inv.HasMedkit.Value) s += "  [medkit]";
                if (inv.UseProgress.Value > 0f) s += $"  using {inv.UseProgress.Value:P0}";

                GUI.Label(new Rect(20f, y, 500f, 22f), s);
                y += 22f;
            }
        }
    }
}