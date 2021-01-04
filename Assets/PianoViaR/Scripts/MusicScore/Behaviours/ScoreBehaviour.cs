using PianoViaR.Helpers;
using PianoViaR.Score.Behaviours.Notes;
using UnityEngine;

namespace PianoViaR.Score.Behaviours
{
    public abstract class ScoreBehaviour
    {
        public StaffsScroll staffs;
        public ScoreBoard scoreBoard;
        public ChordBehaviour[] chords;

        public abstract void OnKeyPressed(object source, PianoNoteEventArgs args);
        public abstract void OnKeyReleased(object source, PianoNoteEventArgs args);
        public abstract void AdaptToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions);
    }
}