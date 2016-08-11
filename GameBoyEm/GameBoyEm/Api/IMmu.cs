namespace GameBoyEm.Api
{
    public interface IMmu
    {
        void Reset();
        void LoadCartridge(ICartridge cartridge);

        bool InterruptsExist { get; }
        bool Vblank { get; set; }
        bool LcdStat { get; set; }
        bool Timer { get; set; }
        bool Serial { get; set; }
        bool JoyPad { get; set; }

        bool KeySelector { get; }
        bool DirSelector { get; }
        bool APressed { get; set; }
        bool BPressed { get; set; }
        bool UpPressed { get; set; }
        bool DownPressed { get; set; }
        bool LeftPressed { get; set; }
        bool RightPressed { get; set; }
        bool SelectPressed { get; set; }
        bool StartPressed { get; set; }

        byte ReadByte(ushort address);
        ushort ReadWord(ushort address);
        void WriteByte(ushort address, byte value);
        void WriteWord(ushort address, ushort value);
    }
}
