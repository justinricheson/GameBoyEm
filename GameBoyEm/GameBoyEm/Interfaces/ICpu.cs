namespace GameBoyEm.Interfaces
{
    public interface ICpu
    {
        byte A { get; }
        byte B { get; }
        byte C { get; }
        byte D { get; }
        byte E { get; }
        byte H { get; }
        byte L { get; }
        byte F { get; }
        byte SP { get; }
        ushort PC { get; }
        ushort BC { get; }
        ushort DE { get; }
        ushort HL { get; }

        void Step();
    }
}
