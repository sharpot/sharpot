using System;
using System.Collections.Generic;
using System.Xml;
using SharpOT.Util;

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

                if (info.Name == null)
                {
                    info.Article = "an";
                    info.Name = "item of type " + id;
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
                                        case "ring":
                                            info.SlotType = SlotType.Ring;
                                            break;
                                    }
                                    break;
                                case "ammotype":
                                    var shootType = (ShootType)Enum.Parse(typeof(ShootType), value, true);
                                    info.AmmoType = shootType.GetAmmoType();
                                    break;
                                case "shoottype":
                                    info.ShootType = (ShootType)Enum.Parse(typeof(ShootType), value, true);
                                    break;
                                case "effect":
                                    switch (value)
                                    {
                                        case "redspark":
                                            info.Effect = Effect.DrawBlood;
                                            break;
                                        case "bluebubble":
                                            info.Effect = Effect.LoseEnergy;
                                            break;
                                        case "poff":
                                            info.Effect = Effect.Puff;
                                            break;
                                        case "yellowspark":
                                            info.Effect = Effect.BlockHit;
                                            break;
                                        case "explosionarea":
                                            info.Effect = Effect.ExplosionArea;
                                            break;
                                        case "explosion":
                                            info.Effect = Effect.ExplosionDamage;
                                            break;
                                        case "firearea":
                                            info.Effect = Effect.FireArea;
                                            break;
                                        case "yellowbubble":
                                            info.Effect = Effect.YellowEnergy;
                                            break;
                                        case "greenbubble":
                                            info.Effect = Effect.PoisonRings;
                                            break;
                                        case "blackspark":
                                            info.Effect = Effect.HitArea;
                                            break;
                                        case "teleport":
                                            info.Effect = Effect.Teleport;
                                            break;
                                        case "energy":
                                            info.Effect = Effect.EnergyDamage;
                                            break;
                                        case "blueshimmer":
                                            info.Effect = Effect.MagicEnergy;
                                            break;
                                        case "redshimmer":
                                            info.Effect = Effect.MagicBlood;
                                            break;
                                        case "greenshimmer":
                                            info.Effect = Effect.MagicPoison;
                                            break;
                                        case "fire":
                                            info.Effect = Effect.HitByFire;
                                            break;
                                        case "greenspark":
                                            info.Effect = Effect.Poison;
                                            break;
                                        case "mortarea":
                                            info.Effect = Effect.MortArea;
                                            break;
                                        case "greennote":
                                            info.Effect = Effect.SoundGreen;
                                            break;
                                        case "rednote":
                                            info.Effect = Effect.SoundRed;
                                            break;
                                        case "poison":
                                            info.Effect = Effect.Poison;
                                            break;
                                        case "yellownote":
                                            info.Effect = Effect.SoundYellow;
                                            break;
                                        case "purplenote":
                                            info.Effect = Effect.SoundPurple;
                                            break;
                                        case "bluenote":
                                            info.Effect = Effect.SoundBlue;
                                            break;
                                        case "whitenote":
                                            info.Effect = Effect.SoundWhite;
                                            break;
                                        case "bubbles":
                                            info.Effect = Effect.Bubbles;
                                            break;
                                        case "dice":
                                            info.Effect = Effect.Craps;
                                            break;
                                        case "giftwraps":
                                            info.Effect = Effect.GiftWraps;
                                            break;
                                        case "yellowfirework":
                                            info.Effect = Effect.FireworkYellow;
                                            break;
                                        case "redfirework":
                                            info.Effect = Effect.FireworkRed;
                                            break;
                                        case "bluefirework":
                                            info.Effect = Effect.FireworkBlue;
                                            break;
                                        case "stun":
                                            info.Effect = Effect.Stun;
                                            break;
                                        case "sleep":
                                            info.Effect = Effect.Sleep;
                                            break;
                                        case "watercreature":
                                            info.Effect = Effect.WaterCreature;
                                            break;
                                        case "groundshaker":
                                            info.Effect = Effect.Groundshaker;
                                            break;
                                        case "hearts":
                                            info.Effect = Effect.Hearts;
                                            break;
                                        case "fireattack":
                                            info.Effect = Effect.FireAttack;
                                            break;
                                        case "energyarea":
                                            info.Effect = Effect.EnergyArea;
                                            break;
                                        case "smallclouds":
                                            info.Effect = Effect.SmallClouds;
                                            break;
                                        case "holydamage":
                                            info.Effect = Effect.HolyDamage;
                                            break;
                                        case "bigclouds":
                                            info.Effect = Effect.BigClouds;
                                            break;
                                        case "icearea":
                                            info.Effect = Effect.IceArea;
                                            break;
                                        case "icetornado":
                                            info.Effect = Effect.IceTornado;
                                            break;
                                        case "iceattack":
                                            info.Effect = Effect.IceAttack;
                                            break;
                                        case "stones":
                                            info.Effect = Effect.Stones;
                                            break;
                                        case "smallplants":
                                            info.Effect = Effect.SmallPlants;
                                            break;
                                        case "carniphila":
                                            info.Effect = Effect.Carniphilia;
                                            break;
                                        case "purpleenergy":
                                            info.Effect = Effect.PurpleEnergy;
                                            break;
                                        case "yellowenergy":
                                            info.Effect = Effect.YellowEnergy;
                                            break;
                                        case "holyarea":
                                            info.Effect = Effect.HolyArea;
                                            break;
                                        case "bigplants":
                                            info.Effect = Effect.BigPlants;
                                            break;
                                        case "cake":
                                            info.Effect = Effect.Cake;
                                            break;
                                        case "giantice":
                                            info.Effect = Effect.GiantIce;
                                            break;
                                        case "watersplash":
                                            info.Effect = Effect.WaterSplash;
                                            break;
                                        case "plantattack":
                                            info.Effect = Effect.PlantAttack;
                                            break;
                                        case "tutorialarrow":
                                            info.Effect = Effect.TutorialArrow;
                                            break;
                                        case "tutorialsquare":
                                            info.Effect = Effect.TutorialSquare;
                                            break;
                                    }
                                    break;
                                case "range":
                                    info.Range = int.Parse(value);
                                    break;
                                case "stopduration":
                                    info.StopDuration = int.Parse(value);
                                    break;
                                case "decayto":
                                    info.DecayTo = ushort.Parse(value);
                                    break;
                                case "transformequipto":
                                    info.TransformEquipTo = ushort.Parse(value);
                                    break;
                                case "transformdequipto":
                                    info.TransformDequipTo = ushort.Parse(value);
                                    break;
                                case "showduration":
                                    info.ShowDuration = int.Parse(value) != 0;
                                    break;
                                case "charges":
                                    info.Charges = int.Parse(value);
                                    break;
                                case "showcharges":
                                    info.ShowCharges = int.Parse(value) != 0;
                                    break;
                                case "breakchance":
                                    info.BreakChance = int.Parse(value);
                                    if (info.BreakChance < 0) info.BreakChance = 0;
                                    if (info.BreakChance > 100) info.BreakChance = 100;
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
