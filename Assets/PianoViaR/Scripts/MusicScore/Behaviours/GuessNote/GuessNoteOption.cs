using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using PianoViaR.MIDI.Helpers;
using PianoViaR.Score.Behaviours.Helpers;
using PianoViaR.Score.Creation;
using PianoViaR.Score.Helpers;
using PianoViaR.Utils;
using UnityEngine;

namespace PianoViaR.Score.Behaviours.GuessNote
{
    [RequireComponent(typeof(InteractionBehaviour))]
    public class GuessNoteOption : MonoBehaviour
    {
        InteractionBehaviour interactionBehaviour;
        // public bool CorrectOption { private get; set; }
        public GameObject scoreBoard;
        public GameObject staffs;
        public Vector3 scoreBoardBoxSize;
        public bool CorrectOption;
        public Color correctColor;
        public Color incorrectColor;

        public event Action<bool> OptionSelected;
        void Awake()
        {
            interactionBehaviour = GetComponent<InteractionBehaviour>();

            scoreBoardBoxSize = scoreBoard.BoxSize(force: true);
        }

        protected virtual void OnContactBegin()
        {
            Evaluate();

            OptionSelected?.Invoke(CorrectOption);
        }

        private void Evaluate()
        {
            var renderers = staffs.GetComponentsInChildren<Renderer>();

            var color = CorrectOption ? correctColor : incorrectColor;

            foreach (var renderer in renderers)
            {
                renderer.material.color = color;
            }

            ContactEnabled(CorrectOption);

            // Unsuscribe
            interactionBehaviour.OnContactBegin -= OnContactBegin;
        }

        public void ContactEnabled(bool enabled)
        {
            interactionBehaviour.ignoreContact = !enabled;
        }

        public void SubscribeContact()
        {
            interactionBehaviour.OnContactBegin -= OnContactBegin;  // Prevent double subscription
            interactionBehaviour.OnContactBegin += OnContactBegin;
        }

        public void SetEnabled(bool enabled)
        {
            gameObject.SetActive(enabled);
        }

        public void CreateScore(ConsecutiveNotes notes, MIDIOptions midiOptions, MusicSymbolFactory factory)
        {
            SheetMusic sheet = new SheetMusic(notes, midiOptions, factory, (scoreBoardBoxSize.x, scoreBoardBoxSize.y));
            Vector3 staffsXYDims;

            staffs.transform.position = Vector3.zero;
            staffs.transform.localScale = Vector3.one;

            sheet.Create(ref staffs, out staffsXYDims);

            // staffs.transform.localPosition = Vector3.zero;

            AdaptToDimensions(scoreBoardBoxSize, staffsXYDims);
            SubscribeContact();
        }

        public void AdaptToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            staffs.transform.position = Vector3.zero;
            staffs.transform.localScale = Vector3.one;

            Vector3 newStaffsDimensions = staffs.AdaptToDimensions(null, scoreBoxSize, staffsDimensions, Axis.X);
            staffs.transform.localPosition = Vector3.zero;

            staffs.transform.SetParent(null);

            scoreBoard.FitOnlyToWidth(newStaffsDimensions.x * 1.025f);
            scoreBoard.FitOnlyToHeight(newStaffsDimensions.y * 1.1f);

            staffs.transform.SetParent(scoreBoard.transform);
            staffs.transform.localPosition = Vector3.zero;
        }

    }
}