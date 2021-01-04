using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PianoViaR.MIDI.Parsing;
using PianoViaR.MIDI.Helpers;
using PianoViaR.Score.Creation;
using PianoViaR.Score.Helpers;
using PianoViaR.MIDI.Playback;
using System.IO;
using PianoViaR.Score.Behaviours.Helpers;

namespace PianoViaR.Score.Behaviours
{

    public class MusicScore : MonoBehaviour
    {
        public MusicSymbolFactory factory;
        public UnityEngine.Object midifile;
        private string currentMidiAssetPath;
        // The game object to hold the staffs created
        public StaffsScroll staffs;
        public ScoreBoard scoreBoard;
        public MIDISongPlayer midiPlayer;
        public int transpose;

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

                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                CreateScore(currentMidiAssetPath);
            }
        }

        void CreateScore(string MidiAssetPath)
        {
            Clear();

            var globalScoreBoxSize = scoreBoard.BoxSize();

            TimeSignature time = TimeSignature.Default;
            // var testTrack = TestTrack(time);
            var testTrack = TrackBuild();
            var options = new MIDIOptions
            {
                scrollVert = false,
                showNoteLetters = MIDIOptions.NoteNameNone,
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
            var midifile = new MIDIFile(MidiAssetPath);
            var midiOptions = new MIDIOptions(midifile);
            midiOptions.transpose = transpose;
            Stream midiStream = new MemoryStream();

            var couldWrite = midifile.Write(midiStream, midiOptions, close: false, reset: true);
            if (couldWrite)
            {
                midiPlayer.LoadMidi(new AudioSynthesis.Midi.MidiFile(midiStream));
            }
            else
            {
                Debug.Log("Could not write to stream");
            }

            var consNotes = new ConsecutiveNotes()
            {
                Notes = new int[] { 60, 62, 64, 66, 64, 66, 68, 70 },
                Duration = NoteDuration.Quarter
            };

            SheetMusic sheet = new SheetMusic(MidiAssetPath, null, factory, (globalScoreBoxSize.x, globalScoreBoxSize.y));
            // SheetMusic sheet = new SheetMusic(midifile, midiOptions, factory, (globalScoreBoxSize.x, globalScoreBoxSize.y));
            // SheetMusic sheet = new SheetMusic(testTrack, time, options, factory, (globalScoreBoxSize.x, globalScoreBoxSize.y));
            // SheetMusic sheet = new SheetMusic(consNotes, MIDIOptions.Default, factory, (globalScoreBoxSize.x, globalScoreBoxSize.y));

            Vector3 staffsXYDims;

            var staffsGO = staffs.GameObject;
            sheet.Create(ref staffsGO, out staffsXYDims);

            AdaptStaffsToDimensions(globalScoreBoxSize, staffsXYDims);
        }

        void AdaptStaffsToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            staffs.AdaptToDimensions(scoreBoxSize, staffsDimensions);
        }

        private MIDITrack TestTrack(TimeSignature time)
        {
            NoteDuration[] durations = {
                NoteDuration.Quarter, NoteDuration.Quarter, NoteDuration.Quarter
            };

            int[] startTimes = {
                0, time.DurationToTime(durations[0]), time.DurationToTime(durations[1]), time.DurationToTime(durations[2])
            };

            // int quarterDuration = time.DurationToTime()

            List<MIDINote> notes = new List<MIDINote> {
                new MIDINote(startTimes[0] + startTimes[1] * 0, 0, 60, startTimes[1]),
                new MIDINote(startTimes[0] + startTimes[1] * 1, 0, 62, startTimes[2]),
                new MIDINote(startTimes[0] + startTimes[1] * 2, 0, 64, startTimes[3]),
                new MIDINote(startTimes[0] + startTimes[1] * 3, 0, 66, startTimes[3]),
            };

            MIDITrack track = new MIDITrack(0)
            {
                Notes = notes,
                Instrument = 0,
                Lyrics = null
            };

            return track;
        }

        private MIDITrack TrackBuild()
        {
            return (new ConsecutiveNotes()
            {
                Notes = new int[] { 66, 68, 70, 72 }
                // Notes = new int[] { 60, 62, 64, 66, }
            }).GetTrack();
        }

        void Clear()
        {
            staffs.Clear();
            staffs.ResetToNormal();
        }
    }
}