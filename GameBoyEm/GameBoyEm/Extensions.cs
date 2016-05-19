namespace GameBoyEm
{
    public static class ByteExtensions
    {
        // These just hide the cast, we'll have to measure to see
        // if the extra function call affects performance too much
        public static byte RS(this byte value, int places = 1)
        {
            return (byte)(value >> places);
        }
        public static byte RS(this bool value, int places = 1)
        {
            return ((byte)(value ? 1 : 0)).RS(places);
        }
        public static byte LS(this byte value, int places = 1)
        {
            return (byte)(value << places);
        }
        public static byte LS(this bool value, int places = 1)
        {
            return ((byte)(value ? 1 : 0)).LS(places);
        }
        public static byte OR(this byte first, byte second)
        {
            return (byte)(first | second);
        }
        public static byte OR(this byte first, bool second)
        {
            return (byte)(first | (second ? 1 : 0));
        }
        public static byte AND(this byte first, byte second)
        {
            return (byte)(first & second);
        }
        public static byte XOR(this byte first, byte second)
        {
            return (byte)(first ^ second);
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
        public static ushort OR(this ushort first, bool second)
        {
            return (ushort)(first | (second ? 1 : 0));
        }
        public static ushort AND(this ushort first, ushort second)
        {
            return (ushort)(first & second);
        }
        public static ushort XOR(this ushort first, ushort second)
        {
            return (ushort)(first ^ second);
        }
    }
}
