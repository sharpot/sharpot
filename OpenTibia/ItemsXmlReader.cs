using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SharpOT.OpenTibia
{
    public static class ItemsXmlReader
    {
        public static void AddAllItemInfo(string fileName, Dictionary<ushort, ItemInfo> itemInfoDictionary)
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
                    continue;
                    // TODO: Unknown item
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
                                            info.Group = ItemGroup.Container;
                                            break;
                                        case "key":
                                            info.Group = ItemGroup.Key;
                                            break;
                                        case "magicfield":
                                            info.Group = ItemGroup.MagicField;
                                            break;
                                        case "depot":
                                            info.Group = ItemGroup.Depot;
                                            break;
                                        case "mailbox":
                                            info.Group = ItemGroup.Mailbox;
                                            break;
                                        case "trashholder":
                                            info.Group = ItemGroup.TrashHolder;
                                            break;
                                        case "teleport":
                                            info.Group = ItemGroup.Teleport;
                                            break;
                                        case "door":
                                            info.Group = ItemGroup.Door;
                                            break;
                                        case "bed":
                                            info.Group = ItemGroup.Bed;
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
                                        case "up":
                                            info.FloorChange = FloorChangeDirection.Up;
                                            break;
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
                                case "fluidsource":
                                    info.FluidSource = (Fluid)Enum.Parse(typeof(Fluid), value, true);
                                    break;
                                case "readable":
                                    info.IsReadable = UInt16.Parse(value) != 0;
                                    break;
                                case "writeable":
                                    info.IsWriteable = UInt16.Parse(value) != 0;
                                    break;
                                case "maxtextlen":
                                    info.MaxTextLength = UInt16.Parse(value);
                                    break;
                                case "writeonceitemid":
                                    info.WriteOnceItemId = UInt16.Parse(value);
                                    break;
                                case "weapontype":
                                    info.WeaponType = (WeaponType)Enum.Parse(typeof(WeaponType), value, true);
                                    break;
                                case "slottype":
                                    switch (value.ToLower())
                                    {
                                        case "head":
                                            info.SlotType = SlotType.Head;
                                            break;
                                        case "body":
                                            info.SlotType = SlotType.Armor;
                                            break;
                                        case "legs":
                                            info.SlotType = SlotType.Legs;
                                            break;
                                        case "feet":
                                            info.SlotType = SlotType.Feet;
                                            break;
                                        case "backpack":
                                            info.SlotType = SlotType.Back;
                                            break;
                                        case "two-handed":
                                            info.SlotType = SlotType.TwoHanded;
                                            break;
                                        case "necklace":
                                            info.SlotType = SlotType.Neck;
                                            break;
                                    }
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
