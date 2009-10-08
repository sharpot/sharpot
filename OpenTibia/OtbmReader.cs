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
                        map.Description = props.GetString();
                        break;
                    case OtbmAttribute.OTBM_ATTR_EXT_SPAWN_FILE:
                        map.SpawnFile = props.GetString();
                        break;
                    case OtbmAttribute.OTBM_ATTR_EXT_HOUSE_FILE:
                        map.HouseFile = props.GetString();
                        break;
                    default:
                        lastError = "Unknown header node.";
                        return false;
                }
            }

            node = node.Child;

            while (node != null)
            {
                switch ((OtbmNodeType)node.Type)
                {
                    case OtbmNodeType.OTBM_TILE_AREA:
                        if (!ParseTileArea(map, loader, node)) return false;
                        break;
                }
            }

            return true;
        }

        private bool ParseTileArea(Map map, FileLoader loader, Node node)
        {
            PropertyReader props;
            if (!loader.GetProps(node, out props))
            {
                lastError = "Invalid map node.";
                return false;
            }

            int baseX = props.ReadUInt16();
            int baseY = props.ReadUInt16();
            int baseZ = props.ReadByte();

            node = node.Child;

            while (node != null)
            {
                if (node.Type == (long)OtbmNodeType.OTBM_TILE ||
                    node.Type == (long)OtbmNodeType.OTBM_HOUSETILE)
                {
                    int tileX = baseX + props.ReadByte();
                    int tileY = baseY + props.ReadByte();
                    int tileZ = baseZ;

                    Tile tile = new Tile();
                    tile.Location = new Location(tileX, tileY, tileZ);

                    bool isHouseTile = false;

                    if (node.Type == (long)OtbmNodeType.OTBM_HOUSETILE)
                    {
                        uint houseId = props.ReadUInt32();
                        isHouseTile = true;
                    }

                    byte attribute;
                    while (props.PeekChar() != -1)
                    {
                        attribute = props.ReadByte();
                        switch ((OtbmAttribute)attribute)
                        {
                            case OtbmAttribute.OTBM_ATTR_TILE_FLAGS:
                            {
                                TileFlags flags = (TileFlags)props.ReadUInt32();
                                if ((flags & TileFlags.TILESTATE_PROTECTIONZONE) == TileFlags.TILESTATE_PROTECTIONZONE)
                                {
                                    tile.IsProtectionZone = true;
                                }
                                else if ((flags & TileFlags.TILESTATE_NOPVPZONE) == TileFlags.TILESTATE_NOPVPZONE)
                                {
                                    tile.IsNoPvpZone = true;
                                }
                                else if ((flags & TileFlags.TILESTATE_PVPZONE) == TileFlags.TILESTATE_PVPZONE)
                                {
                                    tile.IsPvpZone = true;
                                }

                                if ((flags & TileFlags.TILESTATE_NOLOGOUT) == TileFlags.TILESTATE_NOLOGOUT)
                                {
                                    tile.IsNoLogoutZone = true;
                                }

                                if ((flags & TileFlags.TILESTATE_REFRESH) == TileFlags.TILESTATE_REFRESH)
                                {
                                    // TODO: Warn about house
                                    tile.IsRefreshZone = true;
                                }
                                break;
                            }
                            case OtbmAttribute.OTBM_ATTR_ITEM:
                            {
                                ushort itemId = props.ReadUInt16();
                                Item item = new Item(itemId);

                                // TODO: if isHouseTile && !item.Info.IsMoveable

                                if (item.Info.Group == ItemGroup.Ground)
                                {
                                    tile.Ground = item;
                                }
                                else
                                {
                                    tile.Items.Add(item);
                                }
                                break;
                            }
                            default:
                                lastError = tile.Location + " Unknown tile attribute.";
                                return false;
                        }
                    }

                    Node nodeItem = node.Child;

                    while (nodeItem != null)
                    {
                        if (nodeItem.Type == (long)OtbmNodeType.OTBM_ITEM)
                        {
                            loader.GetProps(nodeItem, out props);
                            ushort itemId = props.ReadUInt16();

                            // TODO: subclass item, different deserializations
                            // for different types
                            Item item = new Item(itemId);

                        }
                        else
                        {
                            lastError += tile.Location + " Unknown node type.";
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
