using System.Collections.Generic;

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
        public List<byte> InitialMemory { get; set; }
        public List<MemoryRecord> MemoryRecord { get; set; }

        public CpuState()
        {
            InitialMemory = new List<byte>();
            MemoryRecord = new List<MemoryRecord>();
        }

        public override string ToString()
        {
            return string.Empty;
        }

        public static CpuState FromString(string encoded)
        {
            return null;
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
        ReadWord,
        Write,
        WriteWord
    }
}
