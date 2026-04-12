using PlazmaGames.Audio;
using PlazmaGames.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ColbyO.Untitled.Traffic
{

    [RequireComponent(typeof(AudioSource))]
    public class EngineSound : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;


        private void Awake()
        {
            if (!_audioSource) _audioSource = GetComponent<AudioSource>();
        }

        public void ToggleEngine(bool enabled)
        {
            if (enabled)
            {
                if (!_audioSource.isPlaying) _audioSource.Play();
            }
            else
            {
                if (_audioSource.isPlaying) _audioSource.Stop();
            }
        }
    }
}
