using GameBoyEm.Api;
using System;
using System.Runtime.Serialization;

namespace GameBoyEm.Cartridge
{
    [Serializable]
    public abstract class Cartridge : ICartridge, ISerializable
    {
        protected const ushort RomBankSize = 16384;
        protected const ushort TypeAddress = 0x0147;
        protected const byte RomOnlyType = 0x00;
        protected byte[] _rom;

        public Cartridge(byte[] rom)
        {
            _rom = rom;
        }

        protected Cartridge(SerializationInfo info, StreamingContext ctx)
        {
            _rom = (byte[])info.GetValue("Rom", typeof(byte[]));
        }

        public static CartridgeType GetType(byte[] rom)
        {
            var type = rom[TypeAddress];
            return type == RomOnlyType
                ? CartridgeType.RomOnly
                : CartridgeType.Unknown;
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
        RomOnly
    }
}
