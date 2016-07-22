using GameBoyEm.Interfaces;
using System;

namespace GameBoyEm.Cartridge
{
    public static class CartridgeBuilder
    {
        public static ICartridge Build(byte[] rom)
        {
            var cartridgeType = Cartridge.GetType(rom);
            switch (cartridgeType)
            {
                case CartridgeType.RomOnly:
                    return new RomOnlyCartridge(rom);
                // TODO
                default:
                    throw new NotSupportedException("Unsupported cartridge type");
            }
        }
    }
}
