using Citlali.Player;
using TMPro;
using UnityEngine;

namespace Citlali.UI
{
    public class SpectateLabel : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text text;

        void Update()
        {
            string n = PlayerCameraRig.SpectatingName;
            root.SetActive(n != null);
            if (n != null) text.text = $"Spectating: {n}\nClick to switch";
        }
    }
}