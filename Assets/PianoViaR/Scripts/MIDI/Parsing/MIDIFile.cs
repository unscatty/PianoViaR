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

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using PianoViaR.MIDI.Helpers;

namespace PianoViaR.MIDI.Parsing
{
    /* This file contains the classes for parsing and modifying
     * MIDI music files.
     */

    /* MIDI file format.
     *
     * The Midi File format is described below.  The description uses
     * the following abbreviations.
     *
     * u1     - One byte
     * u2     - Two bytes (big endian)
     * u4     - Four bytes (big endian)
     * varlen - A variable length integer, that can be 1 to 4 bytes. The 
     *          integer ends when you encounter a byte that doesn't have 
     *          the 8th bit set (a byte less than 0x80).
     * len?   - The length of the data depends on some code
     *          
     *
     * The Midi files begins with the main Midi header
     * u4 = The four ascii characters 'MThd'
     * u4 = The length of the MThd header = 6 bytes
     * u2 = 0 if the file contains a single track
     *      1 if the file contains one or more simultaneous tracks
     *      2 if the file contains one or more independent tracks
     * u2 = number of tracks
     * u2 = if >  0, the number of pulses per quarter note
     *      if <= 0, then ???
     *
     * Next come the individual Midi tracks.  The total number of Midi
     * tracks was given above, in the MThd header.  Each track starts
     * with a header:
     *
     * u4 = The four ascii characters 'MTrk'
     * u4 = Amount of track data, in bytes.
     * 
     * The track data consists of a series of Midi events.  Each Midi event
     * has the following format:
     *
     * varlen  - The time between the previous event and this event, measured
     *           in "pulses".  The number of pulses per quarter note is given
     *           in the MThd header.
     * u1      - The Event code, always betwee 0x80 and 0xFF
     * len?    - The event data.  The length of this data is determined by the
     *           event code.  The first byte of the event data is always < 0x80.
     *
     * The event code is optional.  If the event code is missing, then it
     * defaults to the previous event code.  For example:
     *
     *   varlen, eventcode1, eventdata,
     *   varlen, eventcode2, eventdata,
     *   varlen, eventdata,  // eventcode is eventcode2
     *   varlen, eventdata,  // eventcode is eventcode2
     *   varlen, eventcode3, eventdata,
     *   ....
     *
     *   How do you know if the eventcode is there or missing? Well:
     *   - All event codes are between 0x80 and 0xFF
     *   - The first byte of eventdata is always less than 0x80.
     *   So, after the varlen delta time, if the next byte is between 0x80
     *   and 0xFF, its an event code.  Otherwise, its event data.
     *
     * The Event codes and event data for each event code are shown below.
     *
     * Code:  u1 - 0x80 thru 0x8F - Note Off event.
     *             0x80 is for channel 1, 0x8F is for channel 16.
     * Data:  u1 - The note number, 0-127.  Middle C is 60 (0x3C)
     *        u1 - The note velocity.  This should be 0
     * 
     * Code:  u1 - 0x90 thru 0x9F - Note On event.
     *             0x90 is for channel 1, 0x9F is for channel 16.
     * Data:  u1 - The note number, 0-127.  Middle C is 60 (0x3C)
     *        u1 - The note velocity, from 0 (no sound) to 127 (loud).
     *             A value of 0 is equivalent to a Note Off.
     *
     * Code:  u1 - 0xA0 thru 0xAF - Key Pressure
     * Data:  u1 - The note number, 0-127.
     *        u1 - The pressure.
     *
     * Code:  u1 - 0xB0 thru 0xBF - Control Change
     * Data:  u1 - The controller number
     *        u1 - The value
     *
     * Code:  u1 - 0xC0 thru 0xCF - Program Change
     * Data:  u1 - The program number.
     *
     * Code:  u1 - 0xD0 thru 0xDF - Channel Pressure
     *        u1 - The pressure.
     *
     * Code:  u1 - 0xE0 thru 0xEF - Pitch Bend
     * Data:  u2 - Some data
     *
     * Code:  u1     - 0xFF - Meta Event
     * Data:  u1     - Metacode
     *        varlen - Length of meta event
     *        u1[varlen] - Meta event data.
     *
     *
     * The Meta Event codes are listed below:
     *
     * Metacode: u1         - 0x0  Sequence Number
     *           varlen     - 0 or 2
     *           u1[varlen] - Sequence number
     *
     * Metacode: u1         - 0x1  Text
     *           varlen     - Length of text
     *           u1[varlen] - Text
     *
     * Metacode: u1         - 0x2  Copyright
     *           varlen     - Length of text
     *           u1[varlen] - Text
     *
     * Metacode: u1         - 0x3  Track Name
     *           varlen     - Length of name
     *           u1[varlen] - Track Name
     *
     * Metacode: u1         - 0x58  Time Signature
     *           varlen     - 4 
     *           u1         - numerator
     *           u1         - log2(denominator)
     *           u1         - clocks in metronome click
     *           u1         - 32nd notes in quarter note (usually 8)
     *
     * Metacode: u1         - 0x59  Key Signature
     *           varlen     - 2
     *           u1         - if >= 0, then number of sharps
     *                        if < 0, then number of flats * -1
     *           u1         - 0 if major key
     *                        1 if minor key
     *
     * Metacode: u1         - 0x51  Tempo
     *           varlen     - 3  
     *           u3         - quarter note length in microseconds
     */


    /** @class MidiFile
     *
     * The MidiFile class contains the parsed data from the Midi File.
     * It contains:
     * - All the tracks in the midi file, including all MidiNotes per track.
     * - The time signature (e.g. 4/4, 3/4, 6/8)
     * - The number of pulses per quarter note.
     * - The tempo (number of microseconds per quarter note).
     *
     * The constructor takes a filename as input, and upon returning,
     * contains the parsed data from the midi file.
     *
     * The methods ReadTrack() and ReadMetaEvent() are helper functions called
     * by the constructor during the parsing.
     *
     * After the MidiFile is parsed and created, the user can retrieve the 
     * tracks and notes by using the property Tracks and Tracks.Notes.
     *
     * There are two methods for modifying the midi data based on the menu
     * options selected:
     *
     * - ChangeMidiNotes()
     *   Apply the menu options to the parsed MidiFile.  This uses the helper functions:
     *     SplitTrack()
     *     CombineToTwoTracks()
     *     ShiftTime()
     *     Transpose()
     *     RoundStartTimes()
     *     RoundDurations()
     *
     * - ChangeSound()
     *   Apply the menu options to the MIDI music data, and save the modified midi data 
     *   to a file, for playback. 
     *   
     */

    [System.Serializable]
    public class MIDIFile
    {
        private string filename;          /** The Midi file name */
        private List<MIDIEvent>[] events; /** The raw MidiEvents, one list per track */
        private List<MIDITrack> tracks;  /** The tracks of the midifile that have notes */
        private ushort trackmode;         /** 0 (single track), 1 (simultaneous tracks) 2 (independent tracks) */
        private TimeSignature timesig;    /** The time signature */
        private int quarternote;          /** The number of pulses per quarter note */
        private int totalpulses;          /** The total length of the song, in pulses */
        private bool trackPerChannel;     /** True if we've split each channel into a track */

        /* The list of Midi Events */
        public const int EventNoteOff = 0x80;
        public const int EventNoteOn = 0x90;
        public const int EventKeyPressure = 0xA0;
        public const int EventControlChange = 0xB0;
        public const int EventProgramChange = 0xC0;
        public const int EventChannelPressure = 0xD0;
        public const int EventPitchBend = 0xE0;
        public const int SysexEvent1 = 0xF0;
        public const int SysexEvent2 = 0xF7;
        public const int MetaEvent = 0xFF;

