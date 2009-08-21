using System;
using System.Collections.Generic;
using Tibia;
using Tibia.Objects;

namespace SharpOTMapTracker
{
    public class MapTile
    {
        public Location Location;
        public ushort TileId;
        public List<MapItem> Items = new List<MapItem>();
    }
}
