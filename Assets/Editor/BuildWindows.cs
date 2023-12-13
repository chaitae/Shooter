using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;

public class BuildWindows : EditorWindow
{
    [MenuItem("Build/Build Game")]
    static void BuildGame()
    {
        // Get the target directory.
        string buildPath = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");

        // Clear the contents of the selected directory.
        DeleteBuildContents(buildPath);

        // Define the scenes to include in the build.
        string[] levels = new string[]
        {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/MenuSceneSteam.unity",
            "Assets/Scenes/SteamGameScene.unity"
        };

        // Build the player.
        BuildPipeline.BuildPlayer(levels, Path.Combine(buildPath, "BuiltGame.exe"), BuildTarget.StandaloneWindows, BuildOptions.None);

        UnityEngine.Debug.Log("Build Complete");

        // Zip the contents of the build directory.
        ZipBuildContents(buildPath);
    }

    static void DeleteBuildContents(string buildPath)
    {
        // Delete the contents of the specified folder
        if (Directory.Exists(buildPath))
        {
            string[] files = Directory.GetFiles(buildPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            string[] directories = Directory.GetDirectories(buildPath);
            foreach (string directory in directories)
            {
                Directory.Delete(directory, true);
            }
        }
    }

    static void ZipBuildContents(string buildPath)
    {
        // Zip the contents of the specified folder
        string zipFilePath = Path.Combine(Path.GetDirectoryName(buildPath), "BuiltGame.zip");

        // Make sure the zip file is closed before attempting to create it
        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }
        ZipFile.CreateFromDirectory(buildPath, Path.Combine(Path.GetDirectoryName(buildPath),"BuiltGame.zip"));

        UnityEngine.Debug.Log("Zip File Created: " + zipFilePath);
    }
}
