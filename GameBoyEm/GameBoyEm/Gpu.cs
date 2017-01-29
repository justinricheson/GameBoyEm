﻿using GameBoyEm.Api;
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
        private const byte _maxLcdLine = 153;
        private const ushort _oamTime = 20;
        private const ushort _vramTime = 43;
        private const ushort _hblankTime = 51;
        private const ushort _vblankTime = 114;
        // Only documentation on this I can find says it should be 1140,
        // but it's very sluggish at that speed. Reference implementations show 114
        private const uint _frameTime = 17556; // All times are cycles / 4 (CPU returns quarter cycles)
        private const byte _enableDelay = 61;

        private IMmu _mmu;
        private bool _isEnabled;
        private bool _isLineRendered;
        private uint _clocks;
        private int _delay;
        private int _frameCounter;
        private List<Color> _defaultPalette;
        private List<Color> _bgPalette;
        private List<Color> _spritePalette;

        internal IMmu Mmu { set { _mmu = value; } }

        public IList<Color> FrameBuffer { get; private set; }
        public ushort FrameLimiter { get; set; }

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
            _defaultPalette = ((List<int>)info.GetValue("DefaultPalette", typeof(List<int>)))
                .Select(i => i.FromArgb()).ToList();
            _bgPalette = ((List<int>)info.GetValue("BackgroundPalette", typeof(List<int>)))
                .Select(i => i.FromArgb()).ToList();
            _spritePalette = ((List<int>)info.GetValue("SpritePalette", typeof(List<int>)))
                .Select(i => i.FromArgb()).ToList();
            FrameBuffer = ((List<int>)info.GetValue("FrameBuffer", typeof(List<int>)))
                .Select(i => i.FromArgb()).ToList();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IsEnabled", _isEnabled);
            info.AddValue("IsLineRendered", _isLineRendered);
            info.AddValue("Clocks", _clocks);
            info.AddValue("Delay", _delay);
            info.AddValue("DefaultPalette", _defaultPalette.Select(c => c.ToArgb()).ToList());
            info.AddValue("BackgroundPalette", _bgPalette.Select(c => c.ToArgb()).ToList());
            info.AddValue("SpritePalette", _spritePalette.Select(c => c.ToArgb()).ToList());
            info.AddValue("FrameBuffer", FrameBuffer.Select(c => c.ToArgb()).ToList());
        }

        public void Reset()
        {
            _isEnabled = true;
            _isLineRendered = false;
            _mmu.LcdCurrentLine = _screenHeight;
            _clocks = 0;
            _mmu.LcdMode = Mode.VBlank;
            _delay = 0;
            FrameBuffer = Enumerable.Repeat(Colors.White, _screenPixels).ToList();
        }

        public bool Step(ushort cycles)
        {
            if (_mmu.LcdEnabled)
            {
                if (!_isEnabled && _delay == 0)
                {
                    _delay = _enableDelay;
                }
            }
            else if (_isEnabled)
            {
                Reset();
                _mmu.LcdCurrentLine = 0;
                _mmu.LcdMode = Mode.HBlank;
                _isEnabled = false;
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
                        _clocks = (uint)-_delay;
                        _delay = 0;
                        _mmu.LcdCurrentLine = 0;
                        _mmu.LcdMode = Mode.HBlank;
                        UpdateCoincidenceFlag();
                    }
                }
                else
                {
                    _clocks += cycles;
                    if (_clocks >= _frameTime)
                    {
                        _clocks -= _frameTime;

                        // Don't think this is needed.
                        // Screen is supposed to refresh every _frameTime
                        // clocks but since the ui will hold the old
                        // image indefinitely, there is no need to redraw
                        //return true;
                    }
                }

                return false;
            }

            _clocks += cycles;
            bool draw = false;
            switch (_mmu.LcdMode)
            {
                case Mode.HBlank:
                    if (_clocks >= _hblankTime)
                    {
                        _clocks -= _hblankTime;

                        var currLine = ++_mmu.LcdCurrentLine;
                        if (currLine == _screenHeight)
                        {
                            _mmu.LcdMode = Mode.VBlank;

                            if (_mmu.VBlankStatEnabled)
                            {
                                _mmu.LcdStatInterrupt = true;
                            }

                            _mmu.VblankInterrupt = true;
                        }
                        else
                        {
                            _mmu.LcdMode = Mode.OAM;
                            if (_mmu.OamStatEnabled)
                            {
                                _mmu.LcdStatInterrupt = true;
                            }
                        }
                    }
                    break;
                case Mode.VBlank:
                    if (_clocks >= _vblankTime)
                    {
                        _clocks -= _vblankTime;
                        var currLine = ++_mmu.LcdCurrentLine;
                        if (currLine > _maxLcdLine)
                        {
                            // This is supposed to happen after every hblank
                            // but, doesn't seem to make a difference visually
                            // and this is faster
                            _frameCounter++;
                            if (_frameCounter >= FrameLimiter)
                            {
                                _frameCounter = 0;
                                draw = true;
                            }

                            _mmu.LcdMode = Mode.OAM;
                            _mmu.LcdCurrentLine = 0;

                            if (_mmu.OamStatEnabled)
                            {
                                _mmu.LcdStatInterrupt = true;
                            }
                        }
                    }
                    break;
                case Mode.OAM:
                    if (_clocks >= _oamTime)
                    {
                        _clocks -= _oamTime;
                        _mmu.LcdMode = Mode.VRAM;
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
                    if (_clocks >= _vramTime)
                    {
                        _clocks -= _vramTime;
                        _mmu.LcdMode = Mode.HBlank;

                        if (_mmu.HBlankStatEnabled)
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
            // Don't render unless this frame will be emitted
            if ((_frameCounter + 1) >= FrameLimiter && _mmu.LcdEnabled)
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
            // Note: Performance hotspot
            SetPalette(_bgPalette, _mmu.LcdDefaultPalette);

            var lcdc = _mmu.ReadByte(0xFF40);

            // Second tile set starts at 0x8800, but since the map
            // stores the tile number as a signed byte (-128 - 127)
            // a 0 value corresponds to 0x9000. Using 0x9000 allows
            // the tile number to always be added to the tileSet
            // start address to produce the correct tile address.
            var tileSet = lcdc.AND(0x10) > 0 ? 0x8000 : 0x9000;
            var tileMap = lcdc.AND(0x08) > 0 ? 0x9C00 : 0x9800;

            var scrollY = _mmu.ReadByte(0xFF42);
            var scrollX = _mmu.ReadByte(0xFF43);

            var currLine = _mmu.LcdCurrentLine;
            var yWrap = (scrollY + currLine) % 256;
            var tileY = (byte)(yWrap % 8);
            var tileRow = yWrap / 8;
            var tileMapOffset = tileMap + tileRow * 32;
            var fbOffset = currLine * _screenWidth;

            for (int x = 0; x < _screenWidth; x++)
            {
                var xWrap = (scrollX + x) % 256;
                var tileX = (byte)(xWrap % 8);
                var tileColumn = xWrap / 8;

                var tileNumAddress = (ushort)(tileMapOffset + tileColumn);
                int tileNum = _mmu.ReadByte(tileNumAddress);
                if (tileSet == 0x9000)
                {
                    tileNum = (sbyte)tileNum;
                }

                var tileAddress = (ushort)(tileSet + tileNum * 16);
                var paletteIndex = ReadPaletteIndex(tileAddress, tileX, tileY);
                FrameBuffer[fbOffset + x] = _bgPalette[paletteIndex];
            }
        }

        private void RenderWindow()
        {
            short wndY = _mmu.ReadByte(0xFF4A);
            short wndX = (short)(_mmu.ReadByte(0xFF4B) - 0x7);

            // Check if the window is displayed
            if (wndY < 0 || wndY >= _screenHeight
             || wndX < -7 || wndX >= _screenWidth)
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
                FrameBuffer[f] = _bgPalette[ReadPaletteIndex(
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
                if (xCoord >= _screenWidth || xCoord <= -8
                 || yCoord >= _screenHeight || yCoord <= -(largeSprites ? 16 : 8))
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
                    byte pixel = ReadPaletteIndex(tileAddr, xx, inTileY);
                    if (pixel > 0)
                    {
                        var i = currLine * _screenWidth + xCoord + x;
                        if (priority || (FrameBuffer[i] == _bgPalette[0]))
                        {
                            FrameBuffer[i] = _spritePalette[pixel];
                        }
                    }
                }
            }
        }

        private byte ReadPaletteIndex(ushort address, byte x, byte y)
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
                if (_mmu.CoincidenceStatEnabled)
                {
                    _mmu.LcdStatInterrupt = true;
                }
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
    }
}
