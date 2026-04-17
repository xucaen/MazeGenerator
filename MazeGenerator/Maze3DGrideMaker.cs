using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace MazeGenerator
{
    public class Maze3DGrideMaker
    {

        // Representing the 3D grid as a list of rows. 
        // Index = (z * _height) + y
        private List<List<StringBuilder>> _cube;



        private readonly char _wall;
        private readonly Random _RandomGenerator;
        private bool[,,] _visited;

        public Maze3DGrideMaker(char wallChar, int seed)
        {
            _wall = wallChar;
            _RandomGenerator = new Random(seed);

            // Initialize Grid: 
            _cube = new List<List<StringBuilder>>((int)CubeDimensions.Height);


        }



        public void Generate3DMaze()
        {
            //TODO:

            for(int y = 0; y< (int)CubeDimensions.Height; ++y)
            {
                var gridMaker = new MazeGrideMaker((int)CubeDimensions.Length, (int)CubeDimensions.Width, _wall, y, false);
                gridMaker.Generate();
                gridMaker.MakeMoreRooms();

                _cube.Add(gridMaker.GetFullGrid());
            }


        }




        public List<List<StringBuilder>> GetFullGrid() => _cube;

      

    }
}