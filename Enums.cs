using System;

namespace SharpOT
{
    #region Packets

    public enum ServerPacketType : byte
    {
        Disconnect = 0x0A,
        MessageOfTheDay = 0x14,
        CharacterList = 0x64,

        SelfAppear = 0x0A,
        GMAction = 0x0B,
        ErrorMessage = 0x14,
        FyiMessage = 0x15,
        WaitingList = 0x16,
        Ping = 0x1E,
        Death = 0x28,
        CanReportBugs = 0x32,
        MapDescription = 0x64,
        MapSliceNorth = 0x65,
        MapSliceEast = 0x66,
        MapSliceSouth = 0x67,
        MapSliceWest = 0x68,
        TileUpdate = 0x69,
        TileAddThing = 0x6A,
        TileTransformThing = 0x6B,
        TileRemoveThing = 0x6C,
        CreatureMove = 0x6D,
        ContainerOpen = 0x6E,
        ContainerClose = 0x6F,
        ContainerAddItem = 0x70,
        ContainerUpdateItem = 0x71,
        ContainerRemoveItem = 0x72,
        InventorySetSlot = 0x78,
        InventoryClearSlot = 0x79,
        ShopWindowOpen = 0x7A,
        ShopSaleGoldCount = 0x7B,
        ShopWindowClose = 0x7C,
        SafeTradeRequestAck = 0x7D,
        SafeTradeRequestNoAck = 0x7E,
        SafeTradeClose = 0x7F,
        WorldLight = 0x82,
        Effect = 0x83,
        AnimatedText = 0x84,
        Projectile = 0x85,
        CreatureSquare = 0x86,
        CreatureHealth = 0x8C,
        CreatureLight = 0x8D,
        CreatureOutfit = 0x8E,
        CreatureSpeed = 0x8F,
        CreatureSkull = 0x90,
        CreatureShield = 0x91,
        ItemTextWindow = 0x96,
        HouseTextWindow = 0x97,
        PlayerStatus = 0xA0,
        PlayerSkillsUpdate = 0xA1,
        PlayerFlags = 0xA2,
        CancelTarget = 0xA3,
        CreatureSpeech = 0xAA,
        ChannelList = 0xAB,
        ChannelOpen = 0xAC,
        ChannelOpenPrivate = 0xAD,
        RuleViolationOpen = 0xAE,
        RuleViolationRemove = 0xAF,
        RuleViolationCancel = 0xB0,
        RuleViolationLock = 0xB1,
        PrivateChannelCreate = 0xB2,
        ChannelClosePrivate = 0xB3,
        TextMessage = 0xB4,
        PlayerWalkCancel = 0xB5,
        FloorChangeUp = 0xBE,
        FloorChangeDown = 0xBF,
        OutfitWindow = 0xC8,
        VipState = 0xD2,
        VipLogin = 0xD3,
        VipLogout = 0xD4,
        QuestList = 0xF0,
        QuestPartList = 0xF1,
        ShowTutorial = 0xDC,
        AddMapMarker = 0xDD,
    }

    public enum ClientPacketType : byte
    {
        LoginServerRequest = 0x01,
        GameServerRequest = 0x0A,
        Logout = 0x14,
        ItemMove = 0x78,
        ShopBuy = 0x7A,
        ShopSell = 0x7B,
        ShopClose = 0x7C,
        ItemUse = 0x82,
        ItemUseOn = 0x83,
        ItemRotate = 0x85,
        LookAt = 0x8C,
        PlayerSpeech = 0x96,
        ChannelList = 0x97,
        ClientChannelOpen = 0x98,
        ChannelClose = 0x99,
        Attack = 0xA1,
        Follow = 0xA2,
        CancelMove = 0xBE,
        ItemUseBattlelist = 0x84,
        ContainerClose = 0x87,
        ContainerOpenParent = 0x88,
        TurnNorth = 0x6F,
        TurnWest = 0x70,
        TurnSouth = 0x71,
        TurnEast = 0x72,
        AutoWalk = 0x64,
        AutoWalkCancel = 0x69,
        MoveNorth = 0x65,
        MoveEast = 0x66,
        MoveSouth = 0x67,
        MoveWest = 0x68,
        MoveNorthEast = 0x6A,
        MoveSouthEast = 0x6B,
        MoveSouthWest = 0x6C,
        MoveNorthWest = 0x6D,
        VipAdd = 0xDC,
        VipRemove = 0xDD,
        RequestOutfit = 0xD2,
        ChangeOutfit = 0xD3,
        Ping = 0x1E,
        FightModes = 0xA0,
        ContainerUpdate = 0xCA,
        TileUpdate = 0xC9,
        PrivateChannelOpen = 0x9A,
        NpcChannelClose = 0x9E,
    }

