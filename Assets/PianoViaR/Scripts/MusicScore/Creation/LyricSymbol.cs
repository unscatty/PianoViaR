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

using PianoViaR.Score.Helpers;

namespace PianoViaR.Score.Creation
{

    /** @class LyricSymbol
     *  A lyric contains the lyric to display, the start time the lyric occurs at,
     *  the the x-coordinate where it will be displayed.
     */
    public class LyricSymbol
    {
        private int starttime;   /** The start time, in pulses */
        private string text;     /** The lyric text */
        private float x;           /** The x (horizontal) position within the staff */
        private ScoreDimensions dimensions;

        public LyricSymbol(int starttime, string text, in ScoreDimensions dimensions)
        {
            this.dimensions = dimensions;
            this.starttime = starttime;
            this.text = text;
        }

        public int StartTime
        {
            get { return starttime; }
            set { starttime = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public float X
        {
            get { return x; }
            set { x = value; }
        }

        public float MinWidth
        {
            get { return minWidth(); }
        }

        /* Return the minimum width in pixels needed to display this lyric.
         * This is an estimation, not exact.
         */
        private float minWidth()
        {
            float widthPerChar = dimensions.WidthPerChar;
            float width = text.Length * widthPerChar;
            if (text.IndexOf("i") >= 0)
            {
                width -= widthPerChar / 2;
            }
            if (text.IndexOf("j") >= 0)
            {
                width -= widthPerChar / 2;
            }
            if (text.IndexOf("l") >= 0)
            {
                width -= widthPerChar / 2;
            }
            return width;
        }

        public override
        string ToString()
        {
            return string.Format("Lyric start={0} x={1} text={2}",
                                 starttime, x, text);
        }

    }


}
