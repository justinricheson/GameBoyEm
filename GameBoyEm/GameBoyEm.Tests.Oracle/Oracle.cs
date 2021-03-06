﻿using System.Runtime.InteropServices;
using System.Text;

namespace GameBoyEm.Tests.Oracle
{
    public static class Oracle
    {
        [DllImport("..\\..\\..\\..\\GameBoyRef\\Debug\\GameBoyRef.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Run(
            [MarshalAs(UnmanagedType.LPStr)]string input,
            [MarshalAs(UnmanagedType.LPStr)]StringBuilder output);

        public static CpuState Execute(CpuState state)
        {
            var output = new StringBuilder(16384);
            var result = Run(state.ToString(), output);
            return CpuState.FromString(output.ToString());
        }
    }
}
