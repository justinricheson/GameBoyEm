using GameBoyEm.Api;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace GameBoyEm
{
    [Serializable]
    public class Console : ISerializable
    {
        private object _sync = new object();
        private long _cumulativeCycles;
        private Stopwatch _sw = new Stopwatch();
        private bool _emulating;

        public ICpu Cpu { get; }
        public IMmu Mmu { get; }
        public IGpu Gpu { get; }
        public IController Controller { get; }
        public bool Paused { get; private set; }
        public bool TurnedOn { get; private set; }
        public bool CartridgeLoaded { get; private set; }
        public bool EmitFrames { get; set; }
        public EventHandler<DrawScreenEventArgs> OnDrawScreen;

        public Console(ICpu cpu, IMmu mmu, IGpu gpu, IController controller)
        {
            Cpu = cpu;
            Mmu = mmu;
            Gpu = gpu;
            Controller = controller;
            EmitFrames = true;
        }

        protected Console(SerializationInfo info, StreamingContext ctx)
        {
            Cpu = (ICpu)info.GetValue("Cpu", typeof(ICpu));
            Mmu = (IMmu)info.GetValue("Mmu", typeof(IMmu));
            Gpu = (IGpu)info.GetValue("Gpu", typeof(IGpu));

            // Hack to reset references on deserialize
            (Cpu as Cpu).Mmu = Mmu;
            (Gpu as Gpu).Mmu = Mmu;
            Controller = new Controller(Mmu);

            CartridgeLoaded = true;
            TurnedOn = true;
            Paused = true;
            EmitFrames = true;
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
                    Mmu.LoadCartridge(cartridge);
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

                Cpu.Reset();
                Mmu.Reset();
                Gpu.Reset();

                // Draw blank frame buffer
                OnDrawScreen?.Invoke(this,
                    new DrawScreenEventArgs(Gpu.FrameBuffer));

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
            var cycles = Cpu.Step();
            _cumulativeCycles += cycles;

            var draw = Gpu.Step(cycles);
            if (draw && EmitFrames)
            {
                OnDrawScreen?.Invoke(this,
                    new DrawScreenEventArgs(Gpu.FrameBuffer));
            }
            Controller.Step();
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
                    // Slow down emulation to sync with ~4.194304MHz
                    _cumulativeCycles -= 4194304;
                    while (_sw.ElapsedTicks < TimeSpan.TicksPerSecond) ;
                    _sw.Restart();
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Cpu", Cpu);
            info.AddValue("Mmu", Mmu);
            info.AddValue("Gpu", Gpu);
        }
    }
}
