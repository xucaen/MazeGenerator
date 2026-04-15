using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGenerator
{
    public class Maze3DMetaData
    {
        public string Type { get; set; } // "Wall" or "Room"
        public bool IsLit { get; set; }
        public bool HasAirBelow { get; set; }
        public bool HasAirAbove { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; } // Vertical Axis

        public Maze3DMetaData() { }

        public Maze3DMetaData(int x, int y, int z, string type)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Type = type;
        }
    }
}
