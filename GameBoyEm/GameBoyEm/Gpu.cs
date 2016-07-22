using GameBoyEm.Interfaces;
using System;

namespace GameBoyEm
{
    public class Gpu : IGpu
    {
        private IMmu _mmu;

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
