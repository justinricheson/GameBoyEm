using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GameBoyEm.Tests.Oracle
{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void TestPInvoke()
        {
            var foo = Oracle.Execute(new CpuStartState
            {
                A = 0x00,
                B = 0x00,
                C = 0x00,
                D = 0x00,
                E = 0x00,
                F = 0x00,
                H = 0x00,
                L = 0x00,
                SP = 0x0000,
                PC = 0x0000,
                FZ = false,
                FN = false,
                FH = false,
                FC = false,
                IME = false,
                InitialMemory = new List<byte>
                {
                    0x00, 0x00, 0x00
                }
            });
        }
    }
}
