using GameBoyEm.Interfaces.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.QualityTools.Testing.Fakes;

namespace GameBoyEm.Tests.Oracle
{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void TestPInvoke()
        {
            var startState = new CpuState
            {
                A = 0x01,
                B = 0x02,
                C = 0x03,
                D = 0x04,
                E = 0x05,
                H = 0x06,
                L = 0x07,
                SP = 0x0008,
                PC = 0x0009,
                FZ = false,
                FN = true,
                FH = false,
                FC = true,
                IME = false,
                Memory = new List<MemoryRecord>
                {
                    new MemoryRecord { Address = 0x0000, Value = 0x84 },
                    new MemoryRecord { Address = 0x0001, Value = 0x01 },
                    new MemoryRecord { Address = 0x0002, Value = 0x02 }
                }
            };

            var testState = Test(startState);
            var oracleState = Oracle.Execute(startState);

            Assert.AreEqual(testState, oracleState);
        }

        private CpuState Test(CpuState cpuState)
        {
            var mem = new Dictionary<ushort, byte>();
            foreach (var m in cpuState.Memory)
            {
                mem.Add(m.Address, m.Value);
            }

            var read = new FakesDelegates.Func<ushort, byte>(a =>
            {
                if (!mem.ContainsKey(a))
                {
                    return 0; // Only writes allocate memory
                }
                return mem[a];
            });

            var readW = new FakesDelegates.Func<ushort, ushort>(a =>
            {
                var hi = read(a);
                var lo = read((ushort)(a + 1));
                return (ushort)((hi << 8) | lo);
            });

            var mmu = new StubIMmu();
            mmu.ReadByteUInt16 = read;
            mmu.ReadWordUInt16 = readW;
            mmu.WriteByteUInt16Byte = (a, v) =>
            {
                mem[a] = v;
            };
            mmu.WriteWordUInt16UInt16 = (a, v) =>
            {
                mem[a] = (byte)((v & 0xFF00) >> 8);
                mem[(ushort)(a + 1)] = (byte)(v & 0xFF);
            };

            var cpu = new Cpu(mmu,
                cpuState.A, cpuState.B, cpuState.C,
                cpuState.D, cpuState.E, cpuState.H,
                cpuState.L, cpuState.SP, cpuState.PC,
                cpuState.FZ, cpuState.FN, cpuState.FH,
                cpuState.FC, cpuState.IME);

            cpu.Step();

            return new CpuState
            {
                A = cpu.A,
                B = cpu.B,
                C = cpu.C,
                D = cpu.D,
                E = cpu.E,
                H = cpu.H,
                L = cpu.L,
                SP = cpu.SP,
                PC = cpu.PC,
                FZ = cpu.FZ,
                FN = cpu.FN,
                FH = cpu.FH,
                FC = cpu.FC,
                IME = cpu.IME,
                Memory = mem.Keys.OrderBy(k => k).Select(k =>
                {
                    var v = mem[k];
                    return new MemoryRecord
                    {
                        Address = k,
                        Value = v
                    };
                }).ToList()
            };
        }
    }
}
