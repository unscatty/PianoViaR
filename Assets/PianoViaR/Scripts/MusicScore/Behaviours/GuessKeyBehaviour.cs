using PianoViaR.Helpers;
using PianoViaR.Score.Behaviours.Helpers;
using PianoViaR.Score.Behaviours.Notes;
using PianoViaR.Utils;
using UnityEngine;

namespace PianoViaR.Score.Behaviours
{
    public class GuessKeyBehaviour : ScoreBehaviour
    {
        public int CurrentIndex { get; private set; }
        public ChordBehaviour CurrentChord { get { return chords[CurrentIndex]; } }
        public bool KeyPressed;

        private Vector3 GrabBarXOffset;
        protected GuessKeyBehaviour(
            ScoreElements elements,
            ChordBehaviour[] chords,
            Color correctColor,
            Color incorrectColor,
            Color auxiliarColor
        )
        : base(elements, chords, correctColor, incorrectColor, auxiliarColor)
        { }

        protected GuessKeyBehaviour(
            ScoreElements elements,
            Color correctColor,
            Color incorrectColor,
            Color auxiliarColor
        )
        : this(elements, elements.staffs.GetComponentsInChildren<ChordBehaviour>(), correctColor, incorrectColor, auxiliarColor)
        { }

        public override void Initialize()
        {
            CurrentIndex = 0;
            KeyPressed = false;

            elements.resetButton.enabled = false;

            var grabBarXOffset = elements.scoreBoard.transform.localPosition - elements.grabBar.transform.localPosition;
            grabBarXOffset.y = 0;
            grabBarXOffset.z = 0;

            GrabBarXOffset = grabBarXOffset;

            elements.grabBar.transform.position += grabBarXOffset;

            elements.scoreBoard.DisableContact();
            elements.staffs.CanCollide(false);
        }
        public override void AdaptToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            elements.staffs.AdaptToDimensions(scoreBoxSize, staffsDimensions);
            elements.scoreBoard.GameObject.FitToHeight(staffsDimensions.y);
        }

        // Must suscribe piano key to this
        public override void OnKeyPressed(object source, PianoNoteEventArgs args)
        {
            if (!RoundEnded)
            {
                if (!KeyPressed)
                {
                    KeyPressed = true;

                    EvaluatePressedKey(args);
                }
            }
        }

        private void EvaluatePressedKey(PianoNoteEventArgs args)
        {
            Color color;
            GameplayState state;

            if (IsRightNote(args.Note))
            {
                // Right key
                color = CorrectColor;
                state = GameplayState.CORRECT;
            }
            else
            {
                // Wrong key
                color = IncorrectColor;
                state = GameplayState.INCORRECT;
            }

            var gameplayArgs = new PianoGameplayEventArgs(state);

            // Notify piano key
            OnEvaluateBegin(this, gameplayArgs);

            // Change the chord color
            ChangeColor(CurrentChord, color);
        }

        private bool IsRightNote(int noteValue)
        {
            var chordNote = CurrentChord.FirstNoteValue;

            return noteValue == chordNote;
        }

        private void ChangeColor(ChordBehaviour chord, Color color)
        {
            var renderers = chord.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                renderer.material.color = color;
            }
        }

        // Must suscribe piano key to this
        public override void OnKeyReleased(object source, PianoNoteEventArgs args)
        {
            if (!RoundEnded)
            {
                if (KeyPressed)
                {
                    // Notify piano key to stop
                    OnEvaluateEnd(this, new PianoGameplayEventArgs(GameplayState.IDLE));

                    if (CurrentIndex < (chords.Length - 1))
                    {
                        CurrentIndex++;
                    }
                    else
                    {
                        // Round is over
                        OnRoundEnd();
                    }
                }
            }
        }

        public override void UnInitialize()
        {
            // Revert changes
            elements.resetButton.enabled = true;

            elements.grabBar.transform.position -= GrabBarXOffset;

            elements.scoreBoard.EnableContact();
            elements.staffs.CanCollide(true);
        }
    }
}