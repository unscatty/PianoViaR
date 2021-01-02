using System;
using System.Collections.Generic;
using PianoViaR.Helpers;

namespace PianoViaR.Piano.Behaviours
{
    public abstract class KeySource
    {
        protected List<dynamic> fadeList;
        public PianoNoteEventArgs EventArgs;
        public event EventHandler<PianoNoteEventArgs> NotePlayed;
        public event EventHandler<PianoNoteEventArgs> NoteStopped;

        public virtual int Count { get { return fadeList.Count; } }
        // public abstract void Setup();
        public abstract void Play();
        public abstract void Stop(dynamic source);
        protected virtual void Initialize()
        {
            fadeList = new List<dynamic>();
        }
        protected virtual void OnNotePlayed()
        {
            NotePlayed?.Invoke(this, EventArgs);
        }
        protected virtual void OnNoteStopped()
        {
            NoteStopped?.Invoke(this, EventArgs);
        }
        public virtual void AddFade(dynamic source)
        {
            fadeList.Add(source);
        }
        public virtual void RemoveFade(dynamic source)
        {
            fadeList.Remove(source);
        }
        public abstract void FadeList();
        public virtual void FadeAll()
        {
            if (fadeList.Count > 0)
            {
                fadeList.RemoveRange(0, fadeList.Count);
            }
        }
    }
}