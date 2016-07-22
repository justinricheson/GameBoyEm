using GameBoyEm.Api;
using System;

namespace GameBoyEm
{
    public class Gpu : IGpu
    {
        private IMmu _mmu;

        public byte[] FrameBuffer
        {
            get { throw new NotImplementedException(); }
        }

        public Gpu(IMmu mmu)
        {
            _mmu = mmu;
            Reset();
        }

        public void Reset()
        {
        }

        public bool Step(ushort cycles)
        {
            return false;
        }
    }
}
