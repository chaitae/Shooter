using UnityEngine;
using UnityEditor;

public class MaterialCreator : EditorWindow
{
    private int numberOfMaterials = 5;
    private Color[] colorList = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
    private Vector2 scrollPosition;
    private string creationMessage = "";

    [MenuItem("Tools/CreateMaterials")]
    private static void ShowWindow()
    {
        GetWindow<MaterialCreator>("Material Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Creator", EditorStyles.boldLabel);

        numberOfMaterials = EditorGUILayout.IntField("Number of Materials", numberOfMaterials);

        EditorGUILayout.Space();
        ResizeColorList();

        EditorGUILayout.LabelField("Color List:");

        // Display a scrollable list of colors
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < colorList.Length; i++)
        {
            colorList[i] = EditorGUILayout.ColorField($"Color {i + 1}", colorList[i]);
        }
        EditorGUILayout.EndScrollView();


        if (GUILayout.Button("Create Materials"))
        {
            CreateMaterials();
        }
        EditorGUILayout.LabelField(creationMessage);

    }
    private void ResizeColorList()
    {
        if (numberOfMaterials < colorList.Length)
        {
            System.Array.Resize(ref colorList, numberOfMaterials);
        }
        else if (numberOfMaterials > colorList.Length)
        {
            Color[] newColorList = new Color[numberOfMaterials];
            colorList.CopyTo(newColorList, 0);
            for (int i = colorList.Length; i < numberOfMaterials; i++)
            {
                newColorList[i] = Color.white; // Default color for additional elements
            }
            colorList = newColorList;
        }
    }
    private string GetUniqueMaterialName(string baseName)
    {
        string uniqueName = baseName;
        int count = 1;

        while (AssetDatabase.LoadAssetAtPath($"Assets/Materials/{uniqueName}.mat", typeof(Material)) != null)
        {
            uniqueName = $"{baseName}_{count}";
            count++;
        }

        return uniqueName;
    }
    private void CreateMaterials()
    {
        creationMessage = "CreatingMaterials";

        for (int i = 0; i < numberOfMaterials; i++)
        {
            string materialName = $"Material_{i + 1}";
            materialName = GetUniqueMaterialName(materialName);
            Material newMaterial = new Material(Shader.Find("Standard"));
            newMaterial.color = colorList[i % colorList.Length];

            AssetDatabase.CreateAsset(newMaterial, $"Assets/Materials/{materialName}.mat");
            Debug.Log($"Created Material: {materialName}");
        }

        AssetDatabase.Refresh();
        creationMessage = "Materials have been created!";

    }
}
