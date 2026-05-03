using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(DoorScript))]
[CanEditMultipleObjects]
public class ElevatorEditor : Editor
{
    private SerializedProperty teleportUp;
    private SerializedProperty teleportDown;
    private SerializedProperty isUpAllow;
    private SerializedProperty isDownAllow;

    private SerializedProperty isUpUnlocked;
    private SerializedProperty isDownUnlocked;
    private SerializedProperty keyNameUp1;
    private SerializedProperty keyNameUp2;
    private SerializedProperty keyNameDown1;
    private SerializedProperty keyNameDown2;

    private SerializedProperty upLock1;
    private SerializedProperty upLock2;
    private SerializedProperty downLock1;
    private SerializedProperty downLock2;

    private string[] availableKeyNames;
    private int numKeysUp = 1;
    private int numKeysDown = 1;
    private bool showUpLocks = true;
    private bool showDownLocks = true;

    private void OnEnable()
    {
        teleportUp = serializedObject.FindProperty("teleportUp");
        teleportDown = serializedObject.FindProperty("teleportDown");
        isUpAllow = serializedObject.FindProperty("isUpAllow");
        isDownAllow = serializedObject.FindProperty("isDownAllow");

        isUpUnlocked = serializedObject.FindProperty("isUpUnlocked");
        isDownUnlocked = serializedObject.FindProperty("isDownUnlocked");
        keyNameUp1 = serializedObject.FindProperty("keyNameUp1");
        keyNameUp2 = serializedObject.FindProperty("keyNameUp2");
        keyNameDown1 = serializedObject.FindProperty("keyNameDown1");
        keyNameDown2 = serializedObject.FindProperty("keyNameDown2");

        upLock1 = serializedObject.FindProperty("upLock1");
        upLock2 = serializedObject.FindProperty("upLock2");
        downLock1 = serializedObject.FindProperty("downLock1");
        downLock2 = serializedObject.FindProperty("downLock2");

        LoadKeyNames();

        if (!string.IsNullOrEmpty(keyNameUp2.stringValue)) numKeysUp = 2;
        if (!string.IsNullOrEmpty(keyNameDown2.stringValue)) numKeysDown = 2;
    }

