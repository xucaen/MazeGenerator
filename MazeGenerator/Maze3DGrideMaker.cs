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

        public Maze3DGrideMaker(char wallChar, int seed)
        {
            _wall = wallChar;

            // Initialize Grid: 
            _cube = new List<List<StringBuilder>>((int)CubeDimensions.Height);


        }



        public void Generate3DMaze()
        {
        

            for(int y = 0; y< (int)CubeDimensions.Height-2; ++y)
            {
                var gridMaker = new MazeGrideMaker((int)CubeDimensions.Width , (int)CubeDimensions.Length, _wall, y, false);
                gridMaker.Generate();
               // gridMaker.MakeMoreRooms();

                _cube.Add(gridMaker.GetFullGrid());
            }
            //add the top to the labyrinth
            var flr = GetSolid();
            //need to create an entrance
            flr[5][5] = ' ';
            _cube.Add(flr);
            //add the bottom to the labyrinth
            _cube.Insert(0,GetSolid());


        }

        private List<StringBuilder> GetSolid()
        {
            List<StringBuilder> solidFloor = new List<StringBuilder>();

            // Initialize with walls
            for (int y = 0; y < (int)CubeDimensions.Length; y++)
            {
                solidFloor.Add(new StringBuilder(new string(_wall, (int)CubeDimensions.Width)));
            }



            return solidFloor;
        }


        public List<List<StringBuilder>> GetFullGrid() => _cube;

      

    }
}