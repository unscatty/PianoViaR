using System;
using AudioSynthesis.Bank;
using PianoViaR.Helpers;
using UnityEngine;

namespace PianoViaR.MIDI.Playback
{
    [RequireComponent(typeof(AudioSource))]
    public class MIDINotePlayer : MonoBehaviour
    {
        MIDIPlayer player;
        AudioSource audioSource;
        public AudioSource AudioSource { get { return audioSource; } }
        public event EventHandler<MIDIPlayerEventArgs> BankLoaded;
        public event EventHandler<PianoNoteEventArgs> NoteOn;
        public event EventHandler<PianoNoteEventArgs> NoteOff;
        // Start is called before the first frame update

        public int Volume
        {
            get { return (int)(player.Volume / 1.27); }
            set { player.Volume = (int)(value * 1.27); }
        }

        void Awake()
        {
            player = new MIDIPlayer();
            audioSource = GetComponent<AudioSource>();
            // Play();
        }

        void Start()
        {
            Play();
        }

        void OnAudioFilterRead(float[] data, int channel)
        {
            player.OnAudioFilterRead(data, channel);
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
            var args = new MIDIPlayerEventArgs() { CurrentBank = bank, Volume = Volume };
            BankLoaded?.Invoke(this, args);
        }

        public void PlayNote(int note, int instrument)
        {
            player.NoteOn(note, instrument);

            OnNoteOn(note, instrument);
        }

        public void PlayNote(object source, PianoNoteEventArgs args)
        {
            PlayNote(args.Note, args.Instrument);
        }

        protected virtual void OnNoteOn(int note, int instrument)
        {
            var args = new PianoNoteEventArgs(note, instrument);
            NoteOn?.Invoke(this, args);
        }

        public void StopNote(int note)
        {
            player.NoteOff(note);

            OnNoteOff(note);
        }

        public void StopNote(object source, PianoNoteEventArgs args)
        {
            StopNote(args.Note);
        }
        protected virtual void OnNoteOff(int note)
        {
            var args = new PianoNoteEventArgs(note, -1);
            NoteOff?.Invoke(this, args);
        }

        public void Play()
        {
            player.Play();
            audioSource.Play();
        }

        public void Stop()
        {
            player.Stop();
            audioSource.Stop();
        }
    }
}