        /* The list of Meta Events */
        public const int MetaEventSequence = 0x0;
        public const int MetaEventText = 0x1;
        public const int MetaEventCopyright = 0x2;
        public const int MetaEventSequenceName = 0x3;
        public const int MetaEventInstrument = 0x4;
        public const int MetaEventLyric = 0x5;
        public const int MetaEventMarker = 0x6;
        public const int MetaEventEndOfTrack = 0x2F;
        public const int MetaEventTempo = 0x51;
        public const int MetaEventSMPTEOffset = 0x54;
        public const int MetaEventTimeSignature = 0x58;
        public const int MetaEventKeySignature = 0x59;
        /* End Instruments */

        /** Return a string representation of a Midi event */
        private string EventName(int ev)
        {
            if (ev >= EventNoteOff && ev < EventNoteOff + 16)
                return "NoteOff";
            else if (ev >= EventNoteOn && ev < EventNoteOn + 16)
                return "NoteOn";
            else if (ev >= EventKeyPressure && ev < EventKeyPressure + 16)
                return "KeyPressure";
            else if (ev >= EventControlChange && ev < EventControlChange + 16)
                return "ControlChange";
            else if (ev >= EventProgramChange && ev < EventProgramChange + 16)
                return "ProgramChange";
            else if (ev >= EventChannelPressure && ev < EventChannelPressure + 16)
                return "ChannelPressure";
            else if (ev >= EventPitchBend && ev < EventPitchBend + 16)
                return "PitchBend";
            else if (ev == MetaEvent)
                return "MetaEvent";
            else if (ev == SysexEvent1 || ev == SysexEvent2)
                return "SysexEvent";
            else
                return "Unknown";
        }

        /** Return a string representation of a meta-event */
        private string MetaName(int ev)
        {
            if (ev == MetaEventSequence)
                return "MetaEventSequence";
            else if (ev == MetaEventText)
                return "MetaEventText";
            else if (ev == MetaEventCopyright)
                return "MetaEventCopyright";
            else if (ev == MetaEventSequenceName)
                return "MetaEventSequenceName";
            else if (ev == MetaEventInstrument)
                return "MetaEventInstrument";
            else if (ev == MetaEventLyric)
                return "MetaEventLyric";
            else if (ev == MetaEventMarker)
                return "MetaEventMarker";
            else if (ev == MetaEventEndOfTrack)
                return "MetaEventEndOfTrack";
            else if (ev == MetaEventTempo)
                return "MetaEventTempo";
            else if (ev == MetaEventSMPTEOffset)
                return "MetaEventSMPTEOffset";
            else if (ev == MetaEventTimeSignature)
                return "MetaEventTimeSignature";
            else if (ev == MetaEventKeySignature)
                return "MetaEventKeySignature";
            else
                return "Unknown";
        }


        /** Get the list of tracks */
        public List<MIDITrack> Tracks
        {
            get { return tracks; }
        }

        /** Get the time signature */
        public TimeSignature Time
        {
            get { return timesig; }
        }

        /** Get the file name */
        public string FileName
        {
            get { return filename; }
        }

        /** Get the total length (in pulses) of the song */
        public int TotalPulses
        {
            get { return totalpulses; }
        }


        /** Create a new MidiFile from the file. */
        public MIDIFile(string filename)
        {
            MIDIFileReader file = new MIDIFileReader(filename);
            parse(file, filename);
        }

        public MIDIFile()
        { // wang1ang
            CreateBlank();
        }
        /** Create a new MidiFile from the byte[]. */
        public MIDIFile(byte[] data, string title)
        {
            MIDIFileReader file = new MIDIFileReader(data);
            if (title == null)
                title = "";
            parse(file, title);
        }

        /** Parse the given Midi file, and return an instance of this MidiFile
         * class.  After reading the midi file, this object will contain:
         * - The raw list of midi events
         * - The Time Signature of the song
         * - All the tracks in the song which contain notes. 
         * - The number, starttime, and duration of each note.
         */
        public void parse(MIDIFileReader file, string filename)
        {
            string id;
            int len;

            this.filename = filename;
            tracks = new List<MIDITrack>();
            trackPerChannel = false;

            id = file.ReadAscii(4);
            if (id != "MThd")
            {
                throw new MIDIFileException("Doesn't start with MThd", 0);
            }
            len = file.ReadInt();
            if (len != 6)
            {
                throw new MIDIFileException("Bad MThd header", 4);
            }
            trackmode = file.ReadShort();
            int num_tracks = file.ReadShort();
            quarternote = file.ReadShort();

            events = new List<MIDIEvent>[num_tracks];
            for (int tracknum = 0; tracknum < num_tracks; tracknum++)
            {
                events[tracknum] = ReadTrack(file);
                MIDITrack track = new MIDITrack(events[tracknum], tracknum);
                if (track.Notes.Count > 0 || track.Lyrics != null)
                {
                    tracks.Add(track);
                }
            }

            /* Get the length of the song in pulses */
            foreach (MIDITrack track in tracks)
            {
                MIDINote last = track.Notes[track.Notes.Count - 1];
                if (this.totalpulses < last.StartTime + last.Duration)
                {
                    this.totalpulses = last.StartTime + last.Duration;
                }
            }

            /* If we only have one track with multiple channels, then treat
             * each channel as a separate track.
             */
            if (tracks.Count == 1 && HasMultipleChannels(tracks[0]))
            {
                tracks = SplitChannels(tracks[0], events[tracks[0].TrackNumber]);
                trackPerChannel = true;
            }

            CheckStartTimes(tracks);

            /* Determine the time signature */
            int tempo = 0;
            int numer = 0;
            int denom = 0;
            foreach (List<MIDIEvent> list in events)
            {
                foreach (MIDIEvent mevent in list)
                {
                    if (mevent.Metaevent == MetaEventTempo && tempo == 0)
                    {
                        tempo = mevent.Tempo;
                    }
                    if (mevent.Metaevent == MetaEventTimeSignature && numer == 0)
                    {
                        numer = mevent.Numerator;
                        denom = mevent.Denominator;
                    }
                }
            }
            if (tempo == 0)
            {
                tempo = 500000; /* 500,000 microseconds = 0.05 sec */
            }
            if (numer == 0)
            {
                numer = 4; denom = 4;
            }
            timesig = new TimeSignature(numer, denom, quarternote, tempo);
        }

        /*
         * - The raw list of midi events
         * - The Time Signature of the song
         * - All the tracks in the song which contain notes. 
         * - The number, starttime, and duration of each note.
         */
        public void CreateBlank()
        {
            this.filename = "";
            tracks = new List<MIDITrack>();
            trackPerChannel = false;

            trackmode = 1; // file.ReadShort();
            int num_tracks = 2; // file.ReadShort();
            quarternote = 120; // file.ReadShort(); 

            events = new List<MIDIEvent>[num_tracks];
            events[0] = NewTrack(60);
            tracks.Add(new MIDITrack(events[0], 0));
            events[1] = NewTrack(48);
            tracks.Add(new MIDITrack(events[1], 1));

            /* Get the length of the song in pulses */
            foreach (MIDITrack track in tracks)
            {
                MIDINote last = track.Notes[track.Notes.Count - 1];
                if (this.totalpulses < last.StartTime + last.Duration)
                {
                    this.totalpulses = last.StartTime + last.Duration;
                }
            }

            /* If we only have one track with multiple channels, then treat
             * each channel as a separate track.
             */
            if (tracks.Count == 1 && HasMultipleChannels(tracks[0]))
            {
                tracks = SplitChannels(tracks[0], events[tracks[0].TrackNumber]);
                trackPerChannel = true;
            }

            CheckStartTimes(tracks);

            /* Determine the time signature */
            int tempo = 0;
            int numer = 0;
            int denom = 0;
            foreach (List<MIDIEvent> list in events)
            {
                foreach (MIDIEvent mevent in list)
                {
                    if (mevent.Metaevent == MetaEventTempo && tempo == 0)
                    {
                        tempo = mevent.Tempo;
                    }
                    if (mevent.Metaevent == MetaEventTimeSignature && numer == 0)
                    {
                        numer = mevent.Numerator;
                        denom = mevent.Denominator;
                    }
                }
            }
            if (tempo == 0)
            {
                tempo = 500000; /* 500,000 microseconds = 0.05 sec */
            }
            if (numer == 0)
            {
                numer = 4; denom = 4;
            }
            timesig = new TimeSignature(numer, denom, quarternote, tempo);
        }

