using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    public class Scripter
    {
        List<Script> Scripts;

        public Scripter()
        {
            Scripts = new List<Script>();
        }

        public void Load()
        {
            //TODO: Load from .XML and LUA files
            //TODO: Finish other events

            //This is the test for the creature say function
            //Saying TextNukka with anything following it will return false, meaning you wont say it
            Script New = new Script(EventType.OnCreatureSay, new List<UInt32>(), new List<UInt16>(), new List<UInt16>(), "TestNukka");
            New.LoadScript("function OnCreatureSay(text) return false end");
            Scripts.Add(New);
            //More test to come
        }
        
        public bool RaiseEvent(EventType Event, EventProperties Properties, object[] EventArgs)
        {
            foreach (Script S in Scripts)
            {
                if (S.ShouldRaise(Event, Properties))
                {
                    if (S.Raise(EventArgs) == false) {return false;}
                }
            }
            return true;
        }
    }

    public struct EventProperties
    {
        public EventProperties(UInt32 ItemID, UInt16 UniqueID, UInt16 Actionid, string text)
        {
            this.ItemID = ItemID;
            this.ActionID = Actionid;
            this.UniqueID = UniqueID;
            this.Text = text;
        }

        public UInt32 ItemID;//These are used for items
        public UInt16 UniqueID;
        public UInt16 ActionID;
        public string Text;//This is use for OnSay
    }

}
