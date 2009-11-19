using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    public class InventoryContainer
    {
        public byte Id { get; set; }
        public Location Location { get; set; }
        public Container Parent { get; set; }
        public Container Container { get; set; }
        public InventoryContainer(Container cont, byte id, Location loc, Container par)
        {
            Container = cont;
            Id = id;
            Location = loc;
            Parent = par;
        }
    }

    public class Inventory
    {
        InventoryContainer[] openContainers = new InventoryContainer[Constants.MaxOpenContainers];
        Item[] slotItems = new Item[(int)SlotType.Last + 1];

        public Item GetItemInSlot(SlotType fromSlot)
        {
            return slotItems[(int)fromSlot - 1];
        }

        public void SetItemInSlot(SlotType slot, Item item)
        {
            slotItems[(int)slot - 1] = item;
        }

        public void ClearSlot(SlotType slot)
        {
            slotItems[(int)slot - 1] = null;
        }

        public int GetContainerId(Container container)
        {
            for (int i = 0; i < Constants.MaxOpenContainers; ++i)
            {
                if (openContainers[i] != null && openContainers[i].Container == container)
                    return i;
            }
            return -1;
        }

        public IEnumerable<byte> GetContainersToClose(Location location)
        {
            return openContainers.Where(ic => ic != null && ic.Location != null && !ic.Location.IsNextTo(location))
                .Select(ic => ic.Id);
        }

        public byte OpenContainer(Container container)
        {
            return OpenContainer(container, null, null);
        }

        public byte OpenContainer(Container container, Location location)
        {
            return OpenContainer(container, location, null);
        }

        public byte OpenContainer(Container container, Container parent)
        {
            return OpenContainer(container, null, parent);
        }

        private byte OpenContainer(Container container, Location location, Container parent)
        {
            byte i;
            for (i = 0; i < Constants.MaxOpenContainers; i++)
            {
                if (openContainers[i] == null)
                {
                    openContainers[i] = new InventoryContainer(container, i, location, parent);
                    return i;
                }
            }
            // TODO: handle more than 16, replace last one
            return i;
        }

        public void CloseContainer(byte i)
        {
            openContainers[i] = null;
        }

        public InventoryContainer GetContainer(byte i)
        {
            return openContainers[i];
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
