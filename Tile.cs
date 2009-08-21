using System;
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
    }
}