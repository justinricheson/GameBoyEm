using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GameBoyEm.Tests.Oracle
{
    public static class Oracle
    {
        [DllImport("..\\..\\..\\..\\GameBoyRef\\Debug\\GameBoyRef.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Run(
            [MarshalAs(UnmanagedType.LPStr)]string input,
            [MarshalAs(UnmanagedType.LPStr)]StringBuilder output);

        public static CpuFinishState Execute(CpuStartState state)
        {
            var output = new StringBuilder(1024);
            var result = Run(state.ToString(), output);
            return CpuFinishState.FromString(output.ToString());
        }
    }
}
