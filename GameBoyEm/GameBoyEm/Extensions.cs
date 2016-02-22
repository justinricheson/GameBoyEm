namespace GameBoyEm
{
    public static class ByteExtensions
    {
        public static byte RS(this byte value, int places = 1)
        {
            return (byte)(value >> places);
        }
        public static byte LS(this byte value, int places = 1)
        {
            return (byte)(value << places);
        }
        public static byte OR(this byte first, byte second)
        {
            return (byte)(first | second);
        }
    }

    public static class UShortExtensions
    {
        public static ushort RS(this ushort value, int places = 1)
        {
            return (ushort)(value >> places);
        }
        public static ushort LS(this ushort value, int places = 1)
        {
            return (ushort)(value << places);
        }
        public static ushort OR(this ushort first, ushort second)
        {
            return (ushort)(first | second);
        }
    }
}
