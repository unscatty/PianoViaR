using UnityEngine;

namespace PianoViaR.Score.Behaviours.GuessNote
{
    public class GuessNoteOptionHolder : MonoBehaviour
    {
        public GuessNoteOption[] options;
        void Awake()
        {
            foreach (var option in options)
            {
                option.SetEnabled(false);
            }
        }
    }
}