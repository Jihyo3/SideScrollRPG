using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 효과음 재생기. 클립이 없으면 조용히 아무 것도 하지 않으므로,
    /// 오디오 에셋을 나중에 붙여도 코드를 고칠 필요가 없다.
    /// Resources/Audio/ 아래에 hit_1~3, hurt_1~2, jump, coin, ui 를 넣으면 자동으로 잡힌다.
    /// </summary>
    public class Sfx : MonoBehaviour
    {
        static Sfx _instance;

        AudioSource _source;
        AudioClip[] _hit;
        AudioClip[] _hurt;
        AudioClip _jump, _coin, _ui;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[Sfx]");
            _instance = go.AddComponent<Sfx>();
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;

            _hit = LoadMany("Audio/hit_1", "Audio/hit_2", "Audio/hit_3");
            _hurt = LoadMany("Audio/hurt_1", "Audio/hurt_2");
            _jump = Resources.Load<AudioClip>("Audio/jump");
            _coin = Resources.Load<AudioClip>("Audio/coin");
            _ui = Resources.Load<AudioClip>("Audio/ui");
        }

        static AudioClip[] LoadMany(params string[] paths)
        {
            var list = new System.Collections.Generic.List<AudioClip>();
            foreach (var p in paths)
            {
                var clip = Resources.Load<AudioClip>(p);
                if (clip != null) list.Add(clip);
            }
            return list.ToArray();
        }

        void Play(AudioClip clip, float pitchJitter = 0.05f)
        {
            if (clip == null || _source == null) return;
            _source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            _source.PlayOneShot(clip);
        }

        void PlayRandom(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return;
            Play(clips[Random.Range(0, clips.Length)]);
        }

        public static void PlayHit() { if (_instance != null) _instance.PlayRandom(_instance._hit); }
        public static void PlayHurt() { if (_instance != null) _instance.PlayRandom(_instance._hurt); }
        public static void PlayJump() { if (_instance != null) _instance.Play(_instance._jump); }
        public static void PlayCoin() { if (_instance != null) _instance.Play(_instance._coin); }
        public static void PlayUI() { if (_instance != null) _instance.Play(_instance._ui, 0f); }
    }
}