    private void LoadKeyNames()
    {
        List<string> keys = new List<string> { "None" };
        if (AssetDatabase.IsValidFolder("Assets/Prefabs/Keys"))
        {
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Prefabs/Keys" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                keys.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }
        availableKeyNames = keys.ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DoorScript door = (DoorScript)target;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Elevator Directions", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isUpAllow, new GUIContent("Allow Up"));
        EditorGUILayout.PropertyField(isDownAllow, new GUIContent("Allow Down"));

        EditorGUILayout.Space();

        if (isUpAllow.boolValue)
        {
            DrawDirectionConfig("Up Configuration", teleportUp, isUpUnlocked, keyNameUp1, keyNameUp2, ref numKeysUp, ref showUpLocks);
        }

        if (isDownAllow.boolValue)
        {
            DrawDirectionConfig("Down Configuration", teleportDown, isDownUnlocked, keyNameDown1, keyNameDown2, ref numKeysDown, ref showDownLocks);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            UpdateIndicators(door);
            UpdateLocksScene(door);
        }

        EditorGUILayout.Space(10);

        base.OnInspectorGUI();
    }

    private void DrawDirectionConfig(string header, SerializedProperty tpProp, SerializedProperty unlockedProp, SerializedProperty key1Prop, SerializedProperty key2Prop, ref int numKeys, ref bool showLocks)
    {
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(tpProp, new GUIContent("Teleport Destination"));

        bool isLocked = !unlockedProp.boolValue;
        bool newIsLocked = EditorGUILayout.Toggle("Is It Locked?", isLocked);
        unlockedProp.boolValue = !newIsLocked;

        if (newIsLocked)
        {
            numKeys = EditorGUILayout.IntSlider("Number of Keys Required", numKeys, 1, 2);

            DrawKeyDropdown("Key 1", key1Prop);
            if (numKeys == 2)
            {
                DrawKeyDropdown("Key 2", key2Prop);
            }
            else
            {
                key2Prop.stringValue = "";
            }

            showLocks = EditorGUILayout.Toggle("Show Locks in Scene?", showLocks);
        }
        else
        {
            key1Prop.stringValue = "";
            key2Prop.stringValue = "";
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawKeyDropdown(string label, SerializedProperty keyProp)
    {
        int currentIndex = 0;
        for (int i = 0; i < availableKeyNames.Length; i++)
        {
            if (availableKeyNames[i] == keyProp.stringValue)
            {
                currentIndex = i;
                break;
            }
        }

        int newIndex = EditorGUILayout.Popup(label, currentIndex, availableKeyNames);
        if (newIndex > 0)
        {
            keyProp.stringValue = availableKeyNames[newIndex];
        }
        else
        {
            keyProp.stringValue = "";
        }
    }

    private void UpdateIndicators(DoorScript door)
    {
        Transform upIndT = door.transform.Find("UpIndicator");
        Transform downIndT = door.transform.Find("DownIndicator");

        bool up = isUpAllow.boolValue;
        bool down = isDownAllow.boolValue;

        if (up && !down)
        {
            if (upIndT)
            {
                Undo.RecordObject(upIndT.gameObject, "Update Up Indicator");
                Undo.RecordObject(upIndT, "Update Up Indicator Transform");
                upIndT.gameObject.SetActive(true);
                upIndT.localPosition = new Vector3(0, 4.05f, 0);
            }
            if (downIndT)
            {
                Undo.RecordObject(downIndT.gameObject, "Update Down Indicator");
                downIndT.gameObject.SetActive(false);
            }
        }
        else if (!up && down)
        {
            if (downIndT)
            {
                Undo.RecordObject(downIndT.gameObject, "Update Down Indicator");
                Undo.RecordObject(downIndT, "Update Down Indicator Transform");
                downIndT.gameObject.SetActive(true);
                downIndT.localPosition = new Vector3(0, 4.05f, 0);
            }
            if (upIndT)
            {
                Undo.RecordObject(upIndT.gameObject, "Update Up Indicator");
                upIndT.gameObject.SetActive(false);
            }
        }
        else if (up && down)
        {
            if (upIndT) 
            { 
                Undo.RecordObject(upIndT.gameObject, "Update Indicator"); 
                if (PrefabUtility.IsPartOfPrefabInstance(upIndT))
                {
                    PrefabUtility.RevertObjectOverride(upIndT, InteractionMode.AutomatedAction);
                }
                upIndT.gameObject.SetActive(true); 
            }
            if (downIndT) 
            { 
                Undo.RecordObject(downIndT.gameObject, "Update Indicator"); 
                if (PrefabUtility.IsPartOfPrefabInstance(downIndT))
                {
                    PrefabUtility.RevertObjectOverride(downIndT, InteractionMode.AutomatedAction);
                }
                downIndT.gameObject.SetActive(true); 
            }

            if(upIndT) { upIndT.gameObject.SetActive(true); }
            if(downIndT) { downIndT.gameObject.SetActive(true); }
        }
        else
        {
            if (upIndT) { Undo.RecordObject(upIndT.gameObject, "Update Indicator"); upIndT.gameObject.SetActive(false); }
            if (downIndT) { Undo.RecordObject(downIndT.gameObject, "Update Indicator"); downIndT.gameObject.SetActive(false); }
        }
    }

    private void UpdateLocksScene(DoorScript door)
    {
        foreach (Transform child in door.transform)
        {
            if (child.name.EndsWith("Keyhole"))
            {
                Undo.RecordObject(child.gameObject, "Hide Keyholes");
                child.gameObject.SetActive(false);
            }
        }

        HashSet<GameObject> usedLocks = new HashSet<GameObject>();

        serializedObject.Update();

        if (isUpAllow.boolValue && !isUpUnlocked.boolValue && showUpLocks)
        {
            upLock1.objectReferenceValue = ActivateLockScene(door, keyNameUp1.stringValue, usedLocks);
            if (numKeysUp == 2)
                upLock2.objectReferenceValue = ActivateLockScene(door, keyNameUp2.stringValue, usedLocks);
        }
        else
        {
            upLock1.objectReferenceValue = null;
            upLock2.objectReferenceValue = null;
        }

        if (isDownAllow.boolValue && !isDownUnlocked.boolValue && showDownLocks)
        {
            downLock1.objectReferenceValue = ActivateLockScene(door, keyNameDown1.stringValue, usedLocks);
            if (numKeysDown == 2)
                downLock2.objectReferenceValue = ActivateLockScene(door, keyNameDown2.stringValue, usedLocks);
        }
        else
        {
            downLock1.objectReferenceValue = null;
            downLock2.objectReferenceValue = null;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private GameObject ActivateLockScene(DoorScript door, string keyName, HashSet<GameObject> usedLocks)
    {
        if (string.IsNullOrEmpty(keyName) || keyName == "None") return null;

        string expectedName = keyName.Replace("Key", "Keyhole");

        foreach (Transform child in door.transform)
        {
            if (child.name.StartsWith(expectedName) && !usedLocks.Contains(child.gameObject))
            {
                Undo.RecordObject(child.gameObject, "Show Keyhole");
                child.gameObject.SetActive(true);
                usedLocks.Add(child.gameObject);
                return child.gameObject;
            }
        }
        return null;
    }
}
