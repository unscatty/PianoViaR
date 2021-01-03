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
using UnityEngine;
using System.Collections.Generic;
using PianoViaR.MIDI.Helpers;
using PianoViaR.Score.Helpers;
using PianoViaR.Utils;

namespace PianoViaR.Score.Creation
{

    /* @class Staff
     * The Staff is used to draw a single Staff (a row of measures) in the 
     * SheetMusic Control. A Staff needs to draw
     * - The Clef
     * - The key signature
     * - The horizontal lines
     * - A list of MusicSymbols
     * - The left and right vertical lines
     *
     * The height of the Staff is determined by the number of pixels each
     * MusicSymbol extends above and below the staff.
     *
     * The vertical lines (left and right sides) of the staff are joined
     * with the staffs above and below it, with one exception.  
     * The last track is not joined with the first track.
     */

    public class Staff
    {
        private List<MusicSymbol> symbols;  /** The music symbols in this staff */
        private List<LyricSymbol> lyrics;   /** The lyrics to display (can be null) */
        private float ytop;                   /** The y pixel of the top of the staff */
        private ClefSymbol clefsym;         /** The left-side Clef symbol */
        private AccidSymbol[] keys;         /** The key signature symbols */
        private bool showMeasures;          /** If true, show the measure numbers */
        private float keysigWidth;            /** The width of the clef and key signature */
        private float width;                  /** The width of the staff in pixels */
        private float height;                 /** The height of the staff in pixels */
        private int tracknum;               /** The track this staff represents */
        private int totaltracks;            /** The total number of tracks */
        private int starttime;              /** The time (in pulses) of first symbol */
        private int endtime;                /** The time (in pulses) of last symbol */
        private int measureLength;          /** The time (in pulses) of a measure */
        private ScoreDimensions dimensions;

        /** Create a new staff with the given list of music symbols,
         * and the given key signature.  The clef is determined by
         * the clef of the first chord symbol. The track number is used
         * to determine whether to join this left/right vertical sides
         * with the staffs above and below. The SheetMusicOptions are used
         * to check whether to display measure numbers or not.
         */
        public Staff(
            List<MusicSymbol> symbols,
            KeySignature key,
            MIDIOptions options,
            int tracknum,
            int totaltracks,
            in ScoreDimensions dimensions
        )
        {
            this.dimensions = dimensions;
            keysigWidth = SheetMusic.KeySignatureWidth(key, in dimensions);
            this.tracknum = tracknum;
            this.totaltracks = totaltracks;
            showMeasures = (options.showMeasures && tracknum == 0);
            measureLength = options.time.Measure;
            Clef clef = FindClef(symbols);

            clefsym = new ClefSymbol(clef, 0, false, in dimensions);
            keys = key.GetSymbols(clef);
            this.symbols = symbols;

            CalculateWidth(options.scrollVert);
            CalculateHeight();
            CalculateStartEndTime();
            FullJustify();
        }

        /** Return the width of the staff */
        public float Width
        {
            get { return width; }
        }

        /** Return the height of the staff */
        public float Height
        {
            get { return height; }
        }

        /** Return the track number of this staff (starting from 0 */
        public int Track
        {
            get { return tracknum; }
        }

        /** Return the starting time of the staff, the start time of
         *  the first symbol.  This is used during playback, to 
         *  automatically scroll the music while playing.
         */
        public int StartTime
        {
            get { return starttime; }
        }

        /** Return the ending time of the staff, the endtime of
         *  the last symbol.  This is used during playback, to 
         *  automatically scroll the music while playing.
         */
        public int EndTime
        {
            get { return endtime; }
            set { endtime = value; }
        }

        /** Find the initial clef to use for this staff.  Use the clef of
         * the first ChordSymbol.
         */
        private Clef FindClef(List<MusicSymbol> list)
        {
            foreach (MusicSymbol m in list)
            {
                if (m is ChordSymbol)
                {
                    ChordSymbol c = (ChordSymbol)m;
                    return c.Clef;
                }
            }
            return Clef.Treble;
        }

