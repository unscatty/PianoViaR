using System.Collections;
using System.Collections.Generic;
using MidiSheetMusic;
using MusicScore.Helpers;
using UnityEditor;
using UnityEngine;
using PianoViaR.Utils;

namespace MusicScore
{

    public class MusicScore : MonoBehaviour
    {
        public MusicSymbolFactory factory;
        public UnityEngine.Object midifile;
        private string currentMidiAssetPath;
        // The game object to hold the staffs created
        public StaffsScroll staffs;
        public ScoreBoard scoreBoard;

        // Start is called before the first frame update
        void Start()
        {
            currentMidiAssetPath = AssetDatabase.GetAssetPath(midifile);
            CreateScore(currentMidiAssetPath);
        }

        // Update is called once per frame
        void Update()
        {
            var newMidiAssetPath = AssetDatabase.GetAssetPath(midifile);

            if (currentMidiAssetPath != newMidiAssetPath)
            {
                currentMidiAssetPath = newMidiAssetPath;
                CreateScore(currentMidiAssetPath);
            }
        }

        void CreateScore(string MidiAssetPath)
        {
            Clear();

            var boxSize = scoreBoard.BoxSize();

            SheetMusic sheet = new SheetMusic(MidiAssetPath, null, factory, (boxSize.x, boxSize.y));
            Vector3 staffsXYDims;

            var staffsGO = staffs.GameObject;
            sheet.Create(ref staffsGO, out staffsXYDims);

            AdaptStaffsToDimensions(boxSize, staffsXYDims);
        }

        void AdaptStaffsToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            staffs.AdaptToDimensions(scoreBoxSize, staffsDimensions);
        }

        void Clear()
        {
            staffs.Clear();
            staffs.ResetToNormal();
        }
    }
}