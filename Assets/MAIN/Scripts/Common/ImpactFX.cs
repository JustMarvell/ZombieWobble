using Citlali.Audio;
using UnityEngine;

namespace Citlali.Common
{
    public class ImpactFX : MonoBehaviour
    {
        public static ImpactFX Instance { get; private set; }

        [SerializeField] GameObject hitParticlesPrefab;

        void Awake() => Instance = this;

        public void PlayWhoosh(Vector3 pos) => Sfx.Play(SfxId.Whoosh, pos);

        public void PlayHit(Vector3 pos, float strength)
        {
            Sfx.Play(SfxId.Hit, pos, Mathf.Clamp01(0.5f + 0.3f * strength));
            if (hitParticlesPrefab != null)
                Destroy(Instantiate(hitParticlesPrefab, pos, Quaternion.identity), 2f);
        }
    }
}