        /** Calculate the height of this staff.  Each MusicSymbol contains the
         * number of pixels it needs above and below the staff.  Get the maximum
         * values above and below the staff.
         */
        public void CalculateHeight()
        {
            float above = 0;
            float below = 0;

            foreach (MusicSymbol s in symbols)
            {
                above = Math.Max(above, s.AboveStaff);
                below = Math.Max(below, s.BelowStaff);
            }

            above = Math.Max(above, clefsym.AboveStaff);
            below = Math.Max(below, clefsym.BelowStaff);
            ytop = above;
            height = ytop + dimensions.StaffHeight + below;

            if (showMeasures || lyrics != null)
            {
                height += dimensions.MeasureNameTextHeight;
            }

            /* Add some extra vertical space between staffs
             */
            if (tracknum != totaltracks - 1 && totaltracks != 1)
                height += dimensions.SpaceBetweenStaffs;
        }

        /** Calculate the width of this staff */
        private void CalculateWidth(bool scrollVert, bool extra = true)
        {
            if (scrollVert)
            {
                width = dimensions.PageWidth;
                return;
            }

            width = keysigWidth;
            foreach (MusicSymbol s in symbols)
            {
                width += s.Width;
            }

            if (extra)
            {
                // Add some extra width
                width += dimensions.NoteToNoteDistance;
            }
        }


        /** Calculate the start and end time of this staff. */
        private void CalculateStartEndTime()
        {
            starttime = endtime = 0;
            if (symbols.Count == 0)
            {
                return;
            }
            starttime = symbols[0].StartTime;
            foreach (MusicSymbol m in symbols)
            {
                if (endtime < m.StartTime)
                {
                    endtime = m.StartTime;
                }
                if (m is ChordSymbol)
                {
                    ChordSymbol c = (ChordSymbol)m;
                    if (endtime < c.EndTime)
                    {
                        endtime = c.EndTime;
                    }
                }
            }
        }


        /** Full-Justify the symbols, so that they expand to fill the whole staff. */
        private void FullJustify()
        {
            if (width != dimensions.PageWidth)
                return;

            float totalwidth = keysigWidth;
            int totalsymbols = 0;
            int i = 0;

            while (i < symbols.Count)
            {
                int start = symbols[i].StartTime;
                totalsymbols++;
                totalwidth += symbols[i].Width;
                i++;
                while (i < symbols.Count && symbols[i].StartTime == start)
                {
                    totalwidth += symbols[i].Width;
                    i++;
                }
            }

            float extrawidth = (dimensions.PageWidth - totalwidth - 1) / totalsymbols;
            if (extrawidth > dimensions.NoteHeadWidth * 2)
            {
                extrawidth = dimensions.NoteHeadWidth * 2;
            }
            i = 0;
            while (i < symbols.Count)
            {
                int start = symbols[i].StartTime;
                symbols[i].Width += extrawidth;
                i++;
                while (i < symbols.Count && symbols[i].StartTime == start)
                {
                    i++;
                }
            }
        }


        /** Add the lyric symbols that occur within this staff.
         *  Set the x-position of the lyric symbol. 
         */
        public void AddLyrics(List<LyricSymbol> tracklyrics)
        {
            if (tracklyrics == null)
            {
                return;
            }
            lyrics = new List<LyricSymbol>();
            float xpos = 0;
            int symbolindex = 0;
            foreach (LyricSymbol lyric in tracklyrics)
            {
                if (lyric.StartTime < starttime)
                {
                    continue;
                }
                if (lyric.StartTime > endtime)
                {
                    break;
                }
                /* Get the x-position of this lyric */
                while (symbolindex < symbols.Count &&
                       symbols[symbolindex].StartTime < lyric.StartTime)
                {
                    xpos += symbols[symbolindex].Width;
                    symbolindex++;
                }
                lyric.X = xpos;
                if (symbolindex < symbols.Count &&
                    (symbols[symbolindex] is BarSymbol))
                {
                    lyric.X += dimensions.NoteHeadWidth;
                }
                lyrics.Add(lyric);
            }
            if (lyrics.Count == 0)
            {
                lyrics = null;
            }
        }