        /** Parse a single Midi track into a list of MidiEvents.
         * Entering this function, the file offset should be at the start of
         * the MTrk header.  Upon exiting, the file offset should be at the
         * start of the next MTrk header.
         */
        private List<MIDIEvent> ReadTrack(MIDIFileReader file)
        {
            List<MIDIEvent> result = new List<MIDIEvent>(20);
            int starttime = 0;
            string id = file.ReadAscii(4);

            if (id != "MTrk")
            {
                throw new MIDIFileException("Bad MTrk header", file.GetOffset() - 4);
            }
            int tracklen = file.ReadInt();
            int trackend = tracklen + file.GetOffset();

            int eventflag = 0;

            while (file.GetOffset() < trackend)
            {

                // If the midi file is truncated here, we can still recover.
                // Just return what we've parsed so far.

                int startoffset, deltatime;
                byte peekevent;
                try
                {
                    startoffset = file.GetOffset();
                    deltatime = file.ReadVarlen();
                    starttime += deltatime;
                    peekevent = file.Peek();
                }
                catch (MIDIFileException)
                {
                    return result;
                }

                MIDIEvent mevent = new MIDIEvent();
                result.Add(mevent);
                mevent.DeltaTime = deltatime;
                mevent.StartTime = starttime;

                if (peekevent >= EventNoteOff)
                {
                    mevent.HasEventflag = true;
                    eventflag = file.ReadByte();
                }

                // Console.WriteLine("offset {0}: event {1} {2} start {3} delta {4}", 
                //                   startoffset, eventflag, EventName(eventflag), 
                //                   starttime, mevent.DeltaTime);

                if (eventflag >= EventNoteOn && eventflag < EventNoteOn + 16)
                {
                    mevent.EventFlag = EventNoteOn;
                    mevent.Channel = (byte)(eventflag - EventNoteOn);
                    mevent.Notenumber = file.ReadByte();
                    mevent.Velocity = file.ReadByte();
                }
                else if (eventflag >= EventNoteOff && eventflag < EventNoteOff + 16)
                {
                    mevent.EventFlag = EventNoteOff;
                    mevent.Channel = (byte)(eventflag - EventNoteOff);
                    mevent.Notenumber = file.ReadByte();
                    mevent.Velocity = file.ReadByte();
                }
                else if (eventflag >= EventKeyPressure &&
                         eventflag < EventKeyPressure + 16)
                {
                    mevent.EventFlag = EventKeyPressure;
                    mevent.Channel = (byte)(eventflag - EventKeyPressure);
                    mevent.Notenumber = file.ReadByte();
                    mevent.KeyPressure = file.ReadByte();
                }
                else if (eventflag >= EventControlChange &&
                         eventflag < EventControlChange + 16)
                {
                    mevent.EventFlag = EventControlChange;
                    mevent.Channel = (byte)(eventflag - EventControlChange);
                    mevent.ControlNum = file.ReadByte();
                    mevent.ControlValue = file.ReadByte();
                }
                else if (eventflag >= EventProgramChange &&
                         eventflag < EventProgramChange + 16)
                {
                    mevent.EventFlag = EventProgramChange;
                    mevent.Channel = (byte)(eventflag - EventProgramChange);
                    mevent.Instrument = file.ReadByte();
                }
                else if (eventflag >= EventChannelPressure &&
                         eventflag < EventChannelPressure + 16)
                {
                    mevent.EventFlag = EventChannelPressure;
                    mevent.Channel = (byte)(eventflag - EventChannelPressure);
                    mevent.ChanPressure = file.ReadByte();
                }
                else if (eventflag >= EventPitchBend &&
                         eventflag < EventPitchBend + 16)
                {
                    mevent.EventFlag = EventPitchBend;
                    mevent.Channel = (byte)(eventflag - EventPitchBend);
                    mevent.PitchBend = file.ReadShort();
                }
                else if (eventflag == SysexEvent1)
                {
                    mevent.EventFlag = SysexEvent1;
                    mevent.Metalength = file.ReadVarlen();
                    mevent.Value = file.ReadBytes(mevent.Metalength);
                }
                else if (eventflag == SysexEvent2)
                {
                    mevent.EventFlag = SysexEvent2;
                    mevent.Metalength = file.ReadVarlen();
                    mevent.Value = file.ReadBytes(mevent.Metalength);
                }
                else if (eventflag == MetaEvent)
                {
                    mevent.EventFlag = MetaEvent;
                    mevent.Metaevent = file.ReadByte();
                    mevent.Metalength = file.ReadVarlen();
                    mevent.Value = file.ReadBytes(mevent.Metalength);
                    if (mevent.Metaevent == MetaEventTimeSignature)
                    {
                        if (mevent.Metalength < 2)
                        {
                            // throw new MidiFileException(
                            //  "Meta Event Time Signature len == " + mevent.Metalength  + 
                            //  " != 4", file.GetOffset());
                            mevent.Numerator = (byte)0;
                            mevent.Denominator = (byte)4;
                        }
                        else if (mevent.Metalength >= 2 && mevent.Metalength < 4)
                        {
                            mevent.Numerator = (byte)mevent.Value[0];
                            mevent.Denominator = (byte)System.Math.Pow(2, mevent.Value[1]);
                        }
                        else
                        {
                            mevent.Numerator = (byte)mevent.Value[0];
                            mevent.Denominator = (byte)System.Math.Pow(2, mevent.Value[1]);
                        }
                    }
                    else if (mevent.Metaevent == MetaEventTempo)
                    {
                        if (mevent.Metalength != 3)
                        {
                            throw new MIDIFileException(
                              "Meta Event Tempo len == " + mevent.Metalength +
                              " != 3", file.GetOffset());
                        }
                        mevent.Tempo = ((mevent.Value[0] << 16) | (mevent.Value[1] << 8) | mevent.Value[2]);
                    }
                    else if (mevent.Metaevent == MetaEventEndOfTrack)
                    {
                        /* break;  */
                    }
                }
                else
                {
                    throw new MIDIFileException("Unknown event " + mevent.EventFlag,
                                                 file.GetOffset() - 1);
                }
            }

            return result;
        }

