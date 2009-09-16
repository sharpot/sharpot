using System;
using System.Collections.Generic;
using System.Xml;

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
        public string Article { get; set; }
        public string SpecialDescription { get; set; }
        public ushort Armor { get; set; }

        private static Dictionary<ushort, Item> xmlItems = new Dictionary<ushort, Item>();

        public static void LoadItemsXml()
        {
            xmlItems.Clear();
            XmlDocument document = new XmlDocument();
            document.Load(SharpOT.Properties.Settings.Default.ItemsXmlFile);
            foreach (XmlNode node in document.GetElementsByTagName("item"))
            {
                ushort id = ushort.Parse(node.Attributes["id"].InnerText);
                if (!xmlItems.ContainsKey(id))
                {
                    Item item = new Item();
                    item.Id = id;
                    if (node.Attributes != null)
                    {
                        if (node.Attributes["article"] != null)
                        {
                            item.Article = node.Attributes["article"].InnerText;
                        }
                        if (node.Attributes["name"] != null)
                        {
                            item.Name = node.Attributes["name"].InnerText;
                        }
                    }
                    if (node.HasChildNodes)
                    {
                        foreach (XmlNode attrNode in node.ChildNodes)
                        {
                            if (attrNode.Attributes != null &&
                                attrNode.Attributes.Count > 0 &&
                                attrNode.Attributes["key"] != null && 
                                attrNode.Attributes["value"] != null && 
                                attrNode.Attributes["key"].InnerText == "description")
                            {
                                item.Description = attrNode.Attributes["value"].InnerText;
                            }
                        }
                    }
                    xmlItems.Add(id, item);
                }
            }
        }

        #region Constructors

        private Item()
        {

        }

        public Item(ushort id)
        {
            Id = id;
            Article = "an";
            Name = "item of type " + Id;
            if (xmlItems.ContainsKey(id))
            {
                Article = xmlItems[id].Article;
                Name = xmlItems[id].Name;
                Description = xmlItems[id].Description;
            }
        }

        #endregion

        protected override ushort GetThingId()
        {
            return Id;
        }

        #region LookAt methods

        public override string GetLookAtString()
        {
            string lookat = "You see ";
            if (Article != null && Article.Length > 0)
                lookat += Article + " ";
            lookat += Name + ".";
            if (Description != null && Description.Length > 0)
                lookat += "\n" + Description + SpecialDescription;
            if (Weight > 0)
                lookat += "\nIt weighs " + Weight + " oz.";
            return lookat;
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