    #endregion

    #region Speech

    public enum TextMessageType : byte
    {
        ConsoleRed = 0x12, //Red message in the console
        ConsoleOrange = 0x13, //Orange message in the console
        ConsoleOrange2 = 0x14, //Orange message in the console
        Warning = 0x15, //Red message in game window and in the console
        EventAdvance = 0x16, //White message in game window and in the console
        EventDefault = 0x17, //White message at the bottom of the game window and in the console
        StatusDefault = 0x18, //White message at the bottom of the game window and in the console
        DescriptionGreen = 0x19, //Green message in game window and in the console
        StatusSmall = 0x1A, //White message at the bottom of the game window"
        ConsoleBlue = 0x1B, //Blue message in the console
    }

    public enum SpeechType : byte
    {
        Say = 0x01,	//normal talk
        Whisper = 0x02,	//whispering - #w text
        Yell = 0x03,	//yelling - #y text
        PrivatePlayerToNPC = 0x04, //Player-to-NPC speaking(NPCs channel)
        PrivateNPCToPlayer = 0x05, //NPC-to-Player speaking
        Private = 0x06, //Players speaking privately to players
        ChannelYellow = 0x07,	//Yellow message in chat
        ChannelWhite = 0x08, //White message in chat
        RuleViolationReport = 0x09, //Reporting rule violation - Ctrl+R
        RuleViolationAnswer = 0x0A, //Answering report
        RuleViolationContinue = 0x0B, //Answering the answer of the report
        Broadcast = 0x0C,	//Broadcast a message - #b
        ChannelRed = 0x0D,	//Talk red on chat - #c
        PrivateRed = 0x0E,	//Red private - @name@ text
        ChannelOrange = 0x0F,	//Talk orange on text
        //SPEAK_                = 0x10, //?
        ChannelRedAnonymous = 0x11,	//Talk red anonymously on chat - #d
        //SPEAK_MONSTER_SAY12 = 0x12, //?????
        MonsterSay = 0x13,	//Talk orange
        MonsterYell = 0x14,	//Yell orange
    }


    public enum ChatChannel : ushort
    {
        Guild = 0x00,
        Party = 0x01,
        //?Gamemaster = 0x01,
        Tutor = 0x02,
        RuleReport = 0x03,
        Game = 0x05,
        Trade = 0x06,
        TradeRook = 0x07,
        RealLife = 0x08,
        Help = 0x09,
        OwnPrivate = 0x0E,
        Custom = 0xA0,
        Custom1 = 0xA1,
        Custom2 = 0xA2,
        Custom3 = 0xA3,
        Custom4 = 0xA4,
        Custom5 = 0xA5,
        Custom6 = 0xA6,
        Custom7 = 0xA7,
        Custom8 = 0xA8,
        Custom9 = 0xA9,
        Private = 0xFFFF,
        None = 0xAAAA
    }

    #endregion

    #region Other

