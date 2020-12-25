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
using MusicScore.Helpers;
using PianoViaR.Utils;

namespace MidiSheetMusic
{


    /** @class BarSymbol
     * The BarSymbol represents the vertical bars which delimit measures.
     * The starttime of the symbol is the beginning of the new
     * measure.
     */
    public class BarSymbol : MusicSymbol
    {
        private int starttime;
        private float width;

        /** Create a BarSymbol. The starttime should be the beginning of a measure. */
        public BarSymbol(int starttime)
        {
            this.starttime = starttime;
            width = MinWidth;
        }

        /** Get the time (in pulses) this symbol occurs at.
         * This is used to determine the measure this symbol belongs to.
         */
        public int StartTime
        {
            get { return starttime; }
        }

        /** Get the minimum width (in pixels) needed to draw this symbol */
        public float MinWidth
        {
            get { return ActualWidth + SheetMusic.NoteToNoteDistance * 2; }
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
            get
            {
                return SheetMusic.LineWidth;
            }
        }

        public float Height
        {
            get { return SheetMusic.LineSpace * 4 + SheetMusic.LineWidth * 3; }
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

        /** Draw a vertical bar.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public
        GameObject Create(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float y = ytop;

            var heightToFit = Height;

            // Create the game object
            var bar = factory.CreateSymbol(SymbolType.NOTE_STEM);
            bar.FitToWidth(ActualWidth);
            // Fit the game object in its Y axis (enlarge by Y axis)
            bar.FitOnlyToHeight(heightToFit);
            // Center the bar
            var xyPosition = new Vector3(Width / 2, -y);
            bar.PlaceUpperCenter(position, xyPosition);

            bar.gameObject.name = "bar";

            return bar;
        }

        public override string ToString()
        {
            return string.Format("BarSymbol starttime={0} width={1}",
                                 starttime, width);
        }
    }


}

