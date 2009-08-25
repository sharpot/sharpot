using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpOT
{
    public static class DatReader
    {
        public const byte IdOffset = 100;
        private static Dictionary<uint, DatItem> items = new Dictionary<uint, DatItem>();

        public static void Load()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(SharpOT.Properties.Settings.Default.ItemDataFile));

            uint signature = reader.ReadUInt32();
            uint numberOfItems = reader.ReadUInt16();
            uint numberOfCreatures = reader.ReadUInt16();
            uint numberOfEffects = reader.ReadUInt16();
            uint numberOfMissiles = reader.ReadUInt16();

            long numberOfIds = numberOfItems + numberOfCreatures + numberOfEffects + numberOfMissiles - (IdOffset - 1);

            uint id = IdOffset;
            while (id < IdOffset + numberOfIds)
            {
                items[id] = new DatItem();
                byte option = reader.ReadByte();
                while (option != 0xFF)
                {
                    switch (option)
                    {
                        case 0x00:
                            items[id].IsGroundTile = true;
                            items[id].Speed = reader.ReadUInt16();
                            break;
                        case 0x01:
                            items[id].TopOrder1 = true;
                            break;
                        case 0x02:
                            items[id].TopOrder2 = true;
                            break;
                        case 0x03:
                            items[id].TopOrder3 = true;
                            break;
                        case 0x04:
                            items[id].IsContainer = true;
                            break;
                        case 0x05:
                            items[id].IsStackable = true;
                            break;
                        case 0x06:
                            items[id].IsCorpse = true;
                            break;
                        case 0x07:
                            items[id].IsUseable = true;
                            break;
                        case 0x08:
                            items[id].IsRune = true;
                            break;
                        case 0x09:
                            items[id].IsWriteable = true;
                            reader.ReadUInt16();
                            break;
                        case 0x0A:
                            items[id].IsReadable = true;
                            reader.ReadUInt16();
                            break;
                        case 0x0B:
                            items[id].IsFluidContainer = true;
                            break;
                        case 0x0C:
                            items[id].IsSplash = true;
                            break;
                        case 0x0D:
                            items[id].IsBlocking = true;
                            break;
                        case 0x0E:
                            items[id].IsMoveable = false;
                            break;
                        case 0x0F:
                            items[id].IsMissileBlocking = true;
                            break;
                        case 0x10:
                            items[id].IsPathBlocking = true;
                            break;
                        case 0x11:
                            items[id].IsPickupable = true;
                            break;
                        case 0x12:
                            items[id].IsHangable = true;
                            break;
                        case 0x13:
                            items[id].IsHangableHorizontal = true;
                            break;
                        case 0x14:
                            items[id].IsHangableVertical = true;
                            break;
                        case 0x15:
                            items[id].IsRotatable = true;
                            break;
                        case 0x16:
                            items[id].IsLightSource = true;
                            reader.ReadUInt16(); // Radius
                            reader.ReadUInt16(); // Color
                            break;
                        case 0x18:
                            items[id].IsFloorChange = true;
                            break;
                        case 0x19:
                            items[id].IsOffset = true;
                            reader.ReadUInt16(); // offset x
                            reader.ReadUInt16(); // offset y
                            break;
                        case 0x1A:
                            items[id].IsRaised = true;
                            reader.ReadUInt16(); // height
                            break;
                        case 0x1B:
                            items[id].IsLayer = true;
                            break;
                        case 0x1C:
                            items[id].HasIdleAnimation = true;
                            break;
                        case 0x1D:
                            items[id].IsMinimap = true;
                            reader.ReadUInt16(); // Minimap color
                            break;
                        case 0x1E:
                            items[id].HasHelpByte = true;
                            reader.ReadByte(); // Help byte
                            reader.ReadByte();
                            break;
                        case 0x1F:
                            items[id].IsGround = true;
                            break;
                        case 0x20:
                            items[id].IsSeeThrough = true;
                            break;
                        default:
                            break;
                    }
                    option = reader.ReadByte();
                }

                if (items[id].IsStackable || items[id].IsRune || items[id].IsFluidContainer)
                {
                    items[id].HasExtraByte = true;
                }

                int width = reader.ReadByte();
                int height = reader.ReadByte();
                if (width > 1 || height > 1)
                {
                    reader.ReadByte();
                }
                int blendFrames = reader.ReadByte();
                int xRepeat = reader.ReadByte();
                int yRepeat = reader.ReadByte();
                int zRepeat = reader.ReadByte();
                int animations = reader.ReadByte();

                reader.ReadBytes(width * height * blendFrames * xRepeat * yRepeat * zRepeat * animations * 2);

                ++id;
            }
        }

        public static DatItem GetItem(uint id)
        {
            return items[id];
        }
    }
}
