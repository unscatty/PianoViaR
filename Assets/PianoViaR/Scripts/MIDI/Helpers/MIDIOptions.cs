/*
 * Copyright (c) 2007-2013 Madhav Vaidyanathan
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License version 2.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using System.IO;
using PianoViaR.MIDI.Parsing;

namespace PianoViaR.MIDI.Helpers
{

    /** @class MidiOptions
     *
     * The MidiOptions class contains the available options for
     * modifying the sheet music and sound.  These options are
     * collected from the menu/dialog settings, and then are passed
     * to the SheetMusic and MidiPlayer classes.
     */
    public enum NoteLetterType
    {
        NoteNameNone,
        NoteNameLetter,
        NoteNameFixedDoReMi,
        NoteNameMovableDoReMi,
        NoteNameFixedNumber,
        NoteNameMovableNumber,
    }
    [System.Serializable]
    public class MIDIOptions
    {

        // The possible values for showNoteLetters
        public NoteLetterType namingType;
        // Sheet Music Options
        public string filename;       /** The full Midi filename */
        public string title;          /** The Midi song title */
        public bool[] tracks;         /** Which tracks to display (true = display) */
        public bool scrollVert;       /** Whether to scroll vertically or horizontally */
        public bool largeNoteSize;    /** Display large or small note sizes */
        public bool twoStaffs;        /** Combine tracks into two staffs ? */
        public bool showLyrics;       /** Show the lyrics under each note */
        public bool showMeasures;     /** Show the measure numbers for each staff */
        public int shifttime;         /** Shift note starttimes by the given amount */
        public int transpose;         /** Shift note key up/down by given amount */
        public int key;               /** Use the given KeySignature (notescale) */
        public TimeSignature time;    /** Use the given time signature */
        public int combineInterval;   /** Combine notes within given time interval (msec) */
        // public Color[] colors;        /** The note colors to use */
        // public Color shadeColor;      /** The color to use for shading. */
        // public Color shade2Color;     /** The color to use for shading the left hand piano */

        // Sound options
        public bool[] mute;            /** Which tracks to mute (true = mute) */
        public int tempo;              /** The tempo, in microseconds per quarter note */
        public int pauseTime;          /** Start the midi music at the given pause time */
        public int[] instruments;      /** The instruments to use per track */
        public bool useDefaultInstruments;  /** If true, don't change instruments */
        public bool playMeasuresInLoop;     /** Play the selected measures in a loop */
        public int playMeasuresInLoopStart; /** Start measure to play in loop */
        public int playMeasuresInLoopEnd;   /** End measure to play in loop */

        public static MIDIOptions Default
        {
            get { return new MIDIOptions(); }
        }

        public MIDIOptions(TimeSignature timeSignature)
        {
            scrollVert = false;
            namingType = NoteLetterType.NoteNameNone;
            key = -1;
            shifttime = 0;
            showLyrics = false;
            showMeasures = false;
            time = timeSignature;
            useDefaultInstruments = true;
            largeNoteSize = false;
            twoStaffs = false;
            transpose = 0;
            combineInterval = 40;
            tempo = time.Tempo;
            pauseTime = 0;
            playMeasuresInLoop = false;
            playMeasuresInLoopStart = 0;
            playMeasuresInLoopEnd = 0;
        }

        public MIDIOptions()
        : this(TimeSignature.Default)
        { }

        public MIDIOptions(MIDIFile midifile)
        {
            filename = midifile.FileName;
            title = Path.GetFileName(midifile.FileName);
            int numtracks = midifile.Tracks.Count;
            tracks = new bool[numtracks];
            mute = new bool[numtracks];
            instruments = new int[numtracks];
            for (int i = 0; i < tracks.Length; i++)
            {
                tracks[i] = true;
                mute[i] = false;
                instruments[i] = midifile.Tracks[i].Instrument;
                // Percussion instrument
                if (midifile.Tracks[i].Instrument == 128)
                {
                    tracks[i] = false;
                    mute[i] = true;
                }
            }
            useDefaultInstruments = true;
            scrollVert = true;
            largeNoteSize = false;
            if (tracks.Length == 1)
            {
                twoStaffs = true;
            }
            else
            {
                twoStaffs = false;
            }
            namingType = NoteLetterType.NoteNameNone;
            showLyrics = true;
            showMeasures = false;
            shifttime = 0;
            transpose = 0;
            key = -1;
            time = midifile.Time;
            // colors = null;
            // shadeColor = Color.FromArgb(210, 205, 220);
            // shade2Color = Color.FromArgb(80, 100, 250);
            combineInterval = 40;
            tempo = midifile.Time.Tempo;
            pauseTime = 0;
            playMeasuresInLoop = false;
            playMeasuresInLoopStart = 0;
            playMeasuresInLoopEnd = midifile.EndTime() / midifile.Time.Measure;

            // Change this
            // twoStaffs = false;
            scrollVert = false;
            // showMeasures = true;
            // showNoteLetters = NoteNameFixedDoReMi;
            // transpose = 12;
        }

        /* Merge in the saved options to this MidiOptions.*/
        public void Merge(MIDIOptions saved)
        {
            if (saved.tracks != null && saved.tracks.Length == tracks.Length)
            {
                for (int i = 0; i < tracks.Length; i++)
                {
                    tracks[i] = saved.tracks[i];
                }
            }
            if (saved.mute != null && saved.mute.Length == mute.Length)
            {
                for (int i = 0; i < mute.Length; i++)
                {
                    mute[i] = saved.mute[i];
                }
            }
            if (saved.instruments != null && saved.instruments.Length == instruments.Length)
            {
                for (int i = 0; i < instruments.Length; i++)
                {
                    instruments[i] = saved.instruments[i];
                }
            }
            // if (saved.time != null)
            // {
            //     time = new TimeSignature(saved.time.Numerator, saved.time.Denominator,
            //             saved.time.Quarter, saved.time.Tempo);
            // }
            useDefaultInstruments = saved.useDefaultInstruments;
            scrollVert = saved.scrollVert;
            largeNoteSize = saved.largeNoteSize;
            showLyrics = saved.showLyrics;
            twoStaffs = saved.twoStaffs;
            namingType = saved.namingType;
            transpose = saved.transpose;
            key = saved.key;
            combineInterval = saved.combineInterval;
            showMeasures = saved.showMeasures;
            playMeasuresInLoop = saved.playMeasuresInLoop;
            playMeasuresInLoopStart = saved.playMeasuresInLoopStart;
            playMeasuresInLoopEnd = saved.playMeasuresInLoopEnd;
        }
    }
}