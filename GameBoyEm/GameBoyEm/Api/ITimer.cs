namespace GameBoyEm.Api
{
    public interface ITimer
    {
        void Step(ushort cycles);
    }
}
