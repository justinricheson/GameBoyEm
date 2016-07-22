namespace GameBoyEm.Api
{
    public interface IGpu
    {
        byte[] FrameBuffer { get; }

        void Reset();
        bool Step(ushort cycles);
    }
}
