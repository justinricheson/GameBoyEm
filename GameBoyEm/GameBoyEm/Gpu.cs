using GameBoyEm.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace GameBoyEm
{
    [Serializable]
    public class Gpu : IGpu, ISerializable
    {
        private IMmu _mmu;
        private bool _isEnabled;
        private bool _isLineRendered;
        private uint _clocks;
        private uint _currentLine;
        private int _delay;
        private Mode _mode;
        private List<Color> _frameBuffer;
        private List<Color> _defaultPalette;
        private List<Color> _bgPalette;
        private List<Color> _spritePalette;

        internal IMmu Mmu { set { _mmu = value; } }

        public IList<Color> FrameBuffer { get { return _frameBuffer; } }

        public Gpu(IMmu mmu, params Color[] palette)
        {
            _mmu = mmu;

            _defaultPalette = new List<Color>()
            {
                Colors.GhostWhite,
                Colors.LightSlateGray,
                Colors.DarkSlateBlue,
                Colors.Black
            };

            for (int i = 0; i < 4; i++)
            {
                if (i < palette.Length)
                {
                    _defaultPalette[i] = palette[i];
                }
            }

            _bgPalette = _defaultPalette.Take(4).ToList();
            _spritePalette = _defaultPalette.Take(4).ToList();

            Reset();
        }

        protected Gpu(SerializationInfo info, StreamingContext ctx)
        {
            _isEnabled = info.GetBoolean("IsEnabled");
            _isLineRendered = info.GetBoolean("IsLineRendered");
            _clocks = info.GetUInt32("Clocks");
            _currentLine = info.GetUInt32("CurrentLine");
            _delay = info.GetInt32("Delay");
            _mode = (Mode)info.GetValue("Mode", typeof(Mode));
            _frameBuffer = ((List<int>)info.GetValue("FrameBuffer", typeof(List<int>)))
                .Select(i => i.FromArgb()).ToList();
            _defaultPalette = ((List<int>)info.GetValue("DefaultPalette", typeof(List<int>)))
                .Select(i => i.FromArgb()).ToList();
            _bgPalette = ((List<int>)info.GetValue("BackgroundPalette", typeof(List<int>)))
                .Select(i => i.FromArgb()).ToList();
            _spritePalette = ((List<int>)info.GetValue("SpritePalette", typeof(List<int>)))
                .Select(i => i.FromArgb()).ToList();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IsEnabled", _isEnabled);
            info.AddValue("IsLineRendered", _isLineRendered);
            info.AddValue("Clocks", _clocks);
            info.AddValue("CurrentLine", _currentLine);
            info.AddValue("Delay", _delay);
            info.AddValue("Mode", _mode);
            info.AddValue("FrameBuffer", _frameBuffer.Select(c => c.ToArgb()).ToList());
            info.AddValue("DefaultPalette", _defaultPalette.Select(c => c.ToArgb()).ToList());
            info.AddValue("BackgroundPalette", _bgPalette.Select(c => c.ToArgb()).ToList());
            info.AddValue("SpritePalette", _spritePalette.Select(c => c.ToArgb()).ToList());
        }

        public void Reset()
        {
            _isEnabled = true;
            _isLineRendered = false;
            SetLine(144);
            _clocks = 0;
            ChangeMode(Mode.VBlank);
            _delay = 0;
            _frameBuffer = Enumerable.Repeat(Colors.White, 160 * 144).ToList();
        }

        public bool Step(ushort cycles)
        {
            if (_mmu.ReadByte(0xFF40).AND(0x80) > 0) // LCD enabled
            {
                if (!_isEnabled && _delay == 0)
                {
                    Enable();
                }
            }
            else if (_isEnabled)
            {
                Disable();
            }

            if (!_isEnabled)
            {
                if (_delay > 0)
                {
                    _delay -= cycles;
                    if (_delay <= 0)
                    {
                        Reset();
                        _isEnabled = true;
                        _clocks = (uint)(-_delay);
                        _delay = 0;
                        SetLine(0);
                        ChangeMode(Mode.HBlank);
                        UpdateCoincidenceFlag();
                    }
                }
                else
                {
                    _clocks += cycles;
                    if (_clocks >= 70224)
                    {
                        _clocks -= 70224;
                        return true;
                    }
                }

                return false;
            }

            bool draw = false;
            _clocks += cycles;
            switch (_mode)
            {
                case Mode.HBlank:
                    if (_clocks >= 51)
                    {
                        _clocks -= 51;
                        SetLine(_currentLine + 1);

                        if (_currentLine == 144)
                        {
                            ChangeMode(Mode.VBlank);
                            draw = true;

                            if (_mmu.LcdcVblank)
                            {
                                _mmu.LcdStatInterrupt = true;
                            }

                            _mmu.VblankInterrupt = true;
                        }
                        else
                        {
                            ChangeMode(Mode.OAM);
                            if (_mmu.LcdcOam)
                            {
                                _mmu.LcdStatInterrupt = true;
                            }
                        }
                    }
                    break;
                case Mode.VBlank:
                    if (_clocks >= 114)
                    {
                        _clocks -= 114;
                        SetLine(_currentLine + 1);
                        if (_currentLine > 153)
                        {
                            ChangeMode(Mode.OAM);
                            SetLine(0);

                            if (_mmu.LcdcOam)
                            {
                                _mmu.LcdStatInterrupt = true;
                            }
                        }
                    }
                    break;
                case Mode.OAM:
                    if (_clocks >= 20)
                    {
                        _clocks -= 20;
                        ChangeMode(Mode.VRAM);
                        _isLineRendered = false;
                    }
                    break;
                case Mode.VRAM:
                    if (!_isLineRendered && (_clocks >= (_currentLine > 0 ? 12 : 40)))
                    {
                        _isLineRendered = true;
                        RenderScanLine();
                    }
                    if (_clocks >= 43)
                    {
                        _clocks -= 43;
                        ChangeMode(Mode.HBlank);

                        if (_mmu.LcdcHblank)
                        {
                            _mmu.LcdStatInterrupt = true;
                        }
                    }
                    break;
            }

            UpdateCoincidenceFlag();

            return draw;
        }

        private void RenderScanLine()
        {
            if (_mmu.LcdEnabled)
            {
                if (_mmu.DisplayBackground)
                {
                    RenderBackground();
                }
                if (_mmu.DisplayWindow)
                {
                    RenderWindow();
                }
                if (_mmu.DisplaySprites)
                {
                    RenderSprites();
                }
            }
        }

        private void RenderBackground()
        {
            var palette = _mmu.ReadByte(0xFF47);
            for (int i = 0; i < 4; ++i) // Reset palette
            {
                _bgPalette[i] = _defaultPalette[(palette >> (i * 2)) & 0x3];
            }

            var lcdc = _mmu.ReadByte(0xFF40);
            var tileSet = (lcdc.AND(0x10)) > 0 ? 0x8000 : 0x9000;
            var tileMap = (lcdc.AND(0x08)) > 0 ? 0x9C00 : 0x9800;

            var scrollX = _mmu.ReadByte(0xFF43);
            var scrollY = _mmu.ReadByte(0xFF42);

            int num;
            var tileRow = ((scrollY + _currentLine) % 256) / 8;
            var inTileY = ((scrollY + _currentLine) % 256) % 8;
            for (int x = 0; x < 160; ++x)
            {
                var tileColumn = ((scrollX + x) % 256) / 8;
                var inTileX = ((scrollX + x) % 256) % 8;
                var tileNumLocation = (ushort)(tileMap + tileRow * 32 + tileColumn);

                if (tileSet == 0x8000)
                {
                    num = _mmu.ReadByte(tileNumLocation);
                }
                else
                {
                    num = (sbyte)(_mmu.ReadByte(tileNumLocation));
                }

                var f = (int)(_currentLine * 160 + x);
                _frameBuffer[f] = _bgPalette[ReadTile((ushort)(tileSet + num * 16), (byte)inTileX, (byte)inTileY)];
            }
        }

        private void RenderWindow()
        {
            short wndY = _mmu.ReadByte(0xFF4A);
            short wndX = (short)(_mmu.ReadByte(0xFF4B) - 0x7);

            // Check if the window is displayed
            if (wndY < 0
             || wndY >= 144
             || wndX < -7
             || wndX >= 160)
            {
                return;
            }

            // Check if the window is on the current line
            if (wndY > _currentLine)
            {
                return;
            }

            byte palette = _mmu.ReadByte(0xFF47);
            for (int i = 0; i < 4; ++i)
            {
                _bgPalette[i] = _defaultPalette[(palette >> (i * 2)) & 0x3];
            }

            var lcdc = _mmu.ReadByte(0xFF40);
            ushort tileSet = (lcdc & 0x10) > 0 ? (ushort)0x8000 : (ushort)0x9000;
            ushort tileMap = (lcdc & 0x40) > 0 ? (ushort)0x9C00 : (ushort)0x9800;

            int num;
            var tileRow = (ushort)((_currentLine - wndY) / 8);
            var inTileY = (ushort)(_currentLine - wndY) % 8;

            for (int x = (wndX < 0) ? 0 : wndX; x < 160; ++x)
            {
                var tileColumn = (ushort)((x - wndX) / 8);
                var inTileX = (ushort)((x - wndX) % 8);

                var tileNumLocation = (ushort)(tileMap + tileRow * 32 + tileColumn);

                if (tileSet == 0x8000)
                {
                    num = _mmu.ReadByte(tileNumLocation);
                }
                else
                {
                    num = (sbyte)_mmu.ReadByte(tileNumLocation);
                }

                var f = (int)(_currentLine * 160 + x);
                _frameBuffer[f] = _bgPalette[ReadTile((ushort)(tileSet + num * 16), (byte)inTileX, (byte)inTileY)];
            }
        }

        private void RenderSprites()
        {
            bool largeSprites = _mmu.ReadByte(0xFF40).AND(0x4) > 0;

            for (int h = 39; h >= 0; --h) // Respect sprite priority
            {
                var currentOAMAddr = (ushort)(0xFE00 + 0x4 * h);

                // Top left Corner
                // Byte 0 - Y Position - Vertical position on the screen (minus 16).
                var yCoord = (short)(_mmu.ReadByte(currentOAMAddr) - 0x10);
                // Byte 1 - X Position - Horizontal position on the screen (minus 8)
                var xCoord = (short)(_mmu.ReadByte((ushort)(currentOAMAddr + 1)) - 0x8);

                // Check if the sprite is not on the screen
                if (xCoord >= 160
                 || xCoord <= -8
                 || yCoord >= 144
                 || yCoord <= -(largeSprites ? 16 : 8))
                {
                    continue;
                }

                var inTileY = (byte)(_currentLine - yCoord);

                // Check if it is not on the current line
                if (yCoord > (short)_currentLine || inTileY >= (largeSprites ? 16 : 8))
                {
                    continue;
                }

                var optionsByte = _mmu.ReadByte((ushort)(currentOAMAddr + 3));
                bool priority = (optionsByte & 0x80) == 0;
                bool yFlip = (optionsByte & 0x40) != 0;
                bool xFlip = (optionsByte & 0x20) != 0;
                byte palette = _mmu.ReadByte(optionsByte.AND(0x10) > 0 ? (ushort)0xFF49 : (ushort)0xFF48);

                for (int i = 0; i < 4; ++i)
                {
                    _spritePalette[i] = _defaultPalette[(palette >> (i * 2)) & 0x3];
                }

                var spriteNum = _mmu.ReadByte((ushort)(currentOAMAddr + 2));

                ushort tileAddr;
                if (largeSprites)
                {
                    if (yFlip)
                    {
                        inTileY = (byte)(15 - inTileY);
                    }
                    if (inTileY < 8)
                    {
                        tileAddr = (ushort)(0x8000 + 0x10 * (spriteNum & 0xFE));
                    }
                    else
                    {
                        tileAddr = (ushort)(0x8000 + 0x10 * (spriteNum | 0x01));
                        inTileY -= 8;
                    }
                }
                else
                {
                    if (yFlip)
                    {
                        inTileY = (byte)(7 - inTileY);
                    }

                    tileAddr = (ushort)(0x8000 + 0x10 * spriteNum);
                }

                for (int x = 0; x < 8; ++x)
                {
                    if (xCoord + x < 0 || xCoord + x >= 160)
                    {
                        continue;
                    }

                    byte pixel = ReadTile(tileAddr, (byte)(xFlip ? (7 - x) : x), inTileY);
                    if (pixel > 0 && (priority || (_frameBuffer[(int)(_currentLine * 160 + xCoord + x)] == _bgPalette[0])))
                    {
                        _frameBuffer[(int)(_currentLine * 160 + xCoord + x)] = _spritePalette[pixel];
                    }
                }
            }
        }

        private byte ReadTile(ushort address, byte x, byte y)
        {
            var lo = _mmu.ReadByte((ushort)(address + (y * 2)));
            var hi = _mmu.ReadByte((ushort)(address + (y * 2) + 1));
            var lsb = lo.RS(7 - x).AND(0x1);
            var msb = hi.RS(7 - x).AND(0x1);
            return msb.LS().OR(lsb);
        }

        private void UpdateCoincidenceFlag()
        {
            var status = _mmu.ReadByte(0xFF41);
            var ly = _mmu.ReadByte(0xFF44);
            var lyc = _mmu.ReadByte(0xFF45);
            if (ly == lyc)
            {
                _mmu.WriteByte(0xFF41, status.OR(0x04));
                if (status.AND(0x40) > 0) // Coincidence interrupt enabled
                {
                    _mmu.WriteByte(0xFF0F, _mmu.ReadByte(0xFF0F).OR(0x02));
                }
            }
            else
            {
                _mmu.WriteByte(0xFF41, status.AND(0xFB));
            }
        }

        private void ChangeMode(Mode mode)
        {
            _mode = mode;
            var status = _mmu.ReadByte(0xFF41).AND(0xFC);
            status = status.OR((byte)mode);
            _mmu.WriteByte(0xFF41, status);
        }

        private void Enable()
        {
            _delay = 61;
        }

        private void Disable()
        {
            Reset();
            SetLine(0);
            ChangeMode(Mode.HBlank);
            _isEnabled = false;
            ClearFrameBuffer();
        }

        private void ClearFrameBuffer()
        {
            for (int i = 0; i < 160 * 144; ++i)
            {
                _frameBuffer[i] = Colors.White;
            }
        }

        private void SetLine(uint lineNum)
        {
            _currentLine = lineNum;
            _mmu.WriteByte(0xFF44, (byte)lineNum);
        }

        private enum Mode
        {
            HBlank,
            VBlank,
            OAM,
            VRAM
        }
    }
}
