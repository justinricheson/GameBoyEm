using GameBoyEm.Api.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace GameBoyEm.Tests.Performance
{
    [TestClass]
    public class CpuPerformance
    {
        private const int _boolBias = 1000;
        private const double _numInstructions = 100000000;

        private Random _r = new Random();
        private bool RandomBool() => _r.Next(1, _boolBias + 1) == _boolBias;
        private byte RandomByte()
        {
            var b = _r.Next(0, 256);
            return (byte)(b == 0x76 ? 0x77 : b); // Never return HALT
        }
        private ushort RandomUShort() => (ushort)((RandomByte() << 8) | RandomByte());

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CpuRandomInstructionBenchmark()
        {
            var mmu = GetMmu();
            mmu.InterruptsExistGet = RandomBool;
            mmu.VblankInterruptGet = RandomBool;
            mmu.LcdStatInterruptGet = RandomBool;
            mmu.TimerInterruptGet = RandomBool;
            mmu.SerialInterruptGet = RandomBool;
            mmu.JoyPadInterruptGet = RandomBool;

            RunTest(mmu);
        }

        [TestMethod]
        public void CpuRandomInstructionBenchmark_NoInterrupts()
        {
            RunTest(GetMmu());
        }

        private StubIMmu GetMmu()
        {
            return new StubIMmu
            {
                ReadByteUInt16 = a => RandomByte(),
                ReadWordUInt16 = a => RandomUShort(),
                InterruptsExistGet = () => false
            };
        }

        private void RunTest(StubIMmu mmu)
        {
            var cpu = new Cpu(mmu);

            long cycles = 0;
            var sw = new Stopwatch(); sw.Start();
            for (int i = 0; i < _numInstructions; i++)
            {
                cycles += cpu.Step();
            }
            sw.Stop();

            cycles *= 4; // Cpu returns 1/4 cycles
            var insPerSecond = (decimal)(_numInstructions / sw.Elapsed.TotalSeconds);
            var cyclesPerSecond = (decimal)(cycles / sw.Elapsed.TotalSeconds);
            var speedupFactor = cyclesPerSecond / 4194304;
            TestContext.WriteLine($"Instructions per second: {insPerSecond.ToString("N")}");
            TestContext.WriteLine($"Cycles per second: {cyclesPerSecond.ToString("N")}");
            TestContext.WriteLine($"Speedup factor: {speedupFactor.ToString("N")}");
        }
    }
}
