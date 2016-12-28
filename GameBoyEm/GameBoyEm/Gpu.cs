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
        private const byte _screenWidth = 160;
        private const byte _screenHeight = 144;
        private const ushort _screenPixels = _screenWidth * _screenHeight;

        private IMmu _mmu;
        private bool _isEnabled;
        private bool _isLineRendered;
        private uint _clocks;
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
            _mmu.LcdCurrentLine = _screenHeight;
            _clocks = 0;
            ChangeMode(Mode.VBlank);
            _delay = 0;
            _frameBuffer = Enumerable.Repeat(Colors.White, _screenPixels).ToList();
        }

        public bool Step(ushort cycles)
        {
            if (_mmu.LcdEnabled)
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
                        _mmu.LcdCurrentLine = 0;
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

                        // Don't think we need this.
                        // Screen is supposed to refresh every 70244
                        // clocks but since the ui will hold the old
                        // image indefinitely. there is no need to redraw
                        //return true;
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

                        var currLine = ++_mmu.LcdCurrentLine;
                        if (currLine == _screenHeight)
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
                        var currLine = ++_mmu.LcdCurrentLine;
                        if (currLine > 153)
                        {
                            ChangeMode(Mode.OAM);
                            _mmu.LcdCurrentLine = 0;

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
                    if (!_isLineRendered && (_clocks >=
                        (_mmu.LcdCurrentLine > 0 ? 12 : 40)))
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
            SetPalette(_bgPalette, _mmu.LcdDefaultPalette);

            var lcdc = _mmu.ReadByte(0xFF40);
            var tileSet = (lcdc.AND(0x10)) > 0 ? 0x8000 : 0x9000;
            var tileMap = (lcdc.AND(0x08)) > 0 ? 0x9C00 : 0x9800;

            var scrollX = _mmu.ReadByte(0xFF43);
            var scrollY = _mmu.ReadByte(0xFF42);

            var currLine = _mmu.LcdCurrentLine;

            int num;
            var tileRow = ((scrollY + currLine) % 256) / 8;
            var inTileY = ((scrollY + currLine) % 256) % 8;
            for (int x = 0; x < _screenWidth; x++)
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

                var f = currLine * _screenWidth + x;
                _frameBuffer[f] = _bgPalette[ReadTile(
                    (ushort)(tileSet + num * 16),
                    (byte)inTileX, (byte)inTileY)];
            }
        }

        private void RenderWindow()
        {
            short wndY = _mmu.ReadByte(0xFF4A);
            short wndX = (short)(_mmu.ReadByte(0xFF4B) - 0x7);

            // Check if the window is displayed
            if (wndY < 0
             || wndY >= _screenHeight
             || wndX < -7
             || wndX >= _screenWidth)
            {
                return;
            }

            // Check if the window is on the current line
            var currLine = _mmu.LcdCurrentLine;
            if (wndY > currLine)
            {
                return;
            }

            SetPalette(_bgPalette, _mmu.LcdDefaultPalette);

            var lcdc = _mmu.ReadByte(0xFF40);
            ushort tileSet = (lcdc & 0x10) > 0 ? (ushort)0x8000 : (ushort)0x9000;
            ushort tileMap = (lcdc & 0x40) > 0 ? (ushort)0x9C00 : (ushort)0x9800;

            int num;
            var tileRow = (ushort)((currLine - wndY) / 8);
            var inTileY = (ushort)((currLine - wndY) % 8);

            for (int x = (wndX < 0) ? 0 : wndX; x < _screenWidth; ++x)
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

                var f = currLine * _screenWidth + x;
                _frameBuffer[f] = _bgPalette[ReadTile(
                    (ushort)(tileSet + num * 16),
                    (byte)inTileX, (byte)inTileY)];
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
                if (xCoord >= _screenWidth
                 || xCoord <= -8
                 || yCoord >= _screenHeight
                 || yCoord <= -(largeSprites ? 16 : 8))
                {
                    continue;
                }

                var currLine = _mmu.LcdCurrentLine;
                var inTileY = (byte)(currLine - yCoord);

                // Check if it is not on the current line
                if (yCoord > currLine || inTileY >= (largeSprites ? 16 : 8))
                {
                    continue;
                }

                var optionsByte = _mmu.ReadByte((ushort)(currentOAMAddr + 3));
                bool priority = (optionsByte & 0x80) == 0;
                bool yFlip = (optionsByte & 0x40) != 0;
                bool xFlip = (optionsByte & 0x20) != 0;
                byte palette = _mmu.ReadByte(optionsByte.AND(0x10) > 0
                    ? (ushort)0xFF49 : (ushort)0xFF48);
                SetPalette(_spritePalette, palette);

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
                    if (xCoord + x < 0 || xCoord + x >= _screenWidth)
                    {
                        continue;
                    }

                    byte xx = (byte)(xFlip ? (7 - x) : x);
                    byte pixel = ReadTile(tileAddr, xx, inTileY);
                    if (pixel > 0)
                    {
                        var i = currLine * _screenWidth + xCoord + x;
                        if (priority || (_frameBuffer[i] == _bgPalette[0]))
                        {
                            _frameBuffer[i] = _spritePalette[pixel];
                        }
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
            var currLine = _mmu.LcdCurrentLine;
            var currLineCompare = _mmu.LcdCurrentLineCompare;
            _mmu.CoincidenceFlag = currLine == currLineCompare;

            if (currLine == currLineCompare)
            {
                if (_mmu.Coincidence)
                {
                    _mmu.LcdStatInterrupt = true;
                }
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
            _mmu.LcdCurrentLine = 0;
            ChangeMode(Mode.HBlank);
            _isEnabled = false;
            ClearFrameBuffer();
        }

        private void ClearFrameBuffer()
        {
            for (int i = 0; i < _frameBuffer.Count; i++)
            {
                _frameBuffer[i] = Colors.White;
            }
        }

        private void SetPalette(List<Color> target, byte palette)
        {
            // palette is a byte where each 2 bits
            // is an index into the default palette
            target[0] = _defaultPalette[palette & 0x3]; palette >>= 2;
            target[1] = _defaultPalette[palette & 0x3]; palette >>= 2;
            target[2] = _defaultPalette[palette & 0x3]; palette >>= 2;
            target[3] = _defaultPalette[palette & 0x3];
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
