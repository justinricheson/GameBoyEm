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

        public static string Execute(string input)
        {
            var output = new StringBuilder(1024);
            var result = Run(input, output);
            return output.ToString();
        }
    }
}
