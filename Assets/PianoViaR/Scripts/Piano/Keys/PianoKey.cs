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

        public Color correctColor;
        public Color incorrectColor;
        public Color hintColor;

        private Renderer KeyRenderer;
        private Color DefaultColor;

        void Awake()
        {
            KeyRenderer = GetComponent<Renderer>();
            DefaultColor = KeyRenderer.material.color;
        }

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

        public virtual void OnEvaluateBegin(object source, PianoGameplayEventArgs args)
        {
            Debug.Log("Receiving arguments to key: Evaluate Begin");
            Debug.Log($"Sent note: {args.pianoArgs.Note}, key note: {EventArgs.Note}");
            if (this.EventArgs.Note == args.pianoArgs.Note)
            {
                ChangeColors(args.state);
            }
        }

        public virtual void OnEvaluateEnd(object source, PianoGameplayEventArgs args)
        {
            Debug.Log("Receiving arguments to key: Evaluate End");
            Debug.Log($"Sent note: {args.pianoArgs.Note}, key note: {EventArgs.Note}");
            if (this.EventArgs.Note == args.pianoArgs.Note)
            {
                ChangeColors(args.state);
            }
        }

        private void ChangeColors(GameplayState state)
        {
            Debug.Log("Color changed to " + state);
            switch (state)
            {
                case GameplayState.CORRECT:
                    KeyRenderer.material.color = correctColor;
                    break;
                case GameplayState.INCORRECT:
                    KeyRenderer.material.color = incorrectColor;
                    break;
                case GameplayState.IDLE:
                    KeyRenderer.material.color = DefaultColor;
                    break;
                case GameplayState.HINT:
                    KeyRenderer.material.color = hintColor;
                    break;
            }
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