        /** Draw the lyrics */
        private void CreateLyrics(MusicSymbolFactory factory, in Vector3 position)
        {
            /* Skip the left side Clef symbol and key signature */
            float xpos = keysigWidth;
            float ypos = height - dimensions.NoteHeadHeight * 2;

            // foreach (LyricSymbol lyric in lyrics)
            // {
            //     GameObject textPrefab = factory.CreateSymbol(SymbolType.TEXT, new Vector3(xpos + lyric.X, ypos));

            //     Text textBehavior = textPrefab.GetComponent<Text>();

            //     if (textBehavior == null)
            //     {
            //         throw new ArgumentException("GameObject is not of type Text");
            //     }

            //     textBehavior.textMesh.text = lyric.Text;
            // }
        }

        /** Draw the measure numbers for each measure */
        private GameObject CreateMeasureNumbers(MusicSymbolFactory factory, in Vector3 position)
        {
            /* Skip the left side Clef symbol and key signature */
            float xpos = keysigWidth;
            // float ypos = height - dimensions.WholeLineSpace;
            float ypos = 0;
            // float ypos = -dimensions.WholeLineSpace;
            var offset = new Vector3(xpos, -ypos);

            // GameObject to hold the measures
            GameObject measuresGO = new GameObject();

            foreach (MusicSymbol symbol in symbols)
            {
                if (symbol is BarSymbol)
                {
                    int measure = 1 + symbol.StartTime / measureLength;
                    var measureText = measure.ToString();

                    var measureGO = factory.CreateSymbol(SymbolType.MEASURES_TEXT);
                    measureGO.name = "measure" + measure;
                    measureGO.TextSetText(measureText);

                    var textWidth = measureText.Length * dimensions.WidthPerChar;
                    measureGO.TextFitToHeight(dimensions.NoteNameTextHeight * 2f);
                    measureGO.TextFitOnlyToWidth(textWidth);
                    measureGO.TextPlaceUpperLeft(position, offset, new Vector3(symbol.Width / 2, 0));

                    measureGO.transform.SetParent(measuresGO.transform);
                }
                offset.x += symbol.Width;
            }

            return measuresGO;
        }

        /** Draw the lyrics */


        /** Draw the five horizontal lines of the staff */
        private GameObject CreateHorizontalLines(MusicSymbolFactory factory, Vector3 position)
        {
            GameObject staffLinesGO = new GameObject();

            int line = 1;
            float y = ytop - dimensions.LineWidth;
            // pen.Width = 1;
            for (line = 1; line <= 5; line++)
            {

                // Create the game object
                var bar = factory.CreateSymbol(SymbolType.STAFF_BAR);
                bar.FitToHeight(dimensions.LineWidth);
                // Fit the game object in its X dimension (Y axis of game object since it is rotated 90 degrees)
                bar.FitOnlyTo(width, Axis.Y, Axis.X);

                // Place game object at the right position
                var offset = new Vector3(dimensions.LeftMargin, -y);
                bar.PlaceUpperLeft(position, offset);

                bar.name = "staffLine";
                bar.transform.SetParent(staffLinesGO.transform);

                y += dimensions.WholeLineSpace;
            }

            return staffLinesGO;
        }

        /** Draw the vertical lines at the far left and far right sides. */
        private GameObject CreateEndLines(MusicSymbolFactory factory, Vector3 position)
        {
            // pen.Width = 1;

            /* Draw the vertical lines from 0 to the height of this staff,
             * including the space above and below the staff, with two exceptions:
             * - If this is the first track, don't start above the staff.
             *   Start exactly at the top of the staff (ytop - LineWidth)
             * - If this is the last track, don't end below the staff.
             *   End exactly at the bottom of the staff.
             */
            float ystart, yend;
            var yAbove = ytop - dimensions.LineWidth;
            if (tracknum == 0)
                // ystart = ytop;
                ystart = yAbove;
            else
                ystart = 0;

            if (tracknum == (totaltracks - 1))
                yend = yAbove + dimensions.StaffHeight;
            else
                yend = height;

            var heightToFit = yend - ystart;

            GameObject endLines = new GameObject();

            // Create the game object
            var line1 = factory.CreateSymbol(SymbolType.NOTE_STEM);
            line1.FitToWidth(dimensions.LineWidth);
            // Fit the game object in its Y axis (enlarge by Y axis)
            line1.FitOnlyToHeight(heightToFit);

            line1.name = "endLineLeft";
            line1.transform.SetParent(endLines.transform);

            // Place game object at the right position
            var offset1 = new Vector3(dimensions.LeftMargin, -ystart);
            line1.PlaceUpperLeft(position, offset1);

            // Duplicate Line 1
            var line2 = UnityEngine.Object.Instantiate(line1, line1.transform.parent);
            // Place game object at the right position
            var offset2 = new Vector3(width, -ystart);
            line2.PlaceUpperLeft(position, offset2);

            line2.name = "endLineRight";

            return endLines;
        }

