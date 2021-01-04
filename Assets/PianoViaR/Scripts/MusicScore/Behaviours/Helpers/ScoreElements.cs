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
    }
}