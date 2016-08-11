using GameBoyEm.Api;
using System;
using System.Windows.Input;

namespace GameBoyEm
{
    public class Controller : IController
    {
        private IMmu _mmu;
        private bool _pressed;
        private bool _aPressed;
        private bool _bPressed;
        private bool _selectPressed;
        private bool _startPressed;
        private bool _upPressed;
        private bool _downPressed;
        private bool _leftPressed;
        private bool _rightPressed;

        public bool APressed
        {
            get { return _aPressed; }
            set
            {
                if (_aPressed != value)
                {
                    _aPressed = value;
                    _pressed = _pressed || _aPressed;
                }
            }
        }
        public bool BPressed
        {
            get { return _bPressed; }
            set
            {
                if (_bPressed != value)
                {
                    _bPressed = value;
                    _pressed = _pressed || _bPressed;
                }
            }
        }
        public bool SelectPressed
        {
            get { return _selectPressed; }
            set
            {
                if (_selectPressed != value)
                {
                    _selectPressed = value;
                    _pressed = _pressed || _selectPressed;
                }
            }
        }
        public bool StartPressed
        {
            get { return _startPressed; }
            set
            {
                if (_startPressed != value)
                {
                    _startPressed = value;
                    _pressed = _pressed || _startPressed;
                }
            }
        }
        public bool UpPressed
        {
            get { return _upPressed; }
            set
            {
                if (_upPressed != value)
                {
                    _upPressed = value;
                    _pressed = _pressed || _upPressed;
                }
            }
        }
        public bool DownPressed
        {
            get { return _downPressed; }
            set
            {
                if (_downPressed != value)
                {
                    _downPressed = value;
                    _pressed = _pressed || _downPressed;
                }
            }
        }
        public bool LeftPressed
        {
            get { return _leftPressed; }
            set
            {
                if (_leftPressed != value)
                {
                    _leftPressed = value;
                    _pressed = _pressed || _leftPressed;
                }
            }
        }
        public bool RightPressed
        {
            get { return _rightPressed; }
            set
            {
                if (_rightPressed != value)
                {
                    _rightPressed = value;
                    _pressed = _pressed || _rightPressed;
                }
            }
        }

        public Controller(IMmu mmu)
        {
            _mmu = mmu;
        }

        public void Step()
        {
            if (_mmu.KeySelector)
            {
                _mmu.StartPressed = StartPressed;
                _mmu.SelectPressed = SelectPressed;
                _mmu.APressed = APressed;
                _mmu.BPressed = BPressed;
            }
            else if (_mmu.DirSelector)
            {
                _mmu.UpPressed = UpPressed;
                _mmu.DownPressed = DownPressed;
                _mmu.LeftPressed = LeftPressed;
                _mmu.RightPressed = RightPressed;
            }

            if (_pressed)
            {
                _pressed = false;
                _mmu.JoyPad = true;
            }
        }
    }
}
