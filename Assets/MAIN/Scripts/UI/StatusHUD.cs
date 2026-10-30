using System.Text;
using Citlali.Carry;
using Citlali.Common;
using Citlali.Player;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Citlali.UI
{
    public class StatusHud : MonoBehaviour
    {
        [Header("Object")]
        [SerializeField] Image objectFill;          // Image Type: Filled, Horizontal
        [SerializeField] TMP_Text objectText;
        [SerializeField] GameObject paddingBadge;
        [SerializeField] TMP_Text paddingText;

        [Header("You")]
        [SerializeField] Image hpFill;              // Filled, Horizontal
        [SerializeField] TMP_Text stateText;
        [SerializeField] GameObject medkitIcon;
        [SerializeField] Image useFill;             // Filled (bar or radial)
        [SerializeField] GameObject glovesBadge;
        [SerializeField] TMP_Text glovesText;
        [SerializeField] GameObject bootsBadge;
        [SerializeField] TMP_Text bootsText;

        [Header("Team")]
        [SerializeField] TMP_Text teamText;

        PlayerLife[] players = new PlayerLife[0];
        float nextTeamRefresh;
        readonly StringBuilder sb = new();

        void Update()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening) return;

            UpdateObject();
            UpdateLocalPlayer(nm);

            if (Time.unscaledTime >= nextTeamRefresh)
            {
                nextTeamRefresh = Time.unscaledTime + 0.25f;
                players = FindObjectsByType<PlayerLife>(FindObjectsSortMode.None);
                UpdateTeam();
            }
        }

        void UpdateObject()
        {
            if (CarryObject.All.Count == 0) return;
            var h = CarryObject.All[0].GetComponent<Health>();
            objectFill.fillAmount = h.Hp.Value / h.MaxHp;
            objectText.text = h.IsDead ? "BROKEN" : $"{h.Hp.Value:F0}";
            int pad = h.PaddingSecs.Value;
            paddingBadge.SetActive(pad > 0);
            paddingText.text = pad + "s";
        }

        void UpdateLocalPlayer(NetworkManager nm)
        {
            var local = nm.LocalClient?.PlayerObject;
            if (local == null) return;

            var life = local.GetComponent<PlayerLife>();
            var hp = local.GetComponent<Health>();
            var inv = local.GetComponent<PlayerInventory>();
            var buffs = local.GetComponent<PlayerBuffs>();

            hpFill.fillAmount = hp.Hp.Value / hp.MaxHp;
            stateText.text = life.State.Value switch
            {
                PlayerState.Downed => $"DOWNED {life.BleedOut.Value}s",
                PlayerState.Dead => "DEAD",
                _ => ""
            };
            medkitIcon.SetActive(inv.HasMedkit.Value);
            useFill.gameObject.SetActive(inv.UseProgress.Value > 0f);
            useFill.fillAmount = inv.UseProgress.Value;

            glovesBadge.SetActive(buffs.GlovesSecs.Value > 0);
            glovesText.text = buffs.GlovesSecs.Value + "s";
            bootsBadge.SetActive(buffs.JumpSecs.Value > 0);
            bootsText.text = buffs.JumpSecs.Value + "s";
        }

        void UpdateTeam()
        {
            sb.Clear();
            foreach (var p in players)
            {
                if (p == null || p.IsOwner) continue;
                var h = p.GetComponent<Health>();
                var id = p.GetComponent<PlayerIdentity>();
                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(id.PlayerColor)}>{id.PlayerName.Value}</color>  HP {h.Hp.Value:F0}");
                if (p.IsDowned) sb.Append($"  DOWNED {p.BleedOut.Value}s");
                else if (p.State.Value == PlayerState.Dead) sb.Append("  DEAD");
                if (p.GetComponent<PlayerInventory>().HasMedkit.Value) sb.Append("  +medkit");
                sb.AppendLine();
            }
            teamText.text = sb.ToString();
        }
    }
}
