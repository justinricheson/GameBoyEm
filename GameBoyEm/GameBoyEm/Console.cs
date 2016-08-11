using GameBoyEm.Api;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GameBoyEm
{
    public class Console
    {
        private ICpu _cpu;
        private IMmu _mmu;
        private IGpu _gpu;
        private IController _controller;
        private bool _cartridgeLoaded;
        private long _cumulativeCycles;
        private Stopwatch _sw = new Stopwatch();
        private CancellationTokenSource _tokenSource;

        public bool TurnedOn { get; private set; }
        public ICpu Cpu { get { return _cpu; } }
        public IController Controller { get { return _controller; } }

        public EventHandler<DrawScreenEventArgs> OnDrawScreen;

        public Console(ICpu cpu, IMmu mmu, IGpu gpu, IController controller)
        {
            _cpu = cpu;
            _mmu = mmu;
            _gpu = gpu;
            _controller = controller;
        }

        public void LoadCartridge(ICartridge cartridge)
        {
            _mmu.LoadCartridge(cartridge);
            _cartridgeLoaded = true;
        }

        public async Task PowerOn()
        {
            if (!TurnedOn && _cartridgeLoaded)
            {
                TurnedOn = true;
                Reset();
                await Resume();
            }
        }

        public void PowerOff()
        {
            Pause();
        }

        public void Pause()
        {
            if (TurnedOn && _cartridgeLoaded)
            {
                TurnedOn = false;
                _tokenSource.Cancel();
                _tokenSource = null;
            }
        }

        public async Task Resume()
        {
            if (_tokenSource == null && _cartridgeLoaded)
            {
                _tokenSource = new CancellationTokenSource();
                await Task.Run(() => Emulate(_tokenSource.Token), _tokenSource.Token);
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
            var controller = new Controller(mmu);
            return new Console(cpu, mmu, gpu, controller);
        }

        public bool Step()
        {
            var cycles = _cpu.Step();
            _cumulativeCycles += cycles;

            var draw = _gpu.Step(cycles);
            if (draw)
            {
                OnDrawScreen?.Invoke(this,
                    new DrawScreenEventArgs(_gpu.FrameBuffer));
            }
            _controller.Step();
            return draw;
        }

        private void Emulate(CancellationToken cancel)
        {
            _sw.Restart();
            while (true)
            {
                if (Step())
                {
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }
                }

                if (_cumulativeCycles >= 4194304)
                {
                    // Slow down emulation to sync with ~4.194394MHz
                    _cumulativeCycles -= 4194304;
                    while (_sw.ElapsedTicks < TimeSpan.TicksPerSecond) ;
                    _sw.Restart();
                }
            }
        }
    }
}
