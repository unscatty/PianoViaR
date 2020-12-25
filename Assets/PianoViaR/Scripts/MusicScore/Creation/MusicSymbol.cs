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

namespace MidiSheetMusic
{


    /** @class MusicSymbol
     * The MusicSymbol class represents music symbols that can be displayed
     * on a staff.  This includes:
     *  - Accidental symbols: sharp, flat, natural
     *  - Chord symbols: single notes or chords
     *  - Rest symbols: whole, half, quarter, eighth
     *  - Bar symbols, the vertical bars which delimit measures.
     *  - Treble and Bass clef symbols
     *  - Blank symbols, used for aligning notes in different staffs
     */

    public interface MusicSymbol
    {
        /** Get the time (in pulses) this symbol occurs at.
         * This is used to determine the measure this symbol belongs to.
         */
        int StartTime { get; }

        /** Get the minimum width (in pixels) needed to draw this symbol */
        float MinWidth { get; }

        /** Get/Set the width (in pixels) of this symbol. The width is set
         * in SheetMusic.AlignSymbols() to vertically align symbols.
         */
        float Width { get; set; }

        /** The actual width of this symbol
         *  This value is used to scale the model to th required width with no spacing in both sides 
         */
        float ActualWidth { get; }

        /** Get the number of pixels this symbol extends above the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        float AboveStaff { get; }

        /** Get the number of pixels this symbol extends below the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        float BelowStaff { get; }

        /** Draw the symbol.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        GameObject Create(MusicSymbolFactory factory, Vector3 position, float ytop);
    }

}

