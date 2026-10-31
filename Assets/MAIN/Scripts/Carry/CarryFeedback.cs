using Citlali.Audio;
using Citlali.Common;
using UnityEngine;

namespace Citlali.Carry
{
    public class CarryFeedback : MonoBehaviour
    {
        [Header("Damage tint")]
        [SerializeField] Color damagedColor = new(0.8f, 0.15f, 0.1f);
        [SerializeField, Range(0f, 1f)] float maxTint = 0.6f;       // tint at 0 HP (before the pulse)
        [SerializeField] Color flashColor = Color.white;
        [SerializeField] float flashTime = 0.15f;

        [Header("Low health warning")]
        [SerializeField, Range(0f, 1f)] float lowThreshold = 0.3f;
        [SerializeField] float pulseSpeed = 8f;
        [SerializeField] AudioClip warningClip;                      // optional, for the audio pass
        [SerializeField] float warningInterval = 1f;

        [Header("Optional")]
        [SerializeField] Transform visual;                           // child to squash on hit; leave empty if none
        [SerializeField] GameObject hitParticlesPrefab;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        Health health;
        Renderer[] rends;
        Color[] baseColors;
        MaterialPropertyBlock mpb;
        AudioSource audioSrc;
        Vector3 visualScale = Vector3.one;
        float flash, squash, nextWarn;

        void Awake()
        {
            health = GetComponent<Health>();
            mpb = new MaterialPropertyBlock();

            var all = GetComponentsInChildren<Renderer>();
            var list = new System.Collections.Generic.List<Renderer>();
            foreach (var r in all)
                if (r is MeshRenderer || r is SkinnedMeshRenderer) list.Add(r);
            rends = list.ToArray();

            baseColors = new Color[rends.Length];
            for (int i = 0; i < rends.Length; i++)
            {
                var m = rends[i].sharedMaterial;
                baseColors[i] = m.HasProperty(BaseColorId) ? m.GetColor(BaseColorId)
                              : m.HasProperty(ColorId) ? m.GetColor(ColorId) : Color.white;
            }

            if (visual) visualScale = visual.localScale;
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 1f;
        }

        void Start() => health.Hp.OnValueChanged += OnHpChanged;

        void OnDestroy()
        {
            if (health != null) health.Hp.OnValueChanged -= OnHpChanged;
        }

        void OnHpChanged(float previous, float next)
        {
            if (next >= previous) return;
            flash = 1f;
            squash = 1f;
            if (hitParticlesPrefab != null)
                Destroy(Instantiate(hitParticlesPrefab, transform.position, Quaternion.identity), 2f);

            if (next <= 0f) Sfx.Play(SfxId.CarryBreak, transform.position);
            else Sfx.Play(SfxId.CarryHit, transform.position, Mathf.Clamp01(0.4f + (previous - next) / 20f));
        }

        void Update()
        {
            flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime / flashTime);
            squash = Mathf.MoveTowards(squash, 0f, Time.deltaTime * 6f);

            float frac = health.MaxHp > 0f ? Mathf.Clamp01(health.Hp.Value / health.MaxHp) : 1f;
            float tint = (1f - frac) * maxTint;

            if (frac > 0f && frac <= lowThreshold)
            {
                tint += (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * 0.4f;   // red throb
                if (Time.time >= nextWarn)
                {
                    Sfx.PlayUI(SfxId.CarryWarning);   // 2D on purpose, so the whole team hears the alert
                    nextWarn = Time.time + warningInterval;
                }
            }
            tint = Mathf.Clamp01(tint);

            for (int i = 0; i < rends.Length; i++)
            {
                Color c = Color.Lerp(baseColors[i], damagedColor, tint);
                c = Color.Lerp(c, flashColor, flash);
                rends[i].GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, c);
                mpb.SetColor(ColorId, c);
                rends[i].SetPropertyBlock(mpb);
            }

            if (visual)
                visual.localScale = Vector3.Scale(visualScale,
                    new Vector3(1f + 0.12f * squash, 1f - 0.10f * squash, 1f + 0.12f * squash));
        }
    }
}