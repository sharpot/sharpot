using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    class Container : Item
    {
        public byte Volume { get; set; }
        public List<Item> Items { get; set; }

        public Container(ushort id)
            : base(id)
        {

        }

        public override string GetLookAtString()
        {
            return "You see " + Article + " " + Name +
                ". (Vol:" + Volume +
                Description + SpecialDescription +
                "\n It weighs " + (Weight += Items.Sum(P => P.Weight)) + " oz.";
        }
    }
}
