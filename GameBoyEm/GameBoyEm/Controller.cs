using GameBoyEm.Api;
using System;
using System.Windows.Input;

namespace GameBoyEm
{
    public class Controller : IController
    {
        private IMmu _mmu;

        internal IMmu Mmu { set { _mmu = value; } }

        public bool APressed { get; set; }
        public bool BPressed { get; set; }
        public bool SelectPressed { get; set; }
        public bool StartPressed { get; set; }
        public bool UpPressed { get; set; }
        public bool DownPressed { get; set; }
        public bool LeftPressed { get; set; }
        public bool RightPressed { get; set; }

        public Controller(IMmu mmu)
        {
            _mmu = mmu;
        }

        public void Step()
        {
            if (_mmu.KeySelector)
            {
                if (StartPressed && !_mmu.StartPressed
                 || SelectPressed && !_mmu.SelectPressed
                 || APressed && !_mmu.APressed
                 || BPressed && !_mmu.BPressed)
                {
                    _mmu.JoyPad = true;
                }

                _mmu.StartPressed = StartPressed;
                _mmu.SelectPressed = SelectPressed;
                _mmu.APressed = APressed;
                _mmu.BPressed = BPressed;
            }
            else if (_mmu.DirSelector)
            {
                if (UpPressed && !_mmu.UpPressed
                 || DownPressed && !_mmu.DownPressed
                 || LeftPressed && !_mmu.LeftPressed
                 || RightPressed && !_mmu.RightPressed)
                {
                    _mmu.JoyPad = true;
                }

                _mmu.UpPressed = UpPressed;
                _mmu.DownPressed = DownPressed;
                _mmu.LeftPressed = LeftPressed;
                _mmu.RightPressed = RightPressed;
            }
        }
    }
}
