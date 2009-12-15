using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpOT.Scripting;

namespace SharpOT.Scripting
{
    public interface IActionItem
    {
        ushort GetItemId();
        bool Use(Game game, Player user, Location fromLocation, byte fromStackPosition, byte index, Item item);
        void UseOnItem(Game game, Player user, Location fromLocation, byte fromStackPosition, Item item, Location toLocation, Item onItem);
        void UseOnCreature(Game game, Player user, Location fromLocation, byte fromStackPosition, Item item, Creature creature);
    }
}

namespace SharpOT
{
    public static class ActionItems
    {
        static Dictionary<ushort, IActionItem> actions = new Dictionary<ushort, IActionItem>();

        public static void RegisterAction(IActionItem action)
        {
            if (actions.ContainsKey(action.GetItemId()))
                throw new Exception(String.Format("Action {0} has a duplicate ItemId {1}.", action.GetType().Name, action.GetItemId()));

            actions.Add(action.GetItemId(), action);
        }

        public static void UnRegisterAction(ushort itemId)
        {
            actions.Remove(itemId);
        }

        public static bool ExecuteUse(Game game, Player player, Location fromLocation, byte fromStackPosition, byte index, Item item)
        {
            if (actions.ContainsKey(item.Id))
                return actions[item.Id].Use(game, player, fromLocation, fromStackPosition, index, item);
            return true;
        }

        public static void ExecuteUseOnItem(Game game, Player player, Location fromLocation, byte fromStackPosition, Item item, Location toLocation, Item onItem)
        {
            if (actions.ContainsKey(item.Id))
                actions[item.Id].UseOnItem(game, player, fromLocation, fromStackPosition, item, toLocation, onItem);
        }

        public static void ExecuteUseOnCreature(Game game, Player player, Location fromLocation, byte fromStackPosition, Item item, Creature creature)
        {
            if (actions.ContainsKey(item.Id))
                actions[item.Id].UseOnCreature(game, player, fromLocation, fromStackPosition, item, creature);
        }
    }
}
