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

        public Item(ushort id)
        {
            Id = id;
        }

        #endregion

        protected override ushort GetThingId()
        {
            return Id;
        }

        #region LookAt methods

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

        #endregion

        public ItemInfo Info
        {
            get
            {
                return ItemInfo.GetItemInfo(Id);
            }
        }
    }
}