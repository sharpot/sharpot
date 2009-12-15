using System;
using System.Collections.Generic;
using System.Xml;

namespace SharpOT
{
    public class Item : Thing
    {
        public ushort Id;
        public byte Extra;

        #region Constructors

        private Item()
        {

        }

        protected Item(ushort id)
        {
            Id = id;
        }

        public static Item Create(ushort id)
        {
            ItemInfo info = ItemInfo.GetItemInfo(id);
            switch (info.Group)
            {
                case ItemGroup.Container:
                    return new Container(id);
                default:
                    return new Item(id);
            }
        }

        #endregion

        public byte Count
        {
            get
            {
                if (Info.IsStackable)
                    return Extra;
                else
                    return 1;
            }
        }

        public override ushort GetThingId()
        {
            return Id;
        }

        public override string GetLookAtString()
        {
            return String.Format(
                "You see {0}{1}.{2}{3}\nIt weighs {4} oz.",
                Info.Article,
                Info.Name,
                Info.Description,
                Info.SpecialDescription,
                GetWeight()
            );
        }

        public virtual double GetWeight()
        {
            return Info.Weight;
        }

        public ItemInfo Info
        {
            get
            {
                return ItemInfo.GetItemInfo(Id);
            }
        }
    }
}