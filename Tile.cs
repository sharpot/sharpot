using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpOT
{
    public class Tile
    {
        public Location Location { get; set; }
        public Item Ground { get; set; }
        public IEnumerable<Item> Items { get { return items.AsEnumerable(); } }
        public int ItemCount { get { return items.Count; } }
        public List<Creature> Creatures { get; set; }

        public bool IsProtectionZone = false;
        public bool IsNoPvpZone = false;
        public bool IsPvpZone = false;
        public bool IsNoLogoutZone = false;
        public bool IsRefreshZone = false;

        private LinkedList<Item> items = new LinkedList<Item>();

        public Tile()
        {
            Creatures = new List<Creature>();
        }

        public void AddItem(Item item)
        {
            items.AddFirst(item);
        }

        public void RemoveItem(Item item)
        {
            items.Remove(item);
        }

        public bool IsWalkable { get { return Ground != null && !Ground.Info.IsBlocking; } }

        public FloorChangeDirection FloorChange
        {
            get
            {
                // TODO: compute this only when items change
                if (Ground.Info.FloorChange != FloorChangeDirection.None)
                {
                    return Ground.Info.FloorChange;
                }
                else
                {
                    foreach (Item item in Items)
                    {
                        if (item.Info.FloorChange != FloorChangeDirection.None)
                        {
                            return item.Info.FloorChange;
                        }
                    }
                }
                return FloorChangeDirection.None;
            }
        }

        public IEnumerable<Item> GetTopItems()
        {
            return Items.Where(i => i.GetOrder() < 4).OrderBy(i => i.GetOrder());
        }

        public IEnumerable<Item> GetDownItems()
        {
            return Items.Where(i => i.GetOrder() > 4).OrderBy(i => i.GetOrder()); ;
        }

        public Thing GetThingAtStackPosition(byte stackPosition)
        {
            int n = -1;

            if (Ground != null)
            {
                ++n;
                if (stackPosition == n)
                {
                    return Ground;
                }
            }

            if (ItemCount > 0)
            {
                foreach (Item item in GetTopItems())
                {
                    n++;
                    if (stackPosition == n)
                    {
                        return item;
                    }
                }
            }

            if (Creatures.Count > 0)
            {
                foreach (Creature creature in Creatures)
                {
                    ++n;
                    if (stackPosition == n)
                    {
                        return creature;
                    }
                }
            }

            if (ItemCount > 0)
            {
                foreach (Item item in GetDownItems())
                {
                    n++;
                    if (stackPosition == n)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public byte GetStackPosition(Thing thing)
        {
            int n = -1;

            if (Ground != null)
            {
                if (thing == Ground)
                {
                    return 0;
                }
                ++n;
            }

            if (ItemCount > 0)
            {
                foreach (Item item in GetTopItems())
                {
                    ++n;
                    if (thing == item)
                    {
                        return (byte)n;
                    }
                }
            }

            if (Creatures.Count > 0)
            {
                foreach (Creature creature in Creatures)
                {
                    ++n;
                    if (thing == creature)
                    {
                        return (byte)n;
                    }
                }
            }

            if (ItemCount > 0)
            {
                foreach (Item item in GetDownItems())
                {
                    ++n;
                    if (thing == item)
                    {
                        return (byte)n;
                    }
                }
            }

            throw new Exception("Thing not found in tile.");
        }
    }
}