using System.Collections.Generic;
using UnityEngine;

namespace PianoViaR.Piano.Behaviours.Keys
{
    public class KeySourceSample : KeySource
    {
        AudioSource CurrentAudioSource;
        List<AudioSource> AudioSources;
        bool NoMultiAudioSource;
        GameObject AttachedGameObject;

        public KeySourceSample(AudioSource audioSource, bool noMultiAudioSource, GameObject gameObject)
        {
            AudioSources = new List<AudioSource>();
            AudioSources.Add(audioSource);
            CurrentAudioSource = AudioSources[0];

            NoMultiAudioSource = noMultiAudioSource;
            AttachedGameObject = gameObject;
            CurrentAudioSource.volume = 1;

            Initialize();
        }
        public override void Play()
        {
            if (!NoMultiAudioSource && CurrentAudioSource.isPlaying)
            {
                bool foundReplacement = false;
                int index = AudioSources.IndexOf(CurrentAudioSource);

                for (int i = 0; i < AudioSources.Count; i++)
                {
                    if (i != index && (!AudioSources[i].isPlaying || AudioSources[i].volume <= 0))
                    {
                        foundReplacement = true;
                        CurrentAudioSource = AudioSources[i];
                        RemoveFade(AudioSources[i]);
                        break;
                    }
                }

                if (!foundReplacement)
                {
                    AudioSource newAudioSource = CloneAudioSource();
                    AudioSources.Add(newAudioSource);
                    CurrentAudioSource = newAudioSource;
                }

                AddFade(AudioSources[index]);
            }

            CurrentAudioSource.Play();
        }

        private void Stop(AudioSource audioSource)
        {
            audioSource.Stop();
        }

        public override void Stop(dynamic source)
        {
            Stop(source);
        }

        public override void FadeList()
        {
            for (int i = 0; i < fadeList.Count; i++)
            {
                var current = fadeList[i];

                if (current.isPlaying)
                {
                    // Stop(current);
                    // RemoveFade(current);
                    current.volume -= Time.deltaTime * 2;

                    if (current.volume <= 0)
                    {
                        current.volume = 0;
                        Stop(current);
                        RemoveFade(current);
                        break;
                    }
                }
            }
        }

        public override void FadeAll()
        {
            base.FadeAll();

            foreach (var audioSource in AudioSources)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.volume -= 0.5f;

                    if (audioSource.volume <= 0)
                        audioSource.Stop();
                }
            }
        }

        AudioSource CloneAudioSource()
        {
            AudioSource newAudioSource = AttachedGameObject.AddComponent<AudioSource>();
            newAudioSource.volume = CurrentAudioSource.volume;
            newAudioSource.playOnAwake = CurrentAudioSource.playOnAwake;
            newAudioSource.spatialBlend = CurrentAudioSource.spatialBlend;
            newAudioSource.clip = CurrentAudioSource.clip;
            newAudioSource.outputAudioMixerGroup = CurrentAudioSource.outputAudioMixerGroup;

            return newAudioSource;
        }
    }
}