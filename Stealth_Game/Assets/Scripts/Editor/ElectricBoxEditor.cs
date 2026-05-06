using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(lasers))]
[CanEditMultipleObjects]
public class ElectricBoxEditor : Editor
{
    private SerializedProperty keyName;
    private SerializedProperty lockTransform;
    private SerializedProperty lightBlinkDuration;
    private SerializedProperty indicatorLight;
    private SerializedProperty lasersGameobject;
    private SerializedProperty instructionText;
    private SerializedProperty instructionKey;

    private string[] availableKeyNames;
    private bool showLock = true;

    private void OnEnable()
    {
        keyName = serializedObject.FindProperty("keyName");
        lockTransform = serializedObject.FindProperty("lockTransform");
        lightBlinkDuration = serializedObject.FindProperty("lightBlinkDuration");
        indicatorLight = serializedObject.FindProperty("indicatorLight");
        lasersGameobject = serializedObject.FindProperty("lasersGameobject");
        instructionText = serializedObject.FindProperty("instructionText");
        instructionKey = serializedObject.FindProperty("instructionKey");

        LoadKeyNames();
    }

    private void LoadKeyNames()
    {
        List<string> keys = new List<string>();
        if (AssetDatabase.IsValidFolder("Assets/Prefabs/Keys"))
        {
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Prefabs/Keys" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                keys.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }

        // Just in case the folder is empty
        if (keys.Count == 0)
        {
            keys.Add("No Keys Found");
        }

        availableKeyNames = keys.ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        lasers laserScript = (lasers)target;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Unlock Settings", EditorStyles.boldLabel);

        int currentIndex = 0;
        for (int i = 0; i < availableKeyNames.Length; i++)
        {
            if (availableKeyNames[i] == keyName.stringValue)
            {
                currentIndex = i;
                break;
            }
        }

        int newIndex = EditorGUILayout.Popup("Key Required", currentIndex, availableKeyNames);
        if (availableKeyNames.Length > 0 && availableKeyNames[0] != "No Keys Found")
        {
            keyName.stringValue = availableKeyNames[newIndex];
        }

        showLock = EditorGUILayout.Toggle("Show Lock in Scene?", showLock);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Base Settings & References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(lightBlinkDuration);
        EditorGUILayout.PropertyField(indicatorLight);
        EditorGUILayout.PropertyField(lasersGameobject);
        EditorGUILayout.PropertyField(instructionText);
        EditorGUILayout.PropertyField(instructionKey);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            UpdateLockScene(laserScript);
        }
    }

    private void UpdateLockScene(lasers laserScript)
    {
        foreach (Transform child in laserScript.transform)
        {
            if (child.name.EndsWith("Keyhole"))
            {
                Undo.RecordObject(child.gameObject, "Hide Keyhole");
                child.gameObject.SetActive(false);
            }
        }

        serializedObject.Update();

        string currentKey = keyName.stringValue;
        if (!string.IsNullOrEmpty(currentKey) && currentKey != "No Keys Found")
        {
            string expectedName = currentKey.Replace("Key", "Keyhole");
            bool found = false;

            foreach (Transform child in laserScript.transform)
            {
                if (child.name.StartsWith(expectedName))
                {
                    if (showLock)
                    {
                        Undo.RecordObject(child.gameObject, "Show Keyhole");
                        child.gameObject.SetActive(true);
                    }
                    lockTransform.objectReferenceValue = child.gameObject;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                lockTransform.objectReferenceValue = null;
            }
        }
        else
        {
            lockTransform.objectReferenceValue = null;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
