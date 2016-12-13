using Common.Logging;
using GameBoyEm.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GameBoyEm
{
    [Serializable]
    public class Console : ISerializable
    {
        private const int _cycleBoundary = 100000;
        private const double _cycleTime = _cycleBoundary / 4194304d;
        private long _cumulativeCycles;
        private Stopwatch _sw = new Stopwatch();
        private bool _emulating;
        private HashSet<uint> _breakpoints;

        public ICpu Cpu { get; }
        public IMmu Mmu { get; }
        public IGpu Gpu { get; }
        public IController Controller { get; }
        public bool Paused { get; private set; }
        public bool TurnedOn { get; private set; }
        public bool CartridgeLoaded { get; private set; }
        public bool EmitFrames { get; set; }
        public EventHandler<DrawScreenEventArgs> OnDrawScreen;
        public EventHandler<BreakpointEventArgs> OnBreakpoint;

        public Console(ICpu cpu, IMmu mmu, IGpu gpu, IController controller)
        {
            Cpu = cpu;
            Mmu = mmu;
            Gpu = gpu;
            Controller = controller;
            EmitFrames = true;
            _breakpoints = new HashSet<uint>();
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

            _breakpoints = new HashSet<uint>();
            CartridgeLoaded = true;
            TurnedOn = true;
            Paused = true;
            EmitFrames = true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Cpu", Cpu);
            info.AddValue("Mmu", Mmu);
            info.AddValue("Gpu", Gpu);
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
            if (!TurnedOn)
            {
                Mmu.LoadCartridge(cartridge);
                CartridgeLoaded = true;
            }
        }

        public void PowerOn()
        {
            if (!TurnedOn && CartridgeLoaded)
            {
                TurnedOn = true;
                Task.Run(() => Emulate());
            }
        }

        public void PowerOff()
        {
            if (TurnedOn)
            {
                TurnedOn = false;
                Reset();
                Wait();
            }
        }

        public void Pause()
        {
            if (TurnedOn && !Paused)
            {
                Paused = true;
                Wait();
            }
        }

        public void Resume()
        {
            if (TurnedOn && Paused)
            {
                Paused = false;
                Task.Run(() => Emulate());
            }
        }

        public void Reset()
        {
            var wasTurnedOn = TurnedOn;
            TurnedOn = false;
            Wait();

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

        public void SetBreakpoint(uint pc)
        {
            _breakpoints.Add(pc);
        }

        public void UnsetBreakpoint(uint pc)
        {
            _breakpoints.Remove(pc);
        }

        public void StepMany(int steps, Action<int> progress)
        {
            if (TurnedOn && Paused)
            {
                for (int i = 0; i < steps; i++)
                {
                    progress(i);
                    Step();
                }
            }
        }

        private void Step()
        {
            if (_breakpoints.Count > 0)
            {
                if (_breakpoints.Contains(Cpu.PC))
                {
                    OnBreakpoint?.Invoke(this,
                        new BreakpointEventArgs(Cpu.PC));
                }
            }

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

                Step();

                if (_cumulativeCycles >= _cycleBoundary)
                {
                    // Slow down emulation to sync with ~4.194304MHz
                    var expectedTicks = TimeSpan.FromSeconds(_cycleTime).Ticks;
                    _cumulativeCycles -= _cycleBoundary;

                    _sw.Restart();
                    while (_sw.ElapsedTicks < expectedTicks) ;
                    _sw.Stop();
                }
            }
        }

        private void Wait()
        {
            while (_emulating)
            {
                Thread.Sleep(0);
            }
        }
    }
}
