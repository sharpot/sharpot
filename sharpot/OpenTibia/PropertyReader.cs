using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpOT.OpenTibia
{
    public class PropertyReader : BinaryReader
    {
        public PropertyReader(Stream stream)
            : base(stream) { }

        public string GetString()
        {
            ushort len = ReadUInt16();
            return Encoding.Default.GetString(ReadBytes(len));
        }
    }
}