    public enum Effect : byte
    {
        RedSpark = 1,
        BlueRings = 2,
        Puff = 3,
        YellowSpark = 4,
        ExplosionArea = 5,
        ExplosionDamage = 6,
        FireArea = 7,
        YellowRings = 8,
        GreenRings = 9,
        BlackSpark = 10,
        Teleport = 11,
        EnergyDamage = 12,
        BlueShimmer = 13,
        RedShimmer = 14,
        GreenShimmer = 15,
        FirePlume = 16,
        GreenSpark = 17,
        MortArea = 18,
        GreenNotes = 19,
        RedNotes = 20,
        PoisonArea = 21,
        YellowNotes = 22,
        PurpleNotes = 23,
        BlueNotes = 24,
        WhiteNotes = 25,
        Bubbles = 26,
        Dice = 27,
        GiftWraps = 28,
        FireworkYellow = 29,
        FireworkRed = 30,
        FireworkBlue = 31,
        Stun = 32,
        Sleep = 33,
        WaterCreature = 34,
        Groundshaker = 35,
        Hearts = 36,
        FireAttack = 37,
        EnergyArea = 38,
        SmallClouds = 39,
        HolyDamage = 40,
        BigClouds = 41,
        IceArea = 42,
        IceTornado = 43,
        IceAttack = 44,
        Stones = 45,
        SmallPlants = 46,
        Carniphilia = 47,
        PurpleEnergy = 48,
        YellowEnergy = 49,
        HolyArea = 50,
        BigPlants = 51,
        Cake = 52,
        GiantIce = 53,
        WaterSplash = 54,
        PlantAttack = 55,
        TutorialArrow = 56,
        TutorialSquare = 57,
        MirrorHorizontal = 58,
        MirrorVerticle = 59,
        SkullHorizontal = 60,
        SkullVertical = 61,
        Assassin = 62,
        StepsHorizontal = 63,
        BloodySteps = 64,
        StepsVertical = 65,
        YalahariGhost = 66,
        Bats = 67,
        Smoke = 68,
        None = 0xFF
    }

    public static class LightLevel
    {
        public static byte None = 0;
        public static byte Torch = 7;
        public static byte Full = 27;
        public static byte World = 255;
    }

    public static class LightColor
    {
        public static byte None = 0;
        public static byte Default = 206; // default light color
        public static byte Orange = Default;
        public static byte White = 215;
    }

    public enum Direction : byte
    {
        North = 0,
        East,
        South,
        West,
        NorthEast,
        SouthEast,
        NorthWest,
        SouthWest
    }

    public enum Skull : byte
    {
        None = 0,
        Yellow = 1,
        Green = 2,
        White = 3,
        Red = 4,
        Black = 5
    }

    public enum Party : byte
    {
        None = 0,
        Host = 1,
        Guest = 2,
        Member = 3,
        Leader = 4,
        MemberSharedExp = 5,
        LeaderSharedExp = 6,
        MemberSharedExpInactive = 7,
        LeaderSharedExpInactive = 8,
        MemberNoSharedExp = 9,
        LeaderNoSharedExp = 10
    }

    public enum WarIcon
    {
        None = 0,
        Blue = 1,
        Green = 2,
        Red = 3
    }

    public enum Gender : byte
    {
        Male,
        Female
    }

    // TODO: dynamic?
    public enum Vocation
    {
        None
    }

    public enum FightMode : byte
    {
        FullAttack = 1,
        Balanced = 2,
        FullDefense = 3
    }

    public enum FluidColor : byte
    {
        Empty = 0,
        Blue = 1,
        Red = 2,
        Brown = 3,
        Green = 4,
        Yellow = 5,
        White = 6,
        Purple = 7
    }

    public enum Fluid : byte
    {
        Empty,
        Water,
        Blood,
        Beer,
        Slime,
        Lemonade,
        Milk,
        Mana,
        Life,
        Oil,
        Urine,
        CoconutMilk,
        Wine,
        Mud,
        FruitJuice,
        Lava,
        Rum,
        Swamp
    }

    public enum WeaponType : byte
    {
        None,
        Sword,
        Club,
        Axe,
        Shield,
        Distance,
        Wand,
        Ammunition
    }

