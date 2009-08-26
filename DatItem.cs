using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    public class DatItem
    {
        public bool IsGroundTile = false;
        public ushort Speed = 0;
        public bool TopOrder1 = false;
        public bool TopOrder2 = false;
        public bool TopOrder3 = false;
        public bool IsBlocking = false;
        public bool IsContainer = false;
        public bool IsStackable = false;
        public bool IsCorpse = false;
        public bool IsUseable = false;
        public bool IsRune = false;
        public bool IsWriteable = false;
        public bool IsReadable = false;
        public bool IsFluidContainer = false;
        public bool IsSplash = false;
        public bool IsMoveable = true;
        public bool IsMissileBlocking = false;
        public bool IsPathBlocking = false;
        public bool IsPickupable = false;
        public bool IsHangable = false;
        public bool IsHangableHorizontal = false;
        public bool IsHangableVertical = false;
        public bool IsRotatable = false;
        public bool IsLightSource = false;
        public bool IsFloorChange = false;
        public bool IsOffset = false;
        public bool IsRaised = false;
        public bool IsLayer = false;
        public bool HasIdleAnimation = false;
        public bool IsMinimap = false;
        public bool HasHelpByte = false;
        public bool IsSeeThrough = false;
        public bool IsGroundItem = false;

        public bool HasExtraByte = false;
    }
}
