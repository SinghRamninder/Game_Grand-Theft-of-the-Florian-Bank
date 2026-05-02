using UnityEngine;
using UnityEditor;

public class LevelEditor : EditorWindow
{
    private int selectedTab = 0;

    private string[] tabs = {
    "Basic Settings",
    "Guard",
    "Player"};

    private string guardName;
    private GameObject guardSprite;

    private GameObject[] guardPrefabs;
    private string[] guardPrefabNames;
    private int selectedGuardIndex = 0;

    [MenuItem("Tools/Level Editor")]
    public static void OpenWindow()
    {
        GetWindow<LevelEditor>();
    }

    private void OnEnable()
    {
        LoadGuardPrefabs();
    }

    private void LoadGuardPrefabs()
    {
        string folderPath = "Assets/Prefabs/GuardsInitial";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            guardPrefabs = new GameObject[0];
            guardPrefabNames = new string[0];
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        guardPrefabs = new GameObject[guids.Length];
        guardPrefabNames = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            guardPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            guardPrefabNames[i] = guardPrefabs[i].name;
        }
    }

    private void OnGUI()
    {
        selectedTab = GUILayout.Toolbar(selectedTab, tabs);

        GUILayout.Space(10);

        switch (selectedTab)
        {
            case 0:
                DrawBasicSettingsTab();
                break;

            case 1:
                DrawGuardTab();
                break;

            case 2:
                DrawPlayerTab();
                break;
        }
    }

    private void DrawBasicSettingsTab()
    {
        if (GUILayout.Button("Setup Scene"))
        {
            SetupScene();
        }
    }

    private void DrawGuardTab()
    {
        EditorGUILayout.LabelField("Create New Guard", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        guardName = EditorGUILayout.TextField(new GUIContent("Guard Name", "The name of the guard"), guardName);

        EditorGUILayout.Space();

        if (guardPrefabNames != null && guardPrefabNames.Length > 0)
        {
            selectedGuardIndex = EditorGUILayout.Popup(new GUIContent("Guard Sprite", "Select a guard prefab"), selectedGuardIndex, guardPrefabNames);
            guardSprite = guardPrefabs[selectedGuardIndex];

            if (guardSprite != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Prefab Preview:", EditorStyles.boldLabel);
                Texture2D preview = AssetPreview.GetAssetPreview(guardSprite);
                if (preview != null)
                {
                    GUILayout.Label(preview, GUILayout.Width(100), GUILayout.Height(100));
                }
                else
                {
                    GUILayout.Label("(Loading preview...)");
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No Guard prefabs found in 'Assets/Prefabs/GuardsInitial'.", MessageType.Warning);
            if (GUILayout.Button("Refresh Prefabs"))
            {
                LoadGuardPrefabs();
            }
        }
    }

    private void DrawPlayerTab()
    {

    }

    private void SetupScene()
    {

    }
}
