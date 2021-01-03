using System.Collections.Generic;
using PianoViaR.Helpers;
using PianoViaR.MIDI.Helpers;
using PianoViaR.MIDI.Parsing;

namespace PianoViaR.Score.Behaviours.Helpers
{
    public class ConsecutiveNotes
    {
        private List<int> notes;
        public IEnumerable<int> Notes
        {
            get { return notes; }
            set
            {
                notes = GetNotes(value);
            }
        }

        public IEnumerable<PianoNotes> PianoNotes
        {
            set
            {
                Notes = GetNotes(value);
            }
        }
        public MIDIInstrument Instrument { get; set; }
        public TimeSignature TimeSignature { get; set; }
        public NoteDuration Duration { get; set; }

        private void Initialize(TimeSignature time, NoteDuration duration, MIDIInstrument instrument, List<int> notes)
        {
            TimeSignature = time ?? TimeSignature.Default;
            Duration = duration;
            Instrument = instrument;
            Notes = notes ?? new List<int>();
        }

        public ConsecutiveNotes()
        {
            Initialize(TimeSignature.Default, NoteDuration.Quarter, MIDIInstrument.AUTO, null);
        }
        public ConsecutiveNotes(TimeSignature time, NoteDuration duration, int instrument, List<int> notes)
        {
            Initialize(time, duration, MIDIInstrumentHelper.FromMIDINumber(instrument), notes);
        }

        public ConsecutiveNotes(TimeSignature time, NoteDuration duration, MIDIInstrument instrument, List<int> notes)
        {
            Initialize(time, duration, instrument, notes);
        }

        public ConsecutiveNotes(TimeSignature time, NoteDuration duration, MIDIInstrument instrument, List<PianoNotes> notes)
        {
            Initialize(time, duration, instrument, GetNotes(notes));
        }

        public ConsecutiveNotes(TimeSignature time, NoteDuration duration, MIDIInstrument instrument, params PianoNotes[] notes)
        {
            Initialize(time, duration, instrument, GetNotes(notes));
        }

        public void AddNote(int note)
        {
            notes.Add(note);
        }

        public void AddNote(PianoNotes note)
        {
            AddNote(note.MIDINumber());
        }

        public MIDITrack GetTrack()
        {
            var midiNotes = new List<MIDINote>();
            var durationTime = TimeSignature.DurationToTime(Duration);

            for (int i = 0; i < notes.Count; i++)
            {
                var noteValue = notes[i];
                var midiNote = new MIDINote(durationTime * i, 0, noteValue, durationTime);
                midiNotes.Add(midiNote);
            }

            return new MIDITrack(0)
            {
                Notes = midiNotes,
                Instrument = Instrument.MIDINumber(),
                Lyrics = null
            };
        }

        private static List<int> GetNotes(IEnumerable<PianoNotes> notes)
        {
            var intNotes = new List<int>();

            foreach (var note in notes)
            {
                intNotes.Add(note.MIDINumber());
            }

            return intNotes;
        }

        private static List<int> GetNotes(IEnumerable<int> notes)
        {
            var intNotes = new List<int>();

            foreach (var note in notes)
            {
                intNotes.Add(note);
            }

            return intNotes;
        }
    }
}