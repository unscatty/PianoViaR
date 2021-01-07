using System;
using PianoViaR.Helpers;
using PianoViaR.Score.Behaviours.Helpers;
using PianoViaR.Score.Behaviours.Notes;
using UnityEngine;

namespace PianoViaR.Score.Behaviours
{
    public abstract class ScoreBehaviour
    {
        protected ScoreElements elements;
        protected readonly ChordBehaviour[] chords;
        protected bool RoundEnded = false;

        protected Color CorrectColor { get; set; }
        protected Color IncorrectColor { get; set; }
        protected Color AuxiliarColor { get; set; }

        public event EventHandler<PianoGameplayEventArgs> EvaluateBegin;
        public event EventHandler<PianoGameplayEventArgs> EvaluateEnd;
        public event EventHandler RoundEnd;

        protected ScoreBehaviour(
            ScoreElements elements,
            ChordBehaviour[] chords,
            Color correctColor,
            Color incorrectColor,
            Color auxiliarColor
        )
        {
            this.elements = elements;
            this.chords = chords;

            CorrectColor = correctColor;
            IncorrectColor = incorrectColor;
            AuxiliarColor = auxiliarColor;

            Initialize();
        }

        protected ScoreBehaviour(
            ScoreElements elements,
            Color correctColor,
            Color incorrectColor,
            Color auxiliarColor
        )
        : this(elements, elements.staffs.GetComponentsInChildren<ChordBehaviour>(), correctColor, incorrectColor, auxiliarColor)
        {
        }
        public abstract void Initialize();
        public abstract void UnInitialize();
        public abstract void AdaptToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions);
        public abstract void OnKeyPressed(object source, PianoNoteEventArgs args);
        public abstract void OnKeyReleased(object source, PianoNoteEventArgs args);

        protected virtual void OnEvaluateBegin(PianoGameplayEventArgs args)
        {
            Debug.Log("Sending arguments to key: Evaluate Begin");
            EvaluateBegin?.Invoke(this, args);
        }
        protected virtual void OnEvaluateEnd(PianoGameplayEventArgs args)
        {
            Debug.Log("Sending arguments to key: Evaluate End");
            EvaluateEnd?.Invoke(this, args);
        }

        public virtual void PostSubscription()
        { }

        protected virtual void OnRoundEnd()
        {
            RoundEnded = true;
            RoundEnd?.Invoke(this, EventArgs.Empty);
        }

        // Must be called when suscribed and triggers the next action of the score
        // (moving the symbols to the left when score-hero)
        protected virtual void NextAction()
        { }
    }
}