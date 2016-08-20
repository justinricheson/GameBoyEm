using System;

namespace GameBoyEm.Api
{
    public class BreakpointEventArgs : EventArgs
    {
        public ushort PC { get; private set; }

        public BreakpointEventArgs(ushort pc)
        {
            PC = pc;
        }
    }
}
