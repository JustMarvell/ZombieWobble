using Citlali.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Citlali.UI
{
    public class UiSounds : MonoBehaviour
    {
        void Start()
        {
            foreach (var b in GetComponentsInChildren<Button>(true))
            {
                b.onClick.AddListener(() => Sfx.PlayUI(SfxId.UiClick));
                if (!b.TryGetComponent<ButtonHoverSfx>(out _))
                    b.gameObject.AddComponent<ButtonHoverSfx>();
            }
        }
    }

    public class ButtonHoverSfx : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData e)
        {
            var b = GetComponent<Button>();
            if (b != null && b.interactable) Sfx.PlayUI(SfxId.UiHover);
        }
    }
}