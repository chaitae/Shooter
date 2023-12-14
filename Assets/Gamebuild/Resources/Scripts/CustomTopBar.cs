#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;
using Gamebuild.Scripts;

static class ToolbarStyles
{
    public static readonly GUIStyle commandButtonStyle;

    static ToolbarStyles()
    {
        commandButtonStyle = new GUIStyle("Command")
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            imagePosition = ImagePosition.ImageAbove,
            fontStyle = FontStyle.Bold
        };
    }
}

[InitializeOnLoad]
public class SceneSwitchLeftButton
{
    static SceneSwitchLeftButton()
    {
        ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
    }

    static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();
        GUI.color = Color.white * 0.75f;
        GUI.contentColor = Color.white * 1.15f;

        if (GUILayout.Button(new GUIContent("Build with Gamebuild", EditorGUIUtility.IconContent("Update-Available.png").image, "Build and upload to gamebuild.io")))
        {
            Debug.Log("Test");

            // Check if the instance exists
            if (GameBuildEditorWindow.Instance == null)
            {
                // Create a new instance if it doesn't exist
                GameBuildEditorWindow.CreateInstance<GameBuildEditorWindow>();
            }

            // Now call the OnBuildPressed method
            GameBuildEditorWindow.Instance.OnBuildPressed();
        }
    }
}


#endif