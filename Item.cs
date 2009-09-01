using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Item : Thing
    {
        public ushort Id;
        public byte Extra;

        protected override ushort GetId()
        {
            return Id;
        }

        public DatItem Data
        {
            get
            {
                return DatReader.GetItem(Id);
            }
        }
    }
}