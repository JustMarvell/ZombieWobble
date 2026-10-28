using Citlali.Carry;
using Citlali.Common;
using UnityEngine;

namespace Citlali.UI
{
    public class ObjectHealthBar : MonoBehaviour
    {
        void OnGUI()
        {
            if (CarryObject.All.Count == 0) return;
            var health = CarryObject.All[0].GetComponent<Health>();
            float pct = health.Hp.Value / health.MaxHp;

            var r = new Rect(Screen.width / 2f - 150f, 20f, 300f, 22f);
            GUI.Box(r, GUIContent.none);
            GUI.color = Color.Lerp(Color.red, Color.green, pct);
            GUI.DrawTexture(new Rect(r.x + 2f, r.y + 2f, (r.width - 4f) * pct, r.height - 4f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(r, health.IsDead ? "  OBJECT BROKEN" : $"  OBJECT {health.Hp.Value:F0} / {health.MaxHp:F0}");
        }
    }
}