using System;
using GameBoyEm.Interfaces;

namespace GameBoyEm
{
    public class Console
    {
        private ICpu _cpu;
        private IMmu _mmu;

        public Console(ICpu cpu, IMmu mmu)
        {
            _cpu = cpu;
            _mmu = mmu;
        }

        public void LoadCartridge(ICartridge cartridge)
        {
            _mmu.LoadCartridge(cartridge);
        }

        public void PowerOn()
        {
            throw new NotImplementedException();
        }

        public void PowerOff()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            _cpu.Reset();
            _mmu.Reset();
        }

        public static Console Default()
        {
            var mmu = new Mmu();
            var cpu = new Cpu(mmu);
            return new Console(cpu, mmu);
        }
    }
}
