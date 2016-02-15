using System;
using System.Collections.Generic;

namespace GameBoyEm
{
    public class OpDecoder
    {
        private static Dictionary<byte, Action<ICpu>> _opMap = new Dictionary<byte, Action<ICpu>>
        {
            {0x00, LDrr_bb },
            // TODO
        };

        public static Action<ICpu> Decode(byte op)
        {
            return _opMap[op];
        }

        private static void LDrr_bb(ICpu cpu)
        {
            // TODO
        }
    }
}
