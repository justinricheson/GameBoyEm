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

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Rom", _rom);
        }

        public abstract byte Read(ushort address);

        public abstract void Write(ushort address, byte value);

        // Apparantly some non RAM enabled games read from here (Super Mario Land)
        // So can't throw an exception. Just return stubbed 0
        public virtual byte ReadRam(ushort address) => 0;
        public virtual void WriteRam(ushort address, byte value) { }
    }
}
