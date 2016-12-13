using GameBoyEm.Api;
using System;
using System.Runtime.Serialization;

namespace GameBoyEm.Cartridge
{
    [Serializable]
    public abstract class Cartridge : ICartridge, ISerializable
    {
        public static readonly ushort BankSize = 16384;
        protected byte[] _rom;

        public Cartridge(byte[] rom)
        {
            _rom = rom;
        }

        protected Cartridge(SerializationInfo info, StreamingContext ctx)
        {
            _rom = (byte[])info.GetValue("Rom", typeof(byte[]));
        }

        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte value);
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Rom", _rom);
        }
    }

    public enum CartridgeType
    {
        Unknown,
        RomOnly,
        Mbc1
    }
}
