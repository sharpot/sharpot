using System;
using System.Collections.Generic;

namespace SharpOT
{
    public abstract class Thing
    {
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

            DatItem di = DatReader.GetItem(id);

            if (di.IsGroundTile) return 0;
            else if (di.TopOrder1) return 1;
            else if (di.TopOrder2) return 2;
            else if (di.TopOrder3) return 3;
            else return 5;
        }
    }
}