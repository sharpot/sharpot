using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;

namespace SharpOT
{
    public class Script
    {
        public delegate bool EventDelegate(object[] args);//Our delegate for events
        EventType EventType;//The type of event which raises this script
        List<UInt32> EventItemIDs;//All the items supported by this event
        List<UInt16> EventUniqueIDs;//All the unique IDs supported by this event
        List<UInt16> EventActionIDs;//All the action IDs supported by this event
        string EventText;//The text supported by this event (If its a command)
        Lua lua;//The LUA object

#region "Construct"
        public Script(EventType Event, List<UInt32> EventItemIDs, List<UInt16> EventUniqueIDs, List<UInt16> EventActionIDs, string EventText)
        {
            this.EventItemIDs = EventItemIDs;
            this.EventUniqueIDs = EventUniqueIDs;
            this.EventActionIDs = EventActionIDs;
            this.EventText = EventText;
            this.EventType = Event;
            this.lua = new Lua();
        }

        public void LoadScript(String script){
            this.lua.DoString(script);
        }
#endregion

#region "Events"
        public bool ShouldRaise(EventType Event, EventProperties P) {
            if (Event == EventType.OnPlayerSay && Event == this.EventType){//Player Say
                if (P.Text.Trim() != string.Empty){
                    if (P.Text.ToLower().StartsWith(EventText.Trim().ToLower())){
                        return true;
                    }
                }
            }else if (Event == EventType.OnPlayerLogin && Event == this.EventType){//Player Login
                return true;
            }else if (Event == this.EventType){ //Item or creature event
                if (P.ItemID == 0 || EventItemIDs.Contains(P.ItemID)){
                    if (P.ActionID == 0 || EventActionIDs.Contains(P.ActionID)){
                        if (P.UniqueID == 0 || EventUniqueIDs.Contains(P.UniqueID)){
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Raise(object[] Args){
            EventDelegate Event;
            Event = lua.GetFunction(typeof(EventDelegate), System.Enum.GetName(typeof(EventType), this.EventType)) as EventDelegate;
            if (Event != null)
                return Event.Invoke(Args);

            return true;
        }
#endregion

    }
}
