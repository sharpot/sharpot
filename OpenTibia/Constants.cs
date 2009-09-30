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
