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
        private IMmu _mmu;
        private ushort _quarterCycles;
        private const ushort _divideThreshold = 64;

        internal IMmu Mmu { set { _mmu = value; } }

        public Timer(IMmu mmu)
        {
            _mmu = mmu;
        }

        protected Timer(SerializationInfo info, StreamingContext ctx)
        {
            _quarterCycles = info.GetUInt16("QuarterCycles");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("QuarterCycles", _quarterCycles);
        }

        public void Step(ushort quarterCycles)
        {
            _quarterCycles += quarterCycles;
            if (_quarterCycles >= _divideThreshold)
            {
                // This register is incremented at 16384Hz
                // Since quarterCycles = cycles / 4
                // _divideThreshold is calculated as:
                // 4.194304MHz / 16.386KHz = 256
                // 256 / 4 = 64 = _divideThreshold
                _quarterCycles -= _divideThreshold;
                _mmu.DividerRegister++;
            }
        }
    }
}
