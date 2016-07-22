using GameBoyEm.Api;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameBoyEm
{
    public class Console
    {
        private ICpu _cpu;
        private IMmu _mmu;
        private IGpu _gpu;
        private bool _cartridgeLoaded;

        public EventHandler<DrawScreenEventArgs> OnDrawScreen;

        public Console(ICpu cpu, IMmu mmu, IGpu gpu)
        {
            _cpu = cpu;
            _mmu = mmu;
            _gpu = gpu;
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
            _gpu.Reset();
        }

        public static Console Default()
        {
            var mmu = new Mmu();
            var cpu = new Cpu(mmu);
            var gpu = new Gpu(mmu);
            return new Console(cpu, mmu, gpu);
        }

        private void Emulate(CancellationToken cancel)
        {
            while (true)
            {
                var cycles = _cpu.Step();
                if (_gpu.Step(cycles))
                {
                    if (cancel.IsCancellationRequested)
                    {
                        break;
                    }
                    OnDrawScreen?.Invoke(this,
                        new DrawScreenEventArgs(_gpu.FrameBuffer));
                }
            }
        }
    }
}
