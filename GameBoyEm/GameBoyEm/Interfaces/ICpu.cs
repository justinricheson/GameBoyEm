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
        ushort SP { get; }
        ushort PC { get; }
        bool FZ { get; }
        bool FN { get; }
        bool FH { get; }
        bool FC { get; }
        bool IME { get; }

        void Step();
    }
}
