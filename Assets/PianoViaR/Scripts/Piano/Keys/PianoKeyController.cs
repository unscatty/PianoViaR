using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using PianoViaR.MIDI.Playback;
using UnityEditor;
using AudioSynthesis.Bank;
using PianoViaR.Helpers;
using PianoViaR.MIDI.Helpers;

namespace PianoViaR.Piano.Behaviours.Keys
{
    public class PianoKeyController : MonoBehaviour
    {
        public enum KeyOption
        {
            MIDI, SAMPLE
        }
        private GameObject notesPlayerGO;
        [Header("References")]
        // public MidiNotesPlayer MidiPlayer;
        public Transform PianoKeysParent;
        public AudioClip[] NoteSamples;

        [Header("Properties")]
        public bool Sort = true;                // Sorts the Notes. If regex is not empty, it will use that to do the sorting.
        public bool NoMultiAudioSource;         // Will prevent duplicates if true, if you need to optimise. Multiple Audio sources are necessary to remove crackling.
        public float SustainSeconds = 0.5f;		// May want to reduce this if there's too many AudioSources being generated per key.
        public bool SustainPedalPressed = true;	// When enabled, keys will not stop playing immediately after release.
        public MIDIInstrument Instrument = MIDIInstrument.ACOUSTIC_GRAND_PIANO; // Instrument

        [Header("Note: Leave regex blank to sort alphabetically")]
        public string Regex;

        public UnityEngine.Object patchBank;

        private PianoKey[] pianoKeys;
        public KeyOption option;

        // This must be on Start
        void Start()
        {
            if (Sort)
            {
                Regex sortReg = new Regex(@Regex);
                NoteSamples = NoteSamples.OrderBy(note => sortReg.Match(note.name).Value).ToArray();
            }

            pianoKeys = PianoKeysParent.GetComponentsInChildren<PianoKey>();

            SetupKeys();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SetupKeys();
            }
        }

        public void SetupKeys()
        {
            UnSuscribe(pianoKeys);

            switch (option)
            {
                case KeyOption.MIDI:
                    SetupNotesMIDI(pianoKeys);
                    break;
                case KeyOption.SAMPLE:
                    SetupNotesSamples(pianoKeys);
                    break;
            }
        }

        public void SetupNotesSamples(PianoKey[] pianoKeys)
        {
            for (int i = 0, sampleIdx = 0, note = 21; i < pianoKeys.Length; i++, sampleIdx++, note++)
            {
                PianoKey pianoKey = pianoKeys[i];
                AudioSource keyAudioSource = pianoKey.GetComponent<AudioSource>();
                keyAudioSource.clip = NoteSamples[sampleIdx];

                var eventArgs = new PianoNoteEventArgs(note, Instrument.MIDINumber());
                var keySource = new KeySourceSample(
                    keyAudioSource,
                    NoMultiAudioSource,
                    pianoKey.GameObject,
                    SustainPedalPressed,
                    SustainSeconds
                );

                Suscribe(pianoKey);

                pianoKey.EventArgs = eventArgs;
                pianoKey.KeySource = keySource;
            }
        }

        public void SetupNotesMIDI(PianoKey[] pianoKeys)
        {
            MIDINotePlayer midiNotePlayer;

            if (notesPlayerGO == null)
            {
                notesPlayerGO = new GameObject("PianoKeysMIDIPlayer");
                midiNotePlayer = notesPlayerGO.AddComponent<MIDINotePlayer>();
            }
            else
            {
                midiNotePlayer = notesPlayerGO.GetComponent<MIDINotePlayer>();
            }

            // TODO: change this beacuse it must read directly from the path
            var bank = new PatchBank(AssetDatabase.GetAssetPath(patchBank));

            midiNotePlayer.LoadBank(bank);

            // Assign the corresponding midi note and instrument to every PianoKey child of PianoKeysParent
            for (int i = 0, note = 21; i < pianoKeys.Length; i++, note++)
            {
                PianoKey pianoKey = pianoKeys[i];
                var eventArgs = new PianoNoteEventArgs(note, Instrument.MIDINumber());
                var keySource = new KeySourceMIDI(eventArgs);

                keySource.NotePlayed += midiNotePlayer.PlayNote;
                keySource.NoteStopped += midiNotePlayer.StopNote;

                Suscribe(pianoKey);

                pianoKey.EventArgs = eventArgs;
                pianoKey.KeySource = keySource;
            }
        }

        private void Suscribe(PianoKey key)
        {
            key.KeyPressed += SayKeyPressed;
            key.KeyReleased += SayKeyReleased;
        }

        private void Suscribe(PianoKey[] keys)
        {
            foreach (var key in keys)
            {
                Suscribe(key);
            }
        }

        private void UnSuscribe(PianoKey key)
        {
            key.KeyPressed -= SayKeyPressed;
            key.KeyReleased -= SayKeyReleased;
        }

        private void UnSuscribe(PianoKey[] keys)
        {
            foreach (var key in keys)
            {
                UnSuscribe(key);
            }
        }

        void SayKeyPressed(object source, PianoNoteEventArgs args)
        {
            Debug.Log($"You pressed the key: {args.Note}");
        }

        void SayKeyReleased(object source, PianoNoteEventArgs args)
        {
            Debug.Log($"You released the key: {args.Note}");
        }
    }
}