using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    public class Inventory
    {
        List<Item> slotItems = new List<Item>((int)SlotType.Last + 1);

        public Item GetItemInSlot(SlotType fromSlot)
        {
            return slotItems[(int)fromSlot];
        }

        public Item FindItem(uint id)
        {
            Item item;

            item = FindItem(GetItemInSlot(SlotType.Left), id);
            if (item != null) return item;

            item = FindItem(GetItemInSlot(SlotType.Right), id);
            if (item != null) return item;

            item = FindItem(GetItemInSlot(SlotType.Ammo), id);
            if (item != null) return item;

            return null;
        }

        private Item FindItem(Item item, uint id)
        {
            if (item.Id == id) return item;

            // recurse into container
            return null;
        }
    }
}
