using GameBoyEm.Api;
using System;
using System.Runtime.Serialization;

namespace GameBoyEm
{
    [Serializable]
    public class Timer : ITimer, ISerializable
    {
        private const ushort _divideThreshold = 64;

        private IMmu _mmu;
        private ushort _timerCycles;
        private ushort _dividerCycles;

        internal IMmu Mmu { set { _mmu = value; } }

        public Timer(IMmu mmu)
        {
            _mmu = mmu;
        }

        protected Timer(SerializationInfo info, StreamingContext ctx)
        {
            _timerCycles = info.GetUInt16("TimerCycles");
            _dividerCycles = info.GetUInt16("DividerCycles");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TimerCycles", _timerCycles);
            info.AddValue("DividerCycles", _dividerCycles);
        }

        private bool _wasTimerEnabled;
        public void Step(ushort cycles)
        {
            _dividerCycles += cycles;
            if (_dividerCycles >= _divideThreshold)
            {
                // This register is incremented at 16384Hz
                // Since quarterCycles = cycles / 4
                // _divideThreshold is calculated as:
                // 4.194304MHz / 16.386KHz = 256
                // 256 / 4 = 64 = _divideThreshold
                _dividerCycles -= _divideThreshold;
                _mmu.DividerRegister++;
            }

            // Reset cycles on timer on/off transition
            var timerEnabled = _mmu.TimerEnabled;
            if (timerEnabled && !_wasTimerEnabled)
            {
                _timerCycles = 0;
                _wasTimerEnabled = true;
            }
            else if (!timerEnabled && _wasTimerEnabled)
            {
                _wasTimerEnabled = false;
            }

            if (timerEnabled)
            {
                _timerCycles += cycles;

                ushort threshold = 0;
                switch (_mmu.TimerSpeed)
                {
                    case 1: threshold = 4; break; // 262.144KHz
                    case 2: threshold = 16; break; // 65.536KHz
                    case 3: threshold = 64; break; // 16.384KHz
                    case 0: threshold = 256; break; // 4096Hz
                }

                if (_timerCycles >= threshold)
                {
                    var counter = ++_mmu.TimerCounter;
                    if (counter == 0) // Overflow
                    {
                        _mmu.TimerCounter = _mmu.TimerModulo;
                        _mmu.TimerInterrupt = true;
                    }
                    _timerCycles -= threshold;
                }
            }
        }
    }
}
