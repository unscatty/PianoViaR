using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PianoViaR.MIDI.Parsing;
using PianoViaR.MIDI.Helpers;
using PianoViaR.Score.Creation;
using PianoViaR.Score.Helpers;
using PianoViaR.Score.Behaviours.Helpers;
using PianoViaR.Piano.Behaviours.Keys;
using PianoViaR.Score.Behaviours.Notes;
using System;
using PianoViaR.Score.Behaviours.GuessNote;

namespace PianoViaR.Score.Behaviours
{
    public enum ScoreBehaviourOptions
    {
        SCROLL, GUESS_KEYS, GUESS_NOTES, SCORE_HERO
    }

    public class MusicScore : MonoBehaviour
    {
        public PianoKeyController PianoKeysController;
        public MusicSymbolFactory factory;
        public UnityEngine.Object midifile;
        private string currentMidiAssetPath;
        public ScoreElements elements;
        public ScoreBehaviour behaviour;
        public ScoreBehaviourOptions behaviourOption;
        public GuessNoteOptionHolder optionsHolder;

        [SerializeField]
        public List<ScoreDataHolder> Data;
        private int dataIndex;
        public MIDIOptions MIDIOptions = MIDIOptions.Default;
        private PianoKey[] PianoKeys;
        private ChordBehaviour[] Chords;
        public Color correctColor;
        public Color incorrectColor;
        public Color hintColor;

        private ScoreDataHolder CurrentData { get { return Data[dataIndex]; } }
        public bool keysReady = false;

        // Start is called before the first frame update
        void Start()
        {
            currentMidiAssetPath = AssetDatabase.GetAssetPath(midifile);
            factory = elements.factory;
            dataIndex = 0;

            PianoKeys = PianoKeysController.PianoKeysParent.GetComponentsInChildren<PianoKey>();

            Data = TestData();

            PianoKeysController.PianoKeysReady -= OnPianoKeysReady;
            PianoKeysController.PianoKeysReady += OnPianoKeysReady;

            if (PianoKeysController.KeysReady)
            {
                CreateScore();
                keysReady = true;
            }
        }

        void OnPianoKeysReady(object source, EventArgs args)
        {
            if (this.behaviour != null)
            {
                UnSuscribePianoKeys();
                SuscribePianoKeys();
            }

            if (!keysReady)
            {
                CreateScore();
                keysReady = true;
            }

            PianoKeysController.PianoKeysReady -= OnPianoKeysReady;
        }

        List<ScoreDataHolder> TestData()
        {
            return new List<ScoreDataHolder>()
            {
                // new ScoreDataHolder(
                //     ScoreBehaviourOptions.SCROLL
                // //     new ConsecutiveNotes()
                // //     {
                // //         Notes = new int[] { 60, 62, 64 }
                // //     }
                // )
                // {
                // MIDIFile = new MIDIFile(currentMidiAssetPath)
                // }
                new ScoreDataHolder(
                    new List<ConsecutiveNotes>()
                    {
                        new ConsecutiveNotes()
                        {
                            Notes = new int[] { 60, 64 }
                        },
                        new ConsecutiveNotes()
                        {
                            Notes = new int[] { 64, 68 }
                        },
                        new ConsecutiveNotes()
                        {
                            Notes = new int[] { 60, 64 }
                        }
                    }
                ),
                new ScoreDataHolder(
                    new ConsecutiveNotes()
                    {
                        Notes = new int[] { 62, 64, 66, 68 }
                    }
                )
            };
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                dataIndex = 0;
                CreateScore();
            }
        }

