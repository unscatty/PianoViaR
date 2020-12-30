namespace PianoViaR.Utils
{
    public enum PianoNotes
    {
        INVALID = -1,
        A0 = 21,
        A0_SHARP,
        B0,
        C1,
        C1_SHARP,
        D1,
        D1_SHARP,
        E1,
        F1,
        F1_SHARP,
        G1,
        G1_SHARP,
        A1,
        A1_SHARP,
        B1,
        C2,
        C2_SHARP,
        D2,
        D2_SHARP,
        E2,
        F2,
        F2_SHARP,
        G2,
        G2_SHARP,
        A2,
        A2_SHARP,
        B2,
        C3,
        C3_SHARP,
        D3,
        D3_SHARP,
        E3,
        F3,
        F3_SHARP,
        G3,
        G3_SHARP,
        A3,
        A3_SHARP,
        B3,
        C4,
        C4_SHARP,
        D4,
        D4_SHARP,
        E4,
        F4,
        F4_SHARP,
        G4,
        G4_SHARP,
        A4,
        A4_SHARP,
        B4,
        C5,
        C5_SHARP,
        D5,
        D5_SHARP,
        E5,
        F5,
        F5_SHARP,
        G5,
        G5_SHARP,
        A5,
        A5_SHARP,
        B5,
        C6,
        C6_SHARP,
        D6,
        D6_SHARP,
        E6,
        F6,
        F6_SHARP,
        G6,
        G6_SHARP,
        A6,
        A6_SHARP,
        B6,
        C7,
        C7_SHARP,
        D7,
        D7_SHARP,
        E7,
        F7,
        F7_SHARP,
        G7,
        G7_SHARP,
        A7,
        A7_SHARP,
        B7,
        C8,
    }

    public static class PianoNotesHelper
    {
        public static int MIDINumber(this PianoNotes pianoNote)
        {
            return (int)pianoNote;
        }

        public static int PianoKeyNumber(this PianoNotes pianoNote)
        {
            // A0 is 21 in MIDI
            return pianoNote.MIDINumber() - 21;
        }

        public static PianoNotes FromMIDINumber(int noteNumber)
        {
            // MIDI range for piano notes
            if (noteNumber >= 21 && noteNumber <= 108)
            {
                return (PianoNotes)noteNumber;
            }
            else
            {
                return PianoNotes.INVALID;
            }
        }

        public static PianoNotes FromPianoKeyNumber(int pianoKeyNumber)
        {
            // 88 keys in a piano
            if (pianoKeyNumber >= 0 && pianoKeyNumber <= 87)
            {
                return (PianoNotes)(pianoKeyNumber + 21);
            }
            else
            {
                return PianoNotes.INVALID;
            }
        }
    }
}