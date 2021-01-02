using UnityEngine;
using System;
using PianoViaR.Helpers;

namespace PianoViaR.Piano.Behaviours.Keys
{
    [RequireComponent(typeof(AudioSource))]
    public class PianoKey : MonoBehaviour
    {
        public GameObject GameObject { get { return this.gameObject; } }
        public KeySource KeySource { get; set; }
        [SerializeField]
        public PianoNoteEventArgs EventArgs;
        public event EventHandler<PianoNoteEventArgs> KeyPressed;
        public event EventHandler<PianoNoteEventArgs> KeyReleased;
        private bool played = false;

        void Update()
        {
            if ((transform.eulerAngles.x > 350 && transform.eulerAngles.x < 359.5f) && !played)
            {
                played = true;
                Play();
                // Notify of key pressed (Primarily to score based events)
                OnKeyPressed(EventArgs);

                if (KeySource.Count > 0)
                {
                    FadeList();
                }
            }
            else if ((transform.eulerAngles.x > 359.9 || transform.eulerAngles.x < 350) && played)
            {
                played = false;
                // Notify of key released (Primarily to score based events)
                OnKeyReleased(EventArgs);

                FadeAll();
            }
        }

        protected virtual void OnKeyPressed(PianoNoteEventArgs args)
        {
            KeyPressed?.Invoke(this, args);
        }

        protected virtual void OnKeyReleased(PianoNoteEventArgs args)
        {
            KeyReleased?.Invoke(this, args);
        }

        void Play()
        {
            KeySource.Play();
        }

        void FadeList()
        {
            KeySource.FadeList();
        }

        void FadeAll()
        {
            KeySource.FadeAll();
        }
    }
}