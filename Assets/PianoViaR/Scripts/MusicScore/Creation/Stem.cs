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
using PianoViaR.Score.Creation.Custom;
using PianoViaR.Utils;
using PianoViaR.MIDI.Parsing;
using BasicShapes;

namespace PianoViaR.Score.Creation
{

    /** @class Stem
     * The Stem class is used by ChordSymbol to draw the stem portion of
     * the chord.  The stem has the following fields:
     *
     * duration  - The duration of the stem.
     * direction - Either Up or Down
     * side      - Either left or right
     * top       - The topmost note in the chord
     * bottom    - The bottommost note in the chord
     * end       - The note position where the stem ends.  This is usually
     *             six notes past the last note in the chord.  For 8th/16th
     *             notes, the stem must extend even more.
     *
     * The SheetMusic class can change the direction of a stem after it
     * has been created.  The side and end fields may also change due to
     * the direction change.  But other fields will not change.
     */

    public class Stem
    {
        public const int Up = 1;      /* The stem points up */
        public const int Down = 2;      /* The stem points down */
        public const int LeftSide = 1;  /* The stem is to the left of the note */
        public const int RightSide = 2; /* The stem is to the right of the note */

        private NoteDuration duration; /** Duration of the stem. */
        private int direction;         /** Up, Down, or None */
        private WhiteNote top;         /** Topmost note in chord */
        private WhiteNote bottom;      /** Bottommost note in chord */
        private WhiteNote end;         /** Location of end of the stem */
        private bool notesOverlap;     /** Do the chord notes overlap */
        private bool hasDots;           /** Do the chord has any dotted note */
        private int side;              /** Left side or right side of note */

        private Stem pair;              /** If pair != null, this is a horizontal 
                                     * beam stem to another chord */
        private float widthToPair;      /** The width (in pixels) to the chord pair */
        private bool receiverInPair;  /** This stem is the receiver of a horizontal
                                    * beam stem from another chord. */

        private const int NoteBeamSides = 36; /** Sides to the beam ellipse faces */

        /** Get/Set the direction of the stem (Up or Down) */
        public int Direction
        {
            get { return direction; }
            set { ChangeDirection(value); }
        }

        /** Get the duration of the stem (Eigth, Sixteenth, ThirtySecond) */
        public NoteDuration Duration
        {
            get { return duration; }
        }

        /** Get the top note in the chord. This is needed to determine the stem direction */
        public WhiteNote Top
        {
            get { return top; }
        }

        /** Get the bottom note in the chord. This is needed to determine the stem direction */
        public WhiteNote Bottom
        {
            get { return bottom; }
        }

        /** Get/Set the location where the stem ends.  This is usually six notes
         * past the last note in the chord. See method CalculateEnd.
         */
        public WhiteNote End
        {
            get { return end; }
            set { end = value; }
        }

        /** Set this Stem to be the receiver of a horizontal beam, as part
         * of a chord pair.  In Draw(), if this stem is a receiver, we
         * don't draw a curvy stem, we only draw the vertical line.
         */
        public bool Receiver
        {
            get { return receiverInPair; }
            set { receiverInPair = value; }
        }

        public float ActualWidth
        {
            get
            {
                return SheetMusic.NoteStemWidth;
            }
        }

        public float FlagWidth
        {
            get
            {
                return SheetMusic.NoteHeadWidth;
            }
        }

        /** Create a new stem.  The top note, bottom note, and direction are 
         * needed for drawing the vertical line of the stem.  The duration is 
         * needed to draw the tail of the stem.  The overlap boolean is true
         * if the notes in the chord overlap.  If the notes overlap, the
         * stem must be drawn on the right side.
         */
        public Stem(WhiteNote bottom, WhiteNote top,
                    NoteDuration duration, int direction, bool overlap, bool hasDots = false)
        {

            this.top = top;
            this.bottom = bottom;
            this.duration = duration;
            this.direction = direction;
            this.notesOverlap = overlap;
            this.hasDots = hasDots;
            if (direction == Up || notesOverlap)
                side = RightSide;
            else
                side = LeftSide;
            end = CalculateEnd();
            pair = null;
            widthToPair = 0;
            receiverInPair = false;
        }

