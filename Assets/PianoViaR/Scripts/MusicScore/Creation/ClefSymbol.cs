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

    /** The possible clefs, Treble or Bass */
    public enum Clef { Treble, Bass };

    /** @class ClefSymbol 
     * A ClefSymbol represents either a Treble or Bass Clef image.
     * The clef can be either normal or small size.  Normal size is
     * used at the beginning of a new staff, on the left side.  The
     * small symbols are used to show clef changes within a staff.
     */

    public class ClefSymbol : MusicSymbol
    {
        private int starttime;        /** Start time of the symbol */
        private bool smallsize;       /** True if this is a small clef, false otherwise */
        private Clef clef;            /** The clef, Treble or Bass */
        private float width;

        /** Create a new ClefSymbol, with the given clef, starttime, and size */
        public ClefSymbol(Clef clef, int starttime, bool small)
        {
            this.clef = clef;
            this.starttime = starttime;
            smallsize = small;
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
            get
            {
                return ActualWidth + SheetMusic.ChordWidthOffset * 4;
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
            get
            {
                if (smallsize)
                    return SheetMusic.NoteHeadWidth * 1.8f;
                else
                    return SheetMusic.NoteHeadWidth * 2.6f;
            }
        }

        /** Get the number of pixels this symbol extends above the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        public float AboveStaff
        {
            get
            {
                if (clef == Clef.Treble && !smallsize)
                    return SheetMusic.NoteHeadHeight * 2;
                else
                    return 0;
            }
        }

        /** Get the number of pixels this symbol extends below the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        public float BelowStaff
        {
            get
            {
                if (clef == Clef.Treble && !smallsize)
                    return SheetMusic.NoteHeadHeight * 2;
                else if (clef == Clef.Treble && smallsize)
                    return SheetMusic.NoteHeadHeight;
                else
                    return 0;
            }
        }

        /** Draw the symbol.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public
        GameObject Create(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            float y = ytop - AboveStaff;
            float xOffset = Width / 2;
            string clefName;

            SymbolType clefType;

            if (clef == Clef.Treble)
            {
                clefType = SymbolType.CLEF_TREBLE;
                clefName = "clefTreble";
            }
            else
            {
                clefType = SymbolType.CLEF_BASS;
                clefName = "clefBass";
            }

            // Create the game object
            var clefGameObject = factory.CreateSymbol(clefType);
            // Fit the game object to its width
            clefGameObject.FitToWidth(ActualWidth);
            // Place the game object at the right position
            var xyPosition = new Vector3(xOffset, -y);
            clefGameObject.PlaceUpperCenter(position, xyPosition);
            clefGameObject.name = clefName;

            return clefGameObject;
        }

        public override string ToString()
        {
            return string.Format("ClefSymbol clef={0} small={1} width={2}",
                                 clef, smallsize, width);
        }
    }


}

