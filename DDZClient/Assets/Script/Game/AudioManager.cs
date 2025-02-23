using System.Collections;
using UnityEngine;

namespace Script.Game
{
    public class AudioManager : MonoBehaviour
    {
        // 单例
        public static AudioManager Instance { get; private set; }

        private AudioSource backgroundAudioSource;
        private AudioSource soundEffectAudioSource;

        private bool isPaused = false; // 标记背景音乐是否暂停

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            backgroundAudioSource = gameObject.AddComponent<AudioSource>();
            soundEffectAudioSource = gameObject.AddComponent<AudioSource>();

            backgroundAudioSource.loop = true; // 背景音乐循环播放
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="audioClipName"></param>
        public void PlayBackgroundMusic(string audioClipName)
        {
            StartCoroutine(LoadAndPlayMusic(audioClipName, true));
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="audioClipName"></param>
        public void PlaySoundEffect(string audioClipName)
        {
            StartCoroutine(LoadAndPlayMusic(audioClipName, false));
        }

        /// <summary>
        /// 加载并播放音频
        /// </summary>
        /// <param name="audioClipName"></param>
        /// <param name="isBackgroundMusic"></param>
        /// <returns></returns>
        private IEnumerator LoadAndPlayMusic(string audioClipName, bool isBackgroundMusic)
        {
            string path = "Audio/" + audioClipName;
            AudioClip clip = Resources.Load<AudioClip>(path);

            if (clip != null)
            {
                if (isBackgroundMusic)
                {
                    backgroundAudioSource.clip = clip;
                    backgroundAudioSource.Play();
                }
                else
                {
                    soundEffectAudioSource.clip = clip;
                    soundEffectAudioSource.Play();
                }
            }
            else
            {
                Debug.LogError("音频文件加载失败: " + path);
            }

            yield return null;
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void PauseBackgroundMusic()
        {
            if (!isPaused)
            {
                backgroundAudioSource.Pause();
                isPaused = true;
            }
        }
        
        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        public void ResumeBackgroundMusic()
        {
            if (isPaused)
            {
                backgroundAudioSource.UnPause();
                isPaused = false;
            }
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBackgroundMusic()
        {
            backgroundAudioSource.Stop();
        }
    }
}