        /** Calculate the vertical position (white note key) where 
         * the stem ends 
         */
        public WhiteNote CalculateEnd()
        {
            if (direction == Up)
            {
                WhiteNote w = top;
                w = w.Add(6);
                if (duration == NoteDuration.Sixteenth)
                {
                    w = w.Add(2);
                }
                else if (duration == NoteDuration.ThirtySecond)
                {
                    w = w.Add(4);
                }
                return w;
            }
            else if (direction == Down)
            {
                WhiteNote w = bottom;
                w = w.Add(-6);
                if (duration == NoteDuration.Sixteenth)
                {
                    w = w.Add(-2);
                }
                else if (duration == NoteDuration.ThirtySecond)
                {
                    w = w.Add(-4);
                }
                return w;
            }
            else
            {
                return null;  /* Shouldn't happen */
            }
        }

        /** Change the direction of the stem.  This function is called by 
         * ChordSymbol.MakePair().  When two chords are joined by a horizontal
         * beam, their stems must point in the same direction (up or down).
         */
        public void ChangeDirection(int newdirection)
        {
            direction = newdirection;
            if (direction == Up || notesOverlap)
                side = RightSide;
            else
                side = LeftSide;
            end = CalculateEnd();
        }

        /** Pair this stem with another Chord.  Instead of drawing a curvy tail,
         * this stem will now have to draw a beam to the given stem pair.  The
         * width (in pixels) to this stem pair is passed as argument.
         */
        public void SetPair(Stem pair, float widthToPair)
        {
            this.pair = pair;
            this.widthToPair = widthToPair;
        }

        /** Return true if this Stem is part of a horizontal beam. */
        public bool isBeam
        {
            get { return receiverInPair || (pair != null); }
        }

        /** Draw this stem.
         * @param ytop The y location (in pixels) where the top of the staff starts.
         * @param topstaff  The note at the top of the staff.
         */
        public GameObject Create(MusicSymbolFactory factory, Vector3 position, float ytop, WhiteNote topstaff, in float offsetForBeam)
        {
            GameObject stem = new GameObject();

            if (duration == NoteDuration.Whole)
                return null;

            var stemLine = CreateVerticalLine(factory, position, ytop, topstaff);
            stemLine.name = "verticalLine";
            stemLine.transform.SetParent(stem.transform);

            if (duration == NoteDuration.Quarter ||
                duration == NoteDuration.DottedQuarter ||
                duration == NoteDuration.Half ||
                duration == NoteDuration.DottedHalf ||
                receiverInPair)
            {
                return stem;
            }

            if (pair != null)
            {
                var beams = CreateHorizontalBeams(factory, position, ytop, topstaff, offsetForBeam);
                beams.name = "beams";
                beams.transform.SetParent(stem.transform);
            }
            else
            {
                var flag = CreateFlag(factory, position, ytop, topstaff);
                flag?.transform.SetParent(stem.transform);
            }

            return stem;
        }

