namespace GameBoyEm.Api
{
    public interface IMmu
    {
        void Reset();
        void LoadCartridge(ICartridge cartridge);

        bool InterruptsExist { get; }
        bool Vblank { get; }
        bool LcdStat { get; }
        bool Timer { get; }
        bool Serial { get; }
        bool JoyPad { get; }

        byte ReadByte(ushort address);
        ushort ReadWord(ushort address);
        void WriteByte(ushort address, byte value);
        void WriteWord(ushort address, ushort value);
    }
}
