using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Location
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Location(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Location(Location loc)
        {
            X = loc.X;
            Y = loc.Y;
            Z = loc.Z;
        }

        public ItemLocationType GetItemLocationType()
        {
            if (X == 0xFFFF)
            {
                if ((Y & 0x40) != 0)
                {
                    return ItemLocationType.Container;
                }
                else
                {
                    return ItemLocationType.Slot;
                }
            }
            else
            {
                return ItemLocationType.Ground;
            }
        }

        public SlotType GetSlot()
        {
            return (SlotType)Convert.ToByte(Y);
        }

        public byte GetContainer()
        {
            return Convert.ToByte(Y - 0x40);
        }

        public byte GetContainerPosition()
        {
            return Convert.ToByte(Z);
        }

        public override bool Equals(object other)
        {
            return other is Location && Equals((Location)other);
        }

        public bool Equals(Location other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        public override int GetHashCode()
        {
            ushort shortX = (ushort)X;
            ushort shortY = (ushort)Y;
            byte byteZ = (byte)Z;
            return ((shortX << 3) + (shortY << 1) + byteZ).GetHashCode();
        }

        public override string ToString()
        {
            return X + ", " + Y + ", " + Z;
        }

        public Location Offset(Direction direction)
        {
            return Offset(direction, 1);
        }

        public Location Offset(Direction direction, int amount)
        {
            int x = X, y = Y, z = Z;

            switch (direction)
            {
                case Direction.North:
                    y -= amount;
                    break;
                case Direction.South:
                    y += amount;
                    break;
                case Direction.West:
                    x -= amount;
                    break;
                case Direction.East:
                    x += amount;
                    break;
                case Direction.NorthWest:
                    x -= amount;
                    y -= amount;
                    break;
                case Direction.SouthWest:
                    x -= amount;
                    y += amount;
                    break;
                case Direction.NorthEast:
                    x += amount;
                    y -= amount;
                    break;
                case Direction.SouthEast:
                    x += amount;
                    y += amount;
                    break;
            }

            return new Location(x, y, z);
        }

        public bool CanSee(Location loc)
        {
	        if(Z <= 7){
		        //we are on ground level or above (7 -> 0)
		        //view is from 7 -> 0
		        if(loc.Z > 7){
			        return false;
		        }
	        }
	        else if(Z >= 8){
		        //we are underground (8 -> 15)
		        //view is +/- 2 from the floor we stand on
		        if(Math.Abs(Z - loc.Z) > 2){
			        return false;
		        }
	        }

	        //negative offset means that the action taken place is on a lower floor than ourself
	        int offsetz = Z - loc.Z;

            if ((loc.X >= X - 8 + offsetz) && (loc.X <= X + 9 + offsetz) &&
                (loc.Y >= Y - 6 + offsetz) && (loc.Y <= Y + 7 + offsetz))
		        return true;

	        return false;
        }

        public Direction GetDirectionTo(Location to)
        {
            return GetDirectionTo(this, to);
        }

        public static Direction GetDirectionTo(Location from, Location to)
        {
            int dx = from.X - to.X;
            int dy = from.Y - to.Y;

            if (dx == 0)
            {
                if (dy < 0) return Direction.South;
                else return Direction.North; // if (dy > 0)
            }
            else if (dx < 0)
            {
                if (dy == 0) return Direction.East;
                else if (dy < 0) return Direction.SouthEast;
                else return Direction.NorthEast; // if (dy > 0)
            }
            else // dx > 0
            {
                if (dy == 0) return Direction.West;
                else if (dy < 0) return Direction.SouthWest;
                else return Direction.NorthWest; // if (dy > 0)
            }
        }

        public bool IsNextTo(Location second)
        {
            return IsNextTo(this, second);
        }

        public static bool IsNextTo(Location first, Location second)
        {
            if (first.Z != second.Z) return false;
            int dx = first.X - second.X;
            int dy = first.Y - second.Y;
            return dx <= 1 && dx >= -1 && dy <= 1 && dy >= -1;
        }

        public bool IsInRange(Location second, double range, bool sameFloor)
        {
            return IsInRange(this, second, range, sameFloor);
        }

        public static bool IsInRange(Location first, Location second, double range, bool sameFloor)
        {
            if (sameFloor && first.Z != second.Z) return false;
            int dx = first.X - second.X;
            int dy = first.Y - second.Y;
            return Math.Sqrt(dx * dx + dy * dy) <= range;
        }
    }
}