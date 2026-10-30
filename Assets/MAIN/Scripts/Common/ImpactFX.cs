using UnityEngine;

namespace Citlali.Common
{
    public class ImpactFX : MonoBehaviour
    {
        public static ImpactFX Instance { get; private set; }

        [SerializeField] AudioClip[] whooshClips;
        [SerializeField] AudioClip[] hitClips;
        [SerializeField] GameObject hitParticlesPrefab;
        [SerializeField, Range(0f, 1f)] float volume = .8f;
        [SerializeField] int voices = 8;

        AudioSource[] pool;
        int next;

        void Awake()
        {
            Instance = this;
            pool = new AudioSource[voices];
            for (int i = 0; i < voices; i++)
            {
                var go = new GameObject("FX Voice");
                go.transform.SetParent(transform);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 1f;
                s.minDistance = 4f;
                s.maxDistance = 40f;
                pool[i] = s;
            }
        }

        public void PlayWhoosh(Vector3 pos) => Play(whooshClips, pos, 0.5f);

        public void PlayHit(Vector3 pos, float strength)
        {
            Play(hitClips, pos, Mathf.Clamp01(0.5f + 0.3f * strength));
            if (hitParticlesPrefab != null)
                Destroy(Instantiate(hitParticlesPrefab, pos, Quaternion.identity), 2f);
        }

        void Play(AudioClip[] clips, Vector3 pos, float vol)
        {
            if (clips == null || clips.Length == 0) return;
            var s = pool[next];
            next = (next + 1) % pool.Length;
            s.transform.position = pos;
            s.pitch = Random.Range(0.9f, 1.1f);
            s.PlayOneShot(clips[Random.Range(0, clips.Length)], vol * volume);
        }
    }
}
