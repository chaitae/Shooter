using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class RenameMaterialByColor : EditorWindow
{
    private Dictionary<Color, string> colorNames;
    private List<Material> materials;

    public string colorDataSpreadsheetPath = "Assets/Scripts/Editor/ColorData.csv";
    private const string prefsKey = "ColorNamesDictionary";

    [MenuItem("Window/Rename Material by Color")]
    private void OnGUI()
    {
        GUILayout.Label("Select a Material and Rename by Color", EditorStyles.boldLabel);
        GUILayout.Label("Selected Materials:", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        // "Add Selected Materials" button
        if (GUILayout.Button("Add Selected Materials"))
        {
            AddSelectedMaterials();
        }

        // "Rename Materials" button
        bool canRenameMaterials = materials.Count > 0;
        GUI.enabled = canRenameMaterials;
        if (GUILayout.Button("Rename Materials"))
        {
            LoadColorDataIfNeeded();
            RenameMaterialAssets();
        }
        GUI.enabled = true; // Reset GUI.enabled to its default value

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear Material List"))
        {
            materials.Clear();
        }

        // Display ObjectFields for each selected material
        for (int i = 0; i < materials.Count; i++)
        {
            materials[i] = EditorGUILayout.ObjectField("Material " + i, materials[i], typeof(Material), true) as Material;
        }
    }

    private void OnEnable()
    {
        colorNames = LoadColorNames();
    }

    private void OnDisable()
    {
        SaveColorNames();
    }

    private void AddSelectedMaterials()
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            if (obj is Material material)
            {
                materials.Add(material);
            }
        }
    }

    private void LoadColorDataIfNeeded()
    {
        if (colorNames.Count == 0)
        {
            LoadColorDataFromSpreadsheet();
        }
    }

    private void LoadColorDataFromSpreadsheet()
    {
        colorNames = new Dictionary<Color, string>();

        try
        {
            TextAsset textAsset = Resources.Load<TextAsset>("ColorData");

            if (textAsset != null)
            {
                string[] lines = textAsset.text.Split('\n');

                foreach (string line in lines)
                {
                    string[] entries = line.Split(',');

                    if (entries.Length >= 5)
                    {
                        float redPercentage = float.Parse(entries[2].TrimEnd('%')) / 100f;
                        float greenPercentage = float.Parse(entries[3].TrimEnd('%')) / 100f;
                        float bluePercentage = float.Parse(entries[4].TrimEnd('%')) / 100f;

                        Color color = new Color(redPercentage, greenPercentage, bluePercentage);
                        string colorName = entries[0];

                        colorNames[color] = colorName;
                    }
                }

                Debug.Log("Color data loaded successfully.");
            }
            else
            {
                Debug.LogError("ColorData.csv not found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading color data: " + e.Message);
        }
    }

    private void RenameMaterialAssets()
    {
        foreach (Material material in materials)
        {
            RenameMaterialAsset(material);
        }
    }

    private void RenameMaterialAsset(Material material)
    {
        if (colorNames == null)
        {
            Debug.LogError("Color data not loaded. Load color data first.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(material);

        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError("Selected material is not an asset.");
            return;
        }

        Color color = material.color;
        string colorName = GetClosestColorName(color);

        colorName = CapitalizeAfterSpace(colorName);
        AssetDatabase.RenameAsset(assetPath, colorName.Replace(" ", ""));

        Debug.Log("Material asset renamed to: " + colorName);
    }

    private string CapitalizeAfterSpace(string input)
    {
        char[] charArray = input.ToCharArray();

        for (int i = 0; i < charArray.Length - 1; i++)
        {
            if (char.IsWhiteSpace(charArray[i]))
            {
                charArray[i + 1] = char.ToUpper(charArray[i + 1]);
            }
        }

        return new string(charArray);
    }

    private string GetClosestColorName(Color targetColor)
    {
        float minDistance = float.MaxValue;
        string closestColorName = "CustomColor"; // Default to "CustomColor" if no match is found

        foreach (var kvp in colorNames)
        {
            Color referenceColor = kvp.Key;
            float distance = Vector3.Distance(ColorToVector3(targetColor), ColorToVector3(referenceColor));

            if (distance < minDistance)
            {
                minDistance = distance;
                closestColorName = kvp.Value;
            }
        }

        return closestColorName;
    }

    private Vector3 ColorToVector3(Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }

    private Dictionary<Color, string> LoadColorNames()
    {
        string json = EditorPrefs.GetString(prefsKey, "{}");
        return JsonUtility.FromJson<Dictionary<Color, string>>(json);
    }

    private void SaveColorNames()
    {
        string json = JsonUtility.ToJson(colorNames);
        EditorPrefs.SetString(prefsKey, json);
    }
}
