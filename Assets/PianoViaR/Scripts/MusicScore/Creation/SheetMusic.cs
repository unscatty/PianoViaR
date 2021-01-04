/*
 * Copyright (c) 2007-2012 Madhav Vaidyanathan
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License version 2.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using PianoViaR.Score.Helpers;
using PianoViaR.Utils;
using PianoViaR.MIDI.Parsing;
using PianoViaR.MIDI.Helpers;
using PianoViaR.Score.Behaviours.Helpers;

namespace PianoViaR.Score.Creation
{


    /** @class SheetMusic
     *
     * The SheetMusic Control is the main class for displaying the sheet music.
     * The SheetMusic class has the following public methods:
     *
     * SheetMusic()
     *   Create a new SheetMusic control from the given midi file and options.
     * 
     * SetZoom()
     *   Set the zoom level to display the sheet music at.
     *
     * DoPrint()
     *   Print a single page of sheet music.
     *
     * GetTotalPages()
     *   Get the total number of sheet music pages.
     *
     * OnPaint()
     *   Method called to draw the SheetMuisc
     *
     * These public methods are called from the MidiSheetMusic Form Window.
     *
     */
    public class SheetMusic
    {
        public const int NotesPerStaff = 24;

        public readonly float PageWidth;    /** The width of each page */
        public readonly float PageHeight;  /** The height of each page (when printing) */
        public const float LeftMargin = 0;
        private List<Staff> staffs; /** The array of staffs to display (from top to bottom) */
        private KeySignature mainkey; /** The main key signature */
        private int numtracks;     /** The number of tracks */
        // private float zoom;          /** The zoom level to draw at (1.0 == 100%) */
        private bool scrollVert;    /** Whether to scroll vertically or horizontally */
        private string filename;      /** The name of the midi file */
        private int showNoteLetters;    /** Display the note letters */
        public MusicSymbolFactory factory;
        private ScoreDimensions dimensions;

        public bool hideKeySignature = false;
        public bool showOnlyChords = false;
        public bool hideAccidentals = false;

        /** Create a new SheetMusic control, using the given parsed MIDIFile.
         *  The options can be null.
         */
        public SheetMusic(MIDIFile file, MIDIOptions options, MusicSymbolFactory factory)
        {
            this.factory = factory;
            Initialize(file, options);
        }

        public SheetMusic(
            MIDIFile file,
            MIDIOptions options,
            MusicSymbolFactory factory,
            (float pageWidth, float pageHeight) pageDimensions
        )
        {
            (PageWidth, PageHeight) = pageDimensions;
            this.factory = factory;
            Initialize(file, options);
        }

        /** Create a new SheetMusic control, using the given midi filename.
         *  The options can be null.
         */
        public SheetMusic(string filename, MIDIOptions options, MusicSymbolFactory factory)
        {
            this.factory = factory;
            MIDIFile file = new MIDIFile(filename);
            Initialize(file, options);
        }

        public SheetMusic(
            string filename,
            MIDIOptions options,
            MusicSymbolFactory factory,
            (float pageWidth, float pageHeight) pageDimensions
        )
        {
            (PageWidth, PageHeight) = pageDimensions;
            this.factory = factory;
            MIDIFile file = new MIDIFile(filename);
            Initialize(file, options);
        }

        /** Create a new SheetMusic control, using the given raw midi byte[] data.
         *  The options can be null.
         */
        public SheetMusic(byte[] data, string title, MIDIOptions options, MusicSymbolFactory factory)
        {
            this.factory = factory;
            MIDIFile file = new MIDIFile(data, title);
            Initialize(file, options);
        }

        public SheetMusic(
            byte[] data,
            string title,
            MIDIOptions options,
            MusicSymbolFactory factory,
            (float pageWidth, float pageHeight) pageDimensions
        )
        {
            (PageWidth, PageHeight) = pageDimensions;
            this.factory = factory;
            MIDIFile file = new MIDIFile(data, title);
            Initialize(file, options);
        }

        public SheetMusic(
            List<MIDITrack> tracks,
            TimeSignature time,
            MIDIOptions options,
            MusicSymbolFactory factory,
            (float pageWidth, float pageHeight) pageDimensions
        )
        {
            (PageWidth, PageHeight) = pageDimensions;
            this.factory = factory;
            Initialize(tracks, options, time);
        }

        public SheetMusic(
            MIDITrack track,
            TimeSignature time,
            MIDIOptions options,
            MusicSymbolFactory factory,
            (float pageWidth, float pageHeight) pageDimensions
        )
        {
            (PageWidth, PageHeight) = pageDimensions;
            this.factory = factory;

            List<MIDITrack> tracks = new List<MIDITrack> { track };

            Initialize(tracks, options, time);
        }

        public SheetMusic(
            List<MIDITrack> tracks,
            MIDIOptions options,
            MusicSymbolFactory factory,
            (float pageWidth, float pageHeight) pageDimensions
        ) : this(tracks, TimeSignature.Default, options, factory, pageDimensions) { }

        public SheetMusic(
            MIDITrack track,
            MIDIOptions options,
            MusicSymbolFactory factory,
            (float pageWidth, float pageHeight) pageDimensions
        ) : this(track, TimeSignature.Default, options, factory, pageDimensions) { }

        public SheetMusic(ConsecutiveNotes notes, MIDIOptions options, MusicSymbolFactory factory, (float pageWidth, float pageHeight) pageDimensions)
        {
            showOnlyChords = true;
            hideKeySignature = true;
            hideAccidentals = true;

            var track = notes.GetTrack();

            (PageWidth, PageHeight) = pageDimensions;
            this.factory = factory;

            List<MIDITrack> tracks = new List<MIDITrack> { track };

            Initialize(tracks, options, notes.TimeSignature);
        }


        /** Create a new SheetMusic control.
         * MIDIFile is the parsed midi file to display.
         * SheetMusic Options are the menu options that were selected.
         *
         * - Apply all the Menu Options to the MIDIFile tracks.
         * - Calculate the key signature
         * - For each track, create a list of MusicSymbols (notes, rests, bars, etc)
         * - Vertically align the music symbols in all the tracks
         * - Partition the music notes into horizontal staffs
         */
        public void Initialize(MIDIFile file, MIDIOptions options)
        {
            if (options == null)
            {
                options = new MIDIOptions(file);
            }

            filename = file.FileName;

            List<MIDITrack> tracks = file.ChangeMidiNotes(options);
            TimeSignature time = file.Time;

            Initialize(tracks, options, time, filename);
        }

        private void Initialize(List<MIDITrack> tracks, MIDIOptions options, TimeSignature time, String name = "")
        {
            SetSizes(tracks.Count);
            scrollVert = options.scrollVert;
            showNoteLetters = options.showNoteLetters;

            filename = name;

            if (options.time != null)
            {
                time = options.time;
            }

            if (options.key == -1)
            {
                mainkey = GetKeySignature(tracks);
            }
            else
            {
                mainkey = new KeySignature(options.key);
            }

            numtracks = tracks.Count;

            int lastStart = EndTime(tracks) + options.shifttime;

            /* Create all the music symbols (notes, rests, vertical bars, and
             * clef changes).  The symbols variable contains a list of music 
             * symbols for each track.  The list does not include the left-side 
             * Clef and key signature symbols.  Those can only be calculated 
             * when we create the staffs.
             */
            List<MusicSymbol>[] symbols = new List<MusicSymbol>[numtracks];
            for (int tracknum = 0; tracknum < numtracks; tracknum++)
            {
                MIDITrack track = tracks[tracknum];
                ClefMeasures clefs = new ClefMeasures(track.Notes, time.Measure);
                List<ChordSymbol> chords = CreateChords(track.Notes, mainkey, time, clefs, track.Instrument);
                symbols[tracknum] = CreateSymbols(chords, clefs, time, lastStart);
            }

            List<LyricSymbol>[] lyrics = null;
            if (options.showLyrics)
            {
                lyrics = GetLyrics(tracks, in dimensions);
            }

            /* Vertically align the music symbols */
            SymbolWidths widths = new SymbolWidths(symbols, lyrics);
            // SymbolWidths widths = new SymbolWidths(symbols);
            AlignSymbols(symbols, widths);

            staffs = CreateStaffs(symbols, mainkey, options, time.Measure);
            CreateAllBeamedChords(symbols, time);
            if (lyrics != null)
            {
                AddLyricsToStaffs(staffs, lyrics);
            }

            /* After making chord pairs, the stem directions can change,
             * which affects the staff height.  Re-calculate the staff height.
             */
            foreach (Staff staff in staffs)
            {
                staff.CalculateHeight();
            }
        }

        /** Return the last start time */
        private int EndTime(MIDIFile file)
        {
            return file.EndTime();
        }

        private int EndTime(List<MIDITrack> tracks)
        {
            int lastStart = 0;
            foreach (MIDITrack track in tracks)
            {
                if (track.Notes.Count == 0)
                {
                    continue;
                }
                int last = track.Notes[track.Notes.Count - 1].StartTime;
                lastStart = Math.Max(last, lastStart);
            }
            return lastStart;
        }

        private void SetSizes()
        {
            Ensure.ArgumentNotNull(factory);

            var noteHeadBoxDimensions = factory.noteHead.BoxSize();

            SetSizes(noteHeadBoxDimensions.y, noteHeadBoxDimensions.x);
        }

        public void SetSizes(in int staffs)
        {
            const int notesPerStaff = NotesPerStaff;
            var totalNotes = notesPerStaff * (staffs);

            var noteHeadHeight = PageHeight / totalNotes;
            SetSizes(noteHeadHeight);
        }

        private void SetSizes(float noteHeadHeight)
        {
            Ensure.ArgumentNotNull(factory);

            var noteHeadBoxDimensions = factory.noteHead.BoxSize();

            if (noteHeadHeight < 0)
            {
                SetSizes(noteHeadBoxDimensions.y, noteHeadBoxDimensions.x);
            }
            else
            {
                var scale = noteHeadHeight / noteHeadBoxDimensions.y;
                var noteHeadWidth = noteHeadBoxDimensions.x * scale;
                SetSizes(noteHeadHeight, noteHeadWidth);
            }
        }

        /** Set the size of the notes, large or small.  Smaller notes means
         * more notes per staff.
         */
        public void SetSizes(float noteHeadHeight, float noteHeadWidth)
        {
            dimensions = new ScoreDimensions(noteHeadHeight, noteHeadWidth, PageWidth, PageHeight, LeftMargin);
        }


        /** Get the best key signature given the midi notes in all the tracks. */
        private KeySignature GetKeySignature(List<MIDITrack> tracks)
        {
            List<int> notenums = new List<int>();
            foreach (MIDITrack track in tracks)
            {
                foreach (MIDINote note in track.Notes)
                {
                    notenums.Add(note.Number);
                }
            }
            return KeySignature.Guess(notenums, in dimensions);
        }


        /** Create the chord symbols for a single track.
         * @param midinotes  The Midinotes in the track.
         * @param key        The Key Signature, for determining sharps/flats.
         * @param time       The Time Signature, for determining the measures.
         * @param clefs      The clefs to use for each measure.
         * @ret An array of ChordSymbols
         */
        private
        List<ChordSymbol> CreateChords(List<MIDINote> midinotes,
                                       KeySignature key,
                                       TimeSignature time,
                                       ClefMeasures clefs, int instrument)
        {

            int i = 0;
            List<ChordSymbol> chords = new List<ChordSymbol>();
            List<MIDINote> notegroup = new List<MIDINote>(12);
            int len = midinotes.Count;

            while (i < len)
            {

                int starttime = midinotes[i].StartTime;
                Clef clef = clefs.GetClef(starttime);

                /* Group all the midi notes with the same start time
                 * into the notes list.
                 */
                notegroup.Clear();
                notegroup.Add(midinotes[i]);
                i++;
                while (i < len && midinotes[i].StartTime == starttime)
                {
                    notegroup.Add(midinotes[i]);
                    i++;
                }

                /* Create a single chord from the group of midi notes with
                 * the same start time.
                 */
                ChordSymbol chord = new ChordSymbol(notegroup, key, time, clef, this, instrument, in dimensions);
                chords.Add(chord);
            }

            return chords;
        }

        /** Given the chord symbols for a track, create a new symbol list
         * that contains the chord symbols, vertical bars, rests, and clef changes.
         * Return a list of symbols (ChordSymbol, BarSymbol, RestSymbol, ClefSymbol)
         */
        private List<MusicSymbol>
        CreateSymbols(List<ChordSymbol> chords, ClefMeasures clefs,
                      TimeSignature time, int lastStart)
        {

            List<MusicSymbol> symbols = new List<MusicSymbol>();
            symbols = AddBars(chords, time, lastStart);
            symbols = AddRests(symbols, time);
            symbols = AddClefChanges(symbols, clefs, time);

            return symbols;
        }

        /** Add in the vertical bars delimiting measures. 
         *  Also, add the time signature symbols.
         */
        private
        List<MusicSymbol> AddBars(List<ChordSymbol> chords, TimeSignature time,
                                  int lastStart)
        {

            List<MusicSymbol> symbols = new List<MusicSymbol>();

            TimeSigSymbol timesig = new TimeSigSymbol(time.Numerator, time.Denominator, in dimensions);
            symbols.Add(timesig);

            /* The starttime of the beginning of the measure */
            int measuretime = 0;

            int i = 0;
            while (i < chords.Count)
            {
                if (measuretime <= chords[i].StartTime)
                {
                    symbols.Add(new BarSymbol(measuretime, in dimensions));
                    measuretime += time.Measure;
                }
                else
                {
                    symbols.Add(chords[i]);
                    i++;
                }
            }

            /* Keep adding bars until the last StartTime (the end of the song) */
            while (measuretime < lastStart)
            {
                symbols.Add(new BarSymbol(measuretime, in dimensions));
                measuretime += time.Measure;
            }

            /* Add the final vertical bar to the last measure */
            symbols.Add(new BarSymbol(measuretime, in dimensions));
            return symbols;
        }

        /** Add rest symbols between notes.  All times below are 
         * measured in pulses.
         */
        private
        List<MusicSymbol> AddRests(List<MusicSymbol> symbols, TimeSignature time)
        {
            int prevtime = 0;

            List<MusicSymbol> result = new List<MusicSymbol>(symbols.Count);

            foreach (MusicSymbol symbol in symbols)
            {
                int starttime = symbol.StartTime;
                RestSymbol[] rests = GetRests(time, prevtime, starttime);
                if (rests != null)
                {
                    foreach (RestSymbol r in rests)
                    {
                        result.Add(r);
                    }
                }

                result.Add(symbol);

                /* Set prevtime to the end time of the last note/symbol. */
                if (symbol is ChordSymbol)
                {
                    ChordSymbol chord = (ChordSymbol)symbol;
                    prevtime = Math.Max(chord.EndTime, prevtime);
                }
                else
                {
                    prevtime = Math.Max(starttime, prevtime);
                }
            }
            return result;
        }

        /** Return the rest symbols needed to fill the time interval between
         * start and end.  If no rests are needed, return nil.
         */
        private
        RestSymbol[] GetRests(TimeSignature time, int start, int end)
        {
            RestSymbol[] result;
            RestSymbol r1, r2;

            if (end - start < 0)
                return null;

            NoteDuration dur = time.GetNoteDuration(end - start);
            switch (dur)
            {
                case NoteDuration.Whole:
                case NoteDuration.Half:
                case NoteDuration.Quarter:
                case NoteDuration.Eighth:
                    r1 = new RestSymbol(start, dur, in dimensions);
                    result = new RestSymbol[] { r1 };
                    return result;

                case NoteDuration.DottedHalf:
                    r1 = new RestSymbol(start, NoteDuration.Half, in dimensions);
                    r2 = new RestSymbol(start + time.Quarter * 2,
                                        NoteDuration.Quarter, in dimensions);
                    result = new RestSymbol[] { r1, r2 };
                    return result;

                case NoteDuration.DottedQuarter:
                    r1 = new RestSymbol(start, NoteDuration.Quarter, in dimensions);
                    r2 = new RestSymbol(start + time.Quarter,
                                        NoteDuration.Eighth, in dimensions);
                    result = new RestSymbol[] { r1, r2 };
                    return result;

                case NoteDuration.DottedEighth:
                    r1 = new RestSymbol(start, NoteDuration.Eighth, in dimensions);
                    r2 = new RestSymbol(start + time.Quarter / 2,
                                        NoteDuration.Sixteenth, in dimensions);
                    result = new RestSymbol[] { r1, r2 };
                    return result;

                default:
                    return null;
            }
        }

        /** The current clef is always shown at the beginning of the staff, on
         * the left side.  However, the clef can also change from measure to 
         * measure. When it does, a Clef symbol must be shown to indicate the 
         * change in clef.  This function adds these Clef change symbols.
         * This function does not add the main Clef Symbol that begins each
         * staff.  That is done in the Staff() contructor.
         */
        private
        List<MusicSymbol> AddClefChanges(List<MusicSymbol> symbols,
                                         ClefMeasures clefs,
                                         TimeSignature time)
        {

            List<MusicSymbol> result = new List<MusicSymbol>(symbols.Count);
            Clef prevclef = clefs.GetClef(0);
            foreach (MusicSymbol symbol in symbols)
            {
                /* A BarSymbol indicates a new measure */
                if (symbol is BarSymbol)
                {
                    Clef clef = clefs.GetClef(symbol.StartTime);
                    if (clef != prevclef)
                    {
                        result.Add(new ClefSymbol(clef, symbol.StartTime - 1, true, in dimensions));
                    }
                    prevclef = clef;
                }
                result.Add(symbol);
            }
            return result;
        }


        /** Notes with the same start times in different staffs should be
         * vertically aligned.  The SymbolWidths class is used to help 
         * vertically align symbols.
         *
         * First, each track should have a symbol for every starttime that
         * appears in the Midi File.  If a track doesn't have a symbol for a
         * particular starttime, then add a "blank" symbol for that time.
         *
         * Next, make sure the symbols for each start time all have the same
         * width, across all tracks.  The SymbolWidths class stores
         * - The symbol width for each starttime, for each track
         * - The maximum symbol width for a given starttime, across all tracks.
         *
         * The method SymbolWidths.GetExtraWidth() returns the extra width
         * needed for a track to match the maximum symbol width for a given
         * starttime.
         */
        private
        void AlignSymbols(List<MusicSymbol>[] allsymbols, SymbolWidths widths)
        {

            for (int track = 0; track < allsymbols.Length; track++)
            {
                List<MusicSymbol> symbols = allsymbols[track];
                List<MusicSymbol> result = new List<MusicSymbol>();

                int i = 0;

                /* If a track doesn't have a symbol for a starttime,
                 * add a blank symbol.
                 */
                foreach (int start in widths.StartTimes)
                {

                    /* BarSymbols are not included in the SymbolWidths calculations */
                    while (i < symbols.Count && (symbols[i] is BarSymbol) &&
                        symbols[i].StartTime <= start)
                    {
                        result.Add(symbols[i]);
                        i++;
                    }

                    if (i < symbols.Count && symbols[i].StartTime == start)
                    {

                        while (i < symbols.Count &&
                               symbols[i].StartTime == start)
                        {

                            result.Add(symbols[i]);
                            i++;
                        }
                    }
                    else
                    {
                        result.Add(new BlankSymbol(start, 0));
                    }
                }

                /* For each starttime, increase the symbol width by
                 * SymbolWidths.GetExtraWidth().
                 */
                i = 0;
                while (i < result.Count)
                {
                    if (result[i] is BarSymbol)
                    {
                        i++;
                        continue;
                    }
                    int start = result[i].StartTime;
                    float extra = widths.GetExtraWidth(track, start);
                    result[i].Width += extra;

                    /* Skip all remaining symbols with the same starttime. */
                    while (i < result.Count && result[i].StartTime == start)
                    {
                        i++;
                    }
                }
                allsymbols[track] = result;
            }
        }

        private static bool IsChord(MusicSymbol symbol)
        {
            return symbol is ChordSymbol;
        }


        /** Find 2, 3, 4, or 6 chord symbols that occur consecutively (without any
         *  rests or bars in between).  There can be BlankSymbols in between.
         *
         *  The startIndex is the index in the symbols to start looking from.
         *
         *  Store the indexes of the consecutive chords in chordIndexes.
         *  Store the horizontal distance (pixels) between the first and last chord.
         *  If we failed to find consecutive chords, return false.
         */
        private static bool
        FindConsecutiveChords(List<MusicSymbol> symbols, TimeSignature time,
                              int startIndex, int[] chordIndexes,
                              ref float horizDistance)
        {

            int i = startIndex;
            int numChords = chordIndexes.Length;

            while (true)
            {
                horizDistance = 0;

                /* Find the starting chord */
                while (i < symbols.Count - numChords)
                {
                    if (symbols[i] is ChordSymbol)
                    {
                        ChordSymbol c = (ChordSymbol)symbols[i];
                        if (c.Stem != null)
                        {
                            break;
                        }
                    }
                    i++;
                }
                if (i >= symbols.Count - numChords)
                {
                    chordIndexes[0] = -1;
                    return false;
                }
                chordIndexes[0] = i;
                bool foundChords = true;
                for (int chordIndex = 1; chordIndex < numChords; chordIndex++)
                {
                    i++;
                    int remaining = numChords - 1 - chordIndex;
                    while ((i < symbols.Count - remaining) && (symbols[i] is BlankSymbol))
                    {
                        horizDistance += symbols[i].Width;
                        i++;
                    }
                    if (i >= symbols.Count - remaining)
                    {
                        return false;
                    }
                    if (!(symbols[i] is ChordSymbol))
                    {
                        foundChords = false;
                        break;
                    }

                    chordIndexes[chordIndex] = i;
                    horizDistance += symbols[i].Width;
                }
                if (foundChords)
                {
                    var lastSymbol = symbols[i];
                    // Remove the las chord width, because it is wrong (must be the half)
                    horizDistance -= lastSymbol.Width / 2;

                    return true;
                }

                /* Else, start searching again from index i */
            }
        }


        /** Connect chords of the same duration with a horizontal beam.
         *  numChords is the number of chords per beam (2, 3, 4, or 6).
         *  if startBeat is true, the first chord must start on a quarter note beat.
         */
        private static void
        CreateBeamedChords(List<MusicSymbol>[] allsymbols, TimeSignature time,
                           int numChords, bool startBeat)
        {
            int[] chordIndexes = new int[numChords];
            ChordSymbol[] chords = new ChordSymbol[numChords];

            foreach (List<MusicSymbol> symbols in allsymbols)
            {
                int startIndex = 0;
                while (true)
                {
                    float horizDistance = 0;
                    bool found = FindConsecutiveChords(symbols, time,
                                                       startIndex,
                                                       chordIndexes,
                                                       ref horizDistance);
                    if (!found)
                    {
                        break;
                    }
                    for (int i = 0; i < numChords; i++)
                    {
                        chords[i] = (ChordSymbol)symbols[chordIndexes[i]];
                    }

                    if (ChordSymbol.CanCreateBeam(chords, time, startBeat))
                    {
                        ChordSymbol.CreateBeam(chords, horizDistance);
                        startIndex = chordIndexes[numChords - 1] + 1;
                    }
                    else
                    {
                        startIndex = chordIndexes[0] + 1;
                    }

                    /* What is the value of startIndex here?
                     * If we created a beam, we start after the last chord.
                     * If we failed to create a beam, we start after the first chord.
                     */
                }
            }
        }


        /** Connect chords of the same duration with a horizontal beam.
         *
         *  We create beams in the following order:
         *  - 6 connected 8th note chords, in 3/4, 6/8, or 6/4 time
         *  - Triplets that start on quarter note beats
         *  - 3 connected chords that start on quarter note beats (12/8 time only)
         *  - 4 connected chords that start on quarter note beats (4/4 or 2/4 time only)
         *  - 2 connected chords that start on quarter note beats
         *  - 2 connected chords that start on any beat
         */
        private static void
        CreateAllBeamedChords(List<MusicSymbol>[] allsymbols, TimeSignature time)
        {
            if ((time.Numerator == 3 && time.Denominator == 4) ||
                (time.Numerator == 6 && time.Denominator == 8) ||
                (time.Numerator == 6 && time.Denominator == 4))
            {

                CreateBeamedChords(allsymbols, time, 6, true);
            }
            CreateBeamedChords(allsymbols, time, 3, true);
            CreateBeamedChords(allsymbols, time, 4, true);
            CreateBeamedChords(allsymbols, time, 2, true);
            CreateBeamedChords(allsymbols, time, 2, false);
        }

        /** Get the width (in pixels) needed to display the key signature */
        public static float
        KeySignatureWidth(KeySignature key, in ScoreDimensions dimensions)
        {
            ClefSymbol clefsym = new ClefSymbol(Clef.Treble, 0, false, in dimensions);
            float result = clefsym.MinWidth;
            AccidSymbol[] keys = key.GetSymbols(Clef.Treble);
            foreach (AccidSymbol symbol in keys)
            {
                result += symbol.MinWidth;
            }
            return result + SheetMusic.LeftMargin;
        }


        /** Given MusicSymbols for a track, create the staffs for that track.
         *  Each Staff has a maxmimum width of PageWidth (800 pixels).
         *  Also, measures should not span multiple Staffs.
         */
        private List<Staff>
        CreateStaffsForTrack(List<MusicSymbol> symbols, int measurelen,
                             KeySignature key, MIDIOptions options,
                             int track, int totaltracks)
        {
            float keysigWidth = KeySignatureWidth(key, in dimensions);
            int startindex = 0;
            List<Staff> thestaffs = new List<Staff>(symbols.Count / 50);

            while (startindex < symbols.Count)
            {
                /* startindex is the index of the first symbol in the staff.
                 * endindex is the index of the last symbol in the staff.
                 */
                int endindex = startindex;
                float width = keysigWidth;
                float maxwidth;

                /* If we're scrolling vertically, the maximum width is PageWidth. */
                if (scrollVert)
                {
                    maxwidth = PageWidth;
                }
                else
                {
                    maxwidth = 2000000;
                }

                while (endindex < symbols.Count &&
                       width + symbols[endindex].Width < maxwidth)
                {

                    width += symbols[endindex].Width;
                    endindex++;
                }
                endindex--;

                /* There's 3 possibilities at this point:
                 * 1. We have all the symbols in the track.
                 *    The endindex stays the same.
                 *
                 * 2. We have symbols for less than one measure.
                 *    The endindex stays the same.
                 *
                 * 3. We have symbols for 1 or more measures.
                 *    Since measures cannot span multiple staffs, we must
                 *    make sure endindex does not occur in the middle of a
                 *    measure.  We count backwards until we come to the end
                 *    of a measure.
                 */

                if (endindex == symbols.Count - 1)
                {
                    /* endindex stays the same */
                }
                else if (symbols[startindex].StartTime / measurelen ==
                         symbols[endindex].StartTime / measurelen)
                {
                    /* endindex stays the same */
                }
                else
                {
                    int endmeasure = symbols[endindex + 1].StartTime / measurelen;
                    while (symbols[endindex].StartTime / measurelen ==
                           endmeasure)
                    {
                        endindex--;
                    }
                }
                int range = endindex + 1 - startindex;
                if (scrollVert)
                {
                    width = PageWidth;
                }
                Staff staff = new Staff(symbols.GetRange(startindex, range),
                                        key, options, track, totaltracks, in dimensions, hideKeySignature, showOnlyChords);
                thestaffs.Add(staff);
                startindex = endindex + 1;
            }
            return thestaffs;
        }


        /** Given all the MusicSymbols for every track, create the staffs
         * for the sheet music.  There are two parts to this:
         *
         * - Get the list of staffs for each track.
         *   The staffs will be stored in trackstaffs as:
         *
         *   trackstaffs[0] = { Staff0, Staff1, Staff2, ... } for track 0
         *   trackstaffs[1] = { Staff0, Staff1, Staff2, ... } for track 1
         *   trackstaffs[2] = { Staff0, Staff1, Staff2, ... } for track 2
         *
         * - Store the Staffs in the staffs list, but interleave the
         *   tracks as follows:
         *
         *   staffs = { Staff0 for track 0, Staff0 for track1, Staff0 for track2,
         *              Staff1 for track 0, Staff1 for track1, Staff1 for track2,
         *              Staff2 for track 0, Staff2 for track1, Staff2 for track2,
         *              ... } 
         */
        private List<Staff>
        CreateStaffs(List<MusicSymbol>[] allsymbols, KeySignature key,
                     MIDIOptions options, int measurelen)
        {

            List<Staff>[] trackstaffs = new List<Staff>[allsymbols.Length];
            int totaltracks = trackstaffs.Length;

            for (int track = 0; track < totaltracks; track++)
            {
                List<MusicSymbol> symbols = allsymbols[track];
                trackstaffs[track] = CreateStaffsForTrack(symbols, measurelen, key, options, track, totaltracks);
            }

            /* Update the EndTime of each Staff. EndTime is used for playback */
            foreach (List<Staff> list in trackstaffs)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    list[i].EndTime = list[i + 1].StartTime;
                }
            }

            /* Interleave the staffs of each track into the result array. */
            int maxstaffs = 0;
            for (int i = 0; i < trackstaffs.Length; i++)
            {
                if (maxstaffs < trackstaffs[i].Count)
                {
                    maxstaffs = trackstaffs[i].Count;
                }
            }
            List<Staff> result = new List<Staff>(maxstaffs * trackstaffs.Length);
            for (int i = 0; i < maxstaffs; i++)
            {
                foreach (List<Staff> list in trackstaffs)
                {
                    if (i < list.Count)
                    {
                        result.Add(list[i]);
                    }
                }
            }
            return result;
        }

        /** Get the lyrics for each track */
        private static List<LyricSymbol>[]
        GetLyrics(List<MIDITrack> tracks, in ScoreDimensions dimensions)
        {
            bool hasLyrics = false;
            List<LyricSymbol>[] result = new List<LyricSymbol>[tracks.Count];
            for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
            {
                MIDITrack track = tracks[tracknum];
                if (track.Lyrics == null)
                {
                    continue;
                }
                hasLyrics = true;
                result[tracknum] = new List<LyricSymbol>();
                foreach (MIDIEvent ev in track.Lyrics)
                {
                    String text = System.Text.Encoding.UTF8.GetString(ev.Value, 0, ev.Value.Length);
                    LyricSymbol sym = new LyricSymbol(ev.StartTime, text, in dimensions);
                    result[tracknum].Add(sym);
                }
            }
            if (!hasLyrics)
            {
                return null;
            }
            else
            {
                return result;
            }
        }

        /** Add the lyric symbols to the corresponding staffs */
        static void
        AddLyricsToStaffs(List<Staff> staffs, List<LyricSymbol>[] tracklyrics)
        {
            foreach (Staff staff in staffs)
            {
                List<LyricSymbol> lyrics = tracklyrics[staff.Track];
                staff.AddLyrics(lyrics);
            }
        }

        /** Get whether to show note letters or not */
        public int ShowNoteLetters
        {
            get { return showNoteLetters; }
        }

        /** Get the main key signature */
        public KeySignature MainKey
        {
            get { return mainkey; }
        }

        public GameObject Create(Vector3 position)
        {
            Vector3 _;
            return Create(position, out _);
        }

        public GameObject Create()
        {
            return Create(Vector3.zero);
        }

        public GameObject Create(out Vector3 dimensions)
        {
            return Create(Vector3.zero, out dimensions);
        }

        /** Print the given page of the sheet music. 
         * Page numbers start from 1.
         * A staff should fit within a single page, not be split across two pages.
         * If the sheet music has exactly 2 tracks, then two staffs should
         * fit within a single page, and not be split across two pages.
         */
        public GameObject Create(Vector3 position, out Vector3 dimensions)
        {
            GameObject staffsGO = new GameObject("staffs");

            Create(ref staffsGO, position, out dimensions);

            return staffsGO;
        }

        public void Create(ref GameObject parent, out Vector3 dimensions)
        {
            Create(ref parent, parent.transform.position, out dimensions);
        }

        public void Create(ref GameObject parent)
        {
            Vector3 _;
            Create(ref parent, out _);
        }

        public void Create(ref GameObject parent, Vector3 position, out Vector3 dimensions)
        {
            float viewPageHeight = PageHeight;
            float ypos = 0;
            int staffnum = 0;

            float totaHeight = 0;
            foreach (var staff in staffs)
                totaHeight += staff.Height;

            float totalWidth = staffs[0]?.Width ?? 0;

            var topLeft = position + new Vector3(-totalWidth / 2, totaHeight / 2);

            if (numtracks == 2 && (staffs.Count % 2) == 0)
            {
                ypos = 0;

                for (; staffnum + 1 < staffs.Count; staffnum += 2)
                {
                    var newPosition = new Vector3(0, -ypos) + topLeft;

                    var evenStaff = staffs[staffnum];
                    evenStaff.hideKeySignature = hideKeySignature;
                    evenStaff.showOnlyChords = showOnlyChords;

                    var staffTrack1 = evenStaff.Create(factory, newPosition);
                    staffTrack1.name = $"staffTrack{staffnum + 1}";
                    staffTrack1.transform.position = parent.transform.position;
                    staffTrack1.transform.rotation = parent.transform.rotation;
                    staffTrack1.transform.SetParent(parent.transform);

                    ypos += staffs[staffnum].Height;

                    newPosition.y -= ypos;

                    var oddStafff = staffs[staffnum + 1];
                    oddStafff.hideKeySignature = hideKeySignature;
                    oddStafff.showOnlyChords = showOnlyChords;

                    var staffTrack2 = oddStafff.Create(factory, newPosition);
                    staffTrack2.name = $"staffTrack{staffnum + 2}";
                    staffTrack2.transform.position = parent.transform.position;
                    staffTrack2.transform.rotation = parent.transform.rotation;
                    staffTrack2.transform.SetParent(parent.transform);

                    ypos += staffs[staffnum + 1].Height;
                }
            }
            else
            {
                ypos = 0;

                for (; staffnum < staffs.Count; staffnum++)
                {
                    var newPosition = new Vector3(0, -ypos) + topLeft;

                    var currentStaff = staffs[staffnum];
                    currentStaff.showOnlyChords = showOnlyChords;
                    currentStaff.hideKeySignature = hideKeySignature;

                    var staffGO = currentStaff.Create(factory, newPosition);
                    staffGO.name = $"staffTrack{staffnum + 1}";
                    staffGO.transform.position = parent.transform.position;
                    staffGO.transform.rotation = parent.transform.rotation;
                    staffGO.transform.SetParent(parent.transform);

                    ypos += staffs[staffnum].Height;
                }
            }

            dimensions = new Vector3(x: totalWidth, y: totaHeight);
        }

        /**
         * Return the number of pages needed to print this sheet music.
         * A staff should fit within a single page, not be split across two pages.
         * If the sheet music has exactly 2 tracks, then two staffs should
         * fit within a single page, and not be split across two pages.
         */
        public int GetTotalPages()
        {
            int num = 1;
            // int currheight = TitleHeight;
            float currheight = 0;

            if (numtracks == 2 && (staffs.Count % 2) == 0)
            {
                for (int i = 0; i < staffs.Count; i += 2)
                {
                    float heights = staffs[i].Height + staffs[i + 1].Height;
                    if (currheight + heights > PageHeight)
                    {
                        num++;
                        currheight = heights;
                    }
                    else
                    {
                        currheight += heights;
                    }
                }
            }
            else
            {
                foreach (Staff staff in staffs)
                {
                    if (currheight + staff.Height > PageHeight)
                    {
                        num++;
                        currheight = staff.Height;
                    }
                    else
                    {
                        currheight += staff.Height;
                    }
                }
            }
            return num;
        }

        public override string ToString()
        {
            string result = "SheetMusic staffs=" + staffs.Count + "\n";
            foreach (Staff staff in staffs)
            {
                result += staff.ToString();
            }
            result += "End SheetMusic\n";
            return result;
        }
    }
}
