using System;

namespace GameBoyEm.Api
{
    public class DrawScreenEventArgs : EventArgs
    {
        public byte[] FrameBuffer { get; private set; }

        public DrawScreenEventArgs(byte[] frameBuffer)
        {
            FrameBuffer = frameBuffer;
        }
    }
}
