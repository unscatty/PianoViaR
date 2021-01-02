using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PianoViaR.Piano.Behaviours.Keys
{
    public class KeySourceSample : KeySource
    {
        const float DefaultSustain = 0.5f;
        AudioSource CurrentAudioSource;
        List<AudioSource> AudioSources;
        bool NoMultiAudioSource;
        GameObject AttachedGameObject;
        bool SustainPedalPressed = false;
        float SustainSeconds = DefaultSustain;

        public KeySourceSample(
            AudioSource audioSource,
            bool noMultiAudioSource,
            GameObject gameObject,
            bool sustainPedalPressed = false,
            float sustainSeconds = DefaultSustain
        )
        {
            AudioSources = new List<AudioSource>();
            AudioSources.Add(audioSource);
            CurrentAudioSource = AudioSources[0];

            NoMultiAudioSource = noMultiAudioSource;
            AttachedGameObject = gameObject;
            CurrentAudioSource.volume = 1;
            SustainPedalPressed = sustainPedalPressed;
            SustainSeconds = sustainSeconds;

            Initialize();
        }
        public override IEnumerator Play(YieldInstruction instruction)
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

            var startAngle = AttachedGameObject.transform.eulerAngles.x;

            yield return instruction;
            yield return instruction;

            if (Mathf.Abs(startAngle - AttachedGameObject.transform.eulerAngles.x) > 0)
            {
                CurrentAudioSource.volume = Mathf.Lerp(0, 1, Mathf.Clamp((Mathf.Abs(startAngle - AttachedGameObject.transform.eulerAngles.x) / 2f), 0, 1));
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

        public override IEnumerator FadeAll(YieldInstruction instruction)
        {
            // base.FadeAll();

            if (fadeList.Count > 0)
                fadeList.RemoveRange(0, fadeList.Count);

            foreach (var audioSource in AudioSources)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.volume -= Time.deltaTime / (SustainPedalPressed ? SustainSeconds : DefaultSustain);

                    if (audioSource.volume <= 0)
                        audioSource.Stop();
                }
            }

            yield return instruction;
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