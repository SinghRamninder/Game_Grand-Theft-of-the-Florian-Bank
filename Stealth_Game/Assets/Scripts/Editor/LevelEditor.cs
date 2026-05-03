using UnityEngine;
using UnityEditor;

public class LevelEditor : EditorWindow
{
    private int selectedTab = 0;

    private string[] tabs = {
    "Basic Settings",
    "Guard"};

    private string guardName;
    private GameObject guardSprite;

    private GameObject[] guardPrefabs;
    private string[] guardPrefabNames;
    private int selectedGuardIndex = 0;

    private float guardSpeed = 2f;
    private float chaseSpeed = 4f;
    private float turnDelayAfterHearing = 1f;
    private float lookDuration = 2f;
    private float waitTimeAtTarget = 2f;
    private float chaseLoseSightDuration = 2f;

    private bool attachKey = false;
    private GameObject[] keyPrefabs;
    private string[] keyPrefabNames;
    private int selectedKeyIndex = 0;
    private bool keyGlow = false;

    [MenuItem("Tools/Level Editor")]
    public static void OpenWindow()
    {
        GetWindow<LevelEditor>();
    }

    private void OnEnable()
    {
        LoadGuardPrefabs();
        LoadKeyPrefabs();
    }

    private void LoadKeyPrefabs()
    {
        string folderPath = "Assets/Prefabs/Keys";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            keyPrefabs = new GameObject[0];
            keyPrefabNames = new string[0];
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        keyPrefabs = new GameObject[guids.Length];
        keyPrefabNames = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            keyPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            keyPrefabNames[i] = keyPrefabs[i].name;
        }
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
                EditorGUILayout.LabelField("Guard Preview:", EditorStyles.boldLabel);
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
                LoadKeyPrefabs();
            }
        }

        EditorGUILayout.Space();

        guardSpeed = EditorGUILayout.FloatField(new GUIContent("Speed", "Control the movement speed of the guard"), guardSpeed);
        chaseSpeed = EditorGUILayout.FloatField(new GUIContent("Chase Speed", "Guard speed during chase"), chaseSpeed);
        turnDelayAfterHearing = EditorGUILayout.FloatField(new GUIContent("Turn Delay After Hearing", "Turn delay after hearing a noise"), turnDelayAfterHearing);
        lookDuration = EditorGUILayout.FloatField(new GUIContent("Look Duration", "Duration the guard will look around"), lookDuration);
        waitTimeAtTarget = EditorGUILayout.FloatField(new GUIContent("Wait Time At Target", "Wait time when guard reaches a point"), waitTimeAtTarget);
        chaseLoseSightDuration = EditorGUILayout.FloatField(new GUIContent("Chase Lose Sight Duration", "Duration to drop chase after losing sight"), chaseLoseSightDuration);

        EditorGUILayout.Space();

        attachKey = EditorGUILayout.Toggle(new GUIContent("Attach Key", "Should the guard have a key?"), attachKey);
        if (attachKey)
        {
            if (keyPrefabNames != null && keyPrefabNames.Length > 0)
            {
                selectedKeyIndex = EditorGUILayout.Popup(new GUIContent("Select Key", "The key prefab to attach"), selectedKeyIndex, keyPrefabNames);
                keyGlow = EditorGUILayout.Toggle(new GUIContent("Key Glow", "Should the key glow?"), keyGlow);

                GameObject selectedKeySprite = keyPrefabs[selectedKeyIndex];
                if (selectedKeySprite != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Key Preview:", EditorStyles.boldLabel);
                    Texture2D keyPreview = AssetPreview.GetAssetPreview(selectedKeySprite);
                    if (keyPreview != null)
                    {
                        GUILayout.Label(keyPreview, GUILayout.Width(100), GUILayout.Height(100));
                    }
                    else
                    {
                        GUILayout.Label("(Loading preview...)");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No Key prefabs found in 'Assets/Prefabs/Keys'.", MessageType.Warning);
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Create Guard"))
        {
            CreateGuard();
        }
    }

    private void CreateGuard()
    {
        if (string.IsNullOrEmpty(guardName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a Guard Name.", "OK");
            return;
        }

        if (guardSprite == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Guard Prefab.", "OK");
            return;
        }

        GameObject parentInstance = new GameObject(guardName);
        GuardParent guardParent = parentInstance.AddComponent<GuardParent>();
        parentInstance.AddComponent<VisionConeParent>();

        GameObject childInstance = (GameObject)PrefabUtility.InstantiatePrefab(guardSprite, parentInstance.transform);
        childInstance.name = guardSprite.name;
        childInstance.transform.localPosition = Vector3.zero;

        SecurityOfficerScript officerScript = childInstance.GetComponent<SecurityOfficerScript>();
        if (officerScript != null)
        {
            guardParent.securityOfficerScriptChild = officerScript;

            SerializedObject so = new SerializedObject(officerScript);
            so.Update();
            so.FindProperty("speed").floatValue = guardSpeed;
            so.FindProperty("chaseSpeed").floatValue = chaseSpeed;
            so.FindProperty("turnDelayAfterHearing").floatValue = turnDelayAfterHearing;
            so.FindProperty("lookDuration").floatValue = lookDuration;
            so.FindProperty("waitTimeAtTarget").floatValue = waitTimeAtTarget;
            so.FindProperty("chaseLoseSightDuration").floatValue = chaseLoseSightDuration;
            so.FindProperty("hasKey").boolValue = attachKey;

            LevelReferences activeRefs = UnityEngine.Object.FindFirstObjectByType<LevelReferences>();
            if (activeRefs != null)
            {
                if (activeRefs.gameOverDisplay != null)
                {
                    so.FindProperty("gameOverDisplay").objectReferenceValue = activeRefs.gameOverDisplay;
                }
                else
                {
                    Debug.LogWarning("LevelReferences script was found in the scene, but its gameOverDisplay reference is null.");
                }
            }
            else
            {
                Debug.LogWarning("Could not find a LevelReferences script in the current active scene to set gameOverDisplay.");
            }

            so.ApplyModifiedProperties();

            if (attachKey && keyPrefabs != null && keyPrefabs.Length > selectedKeyIndex)
            {
                Transform keyHolder = childInstance.transform.Find("KeyHolder");
                if (keyHolder != null)
                {
                    GameObject keyPrefab = keyPrefabs[selectedKeyIndex];
                    GameObject keyInstance = (GameObject)PrefabUtility.InstantiatePrefab(keyPrefab, keyHolder);
                    keyInstance.transform.localPosition = Vector3.zero;

                    so.Update();
                    so.FindProperty("key").objectReferenceValue = keyInstance;
                    so.FindProperty("keyName").stringValue = keyPrefab.name;
                    so.ApplyModifiedProperties();

                    KeyGlow keyGlowScript = keyInstance.GetComponentInChildren<KeyGlow>();
                    if (keyGlowScript != null)
                    {
                        SerializedObject soKey = new SerializedObject(keyGlowScript);
                        soKey.Update();
                        soKey.FindProperty("keyGlow").boolValue = keyGlow;
                        soKey.ApplyModifiedProperties();
                    }
                }
                else
                {
                    Debug.LogWarning("KeyHolder object not found as a child of the guard prefab.");
                }
            }

            GameObject pointA = new GameObject("Point A");
            pointA.transform.SetParent(parentInstance.transform);
            pointA.transform.localPosition = new Vector3(-30, 0, 0);
            pointA.transform.localScale = new Vector3(6.66f, 6.66f, 6.66f);
            SpriteRenderer srA = pointA.AddComponent<SpriteRenderer>();
            srA.color = GetColorFromHex("#FF5D00");

            GameObject pointB = new GameObject("Point B");
            pointB.transform.SetParent(parentInstance.transform);
            pointB.transform.localPosition = new Vector3(30, 0, 0);
            pointB.transform.localScale = new Vector3(6.66f, 6.66f, 6.66f);
            SpriteRenderer srB = pointB.AddComponent<SpriteRenderer>();
            srB.color = GetColorFromHex("#FF5D00");

            string[] circleGuids = AssetDatabase.FindAssets("Circle t:Sprite");
            if (circleGuids.Length > 0)
            {
                Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(circleGuids[0]));
                srA.sprite = circleSprite;
                srB.sprite = circleSprite;
            }

            so.Update();
            so.FindProperty("pointA").objectReferenceValue = pointA;
            so.FindProperty("pointB").objectReferenceValue = pointB;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("The selected Guard Prefab does not have a SecurityOfficerScript attached.");
        }

        string saveFolderPath = "Assets/LEVEL EDITOR/Guards";
        if (!AssetDatabase.IsValidFolder(saveFolderPath))
        {
            string[] folderDirs = saveFolderPath.Split('/');
            string currentPath = folderDirs[0];
            for (int i = 1; i < folderDirs.Length; i++)
            {
                string nextPath = currentPath + "/" + folderDirs[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folderDirs[i]);
                }
                currentPath = nextPath;
            }
        }

        string prefabPath = $"{saveFolderPath}/{parentInstance.name}.prefab";
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(parentInstance, prefabPath, InteractionMode.UserAction);

        Undo.RegisterCreatedObjectUndo(parentInstance, "Create Guard");
        Selection.activeGameObject = parentInstance;
    }

    private Color GetColorFromHex(string hex)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    private void SetupScene()
    {

    }
}
