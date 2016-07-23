using System.Collections.Generic;
using System.Windows.Media;

namespace GameBoyEm.Api
{
    public interface IGpu
    {
        IList<Color> FrameBuffer { get; }

        void Reset();
        bool Step(ushort cycles);
    }
}
