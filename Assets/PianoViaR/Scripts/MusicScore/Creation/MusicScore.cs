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
            // staffs.Initialize();
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

            var globalScoreBoxSize = scoreBoard.BoxSize();

            TimeSignature time = TimeSignature.Default;
            var testTrack = TestTrack(time);
            var options = new MidiOptions
            {
                scrollVert = false,
                showNoteLetters = MidiOptions.NoteNameNone,
                key = -1,
                shifttime = 0,
                showLyrics = false,
                showMeasures = false,
                time = time,

                useDefaultInstruments = true,
                largeNoteSize = false,
                twoStaffs = false,
                transpose = 0,
                combineInterval = 40,
                tempo = time.Tempo,
                pauseTime = 0,
                playMeasuresInLoop = false,
                playMeasuresInLoopStart = 0,
                playMeasuresInLoopEnd = 0,
            };

            SheetMusic sheet = new SheetMusic(MidiAssetPath, null, factory, (globalScoreBoxSize.x, globalScoreBoxSize.y));
            // SheetMusic sheet = new SheetMusic(testTrack, time, options, factory, (globalScoreBoxSize.x, globalScoreBoxSize.y));

            Vector3 staffsXYDims;

            var staffsGO = staffs.GameObject;
            sheet.Create(ref staffsGO, out staffsXYDims);

            AdaptStaffsToDimensions(globalScoreBoxSize, staffsXYDims);
        }

        void AdaptStaffsToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            staffs.AdaptToDimensions(scoreBoxSize, staffsDimensions);
        }

        private MidiTrack TestTrack(TimeSignature time)
        {
            NoteDuration[] durations = {
                NoteDuration.Quarter, NoteDuration.Quarter, NoteDuration.Quarter
            };

            int[] startTimes = {
                0, time.DurationToTime(durations[0]), time.DurationToTime(durations[1]), time.DurationToTime(durations[2])
            };

            // int quarterDuration = time.DurationToTime()

            List<MidiSheetMusic.MidiNote> notes = new List<MidiSheetMusic.MidiNote> {
                new MidiSheetMusic.MidiNote(startTimes[0] + startTimes[1] * 0, 0, 60, startTimes[1]),
                new MidiSheetMusic.MidiNote(startTimes[0] + startTimes[1] * 1, 0, 62, startTimes[2]),
                new MidiSheetMusic.MidiNote(startTimes[0] + startTimes[1] * 2, 0, 64, startTimes[3]),
                new MidiSheetMusic.MidiNote(startTimes[0] + startTimes[1] * 3, 0, 66, startTimes[3]),
            };

            MidiTrack track = new MidiTrack(0)
            {
                Notes = notes,
                Instrument = 0,
                Lyrics = null
            };

            return track;
        }

        void Clear()
        {
            staffs.Clear();
            staffs.ResetToNormal();
        }
    }
}