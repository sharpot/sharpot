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
        public ushort SpriteId = 100;
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

        public ItemGroup Group = ItemGroup.None;

        public ushort Speed = 0;
        public bool IsAlwaysOnTop = false;
        public byte TopOrder = 0;
        public bool IsBlocking = false;
        public bool IsProjectileBlocking = false;
        public bool IsPathBlocking = false;
        public bool HasHeight = false;
        public bool IsUseable = false;
        public bool IsPickupable = false;
        public bool IsMoveable = false;
        public bool IsStackable = false;
        public bool IsVertical = false;
        public bool IsHorizontal = false;
        public bool IsHangable = false;
        public bool IsDistanceReadable = false;
        public bool IsRotatable = false;
        public bool IsReadable = false;
        public bool HasClientCharges = false;
        public bool CanLookThrough = false;

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

        public static IEnumerable<ItemInfo> GetEnumerator()
        {
            foreach (ItemInfo info in itemInfoDictionary.Values)
            {
                if (info != null)
                    yield return info;
            }
        }

        public static void LoadItemsOtb(string fileName)
        {
            OpenTibia.OtbReader reader = new SharpOT.OpenTibia.OtbReader(fileName);
            foreach (var info in reader.GetAllItemInfo())
            {
                if (!itemInfoDictionary.ContainsKey(info.Id))
                {
                    itemInfoDictionary.Add(info.Id, info);
                }
            }
        }

        public static void LoadItemsXml(string fileName)
        {
            OpenTibia.ItemsXmlReader.AddAllItemInfo(fileName, itemInfoDictionary);
        }
    }
}
