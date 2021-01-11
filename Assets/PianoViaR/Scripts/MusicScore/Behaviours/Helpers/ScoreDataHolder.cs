using System.Collections.Generic;
using PianoViaR.MIDI.Parsing;

namespace PianoViaR.Score.Behaviours.Helpers
{
    [System.Serializable]
    public class ScoreDataHolder
    {
        public ScoreBehaviourOptions behaviourOption;
        public MIDIFile MIDIFile = null;
        public ConsecutiveNotes SingleScoreNotes = ConsecutiveNotes.Empty;
        public List<ConsecutiveNotes> MultiScoreNotes = null;

        public ScoreDataHolder(ScoreBehaviourOptions behaviour)
        {
            this.behaviourOption = behaviour;
        }

        // TODO: change this
        public ScoreDataHolder(ConsecutiveNotes singleNotes)
        : this(ScoreBehaviourOptions.GUESS_KEYS)
        {
            SingleScoreNotes = singleNotes;
        }

        public ScoreDataHolder(List<ConsecutiveNotes> multiScoreNotes)
        : this(ScoreBehaviourOptions.GUESS_NOTES)
        {
            MultiScoreNotes = multiScoreNotes;
        }
    }
}