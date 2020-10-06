using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AudioSynthesis.Sequencer;
using AudioSynthesis.Bank;
using AudioSynthesis.Midi;
using AudioSynthesis.Synthesis;

[System.Serializable]
public struct MidiNote
{
    public int value;
    public int instrument;
    public int channel;
}

public class MIDIPlayer : MonoBehaviour
{
    [SerializeField] UnityEngine.Object bankSource;

    [SerializeField] UnityEngine.Object midiSource;
    [SerializeField] bool loadOnAwake = true;
    [SerializeField] bool playOnAwake = true;
    [SerializeField] int channel = 1;
    [SerializeField] int sampleRate = 44100;
    [SerializeField] int bufferSize = 1024;

    public MidiNote[] midiNotes;

    // public int midiNote = 60;
    public int midiNoteVolume = 100;

    [Range(0, 127)] //From Piano to Gunshot
    public int midiNoteInstrument = 0;
    public int midiNoteChannel = 0;
    PatchBank bank;
    MidiFile midi;
    Synthesizer synthesizer;
    AudioSource audioSource;
    MidiFileSequencer sequencer;
    int bufferHead;
    float[] currentBuffer;

    public int NotesChannel = 2;

    public AudioSource AudioSource { get { return audioSource; } }

    public MidiFileSequencer Sequencer { get { return sequencer; } }

    public PatchBank Bank { get { return bank; } }

    public MidiFile MidiFile { get { return midi; } }

    public void Awake()
    {
        synthesizer = new Synthesizer(sampleRate, channel, bufferSize, 1);
        sequencer = new MidiFileSequencer(synthesizer);
        audioSource = GetComponent<AudioSource>();

        if (loadOnAwake)
        {
            LoadBank(new PatchBank(AssetDatabase.GetAssetPath(bankSource)));
            LoadMidi(new MidiFile(AssetDatabase.GetAssetPath(midiSource)));
        }

        if (playOnAwake)
        {
            Play();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // synthesizer.NoteOn(midiNotes[0].channel, midiNotes[0].value, midiNoteVolume, midiNotes[0].instrument);
            NoteOn(midiNotes[0].value, midiNotes[0].instrument);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            synthesizer.NoteOff(midiNotes[0].channel, midiNotes[0].value);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            synthesizer.NoteOn(midiNotes[1].channel, midiNotes[1].value, midiNoteVolume, midiNotes[1].instrument);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            synthesizer.NoteOff(midiNotes[1].channel, midiNotes[1].value);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (midiNotes[0].instrument < 127) midiNotes[0].instrument++;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (midiNotes[0].instrument > 0) midiNotes[0].instrument--;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Play();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            sequencer.Stop();
            audioSource.Stop();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            audioSource.Pause();
            sequencer.Pause();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            LoadMidi(new MidiFile(AssetDatabase.GetAssetPath(midiSource)));
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            LoadBank(new PatchBank(AssetDatabase.GetAssetPath(bankSource)));
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            sequencer.Stop();
            sequencer.UnloadMidi();
        }
    }

    public void LoadBank(PatchBank bank)
    {
        this.bank = bank;
        synthesizer.UnloadBank();
        synthesizer.LoadBank(bank);
    }

    public void LoadMidi(MidiFile midi)
    {
        this.midi = midi;
        sequencer.Stop();
        sequencer.UnloadMidi();
        sequencer.LoadMidi(midi);
    }

    public void Play()
    {
        if (!sequencer.IsPlaying)
        {
            sequencer.Play();
            audioSource.Play();
        }
    }

    public void NoteOn(int note, int instrument)
    {
        synthesizer.NoteOn(NotesChannel, note, midiNoteVolume, midiNoteInstrument);
        // Debug.Log($"Playing note: {note}");
    }

    public void NoteOff(int note)
    {
        synthesizer.NoteOff(NotesChannel, note);
        // Debug.Log($"Stopping note: {note}");
    }

    void OnAudioFilterRead(float[] data, int channel)
    {
        Debug.Assert(this.channel == channel);
        int count = 0;
        while (count < data.Length)
        {
            if (currentBuffer == null || bufferHead >= currentBuffer.Length)
            {
                sequencer.FillMidiEventQueue();
                // synthesizer.GetNext(currentBuffer);
                synthesizer.GetNext();
                currentBuffer = synthesizer.WorkingBuffer;
                bufferHead = 0;
            }
            var length = Mathf.Min(currentBuffer.Length - bufferHead, data.Length - count);
            System.Array.Copy(currentBuffer, bufferHead, data, count, length);
            bufferHead += length;
            count += length;
        }
    }
}