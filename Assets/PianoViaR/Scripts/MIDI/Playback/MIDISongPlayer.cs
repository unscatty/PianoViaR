using UnityEngine;
using AudioSynthesis.Bank;
using AudioSynthesis.Midi;
using System;
using PianoViaR.Helpers;
using System.IO;

namespace PianoViaR.MIDI.Playback
{
    [RequireComponent(typeof(AudioSource))]
    public class MIDISongPlayer : MonoBehaviour
    {
        MIDIPlayer player;
        MidiFile midiFile;
        public MidiFile MidiFile { get { return midiFile; } }
        AudioSource audioSource;
        PlayBackStatus status;
        public PlayBackStatus Status { get { return status; } }
        public float PlaySpeed
        {
            get { return player.PlaySpeed; }
            set { player.PlaySpeed = value; }
        }
        public int Volume
        {
            get { return (int)(player.Volume / 1.27); }
            set { player.Volume = (int)(value * 1.27); }
        }

        private MIDIPlayerEventArgs PlayerState()
        {
            return new MIDIPlayerEventArgs()
            {
                Volume = Volume,
                Status = Status,
                PlaySpeed = PlaySpeed
            };
        }
        // Callbacks
        public event EventHandler<MIDIPlayerEventArgs> Stopped;
        public event EventHandler<MIDIPlayerEventArgs> Paused;
        public event EventHandler<MIDIPlayerEventArgs> Playing;
        public event EventHandler<MIDIPlayerEventArgs> MidiLoaded;
        public event EventHandler<MIDIPlayerEventArgs> BankLoaded;

        void Awake()
        {
            player = new MIDIPlayer();
            audioSource = GetComponent<AudioSource>();

            status = PlayBackStatus.STOPPED;
        }

        public void LoadBank(PatchBank bank)
        {
            player.LoadBank(bank);

            OnBankLoaded(bank);
        }

        public void LoadBank(string bankPath)
        {
            LoadBank(new PatchBank(bankPath));
        }

        protected virtual void OnBankLoaded(PatchBank bank)
        {
            var args = PlayerState();
            args.CurrentBank = bank;

            BankLoaded?.Invoke(this, args);
        }

        public void LoadMidi(MidiFile midi)
        {
            Stop();

            player.LoadMidi(midi);

            OnMidiLoaded(midi);
        }

        public void LoadMidi(string midiPath)
        {
            LoadMidi(new MidiFile(midiPath));
        }

        public void LoadFromStream(Stream stream)
        {
            LoadMidi(new MidiFile(stream));
        }

        protected virtual void OnMidiLoaded(MidiFile midi)
        {
            var args = PlayerState();
            args.CurrentFile = midi
            ;
            MidiLoaded?.Invoke(this, args);
        }

        public void Play()
        {
            if (status != PlayBackStatus.PLAYING)
            {
                player.Play();
                audioSource.Play();

                status = PlayBackStatus.PLAYING;

                OnPlaying();
            }
        }

        protected virtual void OnPlaying()
        {
            Playing?.Invoke(this, PlayerState());
        }

        public void Pause()
        {
            if (Status == PlayBackStatus.PLAYING)
            {
                audioSource.Pause();
                player.Pause();

                status = PlayBackStatus.PAUSED;

                OnPaused();
            }
        }

        protected virtual void OnPaused()
        {
            Paused?.Invoke(this, PlayerState());
        }

        public void Stop()
        {
            if (Status != PlayBackStatus.STOPPED)
            {
                audioSource.Stop();
                player.Stop();

                status = PlayBackStatus.STOPPED;

                OnStopped();
            }
        }

        protected virtual void OnStopped()
        {
            Stopped?.Invoke(this, PlayerState());
        }

        void OnAudioFilterRead(float[] data, int channel)
        {
            player.OnAudioFilterRead(data, channel);
        }
    }
}