using System;
using System.Collections.Generic;
using PianoViaR.Helpers;
using PianoViaR.MIDI.Helpers;
using PianoViaR.Score.Behaviours.GuessNote;
using PianoViaR.Score.Behaviours.Helpers;
using PianoViaR.Score.Helpers;
using PianoViaR.Utils;
using UnityEngine;

namespace PianoViaR.Score.Behaviours
{
    public class GuessNoteBehaviour : ScoreBehaviour
    {
        public GuessNoteOptionHolder optionsHolder;
        public ConsecutiveNotes correctOption { get; private set; }

        private int randomSeed = Environment.TickCount;

        public GuessNoteBehaviour(
            ScoreElements elements,
            GuessNoteOptionHolder optionsHolder,
            List<ConsecutiveNotes> notes,
            MIDIOptions midiOptions,
            Color correctColor,
            Color incorrectColor,
            Color auxiliarColor
        )
        : base(elements, null, correctColor, incorrectColor, auxiliarColor)
        {
            this.optionsHolder = optionsHolder;

            if (notes == null || notes.Count <= 0)
            {
                throw new ArgumentException("List of options must not be null or empty");
            }

            CreateOptions(optionsHolder.options, notes, midiOptions, elements.factory);
        }

        private void CreateOptions(GuessNoteOption[] options, List<ConsecutiveNotes> notes, MIDIOptions midiOptions, MusicSymbolFactory factory)
        {
            correctOption = notes[0];
            var randomIndexer = new RandomIndexer(options.Length, randomSeed);

            var randomOptionCorrect = options[randomIndexer.Next()];

            CreateOption(randomOptionCorrect, true, correctOption, midiOptions, factory);

            for (int i = 1; i < options.Length; i++)
            {
                var randomOptionIncorrect = options[randomIndexer.Next()];
                CreateOption(randomOptionIncorrect, false, notes[i], midiOptions, factory);
            }
        }

        private void CreateOption(
            GuessNoteOption option,
            bool isCorrectOption,
            ConsecutiveNotes staffNotes,
            MIDIOptions midiOptions,
            MusicSymbolFactory factory
        )
        {
            option.SetEnabled(true);
            option.ContactEnabled(true);
            option.CorrectOption = isCorrectOption;
            option.correctColor = CorrectColor;
            option.incorrectColor = IncorrectColor;

            // Suscribe 
            option.OptionSelected -= OnOptionSelected;  // Prevent double subscription
            option.OptionSelected += OnOptionSelected;

            option.CreateScore(staffNotes, midiOptions, factory);
        }
        public override void AdaptToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        { }

        public override void PostSubscription()
        {
            foreach (int note in correctOption.Notes)
            {
                // Tint corresponding piano keys to hint color
                OnEvaluateBegin(new PianoGameplayEventArgs(GameplayState.HINT, new PianoNoteEventArgs(note, 0)));
            }
        }

        protected virtual void OnOptionSelected(bool isCorrectOption)
        {
            if (isCorrectOption)
            {
                OnSuccess();
                OnRoundEnd();
            }
            else
            {
                OnFailed();
            }
        }

        protected override void OnRoundEnd()
        {
            // UnInitialize();

            base.OnRoundEnd();
        }

        public override void Initialize()
        {
            // Disable original elements
            elements.SetActive(false);
        }

        public override void OnKeyPressed(object source, PianoNoteEventArgs args)
        {
            // Ignore Key pressing
        }

        public override void OnKeyReleased(object source, PianoNoteEventArgs args)
        {
            // Ignore Key pressing
        }

        public override void UnInitialize()
        {
            foreach (var option in optionsHolder.options)
            {
                // Unsuscribe
                option.OptionSelected -= OnOptionSelected;
                // Disable every option gameObject
                option.SetEnabled(false);
                option.staffs.Clear();
            }

            foreach (int note in correctOption.Notes)
            {
                // Tint corresponding piano keys to their normal color
                OnEvaluateEnd(new PianoGameplayEventArgs(GameplayState.IDLE, new PianoNoteEventArgs(note, 0)));
            }

            // Enable original elements
            elements.SetActive(true);
        }
    }
}