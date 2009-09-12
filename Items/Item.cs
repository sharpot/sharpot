using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Item : Thing
    {
        //TODO:
        //Loading all items from item.XML
        //The plan is to make a list of every item from Items.XML, and then index
        //through it when a new item is created.
        //This will also be used for items on the map or in a players inventory, ofcourse
        public double Weight { get; set; }
        public ushort Id;
        public byte Extra;
        public Location Location { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SpecialDescription { get; set; }
        public ushort Armor { get; set; }


        #region "Constructors"
        public Item()
        {
        }
        public Item(ushort Id, Location Location)
            : this(Id, Location, "item of type " + Id) { }

        public Item(ushort Id, Location Location, string Name)
            : this(Id, Location, Name, "")
        {
        }

        public Item(ushort Id, Location Location, string Name, string Description)
            : this(Id, Location, Name, Description, "")
        {
        }

        public Item(ushort Id, Location Location, string Name, string Description, string SpecialDescription)
        {
            this.Id = Id;
            this.Location = Location;
            this.Name = Name;
            this.Description = Description;
            this.SpecialDescription = SpecialDescription;
        }
        #endregion

        protected override ushort GetThingId()
        {
            return Id;
        }

        #region "LookAt methods"
        protected virtual void LookAt(Player player)
        {
            if (player.Tile.Location.CanSee(Location))
            {
                player.Connection.SendTextMessage(TextMessageType.DescriptionGreen,
                "You see " + GetArticle(Name) + " " + Name +//Name
                Description + SpecialDescription + Description +
                "\n It weighs " + Weight + " oz.");//Weight
            }
        }
        public string GetArticle(string text)
        {
            if (text.Length == 0) return string.Empty;

            if ((text.ToLower()[0] == 'a') || (text.ToLower()[0] == 'e') ||
            text.ToLower()[0] == 'i' || text.ToLower()[0] == 'o' || text.ToLower()[0] == 'u')
            {
                return "an";
            }

            return "a";
        }
        #endregion

        public DatItem Data
        {
            get
            {
                return DatReader.GetItem(Id);
            }
        }
    }
}