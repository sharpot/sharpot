using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SharpOT
{
    public class ItemInfo
    {
        public ushort Id;
        public ushort SpriteId;
        public ItemType Type = ItemType.Normal;
        public double Weight = 0.0;
        public string Name;
        public string Description;
        public string Article;
        public string PluralName;
        public string SpecialDescription;
        public string RuneSpellName;
        public ushort Armor = 0;
        public byte Volume = 0;
        public ushort Attack = 0;
        public ushort Defense = 0;
        public short ExtraAttack = 0;
        public short ExtraDefense = 0;
        public ushort RotateTo = 0;
        public bool ShowCount = false;
        public FloorChangeDirection FloorChange = FloorChangeDirection.None;
        public CorpseType CorpseType = CorpseType.None;
        public Fluid FluidSource = Fluid.Empty;
        public ushort MaxTextLength = 0;
        public ushort WriteOnceItemId = 0;
        public SlotType SlotType = SlotType.None;

        public bool IsGroundTile = false;
        public ushort Speed = 0;
        public bool TopOrder1 = false;
        public bool TopOrder2 = false;
        public bool TopOrder3 = false;
        public bool IsBlocking = false;
        public bool IsContainer = false;
        public bool IsStackable = false;
        public bool IsCorpse = false;
        public bool IsUseable = false;
        public bool IsRune = false;
        public bool IsWriteable = false;
        public bool IsReadable = false;
        public bool IsFluidContainer = false;
        public bool IsSplash = false;
        public bool IsMoveable = true;
        public bool IsProjectileBlocking = false;
        public bool IsPathBlocking = false;
        public bool IsPickupable = false;
        public bool IsHangable = false;
        public bool IsHangableHorizontal = false;
        public bool IsHangableVertical = false;
        public bool IsRotatable = false;
        public bool IsLightSource = false;
        public bool IsFloorChange = false;
        public bool IsOffset = false;
        public bool IsRaised = false;
        public bool IsLayer = false;
        public bool HasIdleAnimation = false;
        public bool IsMinimap = false;
        public bool HasHelpByte = false;
        public bool IsSeeThrough = false;
        public bool IsGroundItem = false;

        public bool HasExtraByte = false;

        private static Dictionary<ushort, ItemInfo> itemInfoDictionary = new Dictionary<ushort, ItemInfo>();

        public static ushort GetSpriteId(ushort itemId)
        {
            return GetItemInfo(itemId).SpriteId;
        }

        public static ItemInfo GetItemInfo(ushort itemId)
        {
            return itemInfoDictionary[itemId];
        }

        public static void LoadItemsOtb(string fileName)
        {
            OpenTibia.OtbReader reader = new SharpOT.OpenTibia.OtbReader(fileName);
            foreach (var kvp in reader.GetServerToSpriteIdPairs())
            {
                ItemInfo info = null;
                if (itemInfoDictionary.ContainsKey(kvp.Key))
                {
                    info = itemInfoDictionary[kvp.Key];
                    info.SpriteId = kvp.Value;
                }
                else
                {
                    info = new ItemInfo();
                    info.Id = kvp.Key;
                    info.SpriteId = kvp.Value;
                    itemInfoDictionary.Add(kvp.Key, info);
                }
            }
        }

        public static void LoadItemsXml(string fileName)
        {
            XmlDocument document = new XmlDocument();
            document.Load(fileName);
            foreach (XmlNode node in document.GetElementsByTagName("item"))
            {
                ushort id = ushort.Parse(node.Attributes["id"].InnerText);
                ItemInfo info = null;
                if (itemInfoDictionary.ContainsKey(id))
                {
                    info = itemInfoDictionary[id];
                }
                else
                {
                    info = new ItemInfo();
                    info.Id = id;
                    itemInfoDictionary.Add(id, info);
                }
                info.Article = "an";
                info.Name = "item of type " + id;
                foreach (XmlAttribute attr in node.Attributes)
                {
                    switch (attr.Name.ToLower())
                    {
                        case "article":
                            info.Article = node.Attributes[attr.Name].InnerText;
                            break;
                        case "name":
                            info.Name = node.Attributes[attr.Name].InnerText;
                            break;
                        case "plural":
                            info.PluralName = node.Attributes[attr.Name].InnerText;
                            break;
                    }
                }
                if (node.HasChildNodes)
                {
                    foreach (XmlNode attrNode in node.ChildNodes)
                    {
                        if (attrNode.Attributes != null &&
                            attrNode.Attributes.Count > 0 &&
                            attrNode.Attributes["key"] != null &&
                            attrNode.Attributes["value"] != null)
                        {
                            string value = attrNode.Attributes["value"].InnerText;
                            switch (attrNode.Attributes["key"].InnerText.ToLower())
                            {
                                case "type":
                                    switch (value)
                                    {
                                        case "container":
                                            info.Type = ItemType.Container;
                                            break;
                                        case "key":
                                            info.Type = ItemType.Key;
                                            break;
                                        case "magicfield":
                                            info.Type = ItemType.MagicField;
                                            break;
                                        case "depot":
                                            info.Type = ItemType.Depot;
                                            break;
                                        case "mailbox":
                                            info.Type = ItemType.Mailbox;
                                            break;
                                        case "trashholder":
                                            info.Type = ItemType.TrashHolder;
                                            break;
                                        case "teleport":
                                            info.Type = ItemType.Teleport;
                                            break;
                                        case "door":
                                            info.Type = ItemType.Door;
                                            break;
                                        case "bed":
                                            info.Type = ItemType.Bed;
                                            break;
                                    }
                                    break;
                                case "name":
                                    info.Name = value;
                                    break;
                                case "article":
                                    info.Article = value;
                                    break;
                                case "plural":
                                    info.PluralName = value;
                                    break;
                                case "description":
                                    info.Description = value;
                                    break;
                                case "runespellname":
                                    info.RuneSpellName = value;
                                    break;
                                case "weight":
                                    info.Weight = Double.Parse(value) / 100.0;
                                    break;
                                case "showcount":
                                    info.ShowCount = UInt16.Parse(value) != 0;
                                    break;
                                case "armor":
                                    info.Armor = UInt16.Parse(value);
                                    break;
                                case "defense":
                                    info.Defense = UInt16.Parse(value);
                                    break;
                                case "extradef":
                                    info.ExtraDefense = Int16.Parse(value);
                                    break;
                                case "attack":
                                    info.Attack = UInt16.Parse(value);
                                    break;
                                case "rotateto":
                                    info.RotateTo = UInt16.Parse(value);
                                    break;
                                case "moveable":
                                    info.IsMoveable = UInt16.Parse(value) != 0;
                                    break;
                                case "blockprojectile":
                                    info.IsProjectileBlocking = UInt16.Parse(value) != 0;
                                    break;
                                case "allowpickupable":
                                    info.IsPickupable = UInt16.Parse(value) != 0;
                                    break;
                                case "floorchange":
                                    switch (value.ToLower())
                                    {
                                        case "down":
                                            info.FloorChange = FloorChangeDirection.Down;
                                            break;
                                        case "north":
                                            info.FloorChange = FloorChangeDirection.North;
                                            break;
                                        case "south":
                                            info.FloorChange = FloorChangeDirection.South;
                                            break;
                                        case "west":
                                            info.FloorChange = FloorChangeDirection.West;
                                            break;
                                        case "east":
                                            info.FloorChange = FloorChangeDirection.East;
                                            break;
                                    }
                                    break;
                                case "corpsetype":
                                    switch (value.ToLower())
                                    {
                                        case "venom":
                                            info.CorpseType = CorpseType.Venom;
                                            break;
                                        case "blood":
                                            info.CorpseType = CorpseType.Blood;
                                            break;
                                        case "undead":
                                            info.CorpseType = CorpseType.Undead;
                                            break;
                                        case "fire":
                                            info.CorpseType = CorpseType.Fire;
                                            break;
                                        case "energy":
                                            info.CorpseType = CorpseType.Energy;
                                            break;
                                    }
                                    break;
                                case "containersize":
                                    info.Volume = Byte.Parse(value);
                                    break;
                                // TODO: Many more to go
                            }
                        }
                    }
                }
            }
        }
    }
}
