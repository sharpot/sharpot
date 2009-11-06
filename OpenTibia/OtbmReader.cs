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

            if ((OtbmNodeType)node.Type != OtbmNodeType.MapData)
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
                    case OtbmAttribute.Description:
                        map.Description = props.GetString();
                        break;
                    case OtbmAttribute.ExtSpawnFile:
                        map.SpawnFile = props.GetString();
                        break;
                    case OtbmAttribute.ExtHouseFile:
                        map.HouseFile = props.GetString();
                        break;
                    default:
                        lastError = "Unknown header node.";
                        return false;
                }
            }

            Node nodeMapData = node.Child;

            while (nodeMapData != null)
            {
                switch ((OtbmNodeType)nodeMapData.Type)
                {
                    case OtbmNodeType.TileArea:
                        if (!ParseTileArea(map, loader, nodeMapData)) return false;
                        break;
                    case OtbmNodeType.Towns:
                        if (!ParseTowns(map, loader, nodeMapData)) return false;
                        break;
                }
                nodeMapData = nodeMapData.Next;
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

            Node nodeTile = node.Child;

            while (nodeTile != null)
            {
                if (nodeTile.Type == (long)OtbmNodeType.Tile ||
                    nodeTile.Type == (long)OtbmNodeType.HouseTile)
                {
                    loader.GetProps(nodeTile, out props);

                    int tileX = baseX + props.ReadByte();
                    int tileY = baseY + props.ReadByte();
                    int tileZ = baseZ;

                    Tile tile = new Tile();
                    tile.Location = new Location(tileX, tileY, tileZ);

                    // TODO: houses
                    if (nodeTile.Type == (long)OtbmNodeType.HouseTile)
                    {
                        uint houseId = props.ReadUInt32();
                    }

                    byte attribute;
                    while (props.PeekChar() != -1)
                    {
                        attribute = props.ReadByte();
                        switch ((OtbmAttribute)attribute)
                        {
                            case OtbmAttribute.TileFlags:
                            {
                                TileFlags flags = (TileFlags)props.ReadUInt32();
                                if ((flags & TileFlags.ProtectionZone) == TileFlags.ProtectionZone)
                                {
                                    tile.IsProtectionZone = true;
                                }
                                else if ((flags & TileFlags.NoPvpZone) == TileFlags.NoPvpZone)
                                {
                                    tile.IsNoPvpZone = true;
                                }
                                else if ((flags & TileFlags.PvpZone) == TileFlags.PvpZone)
                                {
                                    tile.IsPvpZone = true;
                                }

                                if ((flags & TileFlags.NoLogout) == TileFlags.NoLogout)
                                {
                                    tile.IsNoLogoutZone = true;
                                }

                                if ((flags & TileFlags.Refresh) == TileFlags.Refresh)
                                {
                                    // TODO: Warn about house
                                    tile.IsRefreshZone = true;
                                }
                                break;
                            }
                            case OtbmAttribute.Item:
                            {
                                ushort itemId = props.ReadUInt16();
                                Item item = Item.Create(itemId);

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

                    Node nodeItem = nodeTile.Child;

                    while (nodeItem != null)
                    {
                        if (nodeItem.Type == (long)OtbmNodeType.Item)
                        {
                            loader.GetProps(nodeItem, out props);
                            ushort itemId = props.ReadUInt16();

                            // TODO: subclass item, different deserializations
                            // for different types
                            Item item = Item.Create(itemId);
                            if (item.Info.Group == ItemGroup.Ground)
                            {
                                tile.Ground = item;
                            }
                            else
                            {
                                tile.Items.Add(item);
                            }
                        }
                        else
                        {
                            lastError += tile.Location + " Unknown node type.";
                            return false;
                        }
                        nodeItem = nodeItem.Next;
                    }

                    map.SetTile(tile.Location, tile);
                }
                nodeTile = nodeTile.Next;
            }

            return true;
        }

        private bool ParseTowns(Map map, FileLoader loader, Node node)
        {
            PropertyReader props;
            Node nodeTown = node.Child;
            while (nodeTown != null)
            {
                if (!loader.GetProps(nodeTown, out props))
                {
                    lastError = "Could not read town data.";
                    return false;
                }

                uint townid = props.ReadUInt32();
                string townName = props.GetString();
                ushort townTempleX = props.ReadUInt16();
                ushort townTempleY = props.ReadUInt16();
                byte townTempleZ = props.ReadByte();

                Town town = new Town(
                    townid,
                    townName,
                    new Location(townTempleX, townTempleY, townTempleZ));

                map.Towns.Add(town);

                nodeTown = nodeTown.Next;
            }
            return true;
        }
    }
}
