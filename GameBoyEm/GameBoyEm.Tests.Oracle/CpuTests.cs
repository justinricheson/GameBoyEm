using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameBoyEm.Tests.Oracle
{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void TestPInvoke()
        {
            var foo = Oracle.Execute("FOO");
        }
    }
}
