/*
 * Copyright (c) 2007-2011 Madhav Vaidyanathan
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
using PianoViaR.MIDI.Parsing;
using PianoViaR.MIDI.Helpers;
using PianoViaR.Score.Helpers;
using PianoViaR.Utils;

namespace PianoViaR.Score.Creation
{

    public enum StemDir { Up, Down };

    /** @class NoteData
     *  Contains fields for displaying a single note in a chord.
     */
    public class NoteData
    {
        public int number;             /** The Midi note number, used to determine the color */
        public WhiteNote whiteNote;    /** The white note location to draw */
        public NoteDuration duration;  /** The duration of the note */
        public bool leftSide;          /** Whether to draw note to the left or right of the stem */
        public Accid accid;            /** Used to create the AccidSymbols for the chord */
    };

    /** @class ChordSymbol
     * A chord symbol represents a group of notes that are played at the same
     * time.  A chord includes the notes, the accidental symbols for each
     * note, and the stem (or stems) to use.  A single chord may have two 
     * stems if the notes have different durations (e.g. if one note is a
     * quarter note, and another is an eighth note).
     */
    public class ChordSymbol : MusicSymbol
    {
        private Clef clef;             /** Which clef the chord is being drawn in */
        private int startTime;         /** The time (in pulses) the notes occurs at */
        private int endTime;           /** The starttime plus the longest note duration */
        private NoteData[] noteData;   /** The notes to draw */
        private AccidSymbol[] accidSymbols;   /** The accidental symbols to draw */
        private float width;             /** The width of the chord */
        private Stem stem1;            /** The stem of the chord. Can be null. */
        private Stem stem2;            /** The second stem of the chord. Can be null */
        private bool hasTwoStems;      /** True if this chord has two stems */
        private bool hasOverlap;        /** True if this any note in this chord overlaps another note */
        private bool hasDots = false;   /** True if any note in this chord has a dotted duration */
        private bool hasAccidentals = false; /** True if this chord has any accidental */
        private SheetMusic sheetMusic; /** Used to get colors and other options */
        private WhiteNote topNote;         /** Topmost note in chord */
        private WhiteNote bottomNote;      /** Bottommost note in chord */
        private ScoreDimensions dimensions;

        private (int noteIdx, int accidIdx)[] noteToAccidIndexes;


        /** Create a new Chord Symbol from the given list of midi notes.
         * All the midi notes will have the same start time.  Use the
         * key signature to get the white key and accidental symbol for
         * each note.  Use the time signature to calculate the duration
         * of the notes. Use the clef when drawing the chord.
         */
        public ChordSymbol(List<MIDINote> midinotes, KeySignature key,
                           TimeSignature time, Clef c, SheetMusic sheet, in ScoreDimensions dimensions)
        {
            this.dimensions = dimensions;
            int len = midinotes.Count;
            int i;

            hasTwoStems = false;
            clef = c;
            sheetMusic = sheet;

            startTime = midinotes[0].StartTime;
            endTime = midinotes[0].EndTime;

            for (i = 0; i < midinotes.Count; i++)
            {
                if (i > 1)
                {
                    if (midinotes[i].Number < midinotes[i - 1].Number)
                    {
                        throw new System.ArgumentException("Chord notes not in increasing order by number");
                    }
                }
                endTime = Math.Max(endTime, midinotes[i].EndTime);
            }

            // Creates the notes, also checks if any note has a dotted duration
            noteData = CreateNoteData(midinotes, key, time);
            // Creates the accidental symbols, if any *hasAccidentals* becomes true
            accidSymbols = GetAccidSymbols(noteData, clef);


            /* Find out how many stems we need (1 or 2) */
            NoteDuration dur1 = noteData[0].duration;
            NoteDuration dur2 = dur1;
            int change = -1;
            for (i = 0; i < noteData.Length; i++)
            {
                dur2 = noteData[i].duration;
                if (dur1 != dur2)
                {
                    change = i;
                    break;
                }
            }

            // If any note in this chord overlaps another the position changes
            hasOverlap = false;

            if (dur1 != dur2)
            {
                /* We have notes with different durations.  So we will need
                 * two stems.  The first stem points down, and contains the
                 * bottom note up to the note with the different duration.
                 *
                 * The second stem points up, and contains the note with the
                 * different duration up to the top note.
                 */
                hasTwoStems = true;
                bool stem1Overlaps = NotesOverlap(noteData, 0, change);

                stem1 = new Stem(noteData[0].whiteNote,
                                 noteData[change - 1].whiteNote,
                                 dur1,
                                 Stem.Down,
                                 in dimensions,
                                 stem1Overlaps,
                                 hasDots
                                );

                bool stem2Overlaps = NotesOverlap(noteData, change, noteData.Length);
                stem2 = new Stem(noteData[change].whiteNote,
                                 noteData[noteData.Length - 1].whiteNote,
                                 dur2,
                                 Stem.Up,
                                 in dimensions,
                                 stem2Overlaps,
                                 hasDots
                                );

                hasOverlap = stem1Overlaps || stem2Overlaps;
            }
            else
            {
                /* All notes have the same duration, so we only need one stem. */
                int direction = StemDirection(noteData[0].whiteNote,
                                              noteData[noteData.Length - 1].whiteNote,
                                              clef);

                bool stemOverlaps = NotesOverlap(noteData, 0, noteData.Length);
                stem1 = new Stem(noteData[0].whiteNote,
                                 noteData[noteData.Length - 1].whiteNote,
                                 dur1,
                                 direction,
                                 in dimensions,
                                 stemOverlaps
                                );
                stem2 = null;

                hasOverlap = stemOverlaps;
            }

            bottomNote = noteData[0].whiteNote;
            topNote = noteData[noteData.Length - 1].whiteNote;

            /* For whole notes, no stem is drawn. */
            if (dur1 == NoteDuration.Whole)
                stem1 = null;
            if (dur2 == NoteDuration.Whole)
                stem2 = null;

            width = MinWidth;
        }


        /** Given the raw midi notes (the note number and duration in pulses),
         * calculate the following note data:
         * - The white key
         * - The accidental (if any)
         * - The note duration (half, quarter, eighth, etc)
         * - The side it should be drawn (left or side)
         * By default, notes are drawn on the left side.  However, if two notes
         * overlap (like A and B) you cannot draw the next note directly above it.
         * Instead you must shift one of the notes to the right.
         *
         * The KeySignature is used to determine the white key and accidental.
         * The TimeSignature is used to determine the duration.
         */

        private NoteData[]
        CreateNoteData(List<MIDINote> midinotes, KeySignature key,
                                  TimeSignature time)
        {

            int len = midinotes.Count;
            NoteData[] generatedNoteData = new NoteData[len];

            for (int i = 0; i < len; i++)
            {
                MIDINote midi = midinotes[i];
                generatedNoteData[i] = new NoteData();
                generatedNoteData[i].number = midi.Number;
                generatedNoteData[i].leftSide = true;
                generatedNoteData[i].whiteNote = key.GetWhiteNote(midi.Number);
                generatedNoteData[i].duration = time.GetNoteDuration(midi.EndTime - midi.StartTime);
                generatedNoteData[i].accid = key.GetAccidental(midi.Number, midi.StartTime / time.Measure);

                if (i > 0 && (generatedNoteData[i].whiteNote.Dist(generatedNoteData[i - 1].whiteNote) == 1))
                {
                    /* This note (notedata[i]) overlaps with the previous note.
                     * Change the side of this note.
                     */

                    if (generatedNoteData[i - 1].leftSide)
                    {
                        generatedNoteData[i].leftSide = false;
                    }
                    else
                    {
                        generatedNoteData[i].leftSide = true;
                    }
                }
                else
                {
                    generatedNoteData[i].leftSide = true;
                }
            }

            // Check if any note has a dotted duration
            foreach (NoteData note in generatedNoteData)
            {
                if (note.duration == NoteDuration.DottedHalf ||
                    note.duration == NoteDuration.DottedQuarter ||
                    note.duration == NoteDuration.DottedEighth)
                {
                    hasDots = true;
                    break;
                }
            }

            return generatedNoteData;
        }


        /** Given the note data (the white keys and accidentals), create 
         * the Accidental Symbols and return them.
         */
        private AccidSymbol[]
        GetAccidSymbols(NoteData[] notedata, Clef clef)
        {
            int count = 0;
            foreach (NoteData n in notedata)
            {
                if (n.accid != Accid.None)
                {
                    count++;
                }
            }
            AccidSymbol[] symbols = new AccidSymbol[count];
            noteToAccidIndexes = new (int, int)[count];

            for (int noteIdx = 0, accIdx = 0; noteIdx < notedata.Length; noteIdx++)
            {
                var nData = notedata[noteIdx];

                if (nData.accid != Accid.None)
                {
                    symbols[accIdx] = new AccidSymbol(nData.accid, nData.whiteNote, clef, in dimensions, chord: true);
                    noteToAccidIndexes[accIdx] = (noteIdx: noteIdx, accidIdx: accIdx);

                    accIdx++;
                }
            }
            // int i = 0;
            // foreach (NoteData n in notedata)
            // {
            // }

            hasAccidentals = symbols.Length > 0;

            return symbols;
        }

        /** Calculate the stem direction (Up or down) based on the top and
         * bottom note in the chord.  If the average of the notes is above
         * the middle of the staff, the direction is down.  Else, the
         * direction is up.
         */
        private static int
        StemDirection(WhiteNote bottom, WhiteNote top, Clef clef)
        {
            WhiteNote middle;
            if (clef == Clef.Treble)
                middle = new WhiteNote(WhiteNote.B, 5);
            else
                middle = new WhiteNote(WhiteNote.D, 3);

            int dist = middle.Dist(bottom) + middle.Dist(top);
            if (dist >= 0)
                return Stem.Up;
            else
                return Stem.Down;
        }

        /** Return whether any of the notes in notedata (between start and
         * end indexes) overlap.  This is needed by the Stem class to
         * determine the position of the stem (left or right of notes).
         */
        private static bool NotesOverlap(NoteData[] notedata, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                if (!notedata[i].leftSide)
                {
                    return true;
                }
            }
            return false;
        }

        /** Get the time (in pulses) this symbol occurs at.
         * This is used to determine the measure this symbol belongs to.
         */
        public int StartTime
        {
            get { return startTime; }
        }

        /** Get the end time (in pulses) of the longest note in the chord.
         * Used to determine whether two adjacent chords can be joined
         * by a stem.
         */
        public int EndTime
        {
            get { return endTime; }
        }

        /** Return the clef this chord is drawn in. */
        public Clef Clef
        {
            get { return clef; }
        }

        /** Return true if this chord has two stems */
        public bool HasTwoStems
        {
            get { return hasTwoStems; }
        }

        /* Return the stem will the smallest duration.  This property
         * is used when making chord pairs (chords joined by a horizontal
         * beam stem). The stem durations must match in order to make
         * a chord pair.  If a chord has two stems, we always return
         * the one with a smaller duration, because it has a better 
         * chance of making a pair.
         */
        public Stem Stem
        {
            get
            {
                if (stem1 == null) { return stem2; }
                else if (stem2 == null) { return stem1; }
                else if (stem1.Duration < stem2.Duration) { return stem1; }
                else { return stem2; }
            }
        }

        /** Get/Set the width (in pixels) of this symbol. The width is set
         * in SheetMusic.AlignSymbols() to vertically align symbols.
         */
        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        /** Get the minimum width (in pixels) needed to draw this symbol */
        public float MinWidth
        {
            get { return GetMinWidth(); }
        }

        public float AccidsWidth { get; set; }
        public float WholeWidth
        {
            get
            {
                if (hasOverlap)
                    return dimensions.ChordOverlapWidth;
                else
                    return dimensions.ChordWidth;
            }
        }

        public float ActualWidth
        {
            get
            {
                return WholeWidth - dimensions.NoteToNoteDistance;
            }
        }

        /* Return the minimum width needed to display this chord.
         *
         * The accidental symbols can be drawn above one another as long
         * as they don't overlap (they must be at least 6 notes apart).
         * If two accidental symbols do overlap, the accidental symbol
         * on top must be shifted to the right.  So the width needed for
         * accidental symbols depends on whether they overlap or not.
         *
         * If we are also displaying the letters, include extra width.
         */
        float GetMinWidth()
        {
            /* The width needed for the note circles */
            float result = WholeWidth;

            float accidsWidth = 0;

            // Sum the width of every accidental symbol
            if (accidSymbols.Length > 0)
            {
                accidsWidth += accidSymbols[0].MinWidth;
                int i;
                for (i = 1; i < accidSymbols.Length; i++)
                {
                    AccidSymbol accid = accidSymbols[i];
                    AccidSymbol prev = accidSymbols[i - 1];
                    if (accid.Note.Dist(prev.Note) < 6)
                    {
                        accidsWidth += accid.MinWidth;
                    }
                }

                accidsWidth += dimensions.NoteToAccidentalDistance;
                // Remove one of the spacing so left space is ChordWidthOffset
                accidsWidth -= accidSymbols[i - 1].AccidentalSpacing;
            }

            AccidsWidth = accidsWidth;

            float extraLeft = 0;
            float extraRight = 0;

            float spacing = dimensions.ChordWidthOffset;

            if (accidsWidth > spacing)
            {
                // The width of the accids must move the note heads increasing the width
                extraLeft += accidsWidth - spacing;
            }

            // Add spacing to the right
            float dotWholeWidth = 0;
            float nameWidth = 0;

            if (sheetMusic != null && sheetMusic.ShowNoteLetters != MIDIOptions.NoteNameNone)
            {
                // Get Max length for a note name
                var maxNoteNameLength = 0;
                foreach (NoteData data in noteData)
                {
                    maxNoteNameLength = Math.Max(maxNoteNameLength, NoteName(data.number, data.whiteNote).Length);
                }

                nameWidth = maxNoteNameLength * dimensions.WidthPerChar + dimensions.NoteToNameDistance;
            }

            if (hasDots)
            {
                dotWholeWidth = dimensions.DotWidth + dimensions.NoteToDotDistance;
            }

            if (dotWholeWidth + nameWidth > spacing)
            {
                extraRight += dotWholeWidth + nameWidth - spacing;
            }

            result += Mathf.Max(extraLeft, extraRight) * 2;

            return result;
        }


        /** Get the number of pixels this symbol extends above the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        public float AboveStaff
        {
            get { return GetAboveStaff(); }
        }

        private float GetAboveStaff()
        {
            /* Find the topmost note in the chord */
            WhiteNote topnote = noteData[noteData.Length - 1].whiteNote;

            /* The stem.End is the note position where the stem ends.
             * Check if the stem end is higher than the top note.
             */
            if (stem1 != null)
                topnote = WhiteNote.Max(topnote, stem1.End);
            if (stem2 != null)
                topnote = WhiteNote.Max(topnote, stem2.End);

            // float dist = topnote.Dist(WhiteNote.Top(clef)) * SheetMusic.NoteHeadHeight / 2;
            float dist = topnote.Dist(WhiteNote.Top(clef)) * dimensions.NoteVerticalSpacing;
            float result = 0;
            if (dist > 0)
                result = dist;

            /* Check if any accidental symbols extend above the staff */
            foreach (AccidSymbol symbol in accidSymbols)
            {
                if (symbol.AboveStaff > result)
                {
                    result = symbol.AboveStaff;
                }
            }
            return result;
        }

        /** Get the number of pixels this symbol extends below the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        public float BelowStaff
        {
            get { return GetBelowStaff(); }
        }

        private float GetBelowStaff()
        {
            /* Find the bottom note in the chord */
            WhiteNote bottomnote = noteData[0].whiteNote;

            /* The stem.End is the note position where the stem ends.
             * Check if the stem end is lower than the bottom note.
             */
            if (stem1 != null)
                bottomnote = WhiteNote.Min(bottomnote, stem1.End);
            if (stem2 != null)
                bottomnote = WhiteNote.Min(bottomnote, stem2.End);

            float dist = WhiteNote.Bottom(clef).Dist(bottomnote) * dimensions.NoteVerticalSpacing;
            //    SheetMusic.NoteHeadHeight / 2;

            float result = 0;
            if (dist > 0)
                result = dist;

            /* Check if any accidental symbols extend below the staff */
            foreach (AccidSymbol symbol in accidSymbols)
            {
                if (symbol.BelowStaff > result)
                {
                    result = symbol.BelowStaff;
                }
            }
            return result;
        }

        /** Get the name for this note */
        private string NoteName(int notenumber, WhiteNote whitenote)
        {
            if (sheetMusic.ShowNoteLetters == MIDIOptions.NoteNameLetter)
            {
                return Letter(notenumber, whitenote);
            }
            else if (sheetMusic.ShowNoteLetters == MIDIOptions.NoteNameFixedDoReMi)
            {
                string[] fixedDoReMi = {
                "La", "Li", "Ti", "Do", "Di", "Re", "Ri", "Mi", "Fa", "Fi", "So", "Si"
            };
                int notescale = NoteScale.FromNumber(notenumber);
                return fixedDoReMi[notescale];
            }
            else if (sheetMusic.ShowNoteLetters == MIDIOptions.NoteNameMovableDoReMi)
            {
                string[] fixedDoReMi = {
                "La", "Li", "Ti", "Do", "Di", "Re", "Ri", "Mi", "Fa", "Fi", "So", "Si"
            };
                int mainscale = sheetMusic.MainKey.Notescale();
                int diff = NoteScale.C - mainscale;
                notenumber += diff;
                if (notenumber < 0)
                {
                    notenumber += 12;
                }
                int notescale = NoteScale.FromNumber(notenumber);
                return fixedDoReMi[notescale];
            }
            else if (sheetMusic.ShowNoteLetters == MIDIOptions.NoteNameFixedNumber)
            {
                string[] num = {
                "10", "11", "12", "1", "2", "3", "4", "5", "6", "7", "8", "9"
            };
                int notescale = NoteScale.FromNumber(notenumber);
                return num[notescale];
            }
            else if (sheetMusic.ShowNoteLetters == MIDIOptions.NoteNameMovableNumber)
            {
                string[] num = {
                "10", "11", "12", "1", "2", "3", "4", "5", "6", "7", "8", "9"
            };
                int mainscale = sheetMusic.MainKey.Notescale();
                int diff = NoteScale.C - mainscale;
                notenumber += diff;
                if (notenumber < 0)
                {
                    notenumber += 12;
                }
                int notescale = NoteScale.FromNumber(notenumber);
                return num[notescale];
            }
            else
            {
                return "";
            }
        }

        /** Get the letter (A, A#, Bb) representing this note */
        private string Letter(int notenumber, WhiteNote whitenote)
        {
            int notescale = NoteScale.FromNumber(notenumber);
            switch (notescale)
            {
                case NoteScale.A: return "A";
                case NoteScale.B: return "B";
                case NoteScale.C: return "C";
                case NoteScale.D: return "D";
                case NoteScale.E: return "E";
                case NoteScale.F: return "F";
                case NoteScale.G: return "G";
                case NoteScale.Asharp:
                    if (whitenote.Letter == WhiteNote.A)
                        return "A#";
                    else
                        return "Bb";
                case NoteScale.Csharp:
                    if (whitenote.Letter == WhiteNote.C)
                        return "C#";
                    else
                        return "Db";
                case NoteScale.Dsharp:
                    if (whitenote.Letter == WhiteNote.D)
                        return "D#";
                    else
                        return "Eb";
                case NoteScale.Fsharp:
                    if (whitenote.Letter == WhiteNote.F)
                        return "F#";
                    else
                        return "Gb";
                case NoteScale.Gsharp:
                    if (whitenote.Letter == WhiteNote.G)
                        return "G#";
                    else
                        return "Ab";
                default:
                    return "";
            }
        }

        /** Draw the Chord Symbol:
         * - Draw the accidental symbols.
         * - Draw the black circle notes.
         * - Draw the stems.
          @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public GameObject Create(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            GameObject chord = new GameObject("chord");

            float halfWidth = Width / 2;

            var accidentalsPosition = position + new Vector3(halfWidth - AccidsWidth, 0);
            /* Draw the accidentals. */
            WhiteNote topstaff = WhiteNote.Top(clef);

            var accidentals = CreateAccid(factory, accidentalsPosition, ytop);

            var newPosition = position + new Vector3(halfWidth, 0);

            /* Draw the notes */
            var notes = CreateNotes(factory, newPosition, ytop, topstaff);
            // notes.name = "notes";
            // notes.transform.SetParent(chord.transform);

            AssociateNotesWithAccids(ref notes, ref accidentals);

            if (sheetMusic != null && sheetMusic.ShowNoteLetters != 0)
            {
                var noteNames = CreateNoteLetters(factory, newPosition, ytop, topstaff);
                // noteNames.name = "noteNames";
                // noteNames.transform.SetParent(chord.transform);
                AssociateGameObjects(ref notes, ref noteNames);
            }

            /* Draw the stems */
            if (stem1 != null)
            {
                var stemOne = stem1.Create(factory, newPosition, ytop, topstaff, halfWidth);

                if (stemOne != null)
                {
                    stemOne.name = "stemOne";

                    // Stem 1 is for the bottom note
                    stemOne.transform.SetParent(notes[0].transform);
                }
            }

            if (stem2 != null)
            {
                var stemTwo = stem2.Create(factory, newPosition, ytop, topstaff, halfWidth);

                if (stemTwo != null)
                {
                    stemTwo.name = "stemTwo";

                    // Stem 2 is for the top note
                    stemTwo.transform.SetParent(notes[notes.Length - 1].transform);
                }
            }

            // Set all notes as children of the chord game object
            AssociateChildren(ref chord, ref notes);

            return chord;
        }

        private void AssociateChildren(ref GameObject parent, ref GameObject[] children)
        {
            foreach (var child in children)
            {
                child.transform.SetParent(parent.transform);
            }
        }

        private void AssociateNotesWithAccids(ref GameObject[] notes, ref GameObject[] accids)
        {
            // If no accidentals *noteToAccidIndexes* should be empty so no action is taken
            foreach (var item in noteToAccidIndexes)
            {
                var note = notes[item.noteIdx];
                var accid = accids[item.accidIdx];

                accid.transform.SetParent(note.transform);
            }
        }

        private void AssociateGameObjects(ref GameObject[] parents, ref GameObject[] children)
        {
            if (parents.Length != children.Length)
            {
                return;
            }

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                var parent = parents[i];

                child.transform.SetParent(parent.transform);
            }
        }

        /* Draw the accidental symbols.  If two symbols overlap (if they
         * are less than 6 notes apart), we cannot draw the symbol directly
         * above the previous one.  Instead, we must shift it to the right.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         * @return The x pixel width used by all the accidentals.
         */
        public GameObject[] CreateAccid(MusicSymbolFactory factory, Vector3 originalPosition, float ytop)
        {
            float xpos = 0;
            GameObject[] accidentals = new GameObject[accidSymbols.Length];

            AccidSymbol prev = null;

            for (int i = 0; i < accidSymbols.Length; i++)
            {
                var accid = accidSymbols[i];

                if (prev != null && accid.Note.Dist(prev.Note) < 6)
                {
                    xpos += accid.Width;
                }

                var accidGO = accid.Create(factory, originalPosition + new Vector3(xpos, 0), ytop);

                accidentals[i] = accidGO;

                prev = accid;
            }

            if (prev != null)
            {
                xpos += prev.Width;
            }

            return accidentals;
        }

        /** Draw the black circle notes.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         * @param topstaff The white note of the top of the staff.
         */
        public GameObject[] CreateNotes(MusicSymbolFactory factory, Vector3 originalPosition, float ytop, WhiteNote topstaff)
        {
            // Start at the left, right to the accidentals if any
            float xnote = 0;
            float xOffsetRight = 0;

            // The game object to hold all the notes
            var noteArray = new GameObject[noteData.Length];

            if (hasOverlap)
            {
                // The notes in this Chord Symbol overlap, so width is greater and note heads must be placed at the
                // left and right of the center

                // Move left half the width of note stem
                xOffsetRight = ActualWidth / 2 - dimensions.NoteStemWidth / 2;
            }

            // foreach (NoteData note in noteData)
            for (int noteIndex = 0; noteIndex < noteData.Length; noteIndex++)
            {
                var note = noteData[noteIndex];

                GameObject noteGO = new GameObject("note");

                var distanceToTopStaff = topstaff.Dist(note.whiteNote);
                float ynote = ytop + distanceToTopStaff * dimensions.NoteVerticalSpacing;

                float xOffset = xnote;

                if (!note.leftSide)
                    xOffset = xOffsetRight;

                var xyPosition = new Vector3(xOffset, -ynote);

                GameObject noteHead;
                GameObject noteDot;

                if (note.duration == NoteDuration.Whole ||
                    note.duration == NoteDuration.Half ||
                    note.duration == NoteDuration.DottedHalf)
                {
                    // Create the game object (Note Head with Hole)
                    noteHead = factory.CreateSymbol(SymbolType.NOTE_HEAD_HOLE);
                }
                else
                {
                    // Create the game object (Note Head)
                    noteHead = factory.CreateSymbol(SymbolType.NOTE_HEAD);
                }

                // Fit the game object to its width
                noteHead.FitToWidth(dimensions.NoteHeadWidth);
                // Place the game object at the right position
                noteHead.PlaceUpperLeft(originalPosition, xyPosition);

                noteHead.name = "noteHead";
                noteHead.transform.SetParent(noteGO.transform);

                /* Draw a dot if this is a dotted duration. */
                if (note.duration == NoteDuration.DottedHalf ||
                    note.duration == NoteDuration.DottedQuarter ||
                    note.duration == NoteDuration.DottedEighth)
                {
                    // Create the game object (Note Dot)
                    noteDot = factory.CreateSymbol(SymbolType.NOTE_DOT);
                    // Fit the game object to its width
                    noteDot.FitToWidth(dimensions.DotWidth);
                    // Place the game object at the right position
                    // Place note dot at bottom of note head

                    float xDotOffset = ActualWidth + dimensions.NoteToDotDistance;
                    float yDotOffset = dimensions.NoteHeadHeight - dimensions.DotWidth;
                    var dotOffset = new Vector3(xDotOffset, -yDotOffset);
                    noteDot.PlaceUpperLeft(originalPosition, xyPosition, dotOffset);

                    noteDot.name = "noteDot";
                    noteDot.transform.SetParent(noteGO.transform);
                }

                // Gameobject to hold the note bars if any
                GameObject bars = null;
                var barsName = "bars";

                /* Draw horizontal lines if note is above/below the staff */
                WhiteNote top = topstaff.Add(1);
                int dist = note.whiteNote.Dist(top);
                float y = ytop - dimensions.LineWidth;
                var noteBarWidth = dimensions.NoteBarWidth;

                var xBarOffset = xOffset + dimensions.NoteHeadWidth / 2;
                var yBarOffset = dimensions.LineWidth / 2;

                if (dist >= 2)
                {
                    bars = new GameObject(barsName);

                    // Top bars
                    for (int i = 2; i <= dist; i += 2)
                    {
                        // y -= dimensions.NoteHeight;
                        y -= dimensions.WholeLineSpace;
                        // Create the game object (Note Bar)
                        var noteBar = factory.CreateSymbol(SymbolType.NOTE_BAR);
                        var offset = new Vector3(xBarOffset, -(y + yBarOffset));
                        PlaceBar(ref noteBar, originalPosition, offset);

                        noteBar.gameObject.name = "noteBarTop";
                        noteBar.transform.SetParent(bars.transform);
                    }
                }

                WhiteNote bottom = top.Add(-8);
                y = ytop + dimensions.WholeLineSpace * 4;
                dist = bottom.Dist(note.whiteNote);
                if (dist >= 2)
                {
                    if (bars == null) bars = new GameObject(barsName);

                    // Bottom bars
                    for (int i = 2; i <= dist; i += 2)
                    {
                        y += dimensions.WholeLineSpace;
                        // Create the game object (Note Bar)
                        var noteBar = factory.CreateSymbol(SymbolType.NOTE_BAR);
                        var offset = new Vector3(xBarOffset, -(y - yBarOffset));
                        PlaceBar(ref noteBar, originalPosition, offset);

                        noteBar.gameObject.name = "noteBarBottom";
                        noteBar.transform.SetParent(bars.transform);
                    }
                }

                bars?.transform.SetParent(noteGO.transform);
                /* End drawing horizontal lines */

                noteArray[noteIndex] = noteGO;
            }

            return noteArray;
        }

        private void PlaceBar(ref GameObject bar, Vector3 position, Vector3 offset)
        {
            bar.FitToHeight(dimensions.LineWidth);
            // Fit the game object to its width
            bar.FitOnlyTo(dimensions.NoteBarWidth, Axis.Y, Axis.X);
            // Place the game object at the right position
            bar.PlaceCenter(position, offset);
        }

        /** Draw the note letters (A, A#, Bb, etc) next to the note circles.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         * @param topstaff The white note of the top of the staff.
         */
        public GameObject[] CreateNoteLetters(MusicSymbolFactory factory, Vector3 originalPosition, float ytop, WhiteNote topstaff)
        {
            var names = new GameObject[noteData.Length];

            float xnote = ActualWidth + dimensions.NoteToNameDistance;

            if (hasDots)
            {
                // Move to the right the space necessary for the dots
                xnote += dimensions.NoteToDotDistance + dimensions.DotWidth;
            }

            var spacing = dimensions.NoteVerticalSpacing;

            for (int i = 0; i < noteData.Length; i++)
            {
                var note = noteData[i];
                /* Get the x,y position to draw the note */
                var distanceToNote = topstaff.Dist(note.whiteNote);
                float ynote = ytop + distanceToNote * spacing;

                var noteNameGO = factory.CreateSymbol(SymbolType.NOTE_NAME_TEXT);
                var noteName = NoteName(note.number, note.whiteNote);
                noteNameGO.TextSetText(noteName);
                var widthPerChar = dimensions.WidthPerChar;
                var width = noteName.Length * widthPerChar;
                // Fit the game object to its height
                noteNameGO.TextFitToHeight(dimensions.NoteNameTextHeight);
                noteNameGO.TextFitOnlyToWidth(width);
                // Place the game object at the right position (centered vertically)
                var offset = new Vector3(xnote, -(ynote + dimensions.NoteHeadHeight / 2));
                noteNameGO.TextPlaceCenterLeft(originalPosition, offset);

                noteNameGO.name = noteName;

                names[i] = noteNameGO;
            }

            return names;
        }


        /** Return true if the chords can be connected, where their stems are
         * joined by a horizontal beam. In order to create the beam:
         *
         * - The chords must be in the same measure.
         * - The chord stems should not be a dotted duration.
         * - The chord stems must be the same duration, with one exception
         *   (Dotted Eighth to Sixteenth).
         * - The stems must all point in the same direction (up or down).
         * - The chord cannot already be part of a beam.
         *
         * - 6-chord beams must be 8th notes in 3/4, 6/8, or 6/4 time
         * - 3-chord beams must be either triplets, or 8th notes (12/8 time signature)
         * - 4-chord beams are ok for 2/2, 2/4 or 4/4 time, any duration
         * - 4-chord beams are ok for other times if the duration is 16th
         * - 2-chord beams are ok for any duration
         *
         * If startQuarter is true, the first note should start on a quarter note
         * (only applies to 2-chord beams).
         */
        public static
        bool CanCreateBeam(ChordSymbol[] chords, TimeSignature time, bool startQuarter)
        {
            int numChords = chords.Length;
            Stem firstStem = chords[0].Stem;
            Stem lastStem = chords[chords.Length - 1].Stem;
            if (firstStem == null || lastStem == null)
            {
                return false;
            }
            int measure = chords[0].StartTime / time.Measure;
            NoteDuration dur = firstStem.Duration;
            NoteDuration dur2 = lastStem.Duration;

            bool dotted8_to_16 = false;
            if (chords.Length == 2 && dur == NoteDuration.DottedEighth &&
                dur2 == NoteDuration.Sixteenth)
            {
                dotted8_to_16 = true;
            }

            if (dur == NoteDuration.Whole || dur == NoteDuration.Half ||
                dur == NoteDuration.DottedHalf || dur == NoteDuration.Quarter ||
                dur == NoteDuration.DottedQuarter ||
                (dur == NoteDuration.DottedEighth && !dotted8_to_16))
            {

                return false;
            }

            if (numChords == 6)
            {
                if (dur != NoteDuration.Eighth)
                {
                    return false;
                }
                bool correctTime =
                   ((time.Numerator == 3 && time.Denominator == 4) ||
                    (time.Numerator == 6 && time.Denominator == 8) ||
                    (time.Numerator == 6 && time.Denominator == 4));

                if (!correctTime)
                {
                    return false;
                }

                if (time.Numerator == 6 && time.Denominator == 4)
                {
                    /* first chord must start at 1st or 4th quarter note */
                    int beat = time.Quarter * 3;
                    if ((chords[0].StartTime % beat) > time.Quarter / 6)
                    {
                        return false;
                    }
                }
            }
            else if (numChords == 4)
            {
                if (time.Numerator == 3 && time.Denominator == 8)
                {
                    return false;
                }
                bool correctTime =
                  (time.Numerator == 2 || time.Numerator == 4 || time.Numerator == 8);
                if (!correctTime && dur != NoteDuration.Sixteenth)
                {
                    return false;
                }

                /* chord must start on quarter note */
                int beat = time.Quarter;
                if (dur == NoteDuration.Eighth)
                {
                    /* 8th note chord must start on 1st or 3rd quarter note */
                    beat = time.Quarter * 2;
                }
                else if (dur == NoteDuration.ThirtySecond)
                {
                    /* 32nd note must start on an 8th beat */
                    beat = time.Quarter / 2;
                }

                if ((chords[0].StartTime % beat) > time.Quarter / 6)
                {
                    return false;
                }
            }
            else if (numChords == 3)
            {
                bool valid = (dur == NoteDuration.Triplet) ||
                              (dur == NoteDuration.Eighth &&
                               time.Numerator == 12 && time.Denominator == 8);
                if (!valid)
                {
                    return false;
                }

                /* chord must start on quarter note */
                int beat = time.Quarter;
                if (time.Numerator == 12 && time.Denominator == 8)
                {
                    /* In 12/8 time, chord must start on 3*8th beat */
                    beat = time.Quarter / 2 * 3;
                }
                if ((chords[0].StartTime % beat) > time.Quarter / 6)
                {
                    return false;
                }
            }

            else if (numChords == 2)
            {
                if (startQuarter)
                {
                    int beat = time.Quarter;
                    if ((chords[0].StartTime % beat) > time.Quarter / 6)
                    {
                        return false;
                    }
                }
            }

            foreach (ChordSymbol chord in chords)
            {
                if ((chord.StartTime / time.Measure) != measure)
                    return false;
                if (chord.Stem == null)
                    return false;
                if (chord.Stem.Duration != dur && !dotted8_to_16)
                    return false;
                if (chord.Stem.isBeam)
                    return false;
            }

            /* Check that all stems can point in same direction */
            bool hasTwoStems = false;
            int direction = Stem.Up;
            foreach (ChordSymbol chord in chords)
            {
                if (chord.HasTwoStems)
                {
                    if (hasTwoStems && chord.Stem.Direction != direction)
                    {
                        return false;
                    }
                    hasTwoStems = true;
                    direction = chord.Stem.Direction;
                }
            }

            /* Get the final stem direction */
            if (!hasTwoStems)
            {
                WhiteNote note1;
                WhiteNote note2;
                note1 = (firstStem.Direction == Stem.Up ? firstStem.Top : firstStem.Bottom);
                note2 = (lastStem.Direction == Stem.Up ? lastStem.Top : lastStem.Bottom);
                direction = StemDirection(note1, note2, chords[0].Clef);
            }

            /* If the notes are too far apart, don't use a beam */
            if (direction == Stem.Up)
            {
                if (Math.Abs(firstStem.Top.Dist(lastStem.Top)) >= 11)
                {
                    return false;
                }
            }
            else
            {
                if (Math.Abs(firstStem.Bottom.Dist(lastStem.Bottom)) >= 11)
                {
                    return false;
                }
            }
            return true;
        }


        /** Connect the chords using a horizontal beam. 
         *
         * spacing is the horizontal distance (in pixels) between the right side 
         * of the first chord, and the right side of the last chord.
         *
         * To make the beam:
         * - Change the stem directions for each chord, so they match.
         * - In the first chord, pass the stem location of the last chord, and
         *   the horizontal spacing to that last stem.
         * - Mark all chords (except the first) as "receiver" pairs, so that 
         *   they don't draw a curvy stem.
         */
        public static
        void CreateBeam(ChordSymbol[] chords, float spacing)
        {
            Stem firstStem = chords[0].Stem;
            Stem lastStem = chords[chords.Length - 1].Stem;

            /* Calculate the new stem direction */
            int newdirection = -1;
            foreach (ChordSymbol chord in chords)
            {
                if (chord.HasTwoStems)
                {
                    newdirection = chord.Stem.Direction;
                    break;
                }
            }

            if (newdirection == -1)
            {
                WhiteNote note1;
                WhiteNote note2;
                note1 = (firstStem.Direction == Stem.Up ? firstStem.Top : firstStem.Bottom);
                note2 = (lastStem.Direction == Stem.Up ? lastStem.Top : lastStem.Bottom);
                newdirection = StemDirection(note1, note2, chords[0].Clef);
            }
            foreach (ChordSymbol chord in chords)
            {
                chord.Stem.Direction = newdirection;
            }

            if (chords.Length == 2)
            {
                BringStemsCloser(chords);
            }
            else
            {
                LineUpStemEnds(chords);
            }

            firstStem.SetPair(lastStem, spacing);
            for (int i = 1; i < chords.Length; i++)
            {
                chords[i].Stem.Receiver = true;
            }
        }

        /** We're connecting the stems of two chords using a horizontal beam.
         *  Adjust the vertical endpoint of the stems, so that they're closer
         *  together.  For a dotted 8th to 16th beam, increase the stem of the
         *  dotted eighth, so that it's as long as a 16th stem.
         */
        static void
        BringStemsCloser(ChordSymbol[] chords)
        {
            Stem firstStem = chords[0].Stem;
            Stem lastStem = chords[1].Stem;

            /* If we're connecting a dotted 8th to a 16th, increase
             * the stem end of the dotted eighth.
             */
            if (firstStem.Duration == NoteDuration.DottedEighth &&
                lastStem.Duration == NoteDuration.Sixteenth)
            {
                if (firstStem.Direction == Stem.Up)
                {
                    firstStem.End = firstStem.End.Add(2);
                }
                else
                {
                    firstStem.End = firstStem.End.Add(-2);
                }
            }

            /* Bring the stem ends closer together */
            int distance = Math.Abs(firstStem.End.Dist(lastStem.End));
            if (firstStem.Direction == Stem.Up)
            {
                if (WhiteNote.Max(firstStem.End, lastStem.End) == firstStem.End)
                    lastStem.End = lastStem.End.Add(distance / 2);
                else
                    firstStem.End = firstStem.End.Add(distance / 2);
            }
            else
            {
                if (WhiteNote.Min(firstStem.End, lastStem.End) == firstStem.End)
                    lastStem.End = lastStem.End.Add(-distance / 2);
                else
                    firstStem.End = firstStem.End.Add(-distance / 2);
            }
        }

        /** We're connecting the stems of three or more chords using a horizontal beam.
         *  Adjust the vertical endpoint of the stems, so that the middle chord stems
         *  are vertically in between the first and last stem.
         */
        static void
        LineUpStemEnds(ChordSymbol[] chords)
        {
            Stem firstStem = chords[0].Stem;
            Stem lastStem = chords[chords.Length - 1].Stem;
            Stem middleStem = chords[1].Stem;

            if (firstStem.Direction == Stem.Up)
            {
                /* Find the highest stem. The beam will either:
                 * - Slant downwards (first stem is highest)
                 * - Slant upwards (last stem is highest)
                 * - Be straight (middle stem is highest)
                 */
                WhiteNote top = firstStem.End;
                foreach (ChordSymbol chord in chords)
                {
                    top = WhiteNote.Max(top, chord.Stem.End);
                }
                if (top == firstStem.End && top.Dist(lastStem.End) >= 2)
                {
                    firstStem.End = top;
                    middleStem.End = top.Add(-1);
                    lastStem.End = top.Add(-2);
                }
                else if (top == lastStem.End && top.Dist(firstStem.End) >= 2)
                {
                    firstStem.End = top.Add(-2);
                    middleStem.End = top.Add(-1);
                    lastStem.End = top;
                }
                else
                {
                    firstStem.End = top;
                    middleStem.End = top;
                    lastStem.End = top;
                }
            }
            else
            {
                /* Find the bottommost stem. The beam will either:
                 * - Slant upwards (first stem is lowest)
                 * - Slant downwards (last stem is lowest)
                 * - Be straight (middle stem is highest)
                 */
                WhiteNote bottom = firstStem.End;
                foreach (ChordSymbol chord in chords)
                {
                    bottom = WhiteNote.Min(bottom, chord.Stem.End);
                }

                if (bottom == firstStem.End && lastStem.End.Dist(bottom) >= 2)
                {
                    middleStem.End = bottom.Add(1);
                    lastStem.End = bottom.Add(2);
                }
                else if (bottom == lastStem.End && firstStem.End.Dist(bottom) >= 2)
                {
                    middleStem.End = bottom.Add(1);
                    firstStem.End = bottom.Add(2);
                }
                else
                {
                    firstStem.End = bottom;
                    middleStem.End = bottom;
                    lastStem.End = bottom;
                }
            }

            /* All middle stems have the same end */
            for (int i = 1; i < chords.Length - 1; i++)
            {
                Stem stem = chords[i].Stem;
                stem.End = middleStem.End;
            }
        }

        public override string ToString()
        {
            string result = string.Format("ChordSymbol clef={0} start={1} end={2} width={3} hastwostems={4} ",
                                          clef, StartTime, EndTime, Width, hasTwoStems);
            foreach (AccidSymbol symbol in accidSymbols)
            {
                result += symbol.ToString() + " ";
            }
            foreach (NoteData note in noteData)
            {
                result += string.Format("Note whitenote={0} duration={1} leftside={2} ",
                                        note.whiteNote, note.duration, note.leftSide);
            }
            if (stem1 != null)
            {
                result += stem1.ToString() + " ";
            }
            if (stem2 != null)
            {
                result += stem2.ToString() + " ";
            }
            return result;
        }

    }


}


