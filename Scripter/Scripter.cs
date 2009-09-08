using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    public class Scripter
    {
        private List<Script> scripts;

        public Scripter()
        {
            scripts = new List<Script>();
        }

        public void Load()
        {
            //TODO: Load from .XML and LUA files
            //TODO: Finish other events

            //This is the test for the creature say function
            //Saying TextNukka with anything following it will return false, meaning you wont say it
            //It will also make you say "OnSay returning false" as a test for the import of the player object
            Script testScript = new Script(EventType.OnPlayerSay, new List<UInt32>(), new List<UInt16>(), new List<UInt16>(), "TestNukka");
            testScript.LoadScript("function OnPlayerSay(Player, Text) Player.Health = Player.Health - 10 Player:Say(\"OnSay returning false\") return false end");
            scripts.Add(testScript);
            //More test to come
        }
        
        public bool RaiseEvent(EventType eventType, EventProperties eventProperties, Creature cid, object[] eventArgs)
        {
            foreach (Script script in scripts)
            {
                if (script.ShouldRaise(eventType, eventProperties))
                {
                    if (script.Raise(cid, eventArgs) == false) { return false; }
                }
            }
            return true;
        }
    }

    public struct EventProperties
    {
        public EventProperties(UInt32 itemID, UInt16 uniqueID, UInt16 actionid, string text)
        {
            this.ItemID = itemID;
            this.ActionID = actionid;
            this.UniqueID = uniqueID;
            this.Text = text;
        }

        public UInt32 ItemID;//These are used for items
        public UInt16 UniqueID;
        public UInt16 ActionID;
        public string Text;//This is use for OnSay
    }

}