        /** Draw the vertical line of the stem 
         * @param ytop The y location (in pixels) where the top of the staff starts.
         * @param topstaff  The note at the top of the staff.
         */
        private GameObject CreateVerticalLine(MusicSymbolFactory factory, Vector3 position, float ytop, WhiteNote topstaff)
        {
            // Where already at the left of the note head
            float xnote = 0;

            if (side != LeftSide)
                xnote += SheetMusic.NoteHeadWidth - SheetMusic.NoteStemWidth;

            var stemLine = factory.CreateSymbol(SymbolType.NOTE_STEM);
            float heightToFit = 0;
            Vector3 offset = Vector3.zero;

            float spacing = SheetMusic.NoteVerticalSpacing;
            float ystart = 0;
            float yend = 0;

            if (direction == Up)
            {
                var distanceToBottom = topstaff.Dist(bottom) + 1;
                ystart = ytop + distanceToBottom * spacing - SheetMusic.LineWidth / 2;

                var distanceToEnd = topstaff.Dist(end);
                yend = ytop + distanceToEnd * spacing;
            }
            else if (direction == Down)
            {
                yend = ytop + (topstaff.Dist(top) + 1) * spacing - SheetMusic.LineWidth / 2;

                ystart = ytop + (topstaff.Dist(end) + 1) * spacing;
            }

            heightToFit = Mathf.Abs(ystart - yend);
            offset = new Vector3(xnote, -yend);

            stemLine.FitToWidth(ActualWidth);
            // Fit the game object in its Y axis (enlarge by Y axis)
            stemLine.FitOnlyToHeight(heightToFit);
            // Place game object at the right position
            stemLine.PlaceUpperLeft(position, offset);

            return stemLine;
        }

        /** Draw a curvy stem tail.  This is only used for single chords, not chord pairs.
         * @param ytop The y location (in pixels) where the top of the staff starts.
         * @param topstaff  The note at the top of the staff.
         */
        private GameObject CreateFlag(MusicSymbolFactory factory, Vector3 position, float ytop, WhiteNote topstaff)
        {
            GameObject noteFlag = null;

            if (duration == NoteDuration.Eighth ||
                    duration == NoteDuration.DottedEighth ||
                    duration == NoteDuration.Triplet)
            {
                // Create Eighth flag
                // Create the game object
                noteFlag = factory.CreateSymbol(SymbolType.NOTE_FLAG_EIGHTH);
                noteFlag.name = "noteFlagEight";
            }
            else if (duration == NoteDuration.Sixteenth)
            {
                // Create sixteenth flag
                // Create the game object
                noteFlag = factory.CreateSymbol(SymbolType.NOTE_FLAG_SIXTEENTH);
                noteFlag.name = "noteFlagSixteenth";
            }
            else if (duration == NoteDuration.ThirtySecond)
            {
                // Create thirtysecond flag
                // Create the game object
                noteFlag = factory.CreateSymbol(SymbolType.NOTE_FLAG_THIRTY_SECOND);
                noteFlag.name = "noteFlagThirtySecond";
            }
            else
            {
                return null;
            }

            float xstart = 0;
            if (side == LeftSide)
                xstart = SheetMusic.NoteStemWidth * 0.75f;
            else
                xstart = SheetMusic.NoteHeadWidth - SheetMusic.NoteStemWidth * 0.25f;

            float heightToFit = SheetMusic.WholeLineSpace * 3;

            float spacing = SheetMusic.NoteVerticalSpacing;
            float distanceToEnd = topstaff.Dist(end);
            float ystem = ytop + distanceToEnd * spacing;

            var xyPosition = new Vector3(xstart, -ystem);

            // Make sure game object is not null
            Ensure.ArgumentNotNull(noteFlag);

            // Fit the game object to its height
            noteFlag.FitToHeight(heightToFit);
            noteFlag.FitOnlyToWidth(FlagWidth);

            // Place the game object at the right position
            if (direction == Down)
            {
                xyPosition.y -= spacing;
                var rotation = new Vector3(180, 0, 0);
                noteFlag.transform.Rotate(rotation, Space.Self);
                noteFlag.PlaceBottomLeft(position, xyPosition);
            }
            else if (direction == Up)
            {
                noteFlag.PlaceUpperLeft(position, xyPosition);
            }

            return noteFlag;
        }

