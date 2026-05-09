using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class LevelEditor : EditorWindow
{
    private int selectedTab = 0;

    private string[] tabs = {
    "Basic Settings",
    "Guard"};


    private bool useCheckpoint = false;
    private bool useStartCutscene = false;
    private bool useEndManager = false;
    private bool useEndCutscene = false;
    private bool useEndCredits = false;

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
        LevelEditor window = GetWindow<LevelEditor>();
        window.minSize = new Vector2(400, 500); // Give the window some width so buttons aren't as stretched out
        window.Show();
    }

    private bool unsavedChanges = false;

    private void OnEnable()
    {
        LoadGuardPrefabs();
        LoadKeyPrefabs();
        SyncToggleStates();
    }

    private void OnHierarchyChange()
    {
        if (!unsavedChanges)
        {
            SyncToggleStates();
        }
    }

    private void SyncToggleStates()
    {
        useCheckpoint = false;
        useStartCutscene = false;
        useEndManager = false;
        useEndCutscene = false;
        useEndCredits = false;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                continue;

            // Ensure it's in the scene
            if (!EditorUtility.IsPersistent(obj.transform.root.gameObject))
            {
                if (obj.name == "Checkpoint Manager" && obj.activeInHierarchy)
                    useCheckpoint = true;
                
                if (obj.name == "Start Cutscene Manager" && obj.activeInHierarchy)
                    useStartCutscene = true;

                if (obj.name == "End Manager" && obj.activeInHierarchy)
                    useEndManager = true;

                if (obj.name == "End cutscene manager" && obj.activeInHierarchy)
                    useEndCutscene = true;

                if (obj.name == "End credits manager" && obj.activeInHierarchy)
                    useEndCredits = true;
            }
        }
        
        Repaint();
    }

    private void LoadKeyPrefabs()
    {
        string folderPath = "Assets/LEVEL EDITOR/Keys";
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

    private void OnDestroy()
    {
        if (unsavedChanges)
        {
            if (EditorUtility.DisplayDialog("Unsaved Changes", "You have unsaved changes in the Level Editor. Do you want to save them before closing?", "Save", "Don't Save"))
            {
                SaveAllSettings();
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

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
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Scene Initialization", EditorStyles.boldLabel);
        if (GUILayout.Button("Setup Scene", GUILayout.Height(30)))
        {
            SetupScene();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Scene Features", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        useStartCutscene = EditorGUILayout.Toggle("Enable Start Cutscene", useStartCutscene);
        if (EditorGUI.EndChangeCheck())
        {
            unsavedChanges = true;
        }

        if (useStartCutscene)
        {
            GameObject startManagerObj = FindObjectInScene("Start Cutscene Manager");
            if (startManagerObj != null && startManagerObj.activeInHierarchy)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Edit Cutscene"))
                {
                    Selection.activeGameObject = startManagerObj;
                    if (SceneView.lastActiveSceneView != null)
                    {
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        useEndManager = EditorGUILayout.Toggle("Enable End Manager", useEndManager);
        if (EditorGUI.EndChangeCheck())
        {
            unsavedChanges = true;
        }

        if (useEndManager)
        {
            GameObject endManagerObj = FindObjectInScene("End Manager");
            if (endManagerObj != null && endManagerObj.activeInHierarchy)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Edit End Manager"))
                {
                    Selection.activeGameObject = endManagerObj;
                    if (SceneView.lastActiveSceneView != null)
                        SceneView.lastActiveSceneView.FrameSelected();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            useEndCutscene = EditorGUILayout.Toggle("Enable End Cutscene", useEndCutscene);
            if (EditorGUI.EndChangeCheck()) unsavedChanges = true;

            if (useEndCutscene)
            {
                GameObject ecm = FindObjectInScene("End cutscene manager");
                if (ecm != null && ecm.activeInHierarchy)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button("Edit End Cutscene"))
                    {
                        Selection.activeGameObject = ecm;
                        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            useEndCredits = EditorGUILayout.Toggle("Enable End Credits", useEndCredits);
            if (EditorGUI.EndChangeCheck()) unsavedChanges = true;

            if (useEndCredits)
            {
                GameObject ecredm = FindObjectInScene("End credits manager");
                if (ecredm != null && ecredm.activeInHierarchy)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button("Edit End Credits"))
                    {
                        Selection.activeGameObject = ecredm;
                        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        useCheckpoint = EditorGUILayout.Toggle("Enable Checkpoint", useCheckpoint);
        if (EditorGUI.EndChangeCheck())
        {
            unsavedChanges = true;
        }

        if (useCheckpoint)
        {
            GameObject managerObj = FindObjectInScene("Checkpoint Manager");
            if (managerObj != null && managerObj.activeInHierarchy && managerObj.transform.Find("Checkpoint1") != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Set Checkpoint Position"))
                {
                    GameObject cp1 = managerObj.transform.Find("Checkpoint1").gameObject;
                    Selection.activeGameObject = cp1;
                    if (SceneView.lastActiveSceneView != null)
                    {
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save Settings", GUILayout.Height(30)))
        {
            SaveAllSettings();
        }
    }

    private void SaveAllSettings()
    {
        if (useStartCutscene)
        {
            CreateStartCutsceneManager();
        }
        else
        {
            DisableStartCutsceneManager();
        }

        if (useEndManager)
        {
            GameObject endMgr = CreateEndManager();
            if (useEndCutscene) CreateEndCutsceneManager(endMgr.transform); else DisableEndCutsceneManager();
            if (useEndCredits) CreateEndCreditsManager(endMgr.transform); else DisableEndCreditsManager();
        }
        else
        {
            DisableEndCutsceneManager();
            DisableEndCreditsManager();
            DisableEndManager();
        }

        if (useCheckpoint)
        {
            CreateCheckpointManager();
        }
        else
        {
            DisableCheckpointManager();
        }

        unsavedChanges = false;
        SyncToggleStates();
        EditorUtility.DisplayDialog("Saved", "Settings have been saved.", "OK");
    }

    private GameObject FindObjectInScene(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                continue;

            if (!EditorUtility.IsPersistent(obj.transform.root.gameObject) && obj.name == name)
            {
                return obj;
            }
        }
        return null;
    }

    private void DisableStartCutsceneManager()
    {
        GameObject manager = FindObjectInScene("Start Cutscene Manager");
        if (manager != null)
        {
            Undo.RecordObject(manager, "Disable Start Cutscene Manager");
            manager.SetActive(false);
        }
    }

    private void CreateStartCutsceneManager()
    {
        GameObject existingManager = FindObjectInScene("Start Cutscene Manager");
        if (existingManager != null)
        {
            Undo.RecordObject(existingManager, "Enable Start Cutscene Manager");
            existingManager.SetActive(true);
            return;
        }

        GameObject manager = new GameObject("Start Cutscene Manager");
        manager.AddComponent<LevelStart>();
        Undo.RegisterCreatedObjectUndo(manager, "Create Start Cutscene Manager");

        Selection.activeGameObject = manager;
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }
    }

    private GameObject CreateEndManager()
    {
        GameObject existingManager = FindObjectInScene("End Manager");
        if (existingManager != null)
        {
            Undo.RecordObject(existingManager, "Enable End Manager");
            existingManager.SetActive(true);
            return existingManager;
        }

        GameObject manager = new GameObject("End Manager");
        manager.AddComponent<EndManager>();
        BoxCollider2D col = manager.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        Undo.RegisterCreatedObjectUndo(manager, "Create End Manager");

        return manager;
    }

    private void DisableEndManager()
    {
        GameObject manager = FindObjectInScene("End Manager");
        if (manager != null)
        {
            Undo.RecordObject(manager, "Disable End Manager");
            manager.SetActive(false);
        }
    }

    private void CreateEndCutsceneManager(Transform parent)
    {
        GameObject existingManager = FindObjectInScene("End cutscene manager");
        if (existingManager != null)
        {
            Undo.RecordObject(existingManager, "Enable End cutscene manager");
            existingManager.SetActive(true);
            existingManager.transform.SetParent(parent);
            return;
        }

        GameObject manager = new GameObject("End cutscene manager");
        manager.transform.SetParent(parent);
        manager.AddComponent<EndCutsceneManager>();
        Undo.RegisterCreatedObjectUndo(manager, "Create End cutscene manager");
    }

    private void DisableEndCutsceneManager()
    {
        GameObject manager = FindObjectInScene("End cutscene manager");
        if (manager != null)
        {
            Undo.RecordObject(manager, "Disable End cutscene manager");
            manager.SetActive(false);
        }
    }

    private void CreateEndCreditsManager(Transform parent)
    {
        GameObject existingManager = FindObjectInScene("End credits manager");
        if (existingManager != null)
        {
            Undo.RecordObject(existingManager, "Enable End credits manager");
            existingManager.SetActive(true);
            existingManager.transform.SetParent(parent);
            return;
        }

        GameObject manager = new GameObject("End credits manager");
        manager.transform.SetParent(parent);
        EndCreditsManager ecm = manager.AddComponent<EndCreditsManager>();
        Undo.RegisterCreatedObjectUndo(manager, "Create End credits manager");

        // Spawn EndCreditsCanvas prefab as child
        string prefabPath = "Assets/Prefabs/RequiredObjects/EndCreditsCanvas.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            GameObject canvasInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, manager.transform);
            canvasInstance.transform.localPosition = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(canvasInstance, "Create EndCreditsCanvas");
            
            // Assign BlackFade Image natively if we found it
            Image blackFade = canvasInstance.GetComponentInChildren<Image>(true); // Might need a more precise search if multiple exist, but usually it's correct
            if (blackFade != null)
            {
                ecm.blackFadeImage = blackFade;
            }
        }
        else
        {
            Debug.LogWarning("Could not find EndCreditsCanvas prefab at " + prefabPath);
        }
    }

    private void DisableEndCreditsManager()
    {
        GameObject manager = FindObjectInScene("End credits manager");
        if (manager != null)
        {
            Undo.RecordObject(manager, "Disable End credits manager");
            manager.SetActive(false);
        }
    }

    private void DisableCheckpointManager()
    {
        GameObject manager = FindObjectInScene("Checkpoint Manager");
        if (manager != null)
        {
            Undo.RecordObject(manager, "Disable Checkpoint Manager");
            manager.SetActive(false);
        }
    }

    private void CreateCheckpointManager()
    {
        GameObject existingManager = FindObjectInScene("Checkpoint Manager");
        if (existingManager != null)
        {
            Undo.RecordObject(existingManager, "Enable Checkpoint Manager");
            existingManager.SetActive(true);
            return; 
        }

        GameObject manager = new GameObject("Checkpoint Manager");
        manager.AddComponent<CheckPoint>();

        GameObject cp1 = new GameObject("Checkpoint1");
        cp1.transform.SetParent(manager.transform);

        try 
        {
            cp1.tag = "Checkpoint1";
        }
        catch (UnityException e)
        {
            Debug.LogWarning("Tag 'Checkpoint1' is not defined in the Tags & Layers. Please add it first. " + e.Message);
        }

        BoxCollider2D collider = cp1.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        cp1.transform.localPosition = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(manager, "Create Checkpoint Manager");

        Selection.activeGameObject = cp1;
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
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
                EditorGUILayout.HelpBox("No Key prefabs found in 'Assets/LEVEL EDITOR/Keys'.", MessageType.Warning);
            }
        }

        EditorGUILayout.Space();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create Guard", GUILayout.Height(30)))
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
        VisionConeParent visionCone = parentInstance.AddComponent<VisionConeParent>();

        GameObject childInstance = (GameObject)PrefabUtility.InstantiatePrefab(guardSprite, parentInstance.transform);
        childInstance.name = guardSprite.name;
        childInstance.transform.localPosition = Vector3.zero;

        visionCone.visionConeParent = parentInstance.GetComponentInChildren<VisionCone2D>();

        if (visionCone.visionConeParent != null)
        {
            visionCone.visionConeParent.useObstacles = true;
            visionCone.visionConeParent.obstacleMask = LayerMask.GetMask("Walls");
        }
        else
        {
            Debug.LogWarning("VisionCone2D was not found in the instantiated guard prefab.");
        }

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
        if (!EditorUtility.DisplayDialog("Warning", "This will delete all existing gameobjects in the scene. Proceed?", "OK", "Cancel"))
        {
            return;
        }

        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go != null && go.transform.parent == null)
            {
                DestroyImmediate(go);
            }
        }

        GameObject mainCamera = new GameObject("MainCamera");
        mainCamera.tag = "MainCamera";
        Camera cam = mainCamera.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 984.2f;

        mainCamera.AddComponent<AudioListener>();

        System.Type urpCamType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (urpCamType != null)
        {
            Component urpData = mainCamera.AddComponent(urpCamType);
            SerializedObject so = new SerializedObject(urpData);
            so.Update();
            SerializedProperty renderType = so.FindProperty("m_RenderType");
            if (renderType != null) renderType.intValue = 0; // Base
            so.ApplyModifiedProperties();
        }

        System.Type brainType = System.Type.GetType("Unity.Cinemachine.CinemachineBrain, Unity.Cinemachine");
        if (brainType == null) brainType = System.Type.GetType("Cinemachine.CinemachineBrain, Cinemachine");
        if (brainType != null)
        {
            Component brain = mainCamera.AddComponent(brainType);
            SerializedObject so = new SerializedObject(brain);
            so.Update();
            SerializedProperty frustum = so.FindProperty("m_ShowCameraFrustum");
            if (frustum != null) frustum.boolValue = true;
            so.ApplyModifiedProperties();
        }

        GameObject cinemachineCam = new GameObject("CinemachineCamera");
        System.Type cmCamType = System.Type.GetType("Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine");
        Component vcam = null;
        if (cmCamType != null)
        {
            vcam = cinemachineCam.AddComponent(cmCamType);
            SerializedObject so = new SerializedObject(vcam);
            so.Update();
            SerializedProperty lensSize = so.FindProperty("Lens.OrthographicSize");
            if (lensSize == null) lensSize = so.FindProperty("m_Lens.OrthographicSize");
            if (lensSize != null) lensSize.floatValue = 6f;
            so.ApplyModifiedProperties();

            System.Type composerType = System.Type.GetType("Unity.Cinemachine.CinemachinePositionComposer, Unity.Cinemachine");
            if (composerType != null)
            {
                Component composer = cinemachineCam.AddComponent(composerType);
                SerializedObject cSo = new SerializedObject(composer);
                cSo.Update();
                SerializedProperty sx = cSo.FindProperty("Composition.ScreenPosition.x");
                if (sx == null) sx = cSo.FindProperty("m_ScreenPosition.x");
                if (sx != null) sx.floatValue = -0.03f;
                SerializedProperty sy = cSo.FindProperty("Composition.ScreenPosition.y");
                if (sy == null) sy = cSo.FindProperty("m_ScreenPosition.y");
                if (sy != null) sy.floatValue = 0.19f;
                cSo.ApplyModifiedProperties();
            }
        }
        else
        {
            System.Type vcamType = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
            if (vcamType != null)
            {
                vcam = cinemachineCam.AddComponent(vcamType);
                SerializedObject so = new SerializedObject(vcam);
                so.Update();
                SerializedProperty lensSize = so.FindProperty("m_Lens.OrthographicSize");
                if (lensSize != null) lensSize.floatValue = 6f;
                so.ApplyModifiedProperties();
            }
        }

        // Global Light 2D is now spawned automatically via the RequiredObjects loop below

        GameObject levelRefMgr = new GameObject("Level Reference Manager");
        LevelReferences levelRefs = levelRefMgr.AddComponent<LevelReferences>();

        GameObject keyInventoryInst = null;
        GameObject pauseMenuInst = null;
        GameObject audioManagerInst = null;

        string reqFolderPath = "Assets/Prefabs/RequiredObjects";
        if (AssetDatabase.IsValidFolder(reqFolderPath))
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { reqFolderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    // Match against expected components/names based on your instructions
                    if (inst.name.Contains("Guard caught"))
                    {
                        levelRefs.gameOverDisplay = inst;
                        inst.SetActive(false);
                    }
                    else if (inst.name.Contains("Timer"))
                    {
                        levelRefs.timerDisplay = inst;
                        inst.SetActive(false);
                    }
                    else if (inst.name.Contains("TimesUP"))
                    {
                        levelRefs.timesUpDisplay = inst;
                        inst.SetActive(false);
                    }
                    else if (inst.name.Contains("PauseMenu"))
                    {
                        pauseMenuInst = inst;
                        inst.SetActive(false);
                    }
                    else if (inst.name.Contains("AudioManager"))
                    {
                        audioManagerInst = inst;
                    }
                    else if (inst.name.Contains("Instruction"))
                    {
                        Transform tBox = inst.transform.Find("FindElectricBox");
                        if (tBox != null) levelRefs.laserInstructionText = tBox.gameObject;
                        Transform tDeact = inst.transform.Find("LasersDeactiavte");
                        if (tDeact != null) levelRefs.laserDeactivatedText = tDeact.gameObject;

                        foreach (Transform child in inst.transform)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                    else if (inst.name.Contains("KeyInventory"))
                    {
                        keyInventoryInst = inst;
                    }
                }
            }
        }

        GameObject pauseGameObj = new GameObject("PauseGame");
        PauseGame pauseGameScript = pauseGameObj.AddComponent<PauseGame>();
        SerializedObject pgSo = new SerializedObject(pauseGameScript);
        pgSo.Update();
        if (pauseMenuInst != null)
        {
            pgSo.FindProperty("pauseMenu").objectReferenceValue = pauseMenuInst;
        }
        if (audioManagerInst != null)
        {
            AudioManager am = audioManagerInst.GetComponent<AudioManager>();
            if (am != null)
            {
                pgSo.FindProperty("audioManager").objectReferenceValue = am;
            }
        }
        pgSo.ApplyModifiedProperties();

        GameObject playerInst = null;
        string[] playerGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/LEVEL EDITOR/Player" });
        if (playerGuids.Length > 0)
        {
            string pPath = AssetDatabase.GUIDToAssetPath(playerGuids[0]);
            GameObject pPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
            if (pPrefab != null)
            {
                playerInst = (GameObject)PrefabUtility.InstantiatePrefab(pPrefab);
                PickPoket pp = playerInst.GetComponentInChildren<PickPoket>();
                if (pp != null && keyInventoryInst != null)
                {
                    KeyInventoryUI ui = keyInventoryInst.GetComponentInChildren<KeyInventoryUI>();
                    if (ui != null)
                    {
                        SerializedObject so = new SerializedObject(pp);
                        so.Update();
                        so.FindProperty("keyUI").objectReferenceValue = ui;
                        so.ApplyModifiedProperties();
                    }
                }

                if (vcam != null && playerInst != null)
                {
                    SerializedObject so = new SerializedObject(vcam);
                    so.Update();
                    SerializedProperty targetObj = so.FindProperty("Target.TrackingTarget");
                    if (targetObj == null) targetObj = so.FindProperty("m_Follow");
                    if (targetObj != null) targetObj.objectReferenceValue = playerInst.transform;
                    so.ApplyModifiedProperties();
                }
            }
        }

        GameObject esObj = new GameObject("EventSystem");
        System.Type esType = System.Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
        if (esType != null) esObj.AddComponent(esType);
        System.Type inputModType = System.Type.GetType("UnityEngine.EventSystems.StandaloneInputModule, UnityEngine.UI");
        if (inputModType != null) esObj.AddComponent(inputModType);
    }
}
