using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EndCutsceneManager))]
public class EndCutsceneManagerEditor : Editor
{
    private SerializedProperty cameraStepsProp;

    private void OnEnable()
    {
        cameraStepsProp = serializedObject.FindProperty("cameraSteps");
    }

    public override void OnInspectorGUI()
    {
        EndCutsceneManager ecm = (EndCutsceneManager)target;

        // Automatically clean up list entries where the transform was destroyed
        bool listChanged = false;
        for (int i = ecm.cameraSteps.Count - 1; i >= 0; i--)
        {
            if (ecm.cameraSteps[i].targetTransform == null)
            {
                ecm.cameraSteps.RemoveAt(i);
                listChanged = true;
            }
        }

        if (listChanged)
        {
            EditorUtility.SetDirty(ecm);
            serializedObject.Update();
        }

        serializedObject.Update();

        // Draw properties excluding the custom camera steps
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
                Transform t = ecm.cameraSteps[i].targetTransform;
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
            GameObject cameraStep = new GameObject("Camera Step " + ecm.cameraSteps.Count);
            cameraStep.transform.SetParent(ecm.transform);
            cameraStep.transform.localPosition = Vector3.zero;
            
            Undo.RegisterCreatedObjectUndo(cameraStep, "Create Camera Step");

            EndCutsceneManager.CameraMovementStep newStep = new EndCutsceneManager.CameraMovementStep();
            newStep.targetTransform = cameraStep.transform;
            
            Undo.RecordObject(ecm, "Added New Camera Step");
            ecm.cameraSteps.Add(newStep);
            
            EditorUtility.SetDirty(ecm);
            serializedObject.Update();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
