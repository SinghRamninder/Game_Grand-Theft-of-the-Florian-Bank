using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelStart))]
public class LevelStartEditor : Editor
{
    private SerializedProperty cameraStepsProp;

    private void OnEnable()
    {
        cameraStepsProp = serializedObject.FindProperty("cameraSteps");
    }

    public override void OnInspectorGUI()
    {
        LevelStart lvlStart = (LevelStart)target;

        bool listChanged = false;
        for (int i = lvlStart.cameraSteps.Count - 1; i >= 0; i--)
        {
            if (lvlStart.cameraSteps[i].targetTransform == null)
            {
                lvlStart.cameraSteps.RemoveAt(i);
                listChanged = true;
            }
        }

        if (listChanged)
        {
            EditorUtility.SetDirty(lvlStart);
            serializedObject.Update();
        }

        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "cameraSteps");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Camera Sequence", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < cameraStepsProp.arraySize; i++)
        {
            SerializedProperty stepProp = cameraStepsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(stepProp, new GUIContent($"Camera Step {i + 1}"), true);

            if (GUILayout.Button("Remove Step", GUILayout.Width(100)))
            {
                Transform t = lvlStart.cameraSteps[i].targetTransform;
                if (t != null)
                {
                    Undo.DestroyObjectImmediate(t.gameObject);
                }

                cameraStepsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Add new Camera Step"))
        {
            GameObject cameraStep = new GameObject("Camera Step " + lvlStart.cameraSteps.Count);
            cameraStep.transform.SetParent(lvlStart.transform);
            cameraStep.transform.localPosition = Vector3.zero;

            Undo.RegisterCreatedObjectUndo(cameraStep, "Create Camera Step");

            LevelStart.CameraMovementStep newStep = new LevelStart.CameraMovementStep();
            newStep.targetTransform = cameraStep.transform;

            Undo.RecordObject(lvlStart, "Added New Camera Step");
            lvlStart.cameraSteps.Add(newStep);

            EditorUtility.SetDirty(lvlStart);
            serializedObject.Update();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
