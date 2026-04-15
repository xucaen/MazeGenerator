using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MazeGenerator
{
    public static class CubeDimensions
    {
        public static float Length=9, Width=9, Height=11;
        public static float HorizontalSize = 5.0f; // This makes rooms 5m wide/deep
        public static float VerticalSize = 12.0f;   // This makes rooms 12m tall

    }
    class Program
    {


        static void Main(string[] args)
        {
            ///just maming a single level for now
            //a level contains a single Helix of mazes plus alien megastructures surrounding it.
           // GameBuilder game = new GameBuilder();
            //game.Build();


            Maze3DGameBuilder game3d = new Maze3DGameBuilder();
            game3d.Build();




        }

     
    }

}