namespace GameBoyEm.Api
{
    public interface ICartridge
    {
        byte Read(ushort address);
        void Write(ushort address, byte value);
        byte ReadRam(ushort address);
        void WriteRam(ushort address, byte value);
    }
}
