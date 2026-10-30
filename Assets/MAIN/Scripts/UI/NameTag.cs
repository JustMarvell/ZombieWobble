using Citlali.Player;
using TMPro;
using UnityEngine;

namespace Citlali.UI
{
    public class NameTag : MonoBehaviour
    {
        [SerializeField] PlayerIdentity identity;
        [SerializeField] PlayerLife life;
        [SerializeField] TextMeshPro label;
        [SerializeField] bool hideOwnTag = true;

        Transform cam;
        float nextRefresh;

        void LateUpdate()
        {
            if (cam == null)
            {
                if (Camera.main == null) return;
                cam = Camera.main.transform;
            }
            transform.rotation = cam.rotation;
            label.enabled = !(hideOwnTag && identity.IsOwner);

            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;

            string text = identity.PlayerName.Value.ToString();
            if (!life.IsDowned) text += $"\nDOWNED {life.BleedOut.Value}s";
            else if (life.State.Value == PlayerState.Dead) text += $"\nDEAD";
            label.text = text;
            label.color = identity.PlayerColor;
        }
    }
}