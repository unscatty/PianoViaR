using System;
using PianoViaR.MIDI.Helpers;

namespace PianoViaR.Helpers
{
    public class PianoNoteEventArgs : EventArgs
    {
        public int Note { get; set; }
        public int Instrument { get; set; }

        public PianoNoteEventArgs(int note, int instrument)
        {
            Note = note;
            Instrument = instrument;
        }
        public PianoNotes PianoNote
        {
            get { return PianoNotesHelper.FromMIDINumber(Note); }
            set
            {
                Note = PianoNotesHelper.ToMIDINumber(value);
            }
        }
        public MIDIInstrument MIDIInstrument
        {
            get { return MIDIInstrumentHelper.FromMIDINumber(Instrument); }
            set
            {
                Instrument = MIDIInstrumentHelper.ToMIDINumber(value);
            }
        }
    }
}