        private List<MIDIEvent> NewTrack(byte key)
        { // wang1ang
            int length = 120;
            List<MIDIEvent> result = new List<MIDIEvent>(20);
            MIDIEvent mevent = new MIDIEvent();
            result.Add(mevent);
            mevent.DeltaTime = 0;
            mevent.StartTime = 0;
            mevent.HasEventflag = true;
            mevent.EventFlag = MetaEvent;
            mevent.Metaevent = MetaEventTimeSignature;
            mevent.Numerator = 4;
            mevent.Denominator = 4;
            mevent.Value = new byte[] { 4, 2, 24, 8 };
            mevent.Metalength = mevent.Value.Length;

            mevent = new MIDIEvent();
            result.Add(mevent);
            mevent.DeltaTime = 0;
            mevent.StartTime = 0;
            mevent.HasEventflag = true;
            mevent.EventFlag = EventProgramChange;
            mevent.Channel = 0;
            mevent.Instrument = 0;

            mevent = new MIDIEvent();
            result.Add(mevent);
            mevent.DeltaTime = 0;
            mevent.StartTime = 0;
            mevent.HasEventflag = true;
            mevent.EventFlag = EventNoteOn;
            mevent.Channel = 0;
            mevent.Notenumber = key;
            mevent.Velocity = 60;

            mevent = new MIDIEvent();
            result.Add(mevent);
            mevent.DeltaTime = length;
            mevent.StartTime = length;
            mevent.HasEventflag = true;
            mevent.EventFlag = EventNoteOff;
            mevent.Channel = 0;
            mevent.Notenumber = key;
            mevent.Velocity = 0;

            mevent = new MIDIEvent();
            result.Add(mevent);
            mevent.DeltaTime = 0;
            mevent.StartTime = length;
            mevent.HasEventflag = true;
            mevent.EventFlag = MetaEvent;
            mevent.Metaevent = MetaEventEndOfTrack;
            mevent.Value = new byte[0];
            return result;
        }
        /** Return true if this track contains multiple channels.
         * If a MidiFile contains only one track, and it has multiple channels,
         * then we treat each channel as a separate track.
         */
        static bool HasMultipleChannels(MIDITrack track)
        {
            int channel = track.Notes[0].Channel;
            foreach (MIDINote note in track.Notes)
            {
                if (note.Channel != channel)
                {
                    return true;
                }
            }
            return false;
        }

        /** Write a variable length number to the buffer at the given offset.
         * Return the number of bytes written.
         */
        static int VarlenToBytes(int num, byte[] buf, int offset)
        {
            byte b1 = (byte)((num >> 21) & 0x7F);
            byte b2 = (byte)((num >> 14) & 0x7F);
            byte b3 = (byte)((num >> 7) & 0x7F);
            byte b4 = (byte)(num & 0x7F);

            if (b1 > 0)
            {
                buf[offset] = (byte)(b1 | 0x80);
                buf[offset + 1] = (byte)(b2 | 0x80);
                buf[offset + 2] = (byte)(b3 | 0x80);
                buf[offset + 3] = b4;
                return 4;
            }
            else if (b2 > 0)
            {
                buf[offset] = (byte)(b2 | 0x80);
                buf[offset + 1] = (byte)(b3 | 0x80);
                buf[offset + 2] = b4;
                return 3;
            }
            else if (b3 > 0)
            {
                buf[offset] = (byte)(b3 | 0x80);
                buf[offset + 1] = b4;
                return 2;
            }
            else
            {
                buf[offset] = b4;
                return 1;
            }
        }

        /** Write a 4-byte integer to data[offset : offset+4] */
        private static void IntToBytes(int value, byte[] data, int offset)
        {
            data[offset] = (byte)((value >> 24) & 0xFF);
            data[offset + 1] = (byte)((value >> 16) & 0xFF);
            data[offset + 2] = (byte)((value >> 8) & 0xFF);
            data[offset + 3] = (byte)(value & 0xFF);
        }

        /** Calculate the track length (in bytes) given a list of Midi events */
        private static int GetTrackLength(List<MIDIEvent> events)
        {
            int len = 0;
            byte[] buf = new byte[1024];
            foreach (MIDIEvent mevent in events)
            {
                len += VarlenToBytes(mevent.DeltaTime, buf, 0);
                len += 1;  /* for eventflag */
                switch (mevent.EventFlag)
                {
                    case EventNoteOn: len += 2; break;
                    case EventNoteOff: len += 2; break;
                    case EventKeyPressure: len += 2; break;
                    case EventControlChange: len += 2; break;
                    case EventProgramChange: len += 1; break;
                    case EventChannelPressure: len += 1; break;
                    case EventPitchBend: len += 2; break;

                    case SysexEvent1:
                    case SysexEvent2:
                        len += VarlenToBytes(mevent.Metalength, buf, 0);
                        len += mevent.Metalength;
                        break;
                    case MetaEvent:
                        len += 1;
                        len += VarlenToBytes(mevent.Metalength, buf, 0);
                        len += mevent.Metalength;
                        break;
                    default: break;
                }
            }
            return len;
        }


