using PianoViaR.Helpers;
using PianoViaR.Piano.Behaviours.Keys;
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
        private bool correctKey;
        public int pressingNote;

        private Vector3 GrabBarXOffset;
        public GuessKeyBehaviour(
            ScoreElements elements,
            ChordBehaviour[] chords,
            Color correctColor,
            Color incorrectColor,
            Color auxiliarColor
        )
        : base(elements, chords, correctColor, incorrectColor, auxiliarColor)
        { }

        public GuessKeyBehaviour(
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
            correctKey = false;
            pressingNote = -1;

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
            Vector3 newStaffsDimensions = elements.staffs.AdaptToDimensions(scoreBoxSize, staffsDimensions);
            elements.staffs.gameObject.transform.SetParent(null);

            elements.scoreBoard.GameObject.FitOnlyToWidth(newStaffsDimensions.x * 1.025f);
            elements.scoreBoard.GameObject.FitOnlyToHeight(newStaffsDimensions.y * 1.1f);
            elements.staffs.gameObject.transform.SetParent(elements.scoreBoard.transform);
            elements.staffs.transform.localPosition = Vector3.zero;
        }

        // Must suscribe piano key to this
        public override void OnKeyPressed(object source, PianoNoteEventArgs args)
        {
            if (!RoundEnded)
            {
                if (!KeyPressed)
                {
                    KeyPressed = true;
                    pressingNote = args.Note;

                    EvaluatePressedKey(source, args);
                }
            }
        }

        private void EvaluatePressedKey(object source, PianoNoteEventArgs args)
        {
            Color color;
            GameplayState state;

            correctKey = IsRightNote(args.Note);

            if (correctKey)
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

            var gameplayArgs = new PianoGameplayEventArgs(state, args);

            // Notify piano key
            OnEvaluateBegin(gameplayArgs);
            // var pianoKey = (PianoKey)source;
            // pianoKey.OnEvaluateBegin(this, gameplayArgs);

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
                if (KeyPressed && args.Note == pressingNote)
                {
                    KeyPressed = false;

                    // Notify piano key to stop
                    OnEvaluateEnd(new PianoGameplayEventArgs(GameplayState.IDLE, args));
                    // var pianoKey = (PianoKey)source;
                    // pianoKey.OnEvaluateEnd(this, new PianoGameplayEventArgs(GameplayState.IDLE, args));

                    if (correctKey)
                    {
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

                    pressingNote = -1;
                }
            }
        }

        protected override void OnRoundEnd()
        {
            UnInitialize();
            base.OnRoundEnd();
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