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
        public byte F { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }
        public ushort SP { get; set; }
        public ushort PC { get; set; }
        public bool FZ { get; set; }
        public bool FN { get; set; }
        public bool FH { get; set; }
        public bool FC { get; set; }
        public bool IME { get; set; }

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
            return A == y.A
                && B == y.B
                && C == y.C
                && D == y.D
                && E == y.E
                && F == y.F
                && H == y.H
                && L == y.L
                && SP == y.SP
                && PC == y.PC
                && FZ == y.FZ
                && FN == y.FN
                && FH == y.FH
                && FC == y.FC
                && IME == y.IME;
        }
    }

    public class CpuStartState : CpuState
    {
        public List<byte> InitialMemory { get; set; }

        public CpuStartState()
        {
            InitialMemory = new List<byte>();
        }

        public override string ToString()
        {
            string hex = BitConverter.ToString(InitialMemory.ToArray()).Replace("-", "");
            return $"{A}|{B}|{C}|{D}|{E}|{F}|{H}|{L}|{SP}|{PC}|{S(FZ)}|{S(FN)}|{S(FH)}|{S(FC)}|{S(IME)},{hex}";
        }

        private string S(bool flag) => flag ? "1" : "0";
    }

    public class CpuEndState : CpuState
    {
        public List<MemoryRecord> MemoryRecord { get; set; }

        public CpuEndState()
        {
            MemoryRecord = new List<MemoryRecord>();
        }

        public static CpuEndState FromString(string encoded)
        {
            var split1 = encoded.Split(',');
            var registers = split1[0].Split('|');
            var records = split1[1].Split('-');

            return new CpuEndState
            {
                A = byte.Parse(registers[0]),
                B = byte.Parse(registers[1]),
                C = byte.Parse(registers[2]),
                D = byte.Parse(registers[3]),
                E = byte.Parse(registers[4]),
                F = byte.Parse(registers[5]),
                H = byte.Parse(registers[6]),
                L = byte.Parse(registers[7]),
                SP = ushort.Parse(registers[8]),
                PC = ushort.Parse(registers[9]),
                FZ = registers[10] == "1",
                FN = registers[11] == "1",
                FH = registers[12] == "1",
                FC = registers[13] == "1",
                IME = registers[14] == "1",
                MemoryRecord = records.Select(r =>
                {
                    var fields = r.Split('|');
                    return new MemoryRecord
                    {
                        Type = (MemoryRecordType)Enum.Parse(typeof(MemoryRecordType), fields[0]),
                        Address = ushort.Parse(fields[1]),
                        Value = ushort.Parse(fields[2])
                    };
                }).ToList()
            };
        }
    }

    public class MemoryRecord
    {
        public MemoryRecordType Type { get; set; }
        public ushort Address { get; set; }
        public ushort Value { get; set; }
    }

    public enum MemoryRecordType
    {
        Read,
        Write,
        ReadWord,
        WriteWord
    }
}
