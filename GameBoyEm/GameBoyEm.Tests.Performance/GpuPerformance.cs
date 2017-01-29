using GameBoyEm.Api;
using GameBoyEm.Api.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace GameBoyEm.Tests.Performance
{
    [TestClass]
    public class GpuPerformance
    {
        private const long _numSteps = 10000000;

        private Random _r = new Random();
        private ushort RandomCycleCount() => (ushort)_r.Next(0, 7);
        private byte RandomByte() => (byte)_r.Next(0, 256);

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GpuFramesPerSecondBenchmark()
        {
            Mode lcdMode = 0;
            byte currentLine = 0;
            var mmu = new StubIMmu
            {
                ReadByteUInt16 = a => RandomByte(),
                LcdCurrentLineGet = () => currentLine,
                LcdCurrentLineSetByte = l => currentLine = l,
                LcdModeGet = () => lcdMode,
                LcdModeSetMode = m => lcdMode = m,
                LcdEnabledGet = () => true,
                DisplayBackgroundGet = () => true,
                DisplayWindowGet = () => true,
                DisplaySpritesGet = () => true
            };
            var gpu = new Gpu(mmu);

            var drawCount = 0;
            var sw = new Stopwatch(); sw.Start();
            for (int i = 0; i < _numSteps; i++)
            {
                drawCount += gpu.Step(RandomCycleCount()) ? 1 : 0;
            }
            sw.Stop();

            var stepsPerSecond = (decimal)(_numSteps / sw.Elapsed.TotalSeconds);
            var framesPerSecond = (decimal)(drawCount / sw.Elapsed.TotalSeconds);
            var speedupFactor = framesPerSecond / 60;
            TestContext.WriteLine($"Steps per second: {stepsPerSecond.ToString("N")}");
            TestContext.WriteLine($"Frames per second: {framesPerSecond.ToString("N")}");
            TestContext.WriteLine($"Speedup factor: {speedupFactor.ToString("N")}");
        }
    }
}
