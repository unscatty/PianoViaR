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

namespace PianoViaR.Score.Creation
{

    /** @class TimeSigSymbol
     * A TimeSigSymbol represents the time signature at the beginning
     * of the staff. We use pre-made images for the numbers, instead of
     * drawing strings.
     */

    public class TimeSigSymbol : MusicSymbol
    {
        // private static Image[] images;  /** The images for each number */
        private int numerator;         /** The numerator */
        private int denominator;       /** The denominator */
        private float width;             /** The width in pixels */
        private ScoreDimensions dimensions;

        /** Create a new TimeSigSymbol */
        public TimeSigSymbol(int numer, int denom, in ScoreDimensions dimensions)
        {
            this.dimensions = dimensions;
            numerator = numer;
            denominator = denom;

            width = MinWidth;
        }

        /** Load the images into memory. */

        /** Get the time (in pulses) this symbol occurs at. */
        public int StartTime
        {
            get { return -1; }
        }

        /** Get the minimum width (in pixels) needed to draw this symbol */
        public float MinWidth
        {
            get
            {
                return ActualWidth + dimensions.NoteToNoteDistance * 2;
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

        public float ActualWidth
        {
            get { return dimensions.NoteHeadWidth * 2; }
        }

        public float Height
        {
            get
            {
                return dimensions.StaffHeight + dimensions.WholeLineSpace;
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
        public GameObject Create(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float x = Width / 2;
            float ynote = ytop - dimensions.LineWidth + dimensions.StaffHeight / 2;

            float heightToFit = Height;

            var xyPosition = new Vector3(x, -ynote);

            var textSignature = factory.CreateSymbol(SymbolType.SIGNATURE_TEXT);
            textSignature.TextSetText($"{numerator}\n{denominator}");

            textSignature.TextFitToHeight(heightToFit);
            // textSignature.TextPlaceUpperCenter(position, xyPosition);
            textSignature.TextPlaceCenter(position, xyPosition);

            // var dot = factory.CreateSymbol(SymbolType.NOTE_DOT);
            // dot.FitToWidth(dimensions.DotWidth);
            // dot.PlaceCenterLeft(position, xyPosition);

            return textSignature;
        }

        public override string ToString()
        {
            return string.Format("TimeSigSymbol numerator={0} denominator={1}",
                                 numerator, denominator);
        }
    }

}

