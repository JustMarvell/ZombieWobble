using Citlali.Config;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Citlali.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer I { get; private set; }

        [Header("Menu + Lobby")]
        [SerializeField] AudioClip menuMusic;
        [SerializeField, Range(0f, 1f)] float menuVolume = 0.5f;

        [Header("Game")]
        [SerializeField] AudioClip gameMusic;
        [SerializeField, Range(0f, 1f)] float gameVolume = 0.5f;

        [SerializeField] float fadeTime = 1.5f;

        readonly AudioSource[] src = new AudioSource[2];
        readonly float[] level = new float[2];        // current fade 0..1
        readonly float[] fadeTarget = new float[2];
        readonly float[] trackVol = new float[2];
        int active = -1;
        float duck = 1f, duckTarget = 1f;

        void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;

            for (int i = 0; i < 2; i++)
            {
                src[i] = gameObject.AddComponent<AudioSource>();
                src[i].loop = true;
                src[i].playOnAwake = false;
                src[i].spatialBlend = 0f;
                src[i].volume = 0f;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (I == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            duckTarget = 1f;
            if (scene.name == GameConfig.MenuScene || scene.name == GameConfig.LobbyScene)
                Play(menuMusic, menuVolume);
            else if (scene.name == GameConfig.GameScene)
                Play(gameMusic, gameVolume);
        }

        public void Play(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                for (int i = 0; i < 2; i++) fadeTarget[i] = 0f;
                return;
            }

            // same track already playing: keep it going (menu -> lobby)
            if (active >= 0 && src[active].clip == clip) { trackVol[active] = volume; fadeTarget[active] = 1f; return; }

            int next = active == 0 ? 1 : 0;
            src[next].clip = clip;
            src[next].time = 0f;
            src[next].Play();
            trackVol[next] = volume;
            fadeTarget[next] = 1f;
            if (active >= 0) fadeTarget[active] = 0f;
            active = next;
        }

        // 1 = normal, lower = quieter music (used for the result screen)
        public static void Duck(float scale)
        {
            if (I != null) I.duckTarget = scale;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            duck = Mathf.MoveTowards(duck, duckTarget, dt * 2f);

            for (int i = 0; i < 2; i++)
            {
                level[i] = Mathf.MoveTowards(level[i], fadeTarget[i], dt / fadeTime);
                src[i].volume = level[i] * trackVol[i] * duck;
                if (level[i] <= 0f && fadeTarget[i] <= 0f && src[i].isPlaying) src[i].Stop();
            }
        }
    }
}