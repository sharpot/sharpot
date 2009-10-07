using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpOT.Util;

namespace SharpOT.OpenTibia
{
    public class OtbmReader
    {
        private string fileName;
        private string lastError;
        byte[] buffer = new byte[128];

        public OtbmReader(string fileName)
        {
            this.fileName = fileName;
        }

        public bool GetMapTiles(Map map)
        {
            FileLoader loader = new FileLoader();
            loader.OpenFile(fileName);
            Node node = loader.GetRootNode();

            PropertyReader props;

            if (!loader.GetProps(node, out props))
            {
                lastError = "Could not read root property.";
                return false;
            }

            uint version = props.ReadUInt32();
            ushort width = props.ReadUInt16();
            ushort height = props.ReadUInt16();
            uint majorVersionItems = props.ReadUInt32();
            uint minorVersionItems = props.ReadUInt32();

            node = node.Child;

            if ((OtbmNodeType)node.Type != OtbmNodeType.OTBM_MAP_DATA)
            {
                lastError = "Could not read data node.";
                return false;
            }

            if (!loader.GetProps(node, out props))
            {
                lastError = "Could not read map data attributes.";
                return false;
            }

            byte attribute;
            while (props.PeekChar() != -1)
            {
                attribute = props.ReadByte();
                switch ((OtbmAttribute)attribute)
                {
                    case OtbmAttribute.OTBM_ATTR_DESCRIPTION:
                        break;
                }
            }

            return true;
        }
    }
}