        void CreateScore()
        {
            Clear();

            var globalScoreBoxSize = elements.scoreBoard.BoxSize();

            var data = CurrentData;

            var scoreBehaviourOption = data.behaviourOption;

            Vector3 staffsXYDims;
            // Create 
            CreateSheetMusic(data, globalScoreBoxSize, out staffsXYDims);

            // var chords = elements.staffs.GetComponentsInChildren<ChordBehaviour>();
            // Debug.Log("Chords length " + chords.Length);

            switch (scoreBehaviourOption)
            {
                case ScoreBehaviourOptions.GUESS_KEYS:
                    this.behaviour = new GuessKeyBehaviour(elements, correctColor, incorrectColor, hintColor);
                    break;
                case ScoreBehaviourOptions.SCROLL:
                    this.behaviour = new ScrollBehaviour(elements, correctColor, incorrectColor, hintColor);
                    break;
                case ScoreBehaviourOptions.GUESS_NOTES:
                    this.behaviour = new GuessNoteBehaviour(
                        elements,
                        optionsHolder,
                        data.MultiScoreNotes,
                        MIDIOptions,
                        correctColor,
                        incorrectColor,
                        hintColor
                    );
                    break;
                default:
                    break;
            }

            AdaptStaffsToDimensions(globalScoreBoxSize, staffsXYDims);

            UnSuscribePianoKeys();  // Prevent double subscription
            SuscribePianoKeys();

            PostSubscription();

            this.behaviour.RoundEnd += RoundEnded;
        }

        void PostSubscription()
        {
            this.behaviour.PostSubscription();
        }

        void RoundEnded(object source, EventArgs e)
        {
            UnSuscribePianoKeys();

            if (dataIndex < (Data.Count - 1))
            {
                dataIndex++;

                this.behaviour = null;

                CreateScore();
            }
        }

        void CreateSheetMusic(ScoreDataHolder data, in Vector3 scoreBoardBoxSize, out Vector3 staffsXYDims)
        {
            SheetMusic sheet;
            switch (data.behaviourOption)
            {
                case ScoreBehaviourOptions.GUESS_KEYS:
                    sheet = CreateSingleScore(scoreBoardBoxSize, data.SingleScoreNotes);
                    break;
                case ScoreBehaviourOptions.SCROLL:
                    sheet = CreateSingleScore(scoreBoardBoxSize, data.MIDIFile);
                    break;
                default:
                    sheet = null;
                    staffsXYDims = Vector3.zero;
                    return;
            }

            var staffsGO = elements.staffs.GameObject;
            sheet.Create(ref staffsGO, out staffsXYDims);
        }

        void SuscribePianoKeys()
        {
            foreach (PianoKey pianoKey in PianoKeys)
            {
                // Piano key press/release events
                pianoKey.KeyPressed += this.behaviour.OnKeyPressed;
                pianoKey.KeyReleased += this.behaviour.OnKeyReleased;

                // Score evaluation events
                this.behaviour.EvaluateBegin += pianoKey.OnEvaluateBegin;
                this.behaviour.EvaluateEnd += pianoKey.OnEvaluateEnd;
            }
        }

        void UnSuscribePianoKeys()
        {
            foreach (PianoKey pianoKey in PianoKeys)
            {
                // Piano key press/release events
                pianoKey.KeyPressed -= this.behaviour.OnKeyPressed;
                pianoKey.KeyReleased -= this.behaviour.OnKeyReleased;

                // Score evaluation events
                this.behaviour.EvaluateBegin -= pianoKey.OnEvaluateBegin;
                this.behaviour.EvaluateEnd -= pianoKey.OnEvaluateEnd;
            }
        }

        SheetMusic CreateSingleScore(in Vector3 scoreBoardBoxSize, MIDIFile midiFile)
        {
            // TODO: change to merge with 
            return new SheetMusic(midiFile, null, factory, (scoreBoardBoxSize.x, scoreBoardBoxSize.y));
        }

        SheetMusic CreateSingleScore(in Vector3 scoreBoardBoxSize, string midiFilePath)
        {
            return new SheetMusic(midiFilePath, MIDIOptions, factory, (scoreBoardBoxSize.x, scoreBoardBoxSize.y));
        }

        SheetMusic CreateSingleScore(in Vector3 scoreBoardBoxSize, ConsecutiveNotes notes)
        {
            return new SheetMusic(notes, MIDIOptions, factory, (scoreBoardBoxSize.x, scoreBoardBoxSize.y));
        }

        void AdaptStaffsToDimensions(Vector3 scoreBoxSize, Vector3 staffsDimensions)
        {
            this.behaviour.AdaptToDimensions(scoreBoxSize, staffsDimensions);
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
            if (this.behaviour != null)
            {
                this.behaviour.RoundEnd -= RoundEnded;
            }

            elements.staffs.Clear();
            elements.staffs.ResetToNormal();
        }
    }
}