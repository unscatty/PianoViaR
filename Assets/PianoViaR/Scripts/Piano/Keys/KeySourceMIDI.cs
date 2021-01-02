using System.Collections.Generic;
using PianoViaR.Helpers;
using PianoViaR.MIDI.Helpers;

namespace PianoViaR.Piano.Behaviours
{
    public class KeySourceMIDI : KeySource
    {
        public PianoNotes PianoNote
        {
            get { return EventArgs.PianoNote; }
            set
            {
                EventArgs.PianoNote = value;
            }
        }
        public MIDIInstrument MIDIInstrument
        {
            get { return EventArgs.MIDIInstrument; }
            set
            {
                EventArgs.MIDIInstrument = value;
            }
        }
        public KeySourceMIDI(int note, int instrument)
        : this(new PianoNoteEventArgs(note, instrument))
        { }

        public KeySourceMIDI(PianoNotes note, MIDIInstrument instrument)
        : this(new PianoNoteEventArgs(note, instrument))
        { }

        public KeySourceMIDI(PianoNoteEventArgs args)
        {
            EventArgs = args;
            Initialize();
        }

        public override void Play()
        {
            OnNotePlayed();
            AddFade(EventArgs.Note);
        }

        public void Stop(int source)
        {
            OnNoteStopped();
        }

        public override void Stop(dynamic source)
        {
            Stop(source);
        }

        public override void FadeAll()
        {
            Stop(EventArgs.Note);

            base.FadeAll();
        }

        public override void FadeList()
        {
            // Does nothing...
        }
    }
}