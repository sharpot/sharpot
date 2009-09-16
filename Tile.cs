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

        public Tile()
        {
            Items = new List<Item>();
            Creatures = new List<Creature>();
            IsWalkable = true;
        }

        public bool IsWalkable { get; set; }

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
                // check all top items
                // or increment by top item count
                n += GetTopItems().Count();
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
                // check all down items
            }

            throw new Exception("Thing not found in tile.");
        }
    }
}