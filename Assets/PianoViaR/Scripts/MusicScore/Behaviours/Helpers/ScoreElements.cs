using Leap.Unity.Interaction;
using PianoViaR.Score.Helpers;
using UnityEngine;

namespace PianoViaR.Score.Behaviours.Helpers
{
    public class ScoreElements : MonoBehaviour
    {
        public ScoreBoard scoreBoard;
        public StaffsScroll staffs;
        public MusicSymbolFactory factory;
        public InteractionBehaviour grabBar;
        public InteractionBehaviour resetButton;

        public void SetActive(bool active)
        {
            scoreBoard.gameObject.SetActive(active);
            staffs.gameObject.SetActive(active);
            grabBar.gameObject.SetActive(active);
            resetButton.gameObject.SetActive(active);
        }
    }
}