using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpOT
{
    public class Channel
    {
        public ushort Id { get; set; }
        public string Name { get; set; }
        public uint CooldownTime { get; set; }
        
        public Channel(ushort id, string name, uint cooldownTime)
        {
            Id = id;
            Name = name;
            CooldownTime = cooldownTime;
            //builder=new StringBuilder();
        }
        /*Reserved for future better channel management
        StringBuilder builder;
        public void DoLog(Player player, string message)
        {
            builder.AppendLine(DateTime.Now + " " + player.Connection.Ip + " " + player.Name + " : " + message);
        }

        public void SaveLog()
        {
            StreamWriter sw = new StreamWriter("Channel_" + Name + "_" + DateTime.Now + ".txt");
            sw.Write(builder.ToString());
            sw.Close();
        }*/
    }
}
