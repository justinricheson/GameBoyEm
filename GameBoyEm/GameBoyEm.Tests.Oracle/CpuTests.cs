using GameBoyEm.Interfaces.Fakes;
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
            var startState = new CpuStartState
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
                    0x84, 0x00, 0x00
                }
            };

            var testEndState = Test(startState);
            var oracleEndState = Oracle.Execute(startState);
            Assert.AreEqual(testEndState, oracleEndState);
        }

        private static int _next;
        private CpuEndState Test(CpuStartState cpuStartState)
        {
            var mmu = new StubIMmu();
            mmu.ReadByteUInt16 = _ => cpuStartState.InitialMemory[_next++];
            mmu.ReadWordUInt16 = _ =>
            {
                var hi = cpuStartState.InitialMemory[_next + 1];
                var lo = cpuStartState.InitialMemory[_next];
                var memLoc = (hi << 8) | lo;
                _next += 2;
                return (ushort)memLoc;
            };
            mmu.WriteByteUInt16Byte = (a, v) => { };
            mmu.WriteWordUInt16UInt16 = (a, v) => { };

            var cpu = new Cpu(mmu,
                cpuStartState.A, cpuStartState.B, cpuStartState.C, cpuStartState.D,
                cpuStartState.E, cpuStartState.F, cpuStartState.H, cpuStartState.L,
                cpuStartState.SP, cpuStartState.PC, cpuStartState.FZ, cpuStartState.FN,
                cpuStartState.FH, cpuStartState.FC, cpuStartState.IME);

            cpu.Step();

            return new CpuEndState
            {
                A = cpu.A,
                B = cpu.B,
                C = cpu.C,
                D = cpu.D,
                E = cpu.E,
                F = cpu.F,
                H = cpu.H,
                L = cpu.L,
                SP = cpu.SP,
                PC = cpu.PC,
                FZ = cpu.FZ,
                FN = cpu.FN,
                FH = cpu.FH,
                FC = cpu.FC,
                IME = cpu.IME
            };
        }
    }
}
