using GameBoyEm.Api;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GameBoyEm
{
    public class Console
    {
        private object _sync = new object();
        private ICpu _cpu;
        private IMmu _mmu;
        private IGpu _gpu;
        private IController _controller;
        private long _cumulativeCycles;
        private Stopwatch _sw = new Stopwatch();
        private bool _emulating;

        public bool Paused { get; private set; }
        public bool TurnedOn { get; private set; }
        public bool CartridgeLoaded { get; private set; }
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

        public static Console Default()
        {
            var mmu = new Mmu();
            var cpu = new Cpu(mmu);
            var gpu = new Gpu(mmu);
            var controller = new Controller(mmu);
            return new Console(cpu, mmu, gpu, controller);
        }

        public void LoadCartridge(ICartridge cartridge)
        {
            lock (_sync)
            {
                if (!TurnedOn)
                {
                    _mmu.LoadCartridge(cartridge);
                    CartridgeLoaded = true;
                }
            }
        }

        public void PowerOn()
        {
            lock (_sync)
            {
                if (!TurnedOn && CartridgeLoaded)
                {
                    TurnedOn = true;
                    Task.Run(() => Emulate());
                }
            }
        }

        public void PowerOff()
        {
            lock (_sync)
            {
                if (TurnedOn)
                {
                    TurnedOn = false;
                    Reset();

                    while (_emulating) ; // Wait for emulation to quit
                }
            }
        }

        public void Pause()
        {
            lock (_sync)
            {
                if (TurnedOn && !Paused)
                {
                    Paused = true;
                    while (_emulating) ; // Wait for emulation to quit
                }
            }
        }

        public void Resume()
        {
            lock (_sync)
            {
                if (TurnedOn && Paused)
                {
                    Paused = false;
                    Task.Run(() => Emulate());
                }
            }
        }

        public void Reset()
        {
            lock (_sync)
            {
                var wasTurnedOn = TurnedOn;
                TurnedOn = false;
                while (_emulating) ; // Wait for emulation to quit

                _cpu.Reset();
                _mmu.Reset();
                _gpu.Reset();

                // Draw blank frame buffer
                OnDrawScreen?.Invoke(this,
                    new DrawScreenEventArgs(_gpu.FrameBuffer));

                Paused = false;

                if (wasTurnedOn)
                {
                    PowerOn();
                }
            }
        }

        public void Step()
        {
            lock (_sync)
            {
                if (TurnedOn && Paused)
                {
                    StepUnlocked();
                }
            }
        }

        private void StepUnlocked()
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
        }

        private void Emulate()
        {
            _emulating = true;
            _sw.Restart();
            while (true)
            {
                if (Paused || !TurnedOn)
                {
                    _emulating = false;
                    return;
                }

                StepUnlocked();

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
