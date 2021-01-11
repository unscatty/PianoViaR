using UnityEngine;
using TMPro;
using System.IO;
using UnityEditor;
using System;

namespace PianoViaR.Score.Behaviours.Messages
{
    public class UserMessages : MonoBehaviour
    {
        public TMP_Text textObject;
        public TMP_Text hintObject;
        // public GameObject parent;
        // public GameObject messageHolder;
        [SerializeField]
        public ScoreMessages guessNotesMessages;
        [SerializeField]
        public ScoreMessages guessKeysMessages;
        public ScoreMessages scrollMessages;

        private string currentIntroMessage;
        private string[] currentSuccessMessages;
        private string[] currentErrorMessages;
        private System.Random random = new System.Random(6174);

        void Awake()
        {
            SetText(string.Empty);
            SetHint(string.Empty);
        }

        public void DisplayIntroMessage()
        {
            SetText(currentIntroMessage);
        }

        public void OnSuccess(object source, EventArgs e)
        {
            DisplayRandomSuccessMessage();
        }

        public void OnFailed(object source, EventArgs e)
        {
            DisplayRandomErrorMessage();
        }

        private void DisplayRandomSuccessMessage()
        {
            SetText(GetRandomMessage(currentSuccessMessages));
        }
        private void DisplayRandomErrorMessage()
        {
            SetText(GetRandomMessage(currentErrorMessages));
        }

        private string GetRandomMessage(string[] messages)
        {
            return messages[random.Next(0, messages.Length)];
        }

        public void SetText(string text)
        {
            textObject.SetText(text);
        }

        public void SetHint(string hint)
        {
            hintObject.SetText(hint);
        }

        public void SetMessages(ScoreBehaviourOptions option)
        {
            ScoreMessages optionMessages;

            switch (option)
            {
                case ScoreBehaviourOptions.GUESS_KEYS:
                    optionMessages = guessKeysMessages;
                    break;
                case ScoreBehaviourOptions.GUESS_NOTES:
                    optionMessages = guessNotesMessages;
                    break;
                case ScoreBehaviourOptions.SCROLL:
                    optionMessages = scrollMessages;
                    break;
                default:
                    return;
            }

            currentIntroMessage = optionMessages.introMessage;
            currentSuccessMessages = optionMessages.GetSuccessMessages();
            currentErrorMessages = optionMessages.GetErrorMessages();
        }

        public void ResetPositionAndRotation()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    [System.Serializable]
    public struct ScoreMessages
    {
        public string introMessage;
        public UnityEngine.Object successMessages;
        public UnityEngine.Object errorMessages;

        public string[] GetSuccessMessages()
        {
            return GetMessages(successMessages);
        }

        public string[] GetErrorMessages()
        {
            return GetMessages(errorMessages);
        }

        private string[] GetMessages(string path)
        {
            return File.ReadAllLines(path);
        }

        private string[] GetMessages(UnityEngine.Object obj)
        {
            return GetMessages(AssetDatabase.GetAssetPath(obj));
        }
    }
}