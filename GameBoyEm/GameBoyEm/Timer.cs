using GameBoyEm.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEm
{
    [Serializable]
    public class Timer : ITimer, ISerializable
    {
        private const ushort _divideThreshold = 64;

        private IMmu _mmu;
        private ushort _divideCycles;
        private ushort _clockCycles;

        internal IMmu Mmu { set { _mmu = value; } }

        public Timer(IMmu mmu)
        {
            _mmu = mmu;
        }

        protected Timer(SerializationInfo info, StreamingContext ctx)
        {
            _divideCycles = info.GetUInt16("QuarterCycles");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("QuarterCycles", _divideCycles);
        }

        public void Step(ushort quarterCycles)
        {
            _divideCycles += quarterCycles;
            if (_divideCycles >= _divideThreshold)
            {
                // This register is incremented at 16384Hz
                // Since quarterCycles = cycles / 4
                // _divideThreshold is calculated as:
                // 4.194304MHz / 16.386KHz = 256
                // 256 / 4 = 64 = _divideThreshold
                _divideCycles -= _divideThreshold;
                _mmu.DividerRegister++;
            }

            int threshold = -1;
            _clockCycles += quarterCycles;
            if (_mmu.TimerEnabled)
            {
                switch (_mmu.TimerSpeed)
                {
                    case 1: threshold = 4; break; // 262.144KHz
                    case 2: threshold = 16; break; // 65.536KHz
                    case 3: threshold = 64; break; // 16.384KHz
                    case 0: threshold = 256; break; // 4096Hz
                }

                if (_clockCycles >= threshold)
                {
                    var counter = ++_mmu.TimerCounter;
                    if (counter == 0) // Overflow
                    {
                        _mmu.TimerCounter = _mmu.TimerModulo;
                        _mmu.TimerInterrupt = true;
                    }
                }
            }
        }
    }
}
