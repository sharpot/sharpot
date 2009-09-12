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


         protected override void LookAt(Player player)
        {
            if (player.Tile.Location.CanSee(Location))
            {
                player.Connection.SendTextMessage(
                    TextMessageType.DescriptionGreen,
                    "You see " + GetArticle(Name) + " " + Name +
                    ". (Vol:" + Volume +
                    Description + SpecialDescription +
                    "\n It weighs " + (Weight += Items.Sum(P => P.Weight)) + " oz.");
            }
        }
    }
}
