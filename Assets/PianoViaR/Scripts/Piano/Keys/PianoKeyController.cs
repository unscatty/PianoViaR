using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PianoViaR.MIDI.Playback;
using PianoViaR.Piano.Behaviours;
using UnityEditor;
using AudioSynthesis.Bank;
using PianoViaR.Helpers;

public class PianoKeyController : MonoBehaviour
{
    [Header("References")]
    // public MidiNotesPlayer MidiPlayer;
    public Transform PianoKeysParent;
    public Transform SustainPedal;
    public AudioClip[] Notes;

    [Header("Properties")]
    public string StartKey = "A";           // If the first key is not "A", change it to the appropriate note.
    public int StartOctave;                 // Start Octave can be increased if the piano/keyboard is not full length. 
    public float PedalReleasedAngle;        // Local angle that a pedal is considered to be released, or off.
    public float PedalPressedAngle;         // Local angle that a pedal is considered to be pressed, or on.
    public float SustainSeconds = 5;        // May want to reduce this if there's too many AudioSources being generated per key.
    public float PressAngleThreshold = 355f;// Rate of keys being slowly released.
    public float PressAngleDecay = 1f;      // Rate of keys being slowly released.
    public bool Sort = true;                // Sorts the Notes. If regex is not empty, it will use that to do the sorting.
    public bool NoMultiAudioSource;         // Will prevent duplicates if true, if you need to optimise. Multiple Audio sources are necessary to remove crackling.


    [Header("Attributes")]
    public bool SustainPedalPressed = true; // When enabled, keys will not stop playing immediately after release.
    public bool KeyPressAngleDecay = true;  // When enabled, keys will slowly be released.
    public bool RepeatedKeyTeleport = true; // When enabled, during midi mode, a note played on a pressed key will force the rotation to reset.

    public int Instrument = 0; // Instrument;
    public MIDIPlayer MIDIPlayer;
    private float _sustainPedalLerp = 1;

    // Should be controlled via MidiPlayer
    // public KeyMode KeyMode;
    // // {
    // //     get
    // //     {
    // //         // if (MidiPlayer)
    // //         //     return MidiPlayer.KeyMode;
    // //         // else
    // //             return KeyMode.Physical;
    // //     }
    // // }

    [Header("Note: Leave regex blank to sort alphabetically")]
    public string Regex;

    public Dictionary<string, PianoKey> PianoNotes = new Dictionary<string, PianoKey>();

    private readonly string[] _keyIndex = new string[12] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    public UnityEngine.Object patchBank;

    // This must be on Start
    void Awake()
    {
        if (Sort)
        {
            Regex sortReg = new Regex(@Regex);
            Notes = Notes.OrderBy(note => sortReg.Match(note.name).Value).ToArray();
        }

        var midiPlayerGO = new GameObject("midiPlayer");
        var midiNotePlayer = midiPlayerGO.AddComponent<MIDINotePlayer>();
        var bank = new PatchBank(AssetDatabase.GetAssetPath(patchBank));

        midiNotePlayer.LoadBank(bank);

        // Assign the corresponding audio clip to every PianoKey child of PianoKeysParent
        for (int i = 0, note = 21; i < PianoKeysParent.childCount; i++)
        {
            PianoKey pianoKey = PianoKeysParent.GetChild(i).GetComponent<PianoKey>();
            if (pianoKey)
            {
                var eventArgs = new PianoNoteEventArgs(note, 0);
                var keySource = new KeySourceMIDI(eventArgs);

                keySource.NotePlayed += midiNotePlayer.PlayNote;
                keySource.NoteStopped += midiNotePlayer.StopNote;

                pianoKey.KeyPressed += SayKeyPressed;
                pianoKey.KeyReleased += SayKeyReleased;
                
                pianoKey.KeySource = keySource;
                pianoKey.EventArgs = eventArgs;

                note++;
            }

            // AudioSource keyAudioSource = PianoKeysParent.GetChild(i).GetComponent<AudioSource>();

            // if (keyAudioSource)
            // {
            //     PianoKey pianoKey = PianoKeysParent.GetChild(i).GetComponent<PianoKey>();

            //     keyAudioSource.clip = Notes[count];
            //     PianoNotes.Add(KeyString(count + Array.IndexOf(_keyIndex, StartKey)), pianoKey);
            //     pianoKey.PianoKeyController = this;

            //     count++;
            // }
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