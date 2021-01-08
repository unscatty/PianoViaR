namespace PianoViaR.Score.Helpers
{
    public struct ScoreDimensions
    {
        /* Measurements used when drawing.  All measurements are in pixels.
         * The values depend on whether the menu 'Large Notes' or 'Small Notes' is selected.
         */
        public readonly float LineWidth;    /** The width of a line */
        public readonly float LeftMargin;  /** The left margin */
        public readonly float HeightMargin; /** The margin for bottom an top */
        public readonly float LineSpace;        /** The space between lines in the staff */
        public readonly float WholeLineSpace; /** The space between lines in the staff plus the width of a staff bar */
        public readonly float StaffHeight;      /** The height between the 5 horizontal lines of the staff */
        public readonly float SpaceBetweenStaffs; /** The space between every staff */
        public readonly float NoteHeadHeight;      /** The height of a whole note */
        public readonly float NoteHeadWidth;       /** The width of a whole note */
        public readonly float NoteStemWidth;      /** The width of a note stem */
        public readonly float NoteBarWidth; /** The width of a note top/bottom bar */
        public readonly float DotWidth; /** The width of a note dot */
        public readonly float BeamWidth; /** The width of a note beam */
        public readonly float WidthPerChar;   /** The width for every character */
        public readonly float NoteNameTextHeight; /** The height of the text for note names */
        public readonly float MeasureNameTextHeight; /** The height for the measure numbers */
        public readonly float NoteToDotDistance; /** The distance between a note head and a dot */
        public readonly float NoteToNoteDistance; /** The distance between a note head and another */
        public readonly float ChordWidth;         /** The width of a note head plus spacing between another chord */
        public readonly float ChordOverlapWidth;  /** The width of two note heads plus spacing between another chord */
        public readonly float ChordWidthOffset; /** The spacing between chords */
        public readonly float NoteToAccidentalDistance; /** The distance between a note head and an accidental symbol */
        public readonly float AccidentalSpacing;  /** The distance between accidental symbols **/
        public readonly float NoteToNameDistance; /** The distance between a note and its name */
        public readonly float NoteVerticalSpacing; /** The vertical spacing between note heads */
        public readonly float NoteToStemOffset; /** The vertical offset for stems to reach note head curvature */
        public readonly float PageWidth;    /** The width of each page */
        public readonly float PageHeight;  /** The height of each page (when printing) */
        public readonly float MeasureNameTextAdjustScale;

        public ScoreDimensions(float noteHeadHeight, float noteHeadWidth, float pageWidth, float pageHeight, float leftMargin)
        {
            PageWidth = pageWidth;
            PageHeight = pageHeight;
            LeftMargin = leftMargin;

            NoteHeadHeight = noteHeadHeight;
            NoteHeadWidth = noteHeadWidth;

            LeftMargin = 0;

            LineSpace = NoteHeadHeight;
            LineWidth = NoteHeadHeight / 8;
            NoteBarWidth = NoteHeadWidth * 1.5f;
            DotWidth = NoteHeadHeight / 2 - LineWidth / 2;
            BeamWidth = LineWidth * 3;

            WholeLineSpace = LineSpace + LineWidth;
            NoteVerticalSpacing = WholeLineSpace / 2;
            NoteToStemOffset = NoteHeadHeight / 4;
            // There are 4 spaces in the whole staff, plus the size of every line
            StaffHeight = WholeLineSpace * 4 + LineWidth;
            SpaceBetweenStaffs = NoteHeadHeight * 2;
            HeightMargin = NoteHeadHeight;

            WidthPerChar = NoteHeadWidth / 2;
            NoteNameTextHeight = NoteHeadHeight + LineWidth * 3;
            MeasureNameTextHeight = NoteHeadHeight;
            MeasureNameTextAdjustScale = 1.5f;

            NoteStemWidth = LineWidth;
            NoteToDotDistance = LineWidth * 2;
            NoteToNameDistance = LineWidth;

            // Chord Alignment
            NoteToNoteDistance = NoteHeadWidth;
            ChordWidthOffset = NoteToNoteDistance / 2;

            var chorOverlapDisplayWidth = NoteHeadWidth * 2 - NoteStemWidth;
            var chordDisplayWidth = NoteHeadWidth;

            ChordOverlapWidth = chorOverlapDisplayWidth + NoteToNoteDistance;
            ChordWidth = chordDisplayWidth + NoteToNoteDistance;

            // Accidental alignment
            NoteToAccidentalDistance = NoteHeadWidth;
            AccidentalSpacing = LineWidth;
        }
    }
}