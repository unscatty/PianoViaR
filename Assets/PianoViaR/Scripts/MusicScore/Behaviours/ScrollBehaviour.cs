using PianoViaR.Helpers;
using PianoViaR.Score.Behaviours.Helpers;
using PianoViaR.Score.Behaviours.Notes;
using UnityEngine;

namespace PianoViaR.Score.Behaviours
{
    public class ScrollBehaviour : ScoreBehaviour
    {
        public ScrollBehaviour(ScoreElements elements, Color correctColor, Color incorrectColor, Color auxiliarColor)
        : base(elements, correctColor, incorrectColor, auxiliarColor)
        {
        }

        public ScrollBehaviour(ScoreElements elements, ChordBehaviour[] chords, Color correctColor, Color incorrectColor, Color auxiliarColor)
        : base(elements, chords, correctColor, incorrectColor, auxiliarColor)
        {
        }

        public override void AdaptToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            elements.staffs.AdaptToDimensions(scoreBoxSize, staffsDimensions);
        }

        public override void Initialize()
        {
            // Do nothing
        }

        public override void OnKeyPressed(object source, PianoNoteEventArgs args)
        {
            // Do nothing
        }

        public override void OnKeyReleased(object source, PianoNoteEventArgs args)
        {
            // Do nothing
        }

        public override void UnInitialize()
        {
            // Do nothing
        }
    }
}