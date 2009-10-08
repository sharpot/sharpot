using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpOT
{
    public class Tile
    {
        public Location Location { get; set; }
        public Item Ground { get; set; }
        public List<Item> Items { get; set; }
        public List<Creature> Creatures { get; set; }

        public bool IsProtectionZone = false;
        public bool IsNoPvpZone = false;
        public bool IsPvpZone = false;
        public bool IsNoLogoutZone = false;
        public bool IsRefreshZone = false;

        public Tile()
        {
            Items = new List<Item>();
            Creatures = new List<Creature>();
            IsWalkable = true;
        }

        public bool IsWalkable { get; set; }

        public FloorChangeDirection FloorChange
        {
            get
            {
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
            return Items.Where(i => i.GetOrder() < 4);
        }

        public IEnumerable<Item> GetDownItems()
        {
            return Items.Where(i => i.GetOrder() > 4);
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

            if (Items.Count > 0)
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

            if (Items.Count > 0)
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

            if (Items.Count > 0)
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

            if (Items.Count > 0)
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