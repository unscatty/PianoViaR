using System;
using PianoViaR.MIDI.Helpers;

namespace PianoViaR.Helpers
{
    [Serializable]
    public class PianoNoteEventArgs : EventArgs
    {
        public int Note;
        public int Instrument;

        public PianoNoteEventArgs(int note, int instrument)
        {
            Note = note;
            Instrument = instrument;
        }

        public PianoNoteEventArgs(PianoNotes pianoNote, MIDIInstrument midiInstrument)
        : this(pianoNote.MIDINumber(), midiInstrument.MIDINumber())
        { }
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