using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    public class Container : Item
    {
        public Container Parent { get; set; }
        public byte Volume { get; private set; }
        public IEnumerable<Item> Items { get { return items.AsEnumerable(); } }
        public byte ItemCount = 0;
        private List<Item> items = new List<Item>();
        public Container(ushort id)
            : base(id)
        {
            Parent = null;
            Volume = Info.Volume;
        }

        public bool IsFull()
        {
            return ItemCount >= Volume;
        }

        public void AddItem(Item item)
        {
            items.Insert(0, item);
            ++ItemCount;
        }

        public void RemoveItem(byte containerPos)
        {
            items.RemoveAt(containerPos);
            --ItemCount;
        }

        public void UpdateItem(byte containerPos, Item newItem)
        {
            items[containerPos] = newItem;
        }

        public Item GetItem(byte containerPos)
        {
            if (containerPos >= ItemCount)
                return null;
                
            return items[containerPos];
        }

        public IEnumerable<Item> GetItems()
        {
            for (int i = 0; i < ItemCount; ++i)
            {
                if (items[i] != null)
                {
                    yield return items[i];
                }
            }
        }

        public override double GetWeight()
        {
            return Info.Weight + Items.Sum(i => i.Info.Weight);
        }

        public override string GetLookAtString()
        {
            return "You see " + Info.Article + " " + Info.Name +
                ". (Vol:" + Volume +
                Info.Description + Info.SpecialDescription +
                "\n It weighs " + (Info.Weight += Items.Sum(i => i.Info.Weight)) + " oz.";
        }
    }
}
