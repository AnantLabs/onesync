using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSync.Files
{
    class PartialFileStream : FileStream
    {
        public PartialFileStream(string path, FileMode mode, long
        startPosition, long endPosition)
            : base(path, mode)
        {
            base.Seek(startPosition, SeekOrigin.Begin);
            ReadTillPosition = endPosition;
        }

        public long ReadTillPosition { get; set; }

        public override int Read(byte[] array, int offset, int count)
        {
            if (base.Position >= this.ReadTillPosition)
                return 0;

            if (base.Position + count > this.ReadTillPosition)
                count = (int)(this.ReadTillPosition - base.Position);

            return base.Read(array, offset, count);
        }
    }
}
