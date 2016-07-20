using GameBoyEm.Interfaces.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                Debug.WriteLine($"Testing: {state.Memory[0].Value:X} - {state.Memory[1].Value:X}");
                Assert.AreEqual(r1, r2);
            }
        }

        private static HashSet<byte> _skippedInstructions = new HashSet<byte>
        {
            16,  // STOP: oracle implementation increments pc, can't find any documentation to say if this is correct
            118, // HALT: oracle implementation decrements pc, think this only happens when interrupts are enabled, deal with this later
            39,  // DAA: know difference between ref and impl versions (FC flag never unset in ref version)
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
                yield return GenerateRandomState(op, pc);
            }

            // CB tests
            for (byte op = 0; op < 255; op++)
            {
                ushort pc = 0;
                yield return GenerateRandomState(op, pc, true);
            }
        }

        private static Random _r = new Random();
        private CpuState GenerateRandomState(byte op, ushort pc, bool isCbInstruction = false)
        {
            var state = new CpuState
            {
                A = (byte)_r.Next(0, 256),
                B = (byte)_r.Next(0, 256),
                C = (byte)_r.Next(0, 256),
                D = (byte)_r.Next(0, 256),
                E = (byte)_r.Next(0, 256),
                H = (byte)_r.Next(0, 256),
                L = (byte)_r.Next(0, 256),
                SP = (ushort)_r.Next(0, 65536),
                PC = pc,
                FZ = _r.Next() % 2 == 0,
                FN = _r.Next() % 2 == 0,
                FH = _r.Next() % 2 == 0,
                FC = _r.Next() % 2 == 0,
                IME = _r.Next() % 2 == 0
            };

            var startAddress = isCbInstruction ? (ushort)(pc + 1) : pc;
            if (isCbInstruction)
            {
                state.Memory.Add(new MemoryRecord { Address = pc, Value = 0xCB });
            }
            state.Memory.Add(new MemoryRecord { Address = startAddress, Value = op });

            var nextAddress = (ushort)(startAddress + 1);
            for (ushort i = nextAddress; i < 1024; i++)
            {
                state.Memory.Add(new MemoryRecord { Address = i, Value = (byte)_r.Next(0, 256) });
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
