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

        public PianoGameplayEventArgs(GameplayState state)
        {
            this.state = state;
        }
    }
}