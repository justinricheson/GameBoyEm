using System;
using System.Collections.Generic;
using System.Linq;

namespace GameBoyEm.Tests.Oracle
{
    public class CpuState
    {
        public byte A { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }
        public ushort SP { get; set; }
        public ushort PC { get; set; }
        public bool FZ { get; set; }
        public bool FN { get; set; }
        public bool FH { get; set; }
        public bool FC { get; set; }
        public bool IME { get; set; }
        public List<MemoryRecord> Memory { get; set; }

        public CpuState()
        {
            Memory = new List<MemoryRecord>();
        }

        public override string ToString()
        {
            string mem = string.Join("|", Memory.Select(m => $"{m.Address}:{m.Value}"));
            return $"{A}|{B}|{C}|{D}|{E}|{H}|{L}|{SP}|{PC}|{S(FZ)}|{S(FN)}|{S(FH)}|{S(FC)}|{S(IME)},{mem}";
        }
        public static CpuState FromString(string encoded)
        {
            var split1 = encoded.Split(',');
            var registers = split1[0].Split('|');
            var memory = split1[1].Split('|');

            return new CpuState
            {
                A = byte.Parse(registers[0]),
                B = byte.Parse(registers[1]),
                C = byte.Parse(registers[2]),
                D = byte.Parse(registers[3]),
                E = byte.Parse(registers[4]),
                H = byte.Parse(registers[5]),
                L = byte.Parse(registers[6]),
                SP = ushort.Parse(registers[7]),
                PC = ushort.Parse(registers[8]),
                FZ = registers[9] == "1",
                FN = registers[10] == "1",
                FH = registers[11] == "1",
                FC = registers[12] == "1",
                IME = registers[13] == "1",
                Memory = memory.Select(m =>
                {
                    var fields = m.Split(':');
                    return new MemoryRecord
                    {
                        Address = ushort.Parse(fields[0]),
                        Value = byte.Parse(fields[1])
                    };
                }).ToList()
            };
        }
        public static bool operator ==(CpuState state1, CpuState state2)
        {
            if (ReferenceEquals(state1, null))
            {
                return ReferenceEquals(state2, null);
            }

            return state1.Equals(state2);
        }
        public static bool operator !=(CpuState state1, CpuState state2)
        {
            return !(state1 == state2);
        }
        public override bool Equals(object x)
        {
            var y = (CpuState)x;
            var registersEqual =
                   A == y.A
                && B == y.B
                && C == y.C
                && D == y.D
                && E == y.E
                && H == y.H
                && L == y.L
                && SP == y.SP
                && PC == y.PC
                && FZ == y.FZ
                && FN == y.FN
                && FH == y.FH
                && FC == y.FC
                && IME == y.IME;

            if (!registersEqual)
            {
                return false;
            }

            foreach (var m1 in Memory)
            {
                bool match = false;
                foreach (var m2 in y.Memory)
                {
                    if (m1.Address == m2.Address && m1.Value == m2.Value)
                    {
                        match = true;
                        break;
                    }
                }

                if (!match)
                {
                    return false;
                }
            }

            return true;
        }

        private string S(bool flag) => flag ? "1" : "0";
    }

    public class MemoryRecord
    {
        public ushort Address { get; set; }
        public byte Value { get; set; }
    }
}
