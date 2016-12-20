namespace GameBoyEm.Api
{
    public interface IMmu
    {
        void Reset();
        void LoadCartridge(ICartridge cartridge);

        bool InterruptsExist { get; }
        bool VblankInterrupt { get; set; }
        bool LcdStatInterrupt { get; set; }
        bool TimerInterrupt { get; set; }
        bool SerialInterrupt { get; set; }
        bool JoyPadInterrupt { get; set; }

        bool KeySelector { get; }
        bool DirSelector { get; }
        bool APressed { get; set; }
        bool BPressed { get; set; }
        bool SelectPressed { get; set; }
        bool StartPressed { get; set; }
        bool RightPressed { get; set; }
        bool LeftPressed { get; set; }
        bool UpPressed { get; set; }
        bool DownPressed { get; set; }

        bool LcdEnabled { get; }
        bool DisplayBackground { get; }
        bool DisplaySprites { get; }
        bool DisplayWindow { get; }

        bool LcdcHblank { get; }
        bool LcdcVblank { get; }
        bool LcdcOam { get; }

        byte DividerRegister { get; set; }

        byte ReadByte(ushort address);
        ushort ReadWord(ushort address);
        void WriteByte(ushort address, byte value);
        void WriteWord(ushort address, ushort value);
    }
}
