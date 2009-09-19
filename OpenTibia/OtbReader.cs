using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpOT.OpenTibia
{
    public class OtbReader
    {
        private string fileName;
        FileStream stream;
        byte[] buffer = new byte[128];

        public OtbReader(string fileName)
        {
            this.fileName = fileName;
        }
        
        public IEnumerable<KeyValuePair<ushort, ushort>> GetServerToSpriteIdPairs()
        {
            stream = File.OpenRead(fileName);

            KeyValuePair<ushort, ushort>? kvp = null;
            bool unparseNext = false;
            int cur;
            while ((cur = stream.ReadByte()) != -1)
            {
                switch (cur)
                {
                    case Constants.NodeStart:
                        if (unparseNext)
                        {
                            unparseNext = false;
                        }
                        else
                        {
                            int type = stream.ReadByte();
                            if (type >=0 && type <= 13)
                                kvp = HandleItem();
                            if (kvp != null)
                                yield return kvp.Value;
                            break;
                        }
                        break;
                    case Constants.NodeEnd:
                        if (unparseNext)
                        {
                            unparseNext = false;
                        }
                        else
                        {

                        }
                        break;
                    case Constants.Escape:
                        unparseNext = true;
                        break;
                }
            }
        }

        private KeyValuePair<ushort, ushort>? HandleItem()
        {
            ushort serverId = 0;
            ushort clientId = 0;
            // skip 4 flag bytes
            ReadAndUnescape(4);

            byte attr = ReadAndUnescape(1)[0];
            ushort len = BitConverter.ToUInt16(ReadAndUnescape(2), 0);

            if (attr == 0x10)
            {
                serverId = BitConverter.ToUInt16(ReadAndUnescape(2), 0);
            }
            attr = ReadAndUnescape(1)[0];
            if (attr == 0x11)
            {
                len = BitConverter.ToUInt16(ReadAndUnescape(2), 0);
                clientId = BitConverter.ToUInt16(ReadAndUnescape(2), 0);
            }

            if (clientId > 0 )
            {
                return new KeyValuePair<ushort,ushort>(serverId, clientId);
            }
            return null;
        }

        private byte[] ReadAndUnescape(int count)
        {
            byte[] buffer = new byte[count];
            for (int i = 0; i < count; i++)
            {
                // read the server and client ids
                byte tmp = (byte)stream.ReadByte();

                if (tmp == Constants.Escape)
                {
                    buffer[i] = (byte)stream.ReadByte();
                }
                else
                {
                    buffer[i] = tmp;
                }
            }
            return buffer;
        }
    }
}
