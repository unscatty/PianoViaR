﻿namespace AudioSynthesis.Wave
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using AudioSynthesis.Util;
    using AudioSynthesis.Util.Riff;

    public sealed class WaveFileReader : IDisposable
    {
        //--Fields
        private BinaryReader reader;
        //--Properties

        //--Methods
        public WaveFileReader(string fileName)
        {
            if (!Path.GetExtension(fileName).ToLower().Equals(".wav") || !CrossPlatformHelper.ResourceExists(fileName))
                throw new InvalidDataException("Invalid wave file : " + fileName);
            reader = new BinaryReader(CrossPlatformHelper.OpenResource(fileName));
        }
        public WaveFileReader(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public WaveFile ReadWaveFile()
        {
            return new WaveFile(WaveFileReader.ReadAllChunks(reader));
        }
        public Chunk[] ReadAllChunks()
        {
            return WaveFileReader.ReadAllChunks(reader);
        }
        public Chunk ReadNextChunk()
        {
            return WaveFileReader.ReadNextChunk(reader);
        }
        public void Close()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (reader == null)
                return;
            reader.Dispose();
            reader = null;
        }

        internal static Chunk[] ReadAllChunks(BinaryReader reader)
        {
            long offset = reader.BaseStream.Position + 8;
            List<Chunk> chunks = new List<Chunk>();
            RiffTypeChunk head = new RiffTypeChunk(new string(IOHelper.Read8BitChars(reader, 4)), reader.ReadInt32(), reader);
            if (!head.ChunkId.ToLower().Equals("riff") || !head.TypeId.ToLower().Equals("wave"))
                throw new InvalidDataException("The asset could not be loaded because the RIFF chunk was missing or was not of type WAVE.");
            while (reader.BaseStream.Position - offset < head.ChunkSize)
            {
                Chunk chunk = ReadNextChunk(reader);
                if (chunk != null)
                    chunks.Add(chunk);
            }
            return chunks.ToArray();
        }
        internal static Chunk ReadNextChunk(BinaryReader reader)
        {
            string id = new string(IOHelper.Read8BitChars(reader, 4));
            int size = reader.ReadInt32();
            switch (id.ToLower())
            {
                case "riff":
                    return new RiffTypeChunk(id, size, reader);
                case "fact":
                    return new FactChunk(id, size, reader);
                case "data":
                    return new DataChunk(id, size, reader);
                case "fmt ":
                    return new FormatChunk(id, size, reader);
                case "cue ":
                    return new CueChunk(id, size, reader);
                case "plst":
                    return new PlaylistChunk(id, size, reader);
                case "list":
                    return new ListChunk(id, size, reader, new Func<BinaryReader,Chunk>(ReadNextChunk));
                case "labl":
                    return new LabelChunk(id, size, reader);
                case "note":
                    return new NoteChunk(id, size, reader);
                case "ltxt":
                    return new LabeledTextChunk(id, size, reader);
                case "smpl":
                    return new SamplerChunk(id, size, reader);
                case "inst":
                    return new InstrumentChunk(id, size, reader);
                default:
                    return new UnknownChunk(id, size, reader);
            }
        }
        internal static WaveFile ReadWaveFile(BinaryReader reader)
        {
            return new WaveFile(ReadAllChunks(reader));
        }
    }
}
