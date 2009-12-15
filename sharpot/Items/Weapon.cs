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
            return String.Format(
                "You see {0}{1}. {2}{3}{4}\nIt weighs {5} oz.",
                Info.Article,
                Info.Name,
                GetCombatAttributes(),
                Info.Description,
                Info.SpecialDescription,
                GetWeight()
            );
        }

        private string GetCombatAttributes()
        {
            string temp = "(Atk:" + Attack;
            if (ExtraAttack > 0)
            {
                temp += " +" + ExtraAttack;
            }
            else if (ExtraAttack < 0)
            {
                temp += " -" + Math.Abs(ExtraAttack);
            }
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
