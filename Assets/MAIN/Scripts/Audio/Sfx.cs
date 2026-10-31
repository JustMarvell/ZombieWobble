using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Citlali.Audio
{
    public enum SfxId
    {
        Footstep, Jump, Land, Whoosh, Hit, Grab, Release, Hurt, Knockdown, Downed, Revive, Death,
        MedkitPickup, MedkitUse, PowerUpPickup,
        CarryHit, CarryWarning, CarryBreak,
        EnemyGroan, EnemyAttack, EnemyHurt, EnemyDeath,
        CountdownTick, Go, Win, Lose, Fall,
        UiHover, UiClick
    }

    [Serializable]
    public class SfxEntry
    {
        public SfxId id;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 0.8f;
        public Vector2 pitch = new(0.9f, 1.1f);
    }

    public class Sfx : MonoBehaviour
    {
        public static Sfx I { get; private set; }

        [SerializeField] SfxEntry[] entries;
        [SerializeField] int voices = 16;

        readonly Dictionary<SfxId, SfxEntry> map = new();
        AudioSource[] pool;
        AudioSource ui;
        int next;

        void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;

            foreach (var e in entries) map[e.id] = e;

            pool = new AudioSource[voices];
            for (int i = 0; i < voices; i++)
            {
                var s = new GameObject("Sfx Voice").AddComponent<AudioSource>();
                s.transform.SetParent(transform);
                s.playOnAwake = false;
                s.spatialBlend = 1f;
                s.minDistance = 4f;
                s.maxDistance = 40f;
                pool[i] = s;
            }

            ui = new GameObject("Sfx UI").AddComponent<AudioSource>();
            ui.transform.SetParent(transform);
            ui.playOnAwake = false;
            ui.spatialBlend = 0f;
        }

        // 3D sound at a world position
        public static void Play(SfxId id, Vector3 pos, float volumeScale = 1f)
        {
            if (I == null) { Debug.LogWarning("Sfx: no instance"); return; }
            if (!I.TryGet(id, out var e)) { Debug.LogWarning($"Sfx: no clips for {id}"); return; }

            // if (I == null || !I.TryGet(id, out var e)) return;
            var s = I.pool[I.next];

            I.next = (I.next + 1) % I.pool.Length;
            s.transform.position = pos;
            s.clip = e.clips[Random.Range(0, e.clips.Length)];
            s.volume = (e.volume > 0f ? e.volume : 0.8f) * volumeScale;
            s.pitch = e.pitch.y > 0f ? Random.Range(e.pitch.x, e.pitch.y) : 1f;
            s.volume = e.volume * volumeScale;
            s.Play();
        }

        // 2D sound (UI, jingles)
        public static void PlayUI(SfxId id, float volumeScale = 1f)
        {
            if (I == null || !I.TryGet(id, out var e)) return;

            I.ui.pitch =  e.volume > 0f ? Random.Range(e.pitch.x, e.pitch.y) : 1f;
            I.ui.PlayOneShot(e.clips[Random.Range(0, e.clips.Length)], (e.volume > 0f ? e.volume : 0.8f) * volumeScale);
        }

        bool TryGet(SfxId id, out SfxEntry e)
        {
            e = null;
            return map.TryGetValue(id, out e) && e.clips != null && e.clips.Length > 0;
        }
    }
}