using System;
using AudioSynthesis.Bank;
using AudioSynthesis.Midi;
using PianoViaR.MIDI.Playback;

namespace PianoViaR.Helpers
{
    public class MIDIPlayerEventArgs : EventArgs
    {
        public MidiFile CurrentFile { get; set; }
        public PatchBank CurrentBank { get; set; }
        public PlayBackStatus Status { get; set; }
        public int Volume { get; set; }
        public float PlaySpeed { get; set; }
    }
}