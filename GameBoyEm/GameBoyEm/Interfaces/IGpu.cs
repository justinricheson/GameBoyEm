namespace GameBoyEm.Interfaces
{
    public interface IGpu
    {
        void Reset();
        bool Step(ushort cycles);
    }
}
