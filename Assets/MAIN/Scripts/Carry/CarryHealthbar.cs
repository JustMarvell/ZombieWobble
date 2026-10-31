using Citlali.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Citlali.Carry
{
    public class CarryHealthBar : MonoBehaviour
    {
        [SerializeField] Health health;          // leave empty to auto-find in parents
        [SerializeField] Image fill;             // Image Type: Filled, Horizontal
        [SerializeField] Color lowColor = new(0.9f, 0.2f, 0.15f);
        [SerializeField] Color highColor = new(0.3f, 0.9f, 0.35f);
        [SerializeField] float height = 1.5f;    // above the object's centre, in meters
        [SerializeField] float smooth = 8f;

        Transform target;
        float shown = 1f;

        void Awake()
        {
            if (health == null) health = GetComponentInParent<Health>();
            target = health.transform;
        }

        void LateUpdate()
        {
            float frac = health.MaxHp > 0f ? Mathf.Clamp01(health.Hp.Value / health.MaxHp) : 1f;
            shown = Mathf.Lerp(shown, frac, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            fill.fillAmount = shown;
            fill.color = Color.Lerp(lowColor, highColor, shown);

            // stay above the object and face the camera, even if the object tumbles
            transform.position = target.position + Vector3.up * height;
            if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
        }
    }
}