    public enum AmmoType : byte
    {
        None = 0,
        Bolt,
        Arrow,
        Spear,
        ThrowingStar,
        ThrowingKnife,
        Stone,
        Snowball
    }

    public enum ProjectileType
    {
        Spear = 0x00,
        Bolt = 0x01,
        Arrow = 0x02,
        Fire = 0x03,
        Energy = 0x04,
        PoisonArrow = 0x05,
        BurstArrow = 0x06,
        ThrowingStar = 0x07,
        ThrowingKnife = 0x08,
        SmallStone = 0x09,
        Death = 0x0A, //10
        LargeRock = 0x0B, //11
        Snowball = 0x0C, //12
        PowerBolt = 0x0D, //13
        PoisonField = 0x0E, //14
        InfernalBolt = 0x0F, //15
        HuntingSpear = 0x10, //16
        EnchantedSpear = 0x11, //17
        RedStar = 0x12, //18
        GreenStar = 0x13, //19
        RoyalSpear = 0x14, //20
        SniperArrow = 0x15, //21
        OnyxArrow = 0x16, //22
        PiercingBolt = 0x17, //23
        WhirlwindSword= 0x18, //24
        WhirlwindAxe= 0x19, //25
        WhirlwindClub = 0x1A, //26
        EtherealSpear = 0x1B, //27
        Ice = 0x1C, //28
        Earth = 0x1D, //29
        Holy = 0x1E, //30
        SuddenDeath = 0x1F, //31
        FlashArrow = 0x20, //32
        FlamingArrow = 0x21, //33
        ShiverArrow = 0x22, //34
        EnergyBall = 0x23, //35
        SmallIce = 0x24, //36
        SmallHoly = 0x25, //37
        SmallEarth = 0x26, //38
        EarthArrow = 0x27, //39
        Explosion = 0x28, //40
        Cake = 0x29, //41
        //for internal use, dont send to client
        WeaponType = 0xFE, //254
        None = 0xFF,
        Unknown = 0xFFFF
    }

    public enum SlotType : byte
    {
        None = 0,
        Head,
        Neck,
        Back,
        Armor,
        Right,
        Left,
        Legs,
        Feet,
        Ring,
        Ammo,
        Depot, // Internal only
        TwoHanded, // Internal only
        First = Head,
        Last = Ammo
    }

    #endregion

    #region Items

    public enum LocationType
    {
        Container,
        Slot,
        Ground
    }

    public enum ItemGroup
    {
        None = 0,
        Ground,
        Container,
        Weapon,	
        Ammunition,
        Armor,	
        Charges,
        Teleport,
        MagicField,
        Writeable,
        Key,
        Splash,
        Fluid,
        Door,
        Deprecated,
        Depot,
        Mailbox,
        TrashHolder,
        Bed
    }

    public enum FloorChangeDirection
    {
        None,
        Up,
        Down,
        North,
        South,
        West,
        East
    }

    public enum CorpseType
    {
        None,
        Venom,
        Blood,
        Undead,
        Fire,
        Energy
    }

    [FlagsAttribute] 
    public enum ItemFlags : uint
    {
	    BlocksSolid = 1,
	    BlocksProjectile = 2,
	    BlocksPathFinding = 4,
	    HasHeight = 8,
	    Useable = 16,
	    Pickupable = 32,
	    Moveable = 64,
	    Stackable = 128,
	    FloorChangeDown = 256,
	    FloorChangeNorth = 512,
	    FloorChangeEast = 1024,
	    FloorChangeSouth = 2048,
	    FloorChangeWest = 4096,
	    AlwaysOnTop = 8192,
	    Readable = 16384,
	    Rotatable = 32768,
	    Hangable = 65536,
	    Vertical = 131072,
	    Horizontal = 262144,
	    CannotDecay = 524288,
	    AllowDistanceRead = 1048576,
	    Unused = 2097152,
	    ClientCharges = 4194304,
	    LookThrough = 8388608
    }

    #endregion
}