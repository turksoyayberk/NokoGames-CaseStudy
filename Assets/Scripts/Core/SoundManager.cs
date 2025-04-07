using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class SoundManager : MonoBehaviour
    {
        private AudioSource _audioSource;
        private readonly Dictionary<SoundType, AudioClip> _soundAudioClipDictionary = new();
        private readonly Dictionary<SoundType, float> _soundCooldownDictionary = new();

        private const float DefaultVolume = 0.5f;
        private const float SoundCooldownTime = 0.05f;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            InitializeAudioSource();
            LoadAllSounds();
        }

        private void InitializeAudioSource()
        {
            _audioSource = GetComponent<AudioSource>();

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.volume = DefaultVolume;
        }

        private void LoadAllSounds()
        {
            try
            {
                foreach (SoundType sound in Enum.GetValues(typeof(SoundType)))
                {
                    var clip = Resources.Load<AudioClip>(sound.ToString());
                    if (clip == null) continue;

                    clip.LoadAudioData();
                    _soundAudioClipDictionary[sound] = clip;
                    _soundCooldownDictionary[sound] = 0f;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"An error occurred while loading audio {e}");
            }
        }

        public void PlaySound(SoundType sound)
        {
            if (sound == SoundType.None) return;
            if (!CanPlaySound(sound)) return;

            _audioSource.PlayOneShot(_soundAudioClipDictionary[sound], DefaultVolume);
            _soundCooldownDictionary[sound] = Time.time;
        }

        public void PlaySoundWithVolume(SoundType sound, float volume)
        {
            if (sound == SoundType.None) return;
            if (!CanPlaySound(sound)) return;

            _audioSource.PlayOneShot(_soundAudioClipDictionary[sound], volume);
            _soundCooldownDictionary[sound] = Time.time;
        }


        private bool CanPlaySound(SoundType sound)
        {
            return Time.time >= _soundCooldownDictionary[sound] + SoundCooldownTime;
        }
    }

    public enum SoundType
    {
        None,
        Collect,
        Coins
    }
}