using UnityEngine;
using System.Collections.Generic;

public class DebugGUI : MonoBehaviour
{
    // Singleton instance
    private static DebugGUI instance;

    // Example variables for debugging
    private bool showDebugInfo = false;

    // Log variables
    private static List<string> debugLog = new List<string>();
    private Vector2 scrollPosition = Vector2.zero;
    private const int maxLogEntries = 50; // Adjust as needed

    // Background color for the scrollable log
    private Color logBackgroundColor = new Color(0f, 0f, 0f, 0.3f);

    // Singleton instance property
    public static DebugGUI Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DebugGUI");
                instance = go.AddComponent<DebugGUI>();
            }
            return instance;
        }
    }
    private void Awake()
    {
        // Ensure only one instance exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Update()
    {
        // Toggle debug info on/off
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebugInfo = !showDebugInfo;
        }

    }

    void OnGUI()
    {
        if (showDebugInfo)
        {
            DrawDebugLog();
        }

        // Draw the scrollable debug log at the bottom-left
    }

    // Log a message to the debug log
    public static void LogMessage(string message)
    {

        if (instance == null)
        {
            GameObject go = new GameObject("DebugGUI");
            instance = go.AddComponent<DebugGUI>();
        }
        debugLog.Add(message);

        // Trim log if it exceeds the maximum number of entries
        if (debugLog.Count > maxLogEntries)
        {
            debugLog.RemoveAt(0);
        }
    }

    // Draw the scrollable debug log with a black background
    private void DrawDebugLog()
    {
        float logWidth = Screen.width * 0.4f;
        float logHeight = Screen.height * 0.3f;

        // Draw background
        GUI.color = logBackgroundColor;
        GUI.DrawTexture(new Rect(0, Screen.height - logHeight, logWidth, logHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Draw log content
        scrollPosition = GUI.BeginScrollView(new Rect(0, Screen.height - logHeight, logWidth, logHeight),
                                             scrollPosition,
                                             new Rect(0, 0, logWidth - 20, debugLog.Count * 20));

        for (int i = 0; i < debugLog.Count; i++)
        {
            GUI.Label(new Rect(10, i * 20, logWidth - 20, 20), debugLog[i]);
        }

        GUI.EndScrollView();
    }
}
