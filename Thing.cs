using System;
using System.Collections.Generic;

namespace SharpOT
{
    public abstract class Thing
    {
        public Location Location { get; set; }
        protected abstract ushort GetThingId();

        public abstract string GetLookAtString();

        // Thanks to Stepler at http://tpforums.org/forum/showpost.php?p=26654&postcount=5
        // The sections are:
        // 0 - Ground
        // 1 - High priority items
        // 2 - Medium priority items
        // 3 - Low priority items
        // 4 - Creatures
        // 5 - Other items
        public byte GetOrder()
        {
            uint id = GetThingId();
            if ((id >= 0x0061) && (id <= 0x0063)) return 4;

            ItemInfo info = ItemInfo.GetItemInfo((ushort)id);

            byte itemInfoTopOrder = 0;

            if (info.IsAlwaysOnTop)
                itemInfoTopOrder = info.TopOrder;
            else
                itemInfoTopOrder = 5;

            return itemInfoTopOrder;
        }
    }
}