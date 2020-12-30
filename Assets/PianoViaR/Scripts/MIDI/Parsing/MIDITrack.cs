/*
 * Copyright (c) 2007-2012 Madhav Vaidyanathan
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License version 2.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using System.Collections.Generic;
using PianoViaR.MIDI.Helpers;

namespace PianoViaR.MIDI.Parsing
{

    /** @class MidiTrack
     * The MidiTrack takes as input the raw MidiEvents for the track, and gets:
     * - The list of midi notes in the track.
     * - The first instrument used in the track.
     *
     * For each NoteOn event in the midi file, a new MidiNote is created
     * and added to the track, using the AddNote() method.
     * 
     * The NoteOff() method is called when a NoteOff event is encountered,
     * in order to update the duration of the MidiNote.
     */
    public class MIDITrack
    {
        private int trackNumber;             /** The track number */
        private List<MIDINote> notes;     /** List of Midi notes */
        private int instrument;           /** Instrument for this track */
        private List<MIDIEvent> lyrics;   /** The lyrics in this track */

        /** Create an empty MidiTrack.  Used by the Clone method */
        public MIDITrack(int tracknum)
        {
            this.trackNumber = tracknum;
            notes = new List<MIDINote>(20);
            instrument = 0;
        }

        public MIDITrack(List<MIDINote> notes, int trackNumber, int instrument, List<MIDIEvent> lyrics = null)
        {
            this.notes = notes;
            this.trackNumber = trackNumber;
            this.instrument = instrument;
            this.lyrics = lyrics;
        }

        public MIDITrack(List<MIDINote> notes, int trackNumber, MIDIInstrument instrument, List<MIDIEvent> lyrics = null)
        : this(notes, trackNumber, (int)instrument, lyrics)
        { }

        /** Create a MidiTrack based on the Midi events.  Extract the NoteOn/NoteOff
         *  events to gather the list of MidiNotes.
         */
        public MIDITrack(List<MIDIEvent> events, int tracknum)
        {
            this.trackNumber = tracknum;
            notes = new List<MIDINote>(events.Count);
            instrument = 0;

            foreach (MIDIEvent mevent in events)
            {
                if (mevent.EventFlag == MIDIFile.EventNoteOn && mevent.Velocity > 0)
                {
                    MIDINote note = new MIDINote(mevent.StartTime, mevent.Channel, mevent.Notenumber, 0);
                    AddNote(note);
                }
                else if (mevent.EventFlag == MIDIFile.EventNoteOn && mevent.Velocity == 0)
                {
                    NoteOff(mevent.Channel, mevent.Notenumber, mevent.StartTime);
                }
                else if (mevent.EventFlag == MIDIFile.EventNoteOff)
                {
                    NoteOff(mevent.Channel, mevent.Notenumber, mevent.StartTime);
                }
                else if (mevent.EventFlag == MIDIFile.EventProgramChange)
                {
                    instrument = mevent.Instrument;
                }
                else if (mevent.Metaevent == MIDIFile.MetaEventLyric)
                {
                    AddLyric(mevent);
                }
            }
            if (notes.Count > 0 && notes[0].Channel == 9)
            {
                instrument = 128;  /* Percussion */
            }
            int lyriccount = 0;
            if (lyrics != null) { lyriccount = lyrics.Count; }
        }

        public int TrackNumber
        {
            set { trackNumber = value; }
            get { return trackNumber; }
        }

        public List<MIDINote> Notes
        {
            set
            {
                if (value != null && value.Count > 0)
                {
                    notes = value;
                }
                else
                {
                    throw new System.ArgumentException("Note list must contain at least one note");
                }
            }
            get { return notes; }
        }

        public int Instrument
        {
            get { return instrument; }
            set { instrument = value; }
        }

        public MIDIInstrument MIDIInstrument
        {
            get
            {
                return (MIDIInstrument)Instrument;
            }
            set
            {
                instrument = (int)value;
            }
        }

        public string InstrumentName
        {
            get
            {
                if (instrument >= 0 && instrument <= 128)
                    return MIDIInstrument.Name();
                else
                    return "";
            }
        }

        public List<MIDIEvent> Lyrics
        {
            get { return lyrics; }
            set { lyrics = value; }
        }

        /** Add a MidiNote to this track.  This is called for each NoteOn event */
        public void AddNote(MIDINote m)
        {
            notes.Add(m);
        }

        /** A NoteOff event occured.  Find the MidiNote of the corresponding
         * NoteOn event, and update the duration of the MidiNote.
         */
        public void NoteOff(int channel, int notenumber, int endtime)
        {
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                MIDINote note = notes[i];
                if (note.Channel == channel && note.Number == notenumber &&
                    note.Duration == 0)
                {
                    note.NoteOff(endtime);
                    return;
                }
            }
        }

        /** Add a Lyric MidiEvent */
        public void AddLyric(MIDIEvent mevent)
        {
            if (lyrics == null)
            {
                lyrics = new List<MIDIEvent>();
            }
            lyrics.Add(mevent);
        }

        /** Return a deep copy clone of this MidiTrack. */
        public MIDITrack Clone()
        {
            MIDITrack track = new MIDITrack(TrackNumber);
            track.instrument = instrument;
            foreach (MIDINote note in notes)
            {
                track.notes.Add(note.Clone());
            }
            if (lyrics != null)
            {
                track.lyrics = new List<MIDIEvent>();
                foreach (MIDIEvent ev in lyrics)
                {
                    track.lyrics.Add(ev);
                }
            }
            return track;
        }
        public override string ToString()
        {
            string result = "Track number=" + trackNumber + " instrument=" + instrument + "\n";
            foreach (MIDINote n in notes)
            {
                result = result + n + "\n";
            }
            result += "End Track\n";
            return result;
        }
    }
}
