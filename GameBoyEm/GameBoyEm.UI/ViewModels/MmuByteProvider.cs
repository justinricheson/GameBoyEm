using Be.Windows.Forms;
using GameBoyEm.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEm.UI.ViewModels
{
    public class MmuByteProvider : IByteProvider
    {
        private bool _hasChanges;
        private IMmu _mmu;

        public long Length { get { return 65536; } }
        public event EventHandler Changed;
        public event EventHandler LengthChanged;

        public MmuByteProvider(IMmu mmu)
        {
            _mmu = mmu;
        }

        public void InvokeChanged() => Changed?.Invoke(this, EventArgs.Empty);
        public bool HasChanges() => _hasChanges;
        public void ApplyChanges() => _hasChanges = false;
        public bool SupportsWriteByte() => false;
        public bool SupportsDeleteBytes() => false;
        public bool SupportsInsertBytes() => false;

        public byte ReadByte(long index)
        {
            return _mmu.ReadByte((ushort)index);
        }
        public void WriteByte(long index, byte value)
        {
            throw new NotImplementedException();
        }
        public void DeleteBytes(long index, long length)
        {
            throw new NotImplementedException();
        }
        public void InsertBytes(long index, byte[] bs)
        {
            throw new NotImplementedException();
        }
    }
}
