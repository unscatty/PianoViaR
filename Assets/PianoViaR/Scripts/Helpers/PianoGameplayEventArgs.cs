using System;
using PianoViaR.MIDI.Helpers;

namespace PianoViaR.Helpers
{
    public enum GameplayState
    {
        CORRECT, INCORRECT, HINT, IDLE
    }

    [Serializable]
    public class PianoGameplayEventArgs : EventArgs
    {
        public GameplayState state;
        public PianoNoteEventArgs pianoArgs;

        public PianoGameplayEventArgs(GameplayState state, PianoNoteEventArgs pianoArgs)
        {
            this.state = state;
            this.pianoArgs = pianoArgs;
        }
    }
}