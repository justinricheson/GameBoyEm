namespace GameBoyEm
{
    public partial class Cpu
    {
        #region Operations
        private void NOP() { M = 1; T = 4; }

        // 8-bit Loads
        private void LDBCA() { WB(BC, _a); M = 2; T = 8; }
        private void LDNB() { _b = RB(_pc++); M = 2; T = 8; }

        // 8-bit Arithmetic
        private void INCB() { _b++; TrySetZ(_b); M = 1; T = 4; }
        private void DECB() { _b--; TrySetZ(_b); M = 1; T = 4; }

        // 16-bit Loads
        private void LDBCNN() { _c = RB(_pc++); _b = RB(_pc++); M = 3; T = 12; }

        // 16-bit Arithmetic
        private void INCBC() { Inc16(ref _b, ref _c); M = 1; T = 4; }
        #endregion

        #region Helpers
        // MMU Helpers
        private byte RB(ushort address)
        {
            return _mmu.ReadByte(address);
        }
        private void WB(ushort address, byte value)
        {
            _mmu.WriteByte(address, value);
        }

        // 16-bit Helpers
        private void Inc16(ref byte high, ref byte low)
        {
            low++;
            if (low == 0)
            {
                high++;
            }
        }

        // Flag Helpers
        private void TrySetZ(byte value, bool clearFlags = true)
        {
            TryClearFlags(clearFlags);
            if (value == 0)
            {
                _f |= 128;
            }
        }
        private void TryClearFlags(bool clearFlags)
        {
            if (clearFlags)
            {
                _f = 0;
            }
        }
        #endregion
    }
}
