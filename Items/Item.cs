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
            string lookat = "You see ";
            if (Info.Article != null && Info.Article.Length > 0)
                lookat += Info.Article + " ";
            lookat += Info.Name + ".";
            if (Info.Description != null && Info.Description.Length > 0)
                lookat += "\n" + Info.Description + Info.SpecialDescription;
            if (Info.Weight > 0)
                lookat += "\nIt weighs " + Info.Weight + " oz.";
            return lookat;
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