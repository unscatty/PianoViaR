using UnityEngine;
using System;
using PianoViaR.Helpers;
using System.Collections;

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
                StartCoroutine(Play(new WaitForFixedUpdate()));
                // Notify of key pressed (Primarily to score based events)
                OnKeyPressed(EventArgs);

                if (KeySource.Count > 0)
                {
                    FadeList();
                }
            }
            else if ((transform.eulerAngles.x > 359.9 || transform.eulerAngles.x < 350) /* && played */)
            {
                if (played)
                {
                    OnKeyReleased(EventArgs);
                }
                
                played = false;
                // Notify of key released (Primarily to score based events)

                StartCoroutine(FadeAll(null));
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

        IEnumerator Play(YieldInstruction instruction)
        {
            yield return KeySource.Play(instruction);
        }

        void FadeList()
        {
            KeySource.FadeList();
        }

        IEnumerator FadeAll(YieldInstruction instruction)
        {
            yield return KeySource.FadeAll(instruction);
        }
    }
}