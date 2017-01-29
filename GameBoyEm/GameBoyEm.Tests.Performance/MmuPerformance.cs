using GameBoyEm.Api.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace GameBoyEm.Tests.Performance
{
    [TestClass]
    public class MmuPerformance
    {
        private const double _numOperations = 100000000;

        private Random _r = new Random();
        private int RandomOperation() => _r.Next(0, 4);
        private byte RandomByte() => (byte)_r.Next(0, 256);
        private ushort RandomUShort() => (ushort)((RandomByte() << 8) | RandomByte());

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void MmuRandomReadWriteBenchmark()
        {
            var mmu = GetMmu();
            RunTest(mmu, () =>
            {
                switch (RandomOperation())
                {
                    case 0: mmu.ReadByte(RandomUShort()); break;
                    case 1: mmu.ReadWord(RandomUShort()); break;
                    case 2: mmu.WriteByte(RandomUShort(), 0x00); break;
                    case 3: mmu.WriteWord(RandomUShort(), 0x0000); break;
                }
            });
        }

        [TestMethod]
        public void Mmu8BitReadBenchmark()
        {
            var mmu = GetMmu();
            RunTest(mmu, () => mmu.ReadByte(RandomUShort()));
        }

        [TestMethod]
        public void Mmu16BitReadBenchmark()
        {
            var mmu = GetMmu();
            RunTest(mmu, () => mmu.ReadWord(RandomUShort()));
        }

        [TestMethod]
        public void Mmu8BitWriteBenchmark()
        {
            var mmu = GetMmu();
            RunTest(mmu, () => mmu.WriteByte(RandomUShort(), 0x00));
        }

        [TestMethod]
        public void Mmu16BitWriteBenchmark()
        {
            var mmu = GetMmu();
            RunTest(mmu, () => mmu.WriteWord(RandomUShort(), 0x0000));
        }

        private Mmu GetMmu()
        {
            var cartridge = new StubICartridge
            {
                ReadUInt16 = a => 0x00,
                ReadRamUInt16 = a => 0x00
            };
            var mmu = new Mmu();
            mmu.LoadCartridge(cartridge);
            return mmu;
        }

        private void RunTest(Mmu mmu, Action test)
        {
            var sw = new Stopwatch(); sw.Start();
            for (int i = 0; i < _numOperations; i++)
            {
                test();
            }
            sw.Stop();

            var opsPerSecond = (decimal)(_numOperations / sw.Elapsed.TotalSeconds);
            TestContext.WriteLine($"Operations per second: {opsPerSecond.ToString("N")}");
        }
    }
}
