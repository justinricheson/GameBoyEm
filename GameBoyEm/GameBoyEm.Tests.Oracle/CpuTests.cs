using GameBoyEm.Interfaces.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GameBoyEm.Tests.Oracle
{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void RandomTestOracle()
        {
            foreach (var state in GenerateCpuStates())
            {
                var r1 = Test(state);
                var r2 = Oracle.Execute(state);

                Assert.AreEqual(r1, r2);
            }
        }

        private static HashSet<byte> _skippedInstructions = new HashSet<byte>
        {
            16,  // STOP: oracle implementation increments pc, can't find any documentation to say if this is correct
            39,  // DAA: skipping temporarily
            118, // HALT: oracle implementation decrements pc, think this only happens when interrupts are enabled, deal with this later
        };
        private IEnumerable<CpuState> GenerateCpuStates()
        {
            // Non-CB tests
            for (byte op = 0; op < 255; op++)
            {
                if (_skippedInstructions.Contains(op))
                {
                    continue;
                }
                ushort pc = 0;
                //yield return GenerateRandomState(op, pc);
            }

            // CB tests
            for (byte op = 0; op < 255; op++)
            {
                ushort pc = 0;
                yield return GenerateRandomState(op, pc, true);
            }
        }

        private CpuState GenerateRandomState(byte op, ushort pc, bool isCbInstruction = false)
        {
            var state = new CpuState // TODO generate random state
            {
                A = 0x01,
                B = 0x02,
                C = 0x03,
                D = 0x04,
                E = 0x05,
                H = 0x06,
                L = 0x07,
                SP = 0x0008,
                PC = pc,
                FZ = false,
                FN = true,
                FH = false,
                FC = true,
                IME = false
            };

            if (isCbInstruction)
            {
                state.Memory.Insert(0, new MemoryRecord { Address = pc, Value = 0xCB });
                state.Memory.Insert(1, new MemoryRecord { Address = (ushort)(pc + 1), Value = op });
                state.Memory.Insert(2, new MemoryRecord { Address = (ushort)(pc + 2), Value = 0x01 });
                state.Memory.Insert(3, new MemoryRecord { Address = (ushort)(pc + 3), Value = 0x02 });
            }
            else
            {
                state.Memory.Insert(1, new MemoryRecord { Address = (ushort)(pc), Value = op });
                state.Memory.Insert(2, new MemoryRecord { Address = (ushort)(pc + 1), Value = 0x01 });
                state.Memory.Insert(3, new MemoryRecord { Address = (ushort)(pc + 2), Value = 0x02 });
            }

            return state;
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