        /** Write the given list of Midi events to a stream/file.
         *  This method is used for sound playback, for creating new Midi files
         *  with the tempo, transpose, etc changed.
         *
         *  Return true on success, and false on error.
         */
        private static bool
        WriteEvents(Stream file, List<MIDIEvent>[] events, int trackmode, int quarter, bool close = true, bool reset = false)
        {
            try
            {
                byte[] buf = new byte[65536];

                /* Write the MThd, len = 6, track mode, number tracks, quarter note */
                file.Write(ASCIIEncoding.ASCII.GetBytes("MThd"), 0, 4);
                IntToBytes(6, buf, 0);
                file.Write(buf, 0, 4);
                buf[0] = (byte)(trackmode >> 8);
                buf[1] = (byte)(trackmode & 0xFF);
                file.Write(buf, 0, 2);
                buf[0] = 0;
                buf[1] = (byte)events.Length;
                file.Write(buf, 0, 2);
                buf[0] = (byte)(quarter >> 8);
                buf[1] = (byte)(quarter & 0xFF);
                file.Write(buf, 0, 2);

                foreach (List<MIDIEvent> list in events)
                {
                    /* Write the MTrk header and track length */
                    file.Write(ASCIIEncoding.ASCII.GetBytes("MTrk"), 0, 4);
                    int len = GetTrackLength(list);
                    IntToBytes(len, buf, 0);
                    file.Write(buf, 0, 4);

                    foreach (MIDIEvent mevent in list)
                    {
                        int varlen = VarlenToBytes(mevent.DeltaTime, buf, 0);
                        file.Write(buf, 0, varlen);

                        if (mevent.EventFlag == SysexEvent1 ||
                            mevent.EventFlag == SysexEvent2 ||
                            mevent.EventFlag == MetaEvent)
                        {
                            buf[0] = mevent.EventFlag;
                        }
                        else
                        {
                            buf[0] = (byte)(mevent.EventFlag + mevent.Channel);
                        }
                        file.Write(buf, 0, 1);

                        if (mevent.EventFlag == EventNoteOn)
                        {
                            buf[0] = mevent.Notenumber;
                            buf[1] = mevent.Velocity;
                            file.Write(buf, 0, 2);
                        }
                        else if (mevent.EventFlag == EventNoteOff)
                        {
                            buf[0] = mevent.Notenumber;
                            buf[1] = mevent.Velocity;
                            file.Write(buf, 0, 2);
                        }
                        else if (mevent.EventFlag == EventKeyPressure)
                        {
                            buf[0] = mevent.Notenumber;
                            buf[1] = mevent.KeyPressure;
                            file.Write(buf, 0, 2);
                        }
                        else if (mevent.EventFlag == EventControlChange)
                        {
                            buf[0] = mevent.ControlNum;
                            buf[1] = mevent.ControlValue;
                            file.Write(buf, 0, 2);
                        }
                        else if (mevent.EventFlag == EventProgramChange)
                        {
                            buf[0] = mevent.Instrument;
                            file.Write(buf, 0, 1);
                        }
                        else if (mevent.EventFlag == EventChannelPressure)
                        {
                            buf[0] = mevent.ChanPressure;
                            file.Write(buf, 0, 1);
                        }
                        else if (mevent.EventFlag == EventPitchBend)
                        {
                            buf[0] = (byte)(mevent.PitchBend >> 8);
                            buf[1] = (byte)(mevent.PitchBend & 0xFF);
                            file.Write(buf, 0, 2);
                        }
                        else if (mevent.EventFlag == SysexEvent1)
                        {
                            int offset = VarlenToBytes(mevent.Metalength, buf, 0);
                            Array.Copy(mevent.Value, 0, buf, offset, mevent.Value.Length);
                            file.Write(buf, 0, offset + mevent.Value.Length);
                        }
                        else if (mevent.EventFlag == SysexEvent2)
                        {
                            int offset = VarlenToBytes(mevent.Metalength, buf, 0);
                            Array.Copy(mevent.Value, 0, buf, offset, mevent.Value.Length);
                            file.Write(buf, 0, offset + mevent.Value.Length);
                        }
                        else if (mevent.EventFlag == MetaEvent && mevent.Metaevent == MetaEventTempo)
                        {
                            buf[0] = mevent.Metaevent;
                            buf[1] = 3;
                            buf[2] = (byte)((mevent.Tempo >> 16) & 0xFF);
                            buf[3] = (byte)((mevent.Tempo >> 8) & 0xFF);
                            buf[4] = (byte)(mevent.Tempo & 0xFF);
                            file.Write(buf, 0, 5);
                        }
                        else if (mevent.EventFlag == MetaEvent)
                        {
                            buf[0] = mevent.Metaevent;
                            int offset = VarlenToBytes(mevent.Metalength, buf, 1) + 1;
                            Array.Copy(mevent.Value, 0, buf, offset, mevent.Value.Length);
                            file.Write(buf, 0, offset + mevent.Value.Length);
                        }
                    }
                }
                if (close)
                    file.Close();

                if (reset)
                    file.Position = 0;

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }


        /** Clone the list of MidiEvents */
        private static List<MIDIEvent>[] CloneMidiEvents(List<MIDIEvent>[] origlist)
        {
            List<MIDIEvent>[] newlist = new List<MIDIEvent>[origlist.Length];
            for (int tracknum = 0; tracknum < origlist.Length; tracknum++)
            {
                List<MIDIEvent> origevents = origlist[tracknum];
                List<MIDIEvent> newevents = new List<MIDIEvent>(origevents.Count);
                newlist[tracknum] = newevents;
                foreach (MIDIEvent mevent in origevents)
                {
                    newevents.Add(mevent.Clone());
                }
            }
            return newlist;
        }

        /** Create a new Midi tempo event, with the given tempo  */
        private static MIDIEvent CreateTempoEvent(int tempo)
        {
            MIDIEvent mevent = new MIDIEvent();
            mevent.DeltaTime = 0;
            mevent.StartTime = 0;
            mevent.HasEventflag = true;
            mevent.EventFlag = MetaEvent;
            mevent.Metaevent = MetaEventTempo;
            mevent.Metalength = 3;
            mevent.Tempo = tempo;
            return mevent;
        }


        /** Search the events for a ControlChange event with the same
         *  channel and control number.  If a matching event is found,
         *   update the control value.  Else, add a new ControlChange event.
         */
        private static void
        UpdateControlChange(List<MIDIEvent> newevents, MIDIEvent changeEvent)
        {
            foreach (MIDIEvent mevent in newevents)
            {
                if ((mevent.EventFlag == changeEvent.EventFlag) &&
                    (mevent.Channel == changeEvent.Channel) &&
                    (mevent.ControlNum == changeEvent.ControlNum))
                {

                    mevent.ControlValue = changeEvent.ControlValue;
                    return;
                }
            }
            newevents.Add(changeEvent);
        }

        /** Start the Midi music at the given pause time (in pulses).
         *  Remove any NoteOn/NoteOff events that occur before the pause time.
         *  For other events, change the delta-time to 0 if they occur
         *  before the pause time.  Return the modified Midi Events.
         */
        private static List<MIDIEvent>[]
        StartAtPauseTime(List<MIDIEvent>[] list, int pauseTime)
        {
            List<MIDIEvent>[] newlist = new List<MIDIEvent>[list.Length];
            for (int tracknum = 0; tracknum < list.Length; tracknum++)
            {
                List<MIDIEvent> events = list[tracknum];
                List<MIDIEvent> newevents = new List<MIDIEvent>(events.Count);
                newlist[tracknum] = newevents;

                bool foundEventAfterPause = false;
                foreach (MIDIEvent mevent in events)
                {

                    if (mevent.StartTime < pauseTime)
                    {
                        if (mevent.EventFlag == EventNoteOn ||
                            mevent.EventFlag == EventNoteOff)
                        {

                            /* Skip NoteOn/NoteOff event */
                        }
                        else if (mevent.EventFlag == EventControlChange)
                        {
                            mevent.DeltaTime = 0;
                            UpdateControlChange(newevents, mevent);
                        }
                        else
                        {
                            mevent.DeltaTime = 0;
                            newevents.Add(mevent);
                        }
                    }
                    else if (!foundEventAfterPause)
                    {
                        mevent.DeltaTime = (mevent.StartTime - pauseTime);
                        newevents.Add(mevent);
                        foundEventAfterPause = true;
                    }
                    else
                    {
                        newevents.Add(mevent);
                    }
                }
            }
            return newlist;
        }


        /** Write this Midi file to the given filename.
         * If options is not null, apply those options to the midi events
         * before performing the write.
         * Return true if the file was saved successfully, else false.
         */
        public bool ChangeSound(string destfile, MIDIOptions options)
        {
            return Write(destfile, options);
        }

        public bool Write(string destfile, MIDIOptions options)
        {
            try
            {
                FileStream stream;
                stream = new FileStream(destfile, FileMode.Create);
                bool result = Write(stream, options);
                stream.Close();
                return result;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /** Write this Midi file to the given stream.
         * If options is not null, apply those options to the midi events
         * before performing the write.
         * Return true if the file was saved successfully, else false.
         */
        public bool Write(Stream stream, MIDIOptions options, bool close = true, bool reset = false)
        {
            List<MIDIEvent>[] newevents = events;
            if (options != null)
            {
                newevents = ApplyOptionsToEvents(options);
            }
            return WriteEvents(stream, newevents, trackmode, quarternote, close: close, reset: reset);
        }


        /* Apply the following sound options to the midi events:
         * - The tempo (the microseconds per pulse)
         * - The instruments per track
         * - The note number (transpose value)
         * - The tracks to include
         * Return the modified list of midi events.
         */
        private List<MIDIEvent>[]
        ApplyOptionsToEvents(MIDIOptions options)
        {
            int i;
            if (trackPerChannel)
            {
                return ApplyOptionsPerChannel(options);
            }

            /* A midifile can contain tracks with notes and tracks without notes.
             * The options.tracks and options.instruments are for tracks with notes.
             * So the track numbers in 'options' may not match correctly if the
             * midi file has tracks without notes. Re-compute the instruments, and 
             * tracks to keep.
             */
            int num_tracks = events.Length;
            int[] instruments = new int[num_tracks];
            bool[] keeptracks = new bool[num_tracks];
            for (i = 0; i < num_tracks; i++)
            {
                instruments[i] = 0;
                keeptracks[i] = true;
            }
            for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
            {
                MIDITrack track = tracks[tracknum];
                int realtrack = track.TrackNumber;
                instruments[realtrack] = options.instruments[tracknum];

                if (options.mute[tracknum] == true)
                {
                    keeptracks[realtrack] = false;
                }
            }

            List<MIDIEvent>[] newevents = CloneMidiEvents(events);

            /* Set the tempo at the beginning of each track */
            for (int tracknum = 0; tracknum < newevents.Length; tracknum++)
            {
                MIDIEvent mevent = CreateTempoEvent(options.tempo);
                newevents[tracknum].Insert(0, mevent);
            }

            /* Change the note number (transpose), instrument, and tempo */
            for (int tracknum = 0; tracknum < newevents.Length; tracknum++)
            {
                foreach (MIDIEvent mevent in newevents[tracknum])
                {
                    int num = mevent.Notenumber + options.transpose;
                    if (num < 0)
                        num = 0;
                    if (num > 127)
                        num = 127;
                    mevent.Notenumber = (byte)num;
                    if (!options.useDefaultInstruments)
                    {
                        mevent.Instrument = (byte)instruments[tracknum];
                    }
                    mevent.Tempo = options.tempo;
                }
            }

            if (options.pauseTime != 0)
            {
                newevents = StartAtPauseTime(newevents, options.pauseTime);
            }

            /* Change the tracks to include */
            int count = 0;
            for (int tracknum = 0; tracknum < keeptracks.Length; tracknum++)
            {
                if (keeptracks[tracknum])
                {
                    count++;
                }
            }
            List<MIDIEvent>[] result = new List<MIDIEvent>[count];
            i = 0;
            for (int tracknum = 0; tracknum < keeptracks.Length; tracknum++)
            {
                if (keeptracks[tracknum])
                {
                    result[i] = newevents[tracknum];
                    i++;
                }
            }
            return result;
        }

        /* Apply the following sound options to the midi events:
         * - The tempo (the microseconds per pulse)
         * - The instruments per track
         * - The note number (transpose value)
         * - The tracks to include
         * Return the modified list of midi events.
         *
         * This Midi file only has one actual track, but we've split that
         * into multiple fake tracks, one per channel, and displayed that
         * to the end-user.  So changing the instrument, and tracks to
         * include, is implemented differently than ApplyOptionsToEvents().
         *
         * - We change the instrument based on the channel, not the track.
         * - We include/exclude channels, not tracks.
         * - We exclude a channel by setting the note volume/velocity to 0.
         */
        private List<MIDIEvent>[]
        ApplyOptionsPerChannel(MIDIOptions options)
        {
            /* Determine which channels to include/exclude.
             * Also, determine the instruments for each channel.
             */
            int[] instruments = new int[16];
            bool[] keepchannel = new bool[16];
            for (int i = 0; i < 16; i++)
            {
                instruments[i] = 0;
                keepchannel[i] = true;
            }
            for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
            {
                MIDITrack track = tracks[tracknum];
                int channel = track.Notes[0].Channel;
                instruments[channel] = options.instruments[tracknum];

                if (options.mute[tracknum] == true)
                {

                    keepchannel[channel] = false;
                }
            }

            List<MIDIEvent>[] newevents = CloneMidiEvents(events);

            /* Set the tempo at the beginning of each track */
            for (int tracknum = 0; tracknum < newevents.Length; tracknum++)
            {
                MIDIEvent mevent = CreateTempoEvent(options.tempo);
                newevents[tracknum].Insert(0, mevent);
            }

            /* Change the note number (transpose), instrument, and tempo */
            for (int tracknum = 0; tracknum < newevents.Length; tracknum++)
            {
                foreach (MIDIEvent mevent in newevents[tracknum])
                {
                    int num = mevent.Notenumber + options.transpose;
                    if (num < 0)
                        num = 0;
                    if (num > 127)
                        num = 127;
                    mevent.Notenumber = (byte)num;
                    if (!keepchannel[mevent.Channel])
                    {
                        mevent.Velocity = 0;
                    }
                    if (!options.useDefaultInstruments)
                    {
                        mevent.Instrument = (byte)instruments[mevent.Channel];
                    }
                    mevent.Tempo = options.tempo;
                }
            }
            if (options.pauseTime != 0)
            {
                newevents = StartAtPauseTime(newevents, options.pauseTime);
            }
            return newevents;
        }


        /** Apply the given sheet music options to the MidiNotes.
         *  Return the midi tracks with the changes applied.
         */
        public List<MIDITrack> ChangeMidiNotes(MIDIOptions options)
        {
            List<MIDITrack> newtracks = new List<MIDITrack>();

            for (int track = 0; track < tracks.Count; track++)
            {
                if (options.tracks[track])
                {
                    newtracks.Add(tracks[track].Clone());
                }
            }

            /* To make the sheet music look nicer, we round the start times
             * so that notes close together appear as a single chord.  We
             * also extend the note durations, so that we have longer notes
             * and fewer rest symbols.
             */
            TimeSignature time = timesig;
            if (options.time != null)
            {
                time = options.time;
            }
            MIDIFile.RoundStartTimes(newtracks, options.combineInterval, timesig);
            MIDIFile.RoundDurations(newtracks, time.Quarter);

            if (options.twoStaffs)
            {
                newtracks = MIDIFile.CombineToTwoTracks(newtracks, timesig.Measure);
            }
            if (options.shifttime != 0)
            {
                MIDIFile.ShiftTime(newtracks, options.shifttime);
            }
            if (options.transpose != 0)
            {
                MIDIFile.Transpose(newtracks, options.transpose);
            }

            return newtracks;
        }


        /** Shift the starttime of the notes by the given amount.
         * This is used by the Shift Notes menu to shift notes left/right.
         */
        public static void
        ShiftTime(List<MIDITrack> tracks, int amount)
        {
            foreach (MIDITrack track in tracks)
            {
                foreach (MIDINote note in track.Notes)
                {
                    note.StartTime += amount;
                }
            }
        }

        /** Shift the note keys up/down by the given amount */
        public static void
        Transpose(List<MIDITrack> tracks, int amount)
        {
            foreach (MIDITrack track in tracks)
            {
                foreach (MIDINote note in track.Notes)
                {
                    note.Number += amount;
                    if (note.Number < 0)
                    {
                        note.Number = 0;
                    }
                }
            }
        }


        /* Find the highest and lowest notes that overlap this interval (starttime to endtime).
         * This method is used by SplitTrack to determine which staff (top or bottom) a note
         * should go to.
         *
         * For more accurate SplitTrack() results, we limit the interval/duration of this note 
         * (and other notes) to one measure. We care only about high/low notes that are
         * reasonably close to this note.
         */
        private static void
        FindHighLowNotes(List<MIDINote> notes, int measurelen, int startindex,
                         int starttime, int endtime, ref int high, ref int low)
        {

            int i = startindex;
            if (starttime + measurelen < endtime)
            {
                endtime = starttime + measurelen;
            }

            while (i < notes.Count && notes[i].StartTime < endtime)
            {
                if (notes[i].EndTime < starttime)
                {
                    i++;
                    continue;
                }
                if (notes[i].StartTime + measurelen < starttime)
                {
                    i++;
                    continue;
                }
                if (high < notes[i].Number)
                {
                    high = notes[i].Number;
                }
                if (low > notes[i].Number)
                {
                    low = notes[i].Number;
                }
                i++;
            }
        }

        /* Find the highest and lowest notes that start at this exact start time */
        private static void
        FindExactHighLowNotes(List<MIDINote> notes, int startindex, int starttime,
                              ref int high, ref int low)
        {

            int i = startindex;

            while (notes[i].StartTime < starttime)
            {
                i++;
            }

            while (i < notes.Count && notes[i].StartTime == starttime)
            {
                if (high < notes[i].Number)
                {
                    high = notes[i].Number;
                }
                if (low > notes[i].Number)
                {
                    low = notes[i].Number;
                }
                i++;
            }
        }



        /* Split the given MidiTrack into two tracks, top and bottom.
         * The highest notes will go into top, the lowest into bottom.
         * This function is used to split piano songs into left-hand (bottom)
         * and right-hand (top) tracks.
         */
        public static List<MIDITrack> SplitTrack(MIDITrack track, int measurelen)
        {
            List<MIDINote> notes = track.Notes;
            int count = notes.Count;

            MIDITrack top = new MIDITrack(1);
            MIDITrack bottom = new MIDITrack(2);
            List<MIDITrack> result = new List<MIDITrack>(2);
            result.Add(top); result.Add(bottom);

            if (count == 0)
                return result;

            int prevhigh = 76; /* E5, top of treble staff */
            int prevlow = 45; /* A3, bottom of bass staff */
            int startindex = 0;

            foreach (MIDINote note in notes)
            {
                int high, low, highExact, lowExact;

                int number = note.Number;
                high = low = highExact = lowExact = number;

                while (notes[startindex].EndTime < note.StartTime)
                {
                    startindex++;
                }

                /* I've tried several algorithms for splitting a track in two,
                 * and the one below seems to work the best:
                 * - If this note is more than an octave from the high/low notes
                 *   (that start exactly at this start time), choose the closest one.
                 * - If this note is more than an octave from the high/low notes
                 *   (in this note's time duration), choose the closest one.
                 * - If the high and low notes (that start exactly at this starttime)
                 *   are more than an octave apart, choose the closest note.
                 * - If the high and low notes (that overlap this starttime)
                 *   are more than an octave apart, choose the closest note.
                 * - Else, look at the previous high/low notes that were more than an 
                 *   octave apart.  Choose the closeset note.
                 */
                FindHighLowNotes(notes, measurelen, startindex, note.StartTime, note.EndTime,
                                 ref high, ref low);
                FindExactHighLowNotes(notes, startindex, note.StartTime,
                                      ref highExact, ref lowExact);

                if (highExact - number > 12 || number - lowExact > 12)
                {
                    if (highExact - number <= number - lowExact)
                    {
                        top.AddNote(note);
                    }
                    else
                    {
                        bottom.AddNote(note);
                    }
                }
                else if (high - number > 12 || number - low > 12)
                {
                    if (high - number <= number - low)
                    {
                        top.AddNote(note);
                    }
                    else
                    {
                        bottom.AddNote(note);
                    }
                }
                else if (highExact - lowExact > 12)
                {
                    if (highExact - number <= number - lowExact)
                    {
                        top.AddNote(note);
                    }
                    else
                    {
                        bottom.AddNote(note);
                    }
                }
                else if (high - low > 12)
                {
                    if (high - number <= number - low)
                    {
                        top.AddNote(note);
                    }
                    else
                    {
                        bottom.AddNote(note);
                    }
                }
                else
                {
                    if (prevhigh - number <= number - prevlow)
                    {
                        top.AddNote(note);
                    }
                    else
                    {
                        bottom.AddNote(note);
                    }
                }

                /* The prevhigh/prevlow are set to the last high/low
                 * that are more than an octave apart.
                 */
                if (high - low > 12)
                {
                    prevhigh = high;
                    prevlow = low;
                }
            }

            top.Notes.Sort(track.Notes[0]);
            bottom.Notes.Sort(track.Notes[0]);

            return result;
        }


        /** Combine the notes in the given tracks into a single MidiTrack. 
         *  The individual tracks are already sorted.  To merge them, we
         *  use a mergesort-like algorithm.
         */
        public static MIDITrack CombineToSingleTrack(List<MIDITrack> tracks)
        {
            /* Add all notes into one track */
            MIDITrack result = new MIDITrack(1);

            if (tracks.Count == 0)
            {
                return result;
            }
            else if (tracks.Count == 1)
            {
                MIDITrack track = tracks[0];
                foreach (MIDINote note in track.Notes)
                {
                    result.AddNote(note);
                }
                return result;
            }

            int[] noteindex = new int[64];
            int[] notecount = new int[64];

            for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
            {
                noteindex[tracknum] = 0;
                notecount[tracknum] = tracks[tracknum].Notes.Count;
            }
            MIDINote prevnote = null;
            while (true)
            {
                MIDINote lowestnote = null;
                int lowestTrack = -1;
                for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
                {
                    MIDITrack track = tracks[tracknum];
                    if (noteindex[tracknum] >= notecount[tracknum])
                    {
                        continue;
                    }
                    MIDINote note = track.Notes[noteindex[tracknum]];
                    if (lowestnote == null)
                    {
                        lowestnote = note;
                        lowestTrack = tracknum;
                    }
                    else if (note.StartTime < lowestnote.StartTime)
                    {
                        lowestnote = note;
                        lowestTrack = tracknum;
                    }
                    else if (note.StartTime == lowestnote.StartTime && note.Number < lowestnote.Number)
                    {
                        lowestnote = note;
                        lowestTrack = tracknum;
                    }
                }
                if (lowestnote == null)
                {
                    /* We've finished the merge */
                    break;
                }
                noteindex[lowestTrack]++;
                if ((prevnote != null) && (prevnote.StartTime == lowestnote.StartTime) &&
                    (prevnote.Number == lowestnote.Number))
                {

                    /* Don't add duplicate notes, with the same start time and number */
                    if (lowestnote.Duration > prevnote.Duration)
                    {
                        prevnote.Duration = lowestnote.Duration;
                    }
                }
                else
                {
                    result.AddNote(lowestnote);
                    prevnote = lowestnote;
                }
            }

            return result;
        }


        /** Combine the notes in all the tracks given into two MidiTracks,
         * and return them.
         * 
         * This function is intended for piano songs, when we want to display
         * a left-hand track and a right-hand track.  The lower notes go into 
         * the left-hand track, and the higher notes go into the right hand 
         * track.
         */
        public static List<MIDITrack> CombineToTwoTracks(List<MIDITrack> tracks, int measurelen)
        {
            MIDITrack single = CombineToSingleTrack(tracks);
            List<MIDITrack> result = SplitTrack(single, measurelen);

            List<MIDIEvent> lyrics = new List<MIDIEvent>();
            foreach (MIDITrack track in tracks)
            {
                if (track.Lyrics != null)
                {
                    lyrics.AddRange(track.Lyrics);
                }
            }
            if (lyrics.Count > 0)
            {
                lyrics.Sort(lyrics[0]);
                result[0].Lyrics = lyrics;
            }

            return result;
        }


        /** Check that the MidiNote start times are in increasing order.
         * This is for debugging purposes.
         */
        private static void CheckStartTimes(List<MIDITrack> tracks)
        {
            foreach (MIDITrack track in tracks)
            {
                int prevtime = -1;
                foreach (MIDINote note in track.Notes)
                {
                    if (note.StartTime < prevtime)
                    {
                        throw new System.ArgumentException("start times not in increasing order");
                    }
                    prevtime = note.StartTime;
                }
            }
        }


        /** In Midi Files, time is measured in pulses.  Notes that have
         * pulse times that are close together (like within 10 pulses)
         * will sound like they're the same chord.  We want to draw
         * these notes as a single chord, it makes the sheet music much
         * easier to read.  We don't want to draw notes that are close
         * together as two separate chords.
         *
         * The SymbolSpacing class only aligns notes that have exactly the same
         * start times.  Notes with slightly different start times will
         * appear in separate vertical columns.  This isn't what we want.
         * We want to align notes with approximately the same start times.
         * So, this function is used to assign the same starttime for notes
         * that are close together (timewise).
         */
        public static void
        RoundStartTimes(List<MIDITrack> tracks, int millisec, TimeSignature time)
        {
            /* Get all the starttimes in all tracks, in sorted order */
            List<int> starttimes = new List<int>();
            foreach (MIDITrack track in tracks)
            {
                foreach (MIDINote note in track.Notes)
                {
                    starttimes.Add(note.StartTime);
                }
            }
            starttimes.Sort();

            /* Notes within "millisec" milliseconds apart will be combined. */
            int interval = time.Quarter * millisec * 1000 / time.Tempo;

            /* If two starttimes are within interval millisec, make them the same */
            for (int i = 0; i < starttimes.Count - 1; i++)
            {
                if (starttimes[i + 1] - starttimes[i] <= interval)
                {
                    starttimes[i + 1] = starttimes[i];
                }
            }

            CheckStartTimes(tracks);

            /* Adjust the note starttimes, so that it matches one of the starttimes values */
            foreach (MIDITrack track in tracks)
            {
                int i = 0;

                foreach (MIDINote note in track.Notes)
                {
                    while (i < starttimes.Count &&
                           note.StartTime - interval > starttimes[i])
                    {
                        i++;
                    }

                    if (note.StartTime > starttimes[i] &&
                        note.StartTime - starttimes[i] <= interval)
                    {

                        note.StartTime = starttimes[i];
                    }
                }
                track.Notes.Sort(track.Notes[0]);
            }
        }


        /** We want note durations to span up to the next note in general.
         * The sheet music looks nicer that way.  In contrast, sheet music
         * with lots of 16th/32nd notes separated by small rests doesn't
         * look as nice.  Having nice looking sheet music is more important
         * than faithfully representing the Midi File data.
         *
         * Therefore, this function rounds the duration of MidiNotes up to
         * the next note where possible.
         */
        public static void
        RoundDurations(List<MIDITrack> tracks, int quarternote)
        {

            foreach (MIDITrack track in tracks)
            {
                MIDINote prevNote = null;
                for (int i = 0; i < track.Notes.Count - 1; i++)
                {
                    MIDINote note1 = track.Notes[i];
                    if (prevNote == null)
                    {
                        prevNote = note1;
                    }

                    /* Get the next note that has a different start time */
                    MIDINote note2 = note1;
                    for (int j = i + 1; j < track.Notes.Count; j++)
                    {
                        note2 = track.Notes[j];
                        if (note1.StartTime < note2.StartTime)
                        {
                            break;
                        }
                    }
                    int maxduration = note2.StartTime - note1.StartTime;

                    int dur = 0;
                    if (quarternote <= maxduration)
                        dur = quarternote;
                    else if (quarternote / 2 <= maxduration)
                        dur = quarternote / 2;
                    else if (quarternote / 3 <= maxduration)
                        dur = quarternote / 3;
                    else if (quarternote / 4 <= maxduration)
                        dur = quarternote / 4;


                    if (dur < note1.Duration)
                    {
                        dur = note1.Duration;
                    }

                    /* Special case: If the previous note's duration
                     * matches this note's duration, we can make a notepair.
                     * So don't expand the duration in that case.
                     */
                    if ((prevNote.StartTime + prevNote.Duration == note1.StartTime) &&
                        (prevNote.Duration == note1.Duration))
                    {

                        dur = note1.Duration;
                    }
                    note1.Duration = dur;
                    if (track.Notes[i + 1].StartTime != note1.StartTime)
                    {
                        prevNote = note1;
                    }
                }
            }
        }

        /** Split the given track into multiple tracks, separating each
         * channel into a separate track.
         */
        private static List<MIDITrack>
        SplitChannels(MIDITrack origtrack, List<MIDIEvent> events)
        {

            /* Find the instrument used for each channel */
            int[] channelInstruments = new int[16];
            foreach (MIDIEvent mevent in events)
            {
                if (mevent.EventFlag == EventProgramChange)
                {
                    channelInstruments[mevent.Channel] = mevent.Instrument;
                }
            }
            channelInstruments[9] = 128; /* Channel 9 = Percussion */

            List<MIDITrack> result = new List<MIDITrack>();
            foreach (MIDINote note in origtrack.Notes)
            {
                bool foundchannel = false;
                foreach (MIDITrack track in result)
                {
                    if (note.Channel == track.Notes[0].Channel)
                    {
                        foundchannel = true;
                        track.AddNote(note);
                    }
                }
                if (!foundchannel)
                {
                    MIDITrack track = new MIDITrack(result.Count + 1);
                    track.AddNote(note);
                    track.Instrument = channelInstruments[note.Channel];
                    result.Add(track);
                }
            }
            if (origtrack.Lyrics != null)
            {
                foreach (MIDIEvent lyricEvent in origtrack.Lyrics)
                {
                    foreach (MIDITrack track in result)
                    {
                        if (lyricEvent.Channel == track.Notes[0].Channel)
                        {
                            track.AddLyric(lyricEvent);
                        }
                    }
                }
            }
            return result;
        }


        /** Guess the measure length.  We assume that the measure
         * length must be between 0.5 seconds and 4 seconds.
         * Take all the note start times that fall between 0.5 and 
         * 4 seconds, and return the starttimes.
         */
        public List<int>
        GuessMeasureLength()
        {
            List<int> result = new List<int>();

            int pulses_per_second = (int)(1000000.0 / timesig.Tempo * timesig.Quarter);
            int minmeasure = pulses_per_second / 2;  /* The minimum measure length in pulses */
            int maxmeasure = pulses_per_second * 4;  /* The maximum measure length in pulses */

            /* Get the start time of the first note in the midi file. */
            int firstnote = timesig.Measure * 5;
            foreach (MIDITrack track in tracks)
            {
                if (firstnote > track.Notes[0].StartTime)
                {
                    firstnote = track.Notes[0].StartTime;
                }
            }

            /* interval = 0.06 seconds, converted into pulses */
            int interval = timesig.Quarter * 60000 / timesig.Tempo;

            foreach (MIDITrack track in tracks)
            {
                int prevtime = 0;
                foreach (MIDINote note in track.Notes)
                {
                    if (note.StartTime - prevtime <= interval)
                        continue;

                    prevtime = note.StartTime;

                    int time_from_firstnote = note.StartTime - firstnote;

                    /* Round the time down to a multiple of 4 */
                    time_from_firstnote = time_from_firstnote / 4 * 4;
                    if (time_from_firstnote < minmeasure)
                        continue;
                    if (time_from_firstnote > maxmeasure)
                        break;

                    if (!result.Contains(time_from_firstnote))
                    {
                        result.Add(time_from_firstnote);
                    }
                }
            }
            result.Sort();
            return result;
        }

        /** Return the last start time */
        public int EndTime()
        {
            int lastStart = 0;
            foreach (MIDITrack track in tracks)
            {
                if (track.Notes.Count == 0)
                {
                    continue;
                }
                int last = track.Notes[track.Notes.Count - 1].StartTime;
                lastStart = Math.Max(last, lastStart);
            }
            return lastStart;
        }

        /** Return true if this midi file has lyrics */
        public bool HasLyrics()
        {
            foreach (MIDITrack track in tracks)
            {
                if (track.Lyrics != null)
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            string result = "Midi File tracks=" + tracks.Count + " quarter=" + quarternote + "\n";
            result += Time.ToString() + "\n";
            foreach (MIDITrack track in tracks)
            {
                result += track.ToString();
            }
            return result;
        }

        /* Command-line program to print out a parsed Midi file. Used for debugging.
         * To run:
         * - Change Main2 to Main
         * - csc MidiNote.cs MidiEvent.cs MidiTrack.cs MidiFileReader.cs MidiOptions.cs
         *   MidiFile.cs MidiFileException.cs TimeSignature.cs ConfigINI.cs
         * - MidiFile.exe file.mid
         *
         */
        public static void Main(string[] arg)
        {
            if (arg.Length == 0)
            {
                Console.WriteLine("Usage: MidiFile <filename>");
                return;
            }

            MIDIFile f = new MIDIFile(arg[0]);
            Console.Write(f.ToString());
        }

    }  /* End class MidiFile */


}  /* End namespace MidiSheetMusic */

