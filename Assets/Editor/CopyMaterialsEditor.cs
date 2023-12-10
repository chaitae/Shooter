using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class CopyMeshRendererEditor : EditorWindow
{
    private GameObject sourceObject;
    private List<GameObject> targetObjects = new List<GameObject>();

    [MenuItem("Window/Copy MeshRenderer Editor")]
    public static void ShowWindow()
    {
        GetWindow<CopyMeshRendererEditor>("Copy MeshRenderer Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Copy MeshRenderer from Source to Targets", EditorStyles.boldLabel);

        sourceObject = EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true) as GameObject;

        GUILayout.Space(10);

        if (GUILayout.Button("Add Selected Objects to Targets") && sourceObject != null)
        {
            AddSelectedObjectsToTargets();
        }

        if (GUILayout.Button("Clear Target List"))
        {
            ClearTargetList();
        }

        EditorGUILayout.LabelField("Target Objects");

        for (int i = 0; i < targetObjects.Count; i++)
        {
            targetObjects[i] = EditorGUILayout.ObjectField("Target " + (i + 1), targetObjects[i], typeof(GameObject), true) as GameObject;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Copy MeshRenderer") && sourceObject != null && targetObjects.Count > 0)
        {
            CopyMeshRenderer();
        }
    }

    private void AddSelectedObjectsToTargets()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length > 0)
        {
            targetObjects.AddRange(selectedObjects);
        }
    }

    private void ClearTargetList()
    {
        targetObjects.Clear();
    }

    private void CopyMeshRenderer()
    {
        MeshRenderer sourceRenderer = sourceObject.GetComponent<MeshRenderer>();

        if (sourceRenderer == null)
        {
            Debug.LogError("Source object does not have a MeshRenderer component.");
            return;
        }

        foreach (var targetObject in targetObjects)
        {
            if (targetObject == null)
            {
                Debug.LogError("One or more target objects are not assigned.");
                continue;
            }

            MeshRenderer targetRenderer = targetObject.GetComponent<MeshRenderer>();

            if (targetRenderer == null)
            {
                Debug.LogError("Target object does not have a MeshRenderer component: " + targetObject.name);
                continue;
            }

            // Copy the entire MeshRenderer component
            EditorUtility.CopySerialized(sourceRenderer, targetRenderer);

            // Mark the scene as dirty for each target object
            EditorUtility.SetDirty(targetObject);
            EditorSceneManager.MarkSceneDirty(targetObject.scene);
        }

        Debug.Log("MeshRenderer copied to targets successfully!");
    }
}
