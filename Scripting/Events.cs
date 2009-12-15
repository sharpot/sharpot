using System;

namespace SharpOT.Scripting
{
    #region Speech

    public delegate bool BeforeCreatureSpeechHandler(Creature creature, Speech speech);
    public delegate void AfterCreatureDefaultSpeechHandler(Creature creature, SpeechType speechType, string message);
    public delegate void AfterCreaturePrivateSpeechHandler(Creature creature, string receiver, string message);
    public delegate void AfterCreatureChannelSpeechHandler(string sender, SpeechType type, ChatChannel channelId, string message);
    
    public delegate bool BeforePrivateChannelOpenHandler(Player player, string receiver);
    public delegate void AfterPrivateChannelOpenHandler(Player player, string receiver);

    public delegate bool BeforeChannelHandler(Player creature, ChatChannel channel);
    public delegate void AfterChannelOpenHandler(Player creature, ChatChannel channel);

    #endregion

    #region Movement

    public delegate bool BeforeCreatureTurnHandler(Creature creature, Direction direction);
    public delegate void AfterCreatureTurnHandler(Creature creature, Direction direction);

    public delegate bool BeforeCreatureWalkHandler(Creature creature, Direction direction);
    public delegate void AfterCreatureWalkHandler(Creature creature, Direction direction);
    public delegate bool BeforeCreatureMoveHandler(Creature creature, Location fromLocation, Location toLocation, byte fromStackPosition, Tile toTile);
    public delegate void AfterCreatureMoveHandler(Creature creature, Location fromLocation, Location toLocation, byte fromStackPosition, Tile toTile);

    public delegate bool BeforeWalkCancelHandler();
    public delegate void AfterWalkCancelHandler();

    #endregion

    #region Items

    public delegate bool BeforeThingMoveHandler(Player user, Thing thing, Location fromLocation, byte fromStackPosition, Location toLocation, byte count);
    public delegate void AfterThingMoveHandler(Player user, Thing thing, Location fromLocation, byte fromStackPosition, Location toLocation, byte count);

    public delegate bool BeforeItemUseHandler(Player user, Item item, Location fromLocation, byte fromStackPosition, byte index);
    public delegate void AfterItemUseHandler(Player user, Item item, Location fromLocation, byte fromStackPosition, byte index);

    public delegate bool BeforeItemUseOnHandler(Player user, Item item, Location fromLocation, byte fromStackPosition, Location toLocation, byte toStackPosition);
    public delegate void AfterItemUseOnHandler(Player user, Item item, Location fromLocation, byte fromStackPosition, Location toLocation, byte toStackPosition);

    public delegate bool BeforeItemUseOnCreatureHandler(Player user, Item item, Location fromLocation, byte fromStackPosition, Creature creature);
    public delegate void AfterItemUseOnCreatureHandler(Player user, Item item, Location fromLocation, byte fromStackPosition, Creature creature);

    #endregion

    #region Login/Logout

    public delegate bool BeforeLoginHandler(Connection connection, string playerName);
    public delegate void AfterLoginHandler(Player player);
    public delegate bool BeforeLogoutHandler(Player player);
    public delegate void AfterLogoutHandler(Player player);

    #endregion

    #region Misc

    public delegate bool BeforePlayerChangeOutfitHandler(Creature creature, Outfit outfit);
    public delegate void AfterPlayerChangeOutfitHandler(Creature creature, Outfit outfit);
    
    public delegate bool BeforeCreatureUpdateHealthHandler(Creature creature, ushort health);
    public delegate void AfterCreatureUpdateHealthHandler(Creature creature, ushort health);

    public delegate bool BeforeVipAddHandler(Player player, string vipName);
    public delegate void AfterVipAddHandler(Player player, string vipName);
    public delegate void VipRemoveHandler(Player player, uint vipId);

    #endregion
}