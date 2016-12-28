using System;

namespace GameBoyEm.Api
{
    public interface IMmu
    {
        void Reset();
        void LoadCartridge(ICartridge cartridge);

        // Interrupt Register
        bool InterruptsExist { get; }
        bool VblankInterrupt { get; set; }
        bool LcdStatInterrupt { get; set; }
        bool TimerInterrupt { get; set; }
        bool SerialInterrupt { get; set; }
        bool JoyPadInterrupt { get; set; }

        // Controller Register
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

        // Lcd Control Register
        bool LcdEnabled { get; }
        bool DisplayBackground { get; }
        bool DisplaySprites { get; }
        bool DisplayWindow { get; }

        // LCD Status Register
        Mode LcdMode { get; set; }
        bool CoincidenceFlag { get; set; }
        bool HBlankStatEnabled { get; }
        bool VBlankStatEnabled { get; }
        bool OamStatEnabled { get; }
        bool CoincidenceStatEnabled { get; }

        // LCD Palette Register
        byte LcdDefaultPalette { get; }

        // LCD Y-Coordinate Register
        byte LcdCurrentLine { get; set; }
        byte LcdCurrentLineCompare { get; }

        // Timer Register
        byte DividerRegister { get; set; }

        byte ReadByte(ushort address);
        ushort ReadWord(ushort address);
        void WriteByte(ushort address, byte value);
        void WriteWord(ushort address, ushort value);
    }

    public enum Mode
    {
        HBlank,
        VBlank,
        OAM,
        VRAM
    }
}
