using PlazmaGames.Audio;
using PlazmaGames.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ColbyO.Untitled.Wildlife
{
    public class FlockAudio : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private List<AudioClip> _gooseClips;
        [SerializeField] private List<AudioClip> _duckClips;
        [SerializeField] private AudioClip _flyingOffClip;
        [SerializeField] private FowlSpecies _species;
        [SerializeField] private Vector2 _timeRange = new Vector2(1f, 8f);

        [Header("Pitch Settings")]
        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        [Header("Spatial Settings")]
        [Range(0, 1), SerializeField] private float _spatialBlend = 1.0f;
        [SerializeField] private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private float _maxDistance = 50f;

        [SerializeField] private AnimationCurve _customRolloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private IAudioMonoSystem _audioMonoSystem;

        private Coroutine _coroutine;

        private void OnEnable()
        {
            if (_gooseClips.Count > 0 || _duckClips.Count > 0)
            {
                _coroutine = StartCoroutine(PlayRandomFowlSounds());
            }
        }

        private void OnDisable()
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
        }

        private void Start()
        {
            _audioMonoSystem = GameManager.GetMonoSystem<IAudioMonoSystem>();
        }

        public void SetSpecies(FowlSpecies species) => _species = species;

        private IEnumerator PlayRandomFowlSounds()
        {
            while (true)
            {
                if (_audioMonoSystem == null)
                {
                    yield return null;
                    continue;
                }

                float waitTime = Random.Range(_timeRange.x, _timeRange.y);
                yield return new WaitForSeconds(waitTime);

                List<AudioClip> activeList = (_species == FowlSpecies.CanadaGoose) ? _gooseClips : _duckClips;

                if (activeList.Count > 0)
                {
                    AudioClip clipToPlay = activeList[Random.Range(0, activeList.Count)];
                    PlayOneShot(clipToPlay);
                }
            }
        }

        public void PlayTakeOffSound()
        {
            PlayOneShot(_flyingOffClip);

            List<AudioClip> activeList = (_species == FowlSpecies.CanadaGoose) ? _gooseClips : _duckClips;
            AudioClip clipToPlay = activeList[Random.Range(0, activeList.Count)];
            PlayOneShot(clipToPlay);
        }

        private void PlayOneShot(AudioClip clip)
        {
            GameObject tempGO = new GameObject($"{_species}Sound");
            tempGO.transform.parent = transform;
            tempGO.transform.position = transform.position;

            AudioSource source = tempGO.AddComponent<AudioSource>();

            source.volume = _audioMonoSystem.GetOverallVolume() * _audioMonoSystem.GetSfXVolume();
            source.clip = clip;
            source.pitch = Random.Range(_minPitch, _maxPitch);

            source.spatialBlend = _spatialBlend;
            source.minDistance = _minDistance;
            source.maxDistance = _maxDistance;
            source.rolloffMode = _rolloffMode;

            if (_rolloffMode == AudioRolloffMode.Custom)
            {
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _customRolloffCurve);
            }

            source.Play();

            Destroy(tempGO, clip.length / source.pitch);
        }
    }
}