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

using UnityEngine;
using PianoViaR.Score.Helpers;
using PianoViaR.Utils;
using PianoViaR.MIDI.Parsing;

namespace PianoViaR.Score.Creation
{


    /* @class RestSymbol
     * A Rest symbol represents a rest - whole, half, quarter, or eighth.
     * The Rest symbol has a starttime and a duration, just like a regular
     * note.
     */
    public class RestSymbol : MusicSymbol
    {
        private int starttime;          /** The starttime of the rest */
        private NoteDuration duration;  /** The rest duration (eighth, quarter, half, whole) */
        private float width;              /** The width in pixels */

        /** Create a new rest symbol with the given start time and duration */
        public RestSymbol(int start, NoteDuration dur)
        {
            starttime = start;
            duration = dur;
            width = MinWidth;
        }

        /** Get the time (in pulses) this symbol occurs at.
         * This is used to determine the measure this symbol belongs to.
         */
        public int StartTime
        {
            get { return starttime; }
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
            get
            {
                return ActualWidth + SheetMusic.NoteToNoteDistance * 2;
            }
        }

        public float ActualWidth
        {
            get
            {
                switch (duration)
                {
                    case NoteDuration.Whole:
                    case NoteDuration.Half:
                    case NoteDuration.Eighth:
                        return SheetMusic.NoteHeadWidth * 1.5f;
                    case NoteDuration.Quarter:
                        return SheetMusic.NoteHeadWidth;
                    case NoteDuration.Sixteenth:
                        return SheetMusic.NoteHeadWidth * 2;
                    case NoteDuration.ThirtySecond:
                        return SheetMusic.NoteHeadWidth * 2.5f;
                    default:
                        return SheetMusic.NoteHeadWidth;
                }
            }
        }

        /** Get the number of pixels this symbol extends above the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        public float AboveStaff
        {
            get { return 0; }
        }

        /** Get the number of pixels this symbol extends below the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        public float BelowStaff
        {
            get { return 0; }
        }

        /** Draw the symbol.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public
        GameObject Create(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            /* Align the rest symbol to the right */
            // g.TranslateTransform(Width - MinWidth, 0);
            // g.TranslateTransform(SheetMusic.NoteHeight / 2, 0);

            float xOffset = Width / 2;
            var newPosition = new Vector3(xOffset, 0) + position;

            switch (duration)
            {
                case NoteDuration.Whole:
                    return CreateWhole(factory, newPosition, ytop);
                case NoteDuration.Half:
                    return CreateHalf(factory, newPosition, ytop);
                case NoteDuration.Quarter:
                    return CreateQuarter(factory, newPosition, ytop);
                case NoteDuration.Eighth:
                    return CreateEighth(factory, newPosition, ytop);
                case NoteDuration.Sixteenth:
                    return CreateSixteenth(factory, newPosition, ytop);
                case NoteDuration.ThirtySecond:
                    return CreateThirtySecond(factory, newPosition, ytop);
                default:
                    return new GameObject("CreateMe");
            }
        }


        /** Draw a whole rest symbol, a rectangle below a staff line.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public GameObject CreateWhole(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float y = ytop + SheetMusic.NoteHeadHeight;
            float heightToFit = SheetMusic.LineSpace * 0.75f;

            // Create the game object
            var wholeRest = factory.CreateSymbol(SymbolType.REST_WHOLE);
            // Fit the game object to its height
            wholeRest.FitToHeight(heightToFit);
            // Place the game object at the right position
            var xyPosition = new Vector3(0, -y);
            wholeRest.PlaceUpperCenter(position, xyPosition);

            wholeRest.name = "restWhole";

            return wholeRest;
        }

        /** Draw a half rest symbol, a rectangle above a staff line.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public GameObject CreateHalf(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float y = ytop + SheetMusic.WholeLineSpace * 2;
            float heightToFit = SheetMusic.LineSpace * 0.75f;

            // Create the game object
            var halfRest = factory.CreateSymbol(SymbolType.REST_HALF);
            // Fit the game object to its height
            halfRest.FitToHeight(heightToFit);
            // Place the game object at the right position
            var xyPosition = new Vector3(0, -y);
            halfRest.PlaceBottomCenter(position, xyPosition);

            halfRest.name = "restHalf";

            return halfRest;
        }

        /** Draw a quarter rest symbol.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public GameObject CreateQuarter(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float y = ytop - SheetMusic.LineWidth + SheetMusic.StaffHeight / 2;
            float heightToFit = SheetMusic.WholeLineSpace * 3;

            // Create the game object
            var quarterRest = factory.CreateSymbol(SymbolType.REST_QUARTER);
            // Fit the game object to its height
            quarterRest.FitToHeight(heightToFit);
            quarterRest.FitOnlyToWidth(ActualWidth);
            // Place the game object at the right position
            var xyPosition = new Vector3(0, -y);
            quarterRest.PlaceCenter(position, xyPosition);

            quarterRest.name = "restQuarter";

            return quarterRest;
        }

        /** Draw an eighth rest symbol.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public GameObject CreateEighth(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float y = ytop - SheetMusic.LineWidth + SheetMusic.StaffHeight / 2;
            float heightToFit = SheetMusic.WholeLineSpace * 2 - SheetMusic.LineWidth;

            // Create the game object
            var eighthRest = factory.CreateSymbol(SymbolType.REST_EIGHTH);
            // Fit the game object to its height
            eighthRest.FitToHeight(heightToFit);
            // Place the game object at the right position
            var xyPosition = new Vector3(0, -y);
            eighthRest.PlaceCenter(position, xyPosition);

            eighthRest.name = "restEighth";

            return eighthRest;
        }

        public GameObject CreateSixteenth(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float y = ytop;
            float heightToFit = SheetMusic.WholeLineSpace * 3 - SheetMusic.LineWidth;

            // Create the game object
            var sixteenthRest = factory.CreateSymbol(SymbolType.REST_SIXTEENTH);
            // Fit the game object to its height
            sixteenthRest.FitToHeight(heightToFit);
            // Place the game object at the right position
            var xyPosition = new Vector3(0, -y);
            sixteenthRest.PlaceUpperCenter(position, xyPosition);

            sixteenthRest.name = "restSixteenth";

            return sixteenthRest;
        }

        public GameObject CreateThirtySecond(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float y = ytop;
            float heightToFit = SheetMusic.StaffHeight - SheetMusic.LineWidth;

            // Create the game object
            var thirtySecondRest = factory.CreateSymbol(SymbolType.REST_THIRTY_SECOND);
            // Fit the game object to its height
            thirtySecondRest.FitToHeight(heightToFit);
            // Place the game object at the right position
            var xyPosition = new Vector3(0, -y);
            thirtySecondRest.PlaceUpperCenter(position, xyPosition);

            thirtySecondRest.name = "restThirtySecond";

            return thirtySecondRest;
        }

        public override string ToString()
        {
            return string.Format("RestSymbol starttime={0} duration={1} width={2}",
                                 starttime, duration, width);
        }

    }


}

