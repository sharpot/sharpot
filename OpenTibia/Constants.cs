using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.OpenTibia
{
    public class Constants
    {
        public const byte NodeStart = 0xFE;
        public const byte NodeEnd = 0xFF;
        public const byte Escape = 0xFD;
    }

    public enum OtbmNodeType
    {
        OTBM_ROOTV1 = 1,
        OTBM_MAP_DATA = 2,
        OTBM_ITEM_DEF = 3,
        OTBM_TILE_AREA = 4,
        OTBM_TILE = 5,
        OTBM_ITEM = 6,
        OTBM_TILE_SQUARE = 7,
        OTBM_TILE_REF = 8,
        OTBM_SPAWNS = 9,
        OTBM_SPAWN_AREA = 10,
        OTBM_MONSTER = 11,
        OTBM_TOWNS = 12,
        OTBM_TOWN = 13,
        OTBM_HOUSETILE = 14,
        OTBM_WAYPOINTS = 15,
        OTBM_WAYPOINT = 16
    }

    enum OtbmAttribute : byte
    {
        OTBM_ATTR_DESCRIPTION = 1,
        OTBM_ATTR_EXT_FILE = 2,
        OTBM_ATTR_TILE_FLAGS = 3,
        OTBM_ATTR_ACTION_ID = 4,
        OTBM_ATTR_UNIQUE_ID = 5,
        OTBM_ATTR_TEXT = 6,
        OTBM_ATTR_DESC = 7,
        OTBM_ATTR_TELE_DEST = 8,
        OTBM_ATTR_ITEM = 9,
        OTBM_ATTR_DEPOT_ID = 10,
        OTBM_ATTR_EXT_SPAWN_FILE = 11,
        OTBM_ATTR_RUNE_CHARGES = 12,
        OTBM_ATTR_EXT_HOUSE_FILE = 13,
        OTBM_ATTR_HOUSEDOORID = 14,
        OTBM_ATTR_COUNT = 15,
        OTBM_ATTR_DURATION = 16,
        OTBM_ATTR_DECAYING_STATE = 17,
        OTBM_ATTR_WRITTENDATE = 18,
        OTBM_ATTR_WRITTENBY = 19,
        OTBM_ATTR_SLEEPERGUID = 20,
        OTBM_ATTR_SLEEPSTART = 21,
        OTBM_ATTR_CHARGES = 22
    }

    enum TileFlags
    {
        TILESTATE_NONE = 0,
        TILESTATE_PROTECTIONZONE = 1 << 0,
        TILESTATE_DEPRECATED_HOUSE = 1 << 1,
        TILESTATE_NOPVPZONE = 1 << 2,
        TILESTATE_NOLOGOUT = 1 << 3,
        TILESTATE_PVPZONE = 1 << 4,
        TILESTATE_REFRESH = 1 << 5,

        //internal usage
        TILESTATE_HOUSE = 1 << 6,
        TILESTATE_FLOORCHANGE = 1 << 7,
        TILESTATE_FLOORCHANGE_DOWN = 1 << 8,
        TILESTATE_FLOORCHANGE_NORTH = 1 << 9,
        TILESTATE_FLOORCHANGE_SOUTH = 1 << 10,
        TILESTATE_FLOORCHANGE_EAST = 1 << 11,
        TILESTATE_FLOORCHANGE_WEST = 1 << 12,
        TILESTATE_TELEPORT = 1 << 13,
        TILESTATE_MAGICFIELD = 1 << 14,
        TILESTATE_MAILBOX = 1 << 15,
        TILESTATE_TRASHHOLDER = 1 << 16,
        TILESTATE_BED = 1 << 17,
        TILESTATE_BLOCKSOLID = 1 << 18,
        TILESTATE_BLOCKPATH = 1 << 19,
        TILESTATE_IMMOVABLEBLOCKSOLID = 1 << 20,
        TILESTATE_IMMOVABLEBLOCKPATH = 1 << 21,
        TILESTATE_IMMOVABLENOFIELDBLOCKPATH = 1 << 22,
        TILESTATE_NOFIELDBLOCKPATH = 1 << 23,
        TILESTATE_DYNAMIC_TILE = 1 << 24
    }

    public enum ItemAttribute : byte
    {
        ServerId = 0x10,
        ClientId,
        ITEM_ATTR_NAME,				/*deprecated*/
        ITEM_ATTR_DESCR,			/*deprecated*/
        Speed,
        ITEM_ATTR_SLOT,				/*deprecated*/
        ITEM_ATTR_MAXITEMS,			/*deprecated*/
        ITEM_ATTR_WEIGHT,			/*deprecated*/
        ITEM_ATTR_WEAPON,			/*deprecated*/
        ITEM_ATTR_AMU,				/*deprecated*/
        ITEM_ATTR_ARMOR,			/*deprecated*/
        ITEM_ATTR_MAGLEVEL,			/*deprecated*/
        ITEM_ATTR_MAGFIELDTYPE,		/*deprecated*/
        ITEM_ATTR_WRITEABLE,		/*deprecated*/
        ITEM_ATTR_ROTATETO,			/*deprecated*/
        ITEM_ATTR_DECAY,			/*deprecated*/
        ITEM_ATTR_SPRITEHASH,
        ITEM_ATTR_MINIMAPCOLOR,
        ITEM_ATTR_07,
        ITEM_ATTR_08,
        ITEM_ATTR_LIGHT,

        //1-byte aligned
        ITEM_ATTR_DECAY2,			/*deprecated*/
        ITEM_ATTR_WEAPON2,			/*deprecated*/
        ITEM_ATTR_AMU2,				/*deprecated*/
        ITEM_ATTR_ARMOR2,			/*deprecated*/
        ITEM_ATTR_WRITEABLE2,		/*deprecated*/
        ITEM_ATTR_LIGHT2,
        TopOrder,
        ITEM_ATTR_WRITEABLE3		/*deprecated*/
    }
}
