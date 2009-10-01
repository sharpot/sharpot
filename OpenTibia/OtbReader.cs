using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpOT.Util;

namespace SharpOT.OpenTibia
{
    public class OtbReader
    {
        private string fileName;
        byte[] buffer = new byte[128];

        public OtbReader(string fileName)
        {
            this.fileName = fileName;
        }

        public IEnumerable<ItemInfo> GetAllItemInfo()
        {
            FileLoader loader = new FileLoader();
            loader.OpenFile(fileName);
            Node node = loader.GetRootNode();

            BinaryReader props;

            if (loader.GetProps(node, out props))
            {
                // 4 byte flags
                // attributes
                // 0x01 = version data
                uint flags = props.ReadUInt32();
                byte attr = props.ReadByte();
                if (attr == 0x01)
                {
                    ushort datalen = props.ReadUInt16();
                    if (datalen != 140)
                    {
                        yield return null;
                        yield break;
                    }
                    uint majorVersion = props.ReadUInt32();
                    uint minorVersion = props.ReadUInt32();
                    uint buildNumber = props.ReadUInt32();
                }
            }

            node = node.Child;

            while (node != null)
            {
                if (!loader.GetProps(node, out props))
                {
                    yield return null;
                    yield break;
                }

                ItemInfo info = new ItemInfo();
                // info.Group = node.Type;

                ItemFlags flags = (ItemFlags)props.ReadUInt32();
                info.IsBlocking = flags.HasFlag(ItemFlags.FLAG_BLOCK_SOLID);

                // process flags

                byte attr;
                ushort datalen;
                while (props.PeekChar() != -1)
                {
                    attr = props.ReadByte();
                    datalen = props.ReadUInt16();
                    switch ((ItemAttribute)attr)
                    {
                        case ItemAttribute.ServerId:
                            info.Id = props.ReadUInt16();
                            break;
                        case ItemAttribute.ClientId:
                            info.SpriteId = props.ReadUInt16();
                            break;
                        case ItemAttribute.Speed:
                            info.Speed = props.ReadUInt16();
                            break;
                        case ItemAttribute.TopOrder:
                            info.TopOrder = props.ReadByte();
                            break;
                        default:
                            props.ReadBytes(datalen);
                            break;
                    }
                }
                yield return info;
                node = node.Next;
            }
        }
    }
}
