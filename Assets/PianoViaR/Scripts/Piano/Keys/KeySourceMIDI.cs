using PianoViaR.Helpers;
using PianoViaR.MIDI.Helpers;

namespace PianoViaR.Piano.Behaviours.Keys
{
    public class KeySourceMIDI : KeySource
    {
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

        private void Stop()
        {
            OnNoteStopped();
        }

        public override void Stop(dynamic source)
        {
            Stop();
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