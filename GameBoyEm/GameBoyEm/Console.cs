using GameBoyEm.Api;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GameBoyEm
{
    public class Console
    {
        public ICpu _cpu;
        public IMmu _mmu;
        public IGpu _gpu;
        private bool _cartridgeLoaded;
        private long _cumulativeCycles;
        private Stopwatch _sw = new Stopwatch();

        public ICpu Cpu { get { return _cpu; } }

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
                await Resume();
            }
        }

        public void PowerOff()
        {
            Pause();
        }

        public void Pause()
        {
            if (_tokenSource != null && _cartridgeLoaded)
            {
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
            return new Console(cpu, mmu, gpu);
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
            UpdateJoypadRegister();
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
                    while (_sw.ElapsedTicks < TimeSpan.TicksPerSecond);
                    _sw.Restart();
                }
            }
        }

        private void UpdateJoypadRegister()
        {
            // Just disable everything for now
            if (_mmu.ReadByte(0xFF00).AND(0x20) == 0)
            {
                _mmu.WriteByte(0xFF00, _mmu.ReadByte(0xFF00).OR(0x08));
                _mmu.WriteByte(0xFF00, _mmu.ReadByte(0xFF00).OR(0x04));
                _mmu.WriteByte(0xFF00, _mmu.ReadByte(0xFF00).OR(0x02));
                _mmu.WriteByte(0xFF00, _mmu.ReadByte(0xFF00).OR(0x01));
            }
            else if (_mmu.ReadByte(0xFF00).AND(0x10) == 0)
            {
                _mmu.WriteByte(0xFF00, _mmu.ReadByte(0xFF00).OR(0x08));
                _mmu.WriteByte(0xFF00, _mmu.ReadByte(0xFF00).OR(0x04));
                _mmu.WriteByte(0xFF00, _mmu.ReadByte(0xFF00).OR(0x02));
                _mmu.WriteByte(0xFF00, _mmu.ReadByte(0xFF00).OR(0x01));
            }
        }
    }
}
