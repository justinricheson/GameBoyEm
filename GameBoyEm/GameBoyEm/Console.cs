using System;
using GameBoyEm.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace GameBoyEm
{
    public class Console
    {
        private ICpu _cpu;
        private IMmu _mmu;
        private bool _cartridgeLoaded;

        public Console(ICpu cpu, IMmu mmu)
        {
            _cpu = cpu;
            _mmu = mmu;
        }

        public void LoadCartridge(ICartridge cartridge)
        {
            _mmu.LoadCartridge(cartridge);
            _cartridgeLoaded = true;
        }

        private CancellationTokenSource _tokenSource;
        public async Task PowerOn()
        {
            if (_tokenSource == null && _cartridgeLoaded)
            {
                Reset();
                _tokenSource = new CancellationTokenSource();
                await Task.Run(() => Emulate(_tokenSource.Token), _tokenSource.Token);
            }
        }

        public void PowerOff()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource = null;
            }
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

        private void Emulate(CancellationToken cancel)
        {
            while (true)
            {
                if (cancel.IsCancellationRequested)
                {
                    break;
                }

                _cpu.Step();
            }
        }
    }
}
