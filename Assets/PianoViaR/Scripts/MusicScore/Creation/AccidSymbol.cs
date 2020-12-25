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
using System.IO;
using System.Collections.Generic;
using MusicScore.Helpers;
using UnityEngine;
using PianoViaR.Utils;

namespace MidiSheetMusic
{


    /** Accidentals */
    public enum Accid
    {
        None, Sharp, Flat, Natural
    }

    /** @class AccidSymbol
     * An accidental (accid) symbol represents a sharp, flat, or natural
     * accidental that is displayed at a specific position (note and clef).
     */
    public class AccidSymbol : MusicSymbol
    {
        private Accid accid;          /** The accidental (sharp, flat, natural) */
        private WhiteNote whitenote;  /** The white note where the symbol occurs */
        private Clef clef;            /** Which clef the symbols is in */
        private float width;            /** Width of symbol */
        private bool chord; /** If this accidental symbol is used in a Chord symbol */

        /** 
         * Create a new AccidSymbol with the given accidental, that is
         * displayed at the given note in the given clef.
         */
        public AccidSymbol(Accid accid, WhiteNote note, Clef clef, bool chord = false)
        {
            this.accid = accid;
            this.whitenote = note;
            this.clef = clef;
            this.chord = chord;
            width = MinWidth;
        }

        /** Return the white note this accidental is displayed at */
        public WhiteNote Note
        {
            get { return whitenote; }
        }

        /** Get the time (in pulses) this symbol occurs at.
         * Not used.  Instead, the StartTime of the ChordSymbol containing this
         * AccidSymbol is used.
         */
        public int StartTime
        {
            get { return -1; }
        }

        /** Get the minimum width (in pixels) needed to draw this symbol */
        public float MinWidth
        {
            get
            {
                return ActualWidth + AccidentalSpacing;
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
                var actualWidth = SheetMusic.NoteHeadWidth;

                if (accid == Accid.Flat)
                {
                    actualWidth *= 0.75f;
                }

                return actualWidth;
            }
        }

        public float AccidentalSpacing
        {
            get
            {
                if (chord)
                {
                    return SheetMusic.AccidentalSpacing;
                }
                else
                {
                    return SheetMusic.AccidentalSpacing * 4;
                }
            }
        }

        public float Height
        {
            get
            {
                return SheetMusic.WholeLineSpace * 3 - SheetMusic.LineWidth;
            }
        }

        public float ActualHeight
        {
            get
            {
                float height = Height;

                if (accid == Accid.Flat) height *= 0.9f;
                return height;
            }
        }

        /** Get the number of pixels this symbol extends above the staff. Used
         *  to determine the minimum height needed for the staff (Staff.FindBounds).
         */
        public float AboveStaff
        {
            get { return GetAboveStaff(); }
        }

        float GetAboveStaff()
        {
            float dist = WhiteNote.Top(clef).Dist(whitenote) *
                       SheetMusic.NoteVerticalSpacing;

            if (dist < 0)
                return -dist;
            else
                return 0;
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
            float dist = WhiteNote.Bottom(clef).Dist(whitenote) *
                       SheetMusic.NoteVerticalSpacing;

            if (dist > 0)
                return dist;
            else
                return 0;
        }

        /** Draw the symbol.
         * @param ytop The ylocation (in pixels) where the top of the staff starts.
         */
        public GameObject Create(MusicSymbolFactory factory, Vector3 position, float ytop)
        {
            /* Store the y-pixel value of the top of the whitenote in ynote. */
            float ynote;
            var distance = WhiteNote.Top(clef).Dist(whitenote);

            if (chord)
            {
                distance += 1;
            }

            ynote = ytop + (distance * SheetMusic.NoteVerticalSpacing) - SheetMusic.LineWidth / 2;

            GameObject accidental;

            if (accid == Accid.Sharp)
            {
                accidental = factory.CreateSymbol(SymbolType.ACCIDENTAL_SHARP);
                accidental.name = "accidentalSharp";
            }
            else if (accid == Accid.Flat)
            {
                accidental = factory.CreateSymbol(SymbolType.ACCIDENTAL_FLAT);
                accidental.name = "accidentalFlat";
            }
            else if (accid == Accid.Natural)
            {
                accidental = factory.CreateSymbol(SymbolType.ACCIDENTAL_NATURAL);
                accidental.name = "accidentalNatural";
            }
            else
            {
                throw new ArgumentException("Cannot create unexisting accidental");
            }

            CreateAccidental(accidental, position, ynote);

            return accidental;
        }

        private void CreateAccidental(GameObject accidental, Vector3 position, in float ynote)
        {
            accidental.FitToWidth(ActualWidth);
            accidental.FitOnlyToHeight(ActualHeight);

            // Center vertically
            SetRigthPosition(accidental, position, ynote);
        }

        private void SetRigthPosition(GameObject accidental, Vector3 position, in float ynote)
        {
            var offset = new Vector3(0, -ynote);

            if (chord)
            {
                // Place at the right
                offset.x = Width;
                accidental.PlaceCenterRight(position, offset);
            }
            else
            {
                // Center at its width
                offset.x = Width / 2;
                accidental.PlaceCenter(position, offset);
            }
        }

        public override string ToString()
        {
            return string.Format(
              "AccidSymbol accid={0} whitenote={1} clef={2} width={3}",
              accid, whitenote, clef, width);
        }

    }

}


