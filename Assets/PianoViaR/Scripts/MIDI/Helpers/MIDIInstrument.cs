
using System.Globalization;

namespace PianoViaR.MIDI.Helpers
{
    public enum MIDIInstrument
    {
        AUTO = -1,
        ACOUSTIC_GRAND_PIANO = 0,
        BRIGHT_ACOUSTIC_PIANO,
        ELECTRIC_GRAND_PIANO,
        HONKY_TONK_PIANO,
        ELECTRIC_PIANO_1,
        ELECTRIC_PIANO_2,
        HARPSICHORD,
        CLAVI,
        CELESTA,
        GLOCKENSPIEL,
        MUSIC_BOX,
        VIBRAPHONE,
        MARIMBA,
        XYLOPHONE,
        TUBULAR_BELLS,
        DULCIMER,
        DRAWBAR_ORGAN,
        PERCUSSIVE_ORGAN,
        ROCK_ORGAN,
        CHURCH_ORGAN,
        REED_ORGAN,
        ACCORDION,
        HARMONICA,
        TANGO_ACCORDION,
        ACOUSTIC_GUITAR_NYLON,
        ACOUSTIC_GUITAR_STEEL,
        ELECTRIC_GUITAR_JAZZ,
        ELECTRIC_GUITAR_CLEAN,
        ELECTRIC_GUITAR_MUTED,
        OVERDRIVEN_GUITAR,
        DISTORTION_GUITAR,
        GUITAR_HARMONICS,
        ACOUSTIC_BASS,
        ELECTRIC_BASS_FINGER,
        ELECTRIC_BASS_PICK,
        FRETLESS_BASS,
        SLAP_BASS_1,
        SLAP_BASS_2,
        SYNTH_BASS_1,
        SYNTH_BASS_2,
        VIOLIN,
        VIOLA,
        CELLO,
        CONTRABASS,
        TREMOLO_STRINGS,
        PIZZICATO_STRINGS,
        ORCHESTRAL_HARP,
        TIMPANI,
        STRING_ENSEMBLE_1,
        STRING_ENSEMBLE_2,
        SYNTH_STRINGS_1,
        SYNTH_STRINGS_2,
        CHOIR_AAHS,
        VOICE_OOHS,
        SYNTH_VOICE,
        ORCHESTRA_HIT,
        TRUMPET,
        TROMBONE,
        TUBA,
        MUTED_TRUMPET,
        FRENCH_HORN,
        BRASS_SECTION,
        SYNTH_BRASS_1,
        SYNTH_BRASS_2,
        SOPRANO_SAX,
        ALTO_SAX,
        TENOR_SAX,
        BARITONE_SAX,
        OBOE,
        ENGLISH_HORN,
        BASSOON,
        CLARINET,
        PICCOLO,
        FLUTE,
        RECORDER,
        PAN_FLUTE,
        BLOWN_BOTTLE,
        SHAKUHACHI,
        WHISTLE,
        OCARINA,
        LEAD_1_SQUARE,
        LEAD_2_SAWTOOTH,
        LEAD_3_CALLIOPE,
        LEAD_4_CHIFF,
        LEAD_5_CHARANG,
        LEAD_6_VOICE,
        LEAD_7_FIFTHS,
        LEAD_8_BASS_LEAD,
        PAD_1_NEW_AGE,
        PAD_2_WARM,
        PAD_3_POLYSYNTH,
        PAD_4_CHOIR,
        PAD_5_BOWED,
        PAD_6_METALLIC,
        PAD_7_HALO,
        PAD_8_SWEEP,
        FX_1_RAIN,
        FX_2_SOUNDTRACK,
        FX_3_CRYSTAL,
        FX_4_ATMOSPHERE,
        FX_5_BRIGHTNESS,
        FX_6_GOBLINS,
        FX_7_ECHOES,
        FX_8_SCI_FI,
        SITAR,
        BANJO,
        SHAMISEN,
        KOTO,
        KALIMBA,
        BAG_PIPE,
        FIDDLE,
        SHANAI,
        TINKLE_BELL,
        AGOGO,
        STEEL_DRUMS,
        WOODBLOCK,
        TAIKO_DRUM,
        MELODIC_TOM,
        SYNTH_DRUM,
        REVERSE_CYMBAL,
        GUITAR_FRET_NOISE,
        BREATH_NOISE,
        SEASHORE,
        BIRD_TWEET,
        TELEPHONE_RING,
        HELICOPTER,
        APPLAUSE,
        GUNSHOT,
    }

    public static class MIDIInstrumentHelper
    {
        public static string Name(this MIDIInstrument instrument)
        {
            var snakeCase = instrument.ToString().Replace("_", " ").ToLower();
            return ToTitleCase(snakeCase);
        }

        static string ToTitleCase(string snakeCase)
        {
            TextInfo tInfo = new CultureInfo("en-US", false).TextInfo;
            return tInfo.ToTitleCase(snakeCase);
        }

        public static int MIDINumber(this MIDIInstrument instrument)
        {
            return ToMIDINumber(instrument);
        }

        public static int ToMIDINumber(MIDIInstrument instrument)
        {
            return (int)instrument;
        }

        public static MIDIInstrument FromMIDINumber(int instrumentNumber)
        {
            if (instrumentNumber >= 0 && instrumentNumber <= 127)
            {
                return (MIDIInstrument)instrumentNumber;
            }
            else
            {
                return MIDIInstrument.AUTO;
            }
        }
    }
}