        private GameObject CreateBeam(MusicSymbolFactory factory, Vector3 position, Vector3 centerFace1, Vector3 centerFace2)
        {
            float faceWidth = SheetMusic.LineWidth / 2;
            float faceHeight = SheetMusic.BeamWidth / 2;
            const BasicShapes.Plane plane = BasicShapes.Plane.ZY;

            GameObject beam = factory.CreateSymbol(SymbolType.NOTE_BEAM);
            Beam beamBehaviour = beam.GetComponent<Beam>();

            var inverseY = new Vector3(1, -1, 1);
            var newCenterFace1 = centerFace1 + position;
            var newCenterFace2 = centerFace2 + position;

            beamBehaviour.Initialise(
                new Ellipse(NoteBeamSides, faceWidth, faceHeight, center: newCenterFace1, plane),
                new Ellipse(NoteBeamSides, faceWidth, faceHeight, center: newCenterFace2, plane)
            );

            return beam;
        }

        /* Draw a horizontal beam stem, connecting this stem with the Stem pair.
         * @param ytop The y location (in pixels) where the top of the staff starts.
         * @param topstaff  The note at the top of the staff.
         */
        private GameObject CreateHorizontalBeams(MusicSymbolFactory factory, Vector3 position, float ytop, WhiteNote topstaff, in float offset)
        {
            // Game object to hold all the beams in this stem
            GameObject beams = new GameObject();
            var beamName = "beam";

            float xstart = 0;
            float xstart2 = 0;

            if (side == LeftSide)
                xstart = 0;
            else if (side == RightSide)
                xstart = SheetMusic.NoteHeadWidth - SheetMusic.NoteStemWidth;

            if (pair.side == LeftSide)
                xstart2 = SheetMusic.NoteStemWidth;
            else if (pair.side == RightSide)
                xstart2 = SheetMusic.NoteHeadWidth;

            xstart2 += offset;

            var yOffset = SheetMusic.BeamWidth * 2;
            var spacing = SheetMusic.NoteVerticalSpacing;

            float xend = widthToPair + xstart2;
            float ystart = ytop + topstaff.Dist(end) * spacing;
            float yend = ytop + topstaff.Dist(pair.end) * spacing;

            if (direction == Down)
            {
                ystart += spacing;
                yend += spacing;
                yOffset = -yOffset;
            }

            if (duration == NoteDuration.Eighth ||
                duration == NoteDuration.DottedEighth ||
                duration == NoteDuration.Triplet ||
                duration == NoteDuration.Sixteenth ||
                duration == NoteDuration.ThirtySecond)
            {
                var beam = CreateBeam(factory, position, new Vector3(xstart, -ystart), new Vector3(xend, -yend));
                beam.name = beamName;
                beam.transform.SetParent(beams.transform);
            }

            ystart += yOffset;
            yend += yOffset;

            /* A dotted eighth will connect to a 16th note. */
            if (duration == NoteDuration.DottedEighth)
            {
                float x = xend - SheetMusic.NoteHeadWidth;
                float slope = (yend - ystart) * 1.0f / (xend - xstart);
                float y = slope * (x - xend) + yend;

                var beam = CreateBeam(factory, position, new Vector3(x, -y), new Vector3(xend, -yend));
                beam.name = beamName;
                beam.transform.SetParent(beams.transform);
            }

            if (duration == NoteDuration.Sixteenth ||
                duration == NoteDuration.ThirtySecond)
            {
                var beam = CreateBeam(factory, position, new Vector3(xstart, -ystart), new Vector3(xend, -yend));
                beam.name = beamName;
                beam.transform.SetParent(beams.transform);
            }

            ystart += yOffset;
            yend += yOffset;

            if (duration == NoteDuration.ThirtySecond)
            {
                var beam = CreateBeam(factory, position, new Vector3(xstart, -ystart), new Vector3(xend, -yend));
                beam.name = beamName;
                beam.transform.SetParent(beams.transform);
            }

            return beams;
        }

        public override string ToString()
        {
            return string.Format("Stem duration={0} direction={1} top={2} bottom={3} end={4}" +
                                 " overlap={5} side={6} width_to_pair={7} receiver_in_pair={8}",
                                 duration, direction, top.ToString(), bottom.ToString(),
                                 end.ToString(), notesOverlap, side, widthToPair, receiverInPair);
        }

    }


}

