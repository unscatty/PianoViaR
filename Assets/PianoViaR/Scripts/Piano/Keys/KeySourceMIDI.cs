using System.Collections;
using PianoViaR.Helpers;
using PianoViaR.MIDI.Helpers;
using UnityEngine;

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

        public override IEnumerator Play(YieldInstruction instruction)
        {
            OnNotePlayed();
            AddFade(EventArgs.Note);

            yield return null;
        }

        private void Stop()
        {
            OnNoteStopped();
        }

        public override void Stop(dynamic source)
        {
            Stop();
        }

        public override IEnumerator FadeAll(YieldInstruction instruction)
        {
            Stop(EventArgs.Note);

            yield return base.FadeAll(null);
        }

        public override void FadeList()
        {
            // Does nothing...
        }
    }
}