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
        public ushort Id;
        public byte Extra;
        public Location Location { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SpecialDescription { get; set; }
        public double Weight { get; set; }
        public ushort Attack { get; set; }
        public ushort Defense { get; set; }
        public ushort Armor { get; set; }
        public short ExtraAttack { get; set; }
        public short ExtraDefense { get; set; }


        #region "Constructors"
        public void Item(ushort Id, Location Location)
        {
            Item(Id, Location, "item of type " + Id);
        }

        public void Item(ushort Id, Location Location, string Name)
        {
            Item(Id, Location, Name, "");
        }

        public void Item(ushort Id, Location Location, string Name, string Description)
        {
            Item(Id, Location, Name, Description, "");
        }

        public void Item(ushort Id, Location Location, string Name, string Description, string SpecialDescription)
        {
            this.Id = Id;
            this.Location = Location;
            this.Name = Name;
            this.Description = Description;
            this.SpecialDescription = SpecialDescription;
        }
        #endregion

        protected override ushort GetId()
        {
            return Id;
        }

        #region "LookAt methods"
        public void LookAt(Player player)
        {
            if (player.Tile.Location.CanSee(Location))
            {
                player.Connection.SendTextMessage(TextMessageType.DescriptionGreen,
                "You see " + GetArticle(Name) + " " + Name +//Name
                GetCombatAttributes() + Description + SpecialDescription +//Attributes and description
                "\n It weighs " + Weight + " oz.");//Weight
            }
        }

        private string GetArticle(string text)
        {
            if (text.Length = 0) return string.Empty;

            if (text.tolower()[0] = "a" || text.tolower()[0] = "e" ||
            text.tolower()[0] = "i" || text.tolower()[0] = "o" || text.tolower()[0] = "u")
            {
                return "an";
            }

            return "a";
        }

        private string GetCombatAttributes()
        {
            string Temp;
            if (Attack > 0 && Defense > 0)
            {
                //Add the items attack
                Temp = "(Atk:" + Attack;
                if (ExtraAttack > 0)
                {
                    Temp += " +" + ExtraAttack;
                }
                else if (ExtraAttack < 0)
                {
                    Temp += " -" + Math.Abs(ExtraAttack);
                }
                //Add the Defense
                Temp += ", Def:" + Defense;
                if (ExtraDefense > 0)
                {
                    Temp += " +" + ExtraDefense;
                }
                else if (ExtraDefense < 0)
                {
                    Temp += " -" + Math.Abs(ExtraDefense);
                }
                Temp += ")";
            }
            else if (Armor > 0)
            {
                Temp = "(Arm:" + Armor + ")";
            }
            else
            {
                return string.Empty;
            }
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