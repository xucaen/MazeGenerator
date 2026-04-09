using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGenerator
{
    public class MazeMetaData 
    {
        public int Height { get; set; }
        public int Width { get; set; }

      //Type is Room, or Wall
        public string Type { get; set; }
        public bool IsLit { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        // 1. Add a parameterless constructor for the Serializer
        public MazeMetaData() { }

        // 2. Keep your existing constructor for your own use
        public MazeMetaData(int x, int y, string type)
        {
            this.X = x;
            this.Y = y;
            this.Type = type;
        }
    }
}
