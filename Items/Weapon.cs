using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    class Weapon : Item
    {
        public ushort Attack { get; set; }
        public ushort Defense { get; set; }
        public short ExtraAttack { get; set; }
        public short ExtraDefense { get; set; }

        public Weapon(ushort id)
            : base(id)
        {

        }

        public override string GetLookAtString()
        {
            return "You see " + Article + " " + Name +
                    ". " + GetCombatAttributes() + Description + SpecialDescription +
                    "\n It weighs " + Weight + " oz.";
        }

        private string GetCombatAttributes()
        {
            string temp = "";
            //Add the items attack
            temp = "(Atk:" + Attack;
            if (ExtraAttack > 0)
            {
                temp += " +" + ExtraAttack;
            }
            else if (ExtraAttack < 0)
            {
                temp += " -" + Math.Abs(ExtraAttack);
            }
            //Add the Defense
            temp += ", Def:" + Defense;
            if (ExtraDefense > 0)
            {
                temp += " +" + ExtraDefense;
            }
            else if (ExtraDefense < 0)
            {
                temp += " -" + Math.Abs(ExtraDefense);
            }
            temp += ")";
            return temp;
        }
    }
}
