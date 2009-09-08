using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;

namespace SharpOT
{
    public class Script
    {
        public delegate bool EventDelegate(Creature cid, object[] args);//Our delegate for events
        private EventType eventType;//The type of event which raises this script
        private List<UInt32> eventItemIDs;//All the items supported by this event
        private List<UInt16> eventUniqueIDs;//All the unique IDs supported by this event
        private List<UInt16> eventActionIDs;//All the action IDs supported by this event
        private string eventText;//The text supported by this event (If its a command)
        private Lua lua;//The LUA object

        #region Constructor

        public Script(EventType eventType, List<UInt32> eventItemIds, List<UInt16> eventUniqueIDs, List<UInt16> eventActionIDs, string eventText)
        {
            this.eventItemIDs = eventItemIds;
            this.eventUniqueIDs = eventUniqueIDs;
            this.eventActionIDs = eventActionIDs;
            this.eventText = eventText;
            this.eventType = eventType;
            this.lua = new Lua();
        }

        public void LoadScript(String script)
        {
            this.lua.DoString(script);
        }

        #endregion

        #region Events

        public bool ShouldRaise(EventType eventType, EventProperties eventProperties)
        {
            if (eventType == EventType.OnPlayerSay && eventType == this.eventType)
            {//Player Say
                if (eventProperties.Text.Trim() != string.Empty)
                {
                    if (eventProperties.Text.ToLower().StartsWith(eventText.Trim().ToLower()))
                    {
                        return true;
                    }
                }
            }
            else if (eventType == EventType.OnPlayerLogin && eventType == this.eventType)
            {//Player Login
                return true;
            }
            else if (eventType == this.eventType)
            { //Item or creature event
                if (eventProperties.ItemID == 0 || eventItemIDs.Contains(eventProperties.ItemID))
                {
                    if (eventProperties.ActionID == 0 || eventActionIDs.Contains(eventProperties.ActionID))
                    {
                        if (eventProperties.UniqueID == 0 || eventUniqueIDs.Contains(eventProperties.UniqueID))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Raise(Creature cid, object[] args)
        {
            EventDelegate eventDelegate;
            eventDelegate = lua.GetFunction(typeof(EventDelegate), System.Enum.GetName(typeof(EventType), this.eventType)) as EventDelegate;
            if (eventDelegate != null)
            {
                try
                {
                    return eventDelegate.Invoke(cid, args);
                }
                catch (LuaException LuaExep)
                {
                    Console.Write(LuaExep.Message.ToString());
                }
                catch
                {
                    Console.Write("Unknown LUA Error");
                }
                        
            }

            return true;
        }

        #endregion

    }
}
