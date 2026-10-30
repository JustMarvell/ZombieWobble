using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Citlali.Player
{
    public class PlayerIdentity : NetworkBehaviour
    {
        public static readonly Color[] Pallete =
        {
            new Color32(235, 68, 68, 255),    // red
            new Color32(64, 128, 242, 255),   // blue
            new Color32(250, 204, 51, 255),   // yellow
            new Color32(166, 89, 230, 255),   // purple
            new Color32(250, 140, 38, 255),   // orange
            new Color32(242, 115, 179, 255),  // pink
        };

        [SerializeField] Renderer[] renderers;
        [SerializeField] int materialIndex = -1;

        public NetworkVariable<FixedString64Bytes> PlayerName = new();
        public NetworkVariable<int> ColorIndex = new(-1);

        public Color PlayerColor => ColorIndex.Value >= 0 ? Pallete[ColorIndex.Value % Pallete.Length] : Color.white;

        public override void OnNetworkSpawn()
        {
            ColorIndex.OnValueChanged += OnColorChanged;
            ApplyColor();

            if (!IsServer)
            {
                ColorIndex.Value = FirstFreeColor();
                PlayerName.Value = $"Player {OwnerClientId + 1}";
            }

            if (IsOwner)
            {
                string saved = PlayerPrefs.GetString("PlayerName", "");
                if (!string.IsNullOrWhiteSpace(saved)) SubmitNameRpc(saved);
            }
        }

        public override void OnNetworkDespawn()
        {
            ColorIndex.OnValueChanged -= OnColorChanged;
        }

        void OnColorChanged(int previous, int next) => ApplyColor();

        int FirstFreeColor()
        {
            var used = new System.Collections.Generic.HashSet<int>();
            foreach (var other in FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None))
                if (other != this && other.ColorIndex.Value >= 0) used.Add(other.ColorIndex.Value);
            
            for (int i = 0; i < Pallete.Length; i++)
                if (!used.Contains(i)) return i;
            return (int)(OwnerClientId % (ulong)Pallete.Length);
        }

        [Rpc(SendTo.Server)]
        void SubmitNameRpc(string name)
        {
            name = name.Replace("<", "").Replace(">", "").Trim();
            if (name.Length == 0) return;
            if (name.Length > 14) name = name.Substring(0, 14);
            PlayerName.Value = name;
        }

        void ApplyColor()
        {
            if (ColorIndex.Value < 0) return;
            Color c = PlayerColor;
            var block = new MaterialPropertyBlock();

            foreach (var r in renderers)
            {
                if (materialIndex < 0) r.GetPropertyBlock(block); else r.GetPropertyBlock(block, materialIndex);
                block.SetColor("_BaseColor", c);    // URP lit
                block.SetColor("_Color", c);    // Built-in Standard
                if (materialIndex < 0) r.SetPropertyBlock(block); else r.SetPropertyBlock(block, materialIndex);
            }
        }
    }
}