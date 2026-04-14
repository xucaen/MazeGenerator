using MazeGenerator;
using System.Text;

public class Maze3DGameBuilder
{
    private StringBuilder sb = new StringBuilder();
    // No more magic numbers here; controlled via these variables
    private int width = 15;
    private int height = 15;
    private int depth = 15;
    private int numberOfCubes = 1; // Set how many cubes you want to stack

    public void Build()
    {
        // The dimensions of your single "Maze Cube"
       
        Maze3DLevelBuilder mazeBuilder = new Maze3DLevelBuilder(sb, width, height, depth);

        sb.AppendLine("[gd_scene format=3]");
        sb.AppendLine();
        sb.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://player.tscn"" id=""player""]");
        sb.AppendLine(@"[ext_resource type=""PackedScene"" path=""res://world.tscn"" id=""world""]");

        // 3. ITERATION: Build each cube individually
        // This generates the separate .tscn files and adds ext_resource links to 'sb'
        for (int i = 0; i < numberOfCubes; i++)
        {
            mazeBuilder.Build(i);
        }

        //
        sb.AppendLine();
        sb.AppendLine(@"[node name=""Labyrinth3D"" type=""Node3D""]");
        sb.AppendLine(@"[node name=""Player"" parent=""."" instance=ExtResource(""player"")]");
        sb.AppendLine(@"transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 10, 0)");
        sb.AppendLine(@"[node name=""Environment"" parent=""."" instance=ExtResource(""world"")]");

        mazeBuilder.Finish(); // Adds the MazeInstance to Labyrinth3D.tscn

        File.WriteAllText("../../../Labyrinth3D.tscn", sb.ToString());
    }
}