using GameBoyEm.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameBoyEm.Tests
{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void INCBC_BCIs0_ShouldIncrementBC()
        {
            // Arrange
            var mmu = new StubIMmu { ReadByteUInt16 = a => 3 };
            var cpu = new Cpu(mmu);

            // Act
            cpu.Run();

            // Assert
            Assert.AreEqual(1, cpu.BC);
        }

        [TestMethod]
        public void INCBC_BCIs255_ShouldIncrementBC()
        {
            // Arrange
            var mmu = new StubIMmu { ReadByteUInt16 = a => 3 };
            var cpu = new Cpu(mmu, c: 255); // Set the low bits

            // Act
            cpu.Run();

            // Assert
            Assert.AreEqual(256, cpu.BC);
        }

        [TestMethod]
        public void INCBC_BCIs65535_ShouldOverflowBC()
        {
            // Arrange
            var mmu = new StubIMmu { ReadByteUInt16 = a => 3 };
            var cpu = new Cpu(mmu, b: 255, c: 255);

            // Act
            cpu.Run();

            // Assert
            Assert.AreEqual(0, cpu.BC);
        }
    }
}
