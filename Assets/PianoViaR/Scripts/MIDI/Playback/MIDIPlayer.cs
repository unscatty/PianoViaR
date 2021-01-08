using AudioSynthesis.Sequencer;
using AudioSynthesis.Bank;
using AudioSynthesis.Synthesis;
using System;
using AudioSynthesis.Midi;

namespace PianoViaR.MIDI.Playback
{
    public enum PlayBackStatus
    {
        STOPPED,
        PLAYING,
        PAUSED
    }

    public class MIDIPlayer
    {
        public const int channel = 2;
        public const int MinSampleRate = 8000;
        public const int MaxSampleRate = 96000;
        const int sampleRate = 44100;
        const int bufferSize = 1024;
        // 0 - 127
        private int volume = 127;
        public int Volume
        {
            get { return volume; }
            set
            {
                if (value >= 0 && value <= 127)
                {
                    volume = value;
                }
            }
        }
        public float PlaySpeed
        {
            get { return (float)sequencer.PlaySpeed; }
            set { sequencer.PlaySpeed = value; }
        }

        // Used for audio synthesis
        int bufferHead;
        float[] currentBuffer;
        Synthesizer synthesizer;
        MidiFileSequencer sequencer;
        public MidiFileSequencer Sequencer { get { return sequencer; } }
        public PatchBank Bank { get { return synthesizer.SoundBank; } }

        public Synthesizer Synth
        {
            get { return sequencer.Synth; }
            set { sequencer.Synth = value; }
        }

        PlayBackStatus status;

        public MIDIPlayer(Synthesizer synthesizer)
        {
            this.synthesizer = synthesizer;
            sequencer = new MidiFileSequencer(synthesizer);

            status = PlayBackStatus.STOPPED;
        }

        public MIDIPlayer()
        : this(new Synthesizer(sampleRate, channel, bufferSize, 1))
        { }

        public void LoadBank(PatchBank bank)
        {
            synthesizer.UnloadBank();
            synthesizer.LoadBank(bank);
        }

        public void LoadBank(string bankPath)
        {
            LoadBank(new PatchBank(bankPath));
        }

        public void LoadMidi(MidiFile midiFile)
        {
            Stop();

            sequencer.UnloadMidi();
            sequencer.LoadMidi(midiFile);
        }

        public void LoadMidi(string midiPath)
        {
            LoadMidi(new MidiFile(midiPath));
        }

        public void Play()
        {
            if (status != PlayBackStatus.PLAYING)
            {
                sequencer.Play();
                status = PlayBackStatus.PLAYING;
            }
        }

        public void Pause()
        {
            if (status == PlayBackStatus.PLAYING)
            {
                sequencer.Pause();
                status = PlayBackStatus.PAUSED;
            }
        }

        public void NoteOn(int note, int instrument)
        {
            synthesizer.NoteOn(channel, note, volume, instrument);
        }

        public void NoteOff(int note)
        {
            synthesizer.NoteOff(channel, note);
        }

        public void OnAudioFilterRead(float[] data, int channel)
        {
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
                var length = Math.Min(currentBuffer.Length - bufferHead, data.Length - count);
                System.Array.Copy(currentBuffer, bufferHead, data, count, length);
                bufferHead += length;
                count += length;
            }
        }

        public void Stop()
        {
            if (status != PlayBackStatus.STOPPED)
            {
                sequencer.Stop();
                status = PlayBackStatus.STOPPED;
            }
        }
    }
}