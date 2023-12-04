using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class RandomlySetMaterial : EditorWindow
{
    private List<Material> materialList = new List<Material>();
    [MenuItem("Tools/RandomlySetMaterial")]
    public static void ShowWindow()
    {
        GetWindow<RandomlySetMaterial>("RandomlySetMaterial");
    }

    private void OnGUI()
    {
        //GUILayout.Label("Material List Editor", EditorStyles.boldLabel);

        // Display the list of materials
        DisplayMaterialList();

        // Add Material button
        if (GUILayout.Button("Add Material"))
        {
            AddMaterial();
        }
        if (GUILayout.Button("RandomlySetMaterial"))
        {
            SetRandomMaterial();
        }
    }

    private void DisplayMaterialList()
    {
        if (materialList.Count == 0)
        {
            GUILayout.Label("No materials in the list.");
            return;
        }

        EditorGUILayout.Space();

        for (int i = 0; i < materialList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            materialList[i] = EditorGUILayout.ObjectField("Material " + (i + 1), materialList[i], typeof(Material), false) as Material;

            // Remove Material button for each material
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                RemoveMaterial(i);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void AddMaterial()
    {
        materialList.Add(null);
    }
    private void SetRandomMaterial()
    {
        if (materialList.Count > 0)
        {
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                Renderer renderer = selectedObject.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material randomMaterial = materialList[UnityEngine.Random.Range(0, materialList.Count)];
                    renderer.material = randomMaterial;
                    EditorSceneManager.MarkSceneDirty(selectedObject.scene);

                }
                else
                {
                    Debug.LogWarning("Selected GameObject does not have a Renderer component.");
                }
            }
        }
        else
        {
            Debug.LogWarning("No materials in the list to set.");
        }
    }
    private void RemoveMaterial(int index)
    {
        if (index >= 0 && index < materialList.Count)
        {
            materialList.RemoveAt(index);
        }
    }
}
