using System;
using System.Runtime.Serialization;

namespace GameBoyEm.Cartridge
{
    [Serializable]
    public class Mbc1RamBatteryCartridge : Mbc1RamCartridge
    {
        // Save/Restore is already implemented for all games
        // This is a convenience cartridge, maybe we'll add battery functions later
        public Mbc1RamBatteryCartridge(byte[] rom, byte[][] romBanks, byte[][] ramBanks)
            : base(rom, romBanks, ramBanks) { }

        protected Mbc1RamBatteryCartridge(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }
    }
}
