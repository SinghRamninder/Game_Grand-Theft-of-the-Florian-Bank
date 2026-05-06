

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(VaultInspector))]
public class VaultInspectorEditor : Editor
{
    private VaultPuzzle vaultPuzzle;
    private AfterStealMoney afterStealMoney;

    private SerializedObject vaultPuzzleSO;
    private SerializedObject afterStealMoneySO;

    private SerializedProperty normalSpeedProp;
    private SerializedProperty speedIncrementProp;

    private SerializedProperty blinkSpeedProp;
    private SerializedProperty countdownSecondsProp;

    private void OnEnable()
    {
        VaultInspector inspectorTarget = (VaultInspector)target;

        if (inspectorTarget != null)
        {
            vaultPuzzle = inspectorTarget.GetComponentInChildren<VaultPuzzle>(true);
            afterStealMoney = inspectorTarget.GetComponentInChildren<AfterStealMoney>(true);
        }

        if (vaultPuzzle != null)
        {
            vaultPuzzleSO = new SerializedObject(vaultPuzzle);
            normalSpeedProp = vaultPuzzleSO.FindProperty("normalSpeed");
            speedIncrementProp = vaultPuzzleSO.FindProperty("speedIncrement");
        }

        if (afterStealMoney != null)
        {
            afterStealMoneySO = new SerializedObject(afterStealMoney);
            blinkSpeedProp = afterStealMoneySO.FindProperty("alertLightBlinkSpeed");
            countdownSecondsProp = afterStealMoneySO.FindProperty("countdownSeconds");
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (vaultPuzzleSO != null)
        {
            vaultPuzzleSO.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Vault Puzzle Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(normalSpeedProp);
            EditorGUILayout.PropertyField(speedIncrementProp);
            vaultPuzzleSO.ApplyModifiedProperties();
        }
        else
        {
            EditorGUILayout.HelpBox("No VaultPuzzle found in scene.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        if (afterStealMoneySO != null)
        {
            afterStealMoneySO.Update();
            EditorGUILayout.LabelField("After Steal Money Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(blinkSpeedProp);
            EditorGUILayout.PropertyField(countdownSecondsProp);
            afterStealMoneySO.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Alert Light", GUILayout.Height(30)))
            {
                CreateAlertLight();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No AfterStealMoney found in scene.", MessageType.Warning);
        }
    }

    private void CreateAlertLight()
    {
        GameObject lightObj = new GameObject("AlertLight");
        Light2D light2D = lightObj.AddComponent<Light2D>();

        light2D.lightType = Light2D.LightType.Freeform;
        light2D.intensity = 0f;
        light2D.falloffIntensity = 0.5f;

        if (ColorUtility.TryParseHtmlString("#FD0000", out Color color))
        {
            light2D.color = color;
        }

        // Parent to AfterStealMoney or VaultInspector to keep things organized if desired
        // lightObj.transform.parent = afterStealMoney.transform;

        Undo.RegisterCreatedObjectUndo(lightObj, "Create Alert Light");

        Undo.RecordObject(afterStealMoney, "Add Alert Light to List");
        afterStealMoney.alertLights.Add(light2D);
        PrefabUtility.RecordPrefabInstancePropertyModifications(afterStealMoney);

        Selection.activeGameObject = lightObj;
    }
}