        /** Draw this staff. Only draw the symbols inside the clip area */
        public GameObject Create(MusicSymbolFactory factory, Vector3 position)
        {
            GameObject staffGO = new GameObject("staff");
            GameObject signatureGO = new GameObject("keySignature");

            /* Draw the left side Clef symbol */
            var newPosition = new Vector3(dimensions.LeftMargin, 0) + position;

            var clefGO = clefsym.Create(factory, newPosition, ytop);
            clefGO.transform.SetParent(signatureGO.transform);

            newPosition.x += clefsym.Width;

            /* Create the key signature */
            if (keys.Length > 0)
            {
                GameObject signatureAccids = new GameObject("accidentals");

                foreach (AccidSymbol accid in keys)
                {
                    var accidGO = accid.Create(factory, newPosition, ytop);
                    accidGO.transform.SetParent(signatureAccids.transform);

                    newPosition.x += accid.Width;
                }

                signatureAccids.transform.SetParent(signatureGO.transform);
            }

            /* Draw the actual notes, rests, bars.  Draw the symbols one 
             * after another, using the symbol width to determine the
             * x position of the next symbol.
             *
             * For fast performance, only draw symbols that are in the clip area.
             */

            var timeSignature = symbols[0];
            var timeSigGO = timeSignature.Create(factory, newPosition, ytop);
            timeSigGO.name = "timeSignature";
            timeSigGO.transform.SetParent(signatureGO.transform);

            newPosition.x += timeSignature.Width;

            var firstBar = symbols[1];
            var firstBarGO = firstBar.Create(factory, newPosition, ytop);
            firstBarGO.transform.SetParent(signatureGO.transform);

            newPosition.x += firstBar.Width;

            signatureGO.transform.SetParent(staffGO.transform);


            GameObject symbolsGO = new GameObject("symbols");

            for (int i = 2; i < symbols.Count; i++)
            {
                var musicSymbol = symbols[i];
                var symbolGO = musicSymbol.Create(factory, newPosition, ytop);
                symbolGO?.transform.SetParent(symbolsGO.transform);

                newPosition.x += musicSymbol.Width;
            }

            symbolsGO.transform.SetParent(staffGO.transform);

            if (showMeasures)
            {
                var measures = CreateMeasureNumbers(factory, position);
                measures.name = "measures";

                measures.transform.SetParent(staffGO.transform);
            }

            var horizLinesGO = CreateHorizontalLines(factory, position);
            horizLinesGO.name = "staffLines";
            horizLinesGO.transform.SetParent(staffGO.transform);

            var endLines = CreateEndLines(factory, position);
            endLines.name = "endLines";
            endLines.transform.SetParent(staffGO.transform);
            // if (lyrics != null)
            // {
            //     CreateLyrics(factory, position);
            // }

            return staffGO;
        }

        public override string ToString()
        {
            string result = "Staff clef=" + clefsym.ToString() + "\n";
            result += "  Keys:\n";
            foreach (AccidSymbol a in keys)
            {
                result += "    " + a.ToString() + "\n";
            }
            result += "  Symbols:\n";
            foreach (MusicSymbol s in keys)
            {
                result += "    " + s.ToString() + "\n";
            }
            foreach (MusicSymbol m in symbols)
            {
                result += "    " + m.ToString() + "\n";
            }
            result += "End Staff\n";
            return result;
        }

    }

}

