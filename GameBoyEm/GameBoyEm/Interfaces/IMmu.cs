namespace GameBoyEm.Interfaces
{
    public interface IMmu
    {
        void Reset();
        void LoadCartridge(ICartridge cartridge);
        byte ReadByte(ushort address);
        ushort ReadWord(ushort address);
        void WriteByte(ushort address, byte value);
        void WriteWord(ushort address, ushort value);
    }
}
