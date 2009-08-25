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
            Ground = new Item();
            Items = new List<Item>();
            Creatures = new List<Creature>();
            IsWalkable = true;
        }

        public bool IsWalkable { get; set; }

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
                n += Items.Where(i => i.GetOrder() < 5).Count();
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