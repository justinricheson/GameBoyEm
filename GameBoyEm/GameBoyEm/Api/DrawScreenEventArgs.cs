using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace GameBoyEm.Api
{
    public class DrawScreenEventArgs : EventArgs
    {
        public IList<Color> FrameBuffer { get; private set; }

        public DrawScreenEventArgs(IList<Color> frameBuffer)
        {
            FrameBuffer = frameBuffer;
        }
    }
}
