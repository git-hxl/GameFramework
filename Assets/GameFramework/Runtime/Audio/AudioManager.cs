
using UnityEngine;

namespace GameFramework
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        private AudioSource audioSource;

        private float volume;

        protected override void OnDispose()
        {
            //throw new System.NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new System.NotImplementedException();
        }

        private void InitAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void SetVolume(float volume)
        {
            if (audioSource == null)
            {
                return;
            }
            audioSource.volume = volume;
        }

        public void PlayMusic(string path)
        {
            if (audioSource == null)
            {
                return;
            }

            AudioClip clip = ResourceManager.Instance.LoadAsset<AudioClip>(path);

            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }

        public void PlayClip(string path)
        {
            if (audioSource == null)
            {
                return;
            }

            AudioClip clip = ResourceManager.Instance.LoadAsset<AudioClip>(path);
            audioSource.PlayOneShot(clip, audioSource.volume);
        }

        public void PlayClip(string path, Vector3 position)
        {
            if (audioSource == null)
            {
                return;
            }

            AudioClip clip = ResourceManager.Instance.LoadAsset<AudioClip>(path);
            AudioSource.PlayClipAtPoint(clip, position, audioSource.volume);
        }
    }
}