using UnityEditor;
using UnityEngine;
using TMPro;

[CustomEditor(typeof(EndCreditsManager))]
public class EndCreditsManagerEditor : Editor
{
    private SerializedProperty creditsProp;
    private SerializedProperty blackFadeImageProp;
    private SerializedProperty startCreditsDelayProp;
    private SerializedProperty blackFadeInDurationProp;

    private void OnEnable()
    {
        creditsProp = serializedObject.FindProperty("credits");
        blackFadeImageProp = serializedObject.FindProperty("blackFadeImage");
        startCreditsDelayProp = serializedObject.FindProperty("startCreditsDelay");
        blackFadeInDurationProp = serializedObject.FindProperty("blackFadeInDuration");
    }

    public override void OnInspectorGUI()
    {
        EndCreditsManager ecm = (EndCreditsManager)target;

        // Automatically clean up list entries where the objects were destroyed
        bool listChanged = false;
        for (int i = ecm.credits.Count - 1; i >= 0; i--)
        {
            if ((ecm.credits[i].type == EndCreditsManager.CreditType.Text && ecm.credits[i].text == null) ||
                (ecm.credits[i].type == EndCreditsManager.CreditType.Image && ecm.credits[i].image == null))
            {
                ecm.credits.RemoveAt(i);
                listChanged = true;
            }
        }

        if (listChanged)
        {
            EditorUtility.SetDirty(ecm);
            serializedObject.Update();
        }

        serializedObject.Update();

        EditorGUILayout.PropertyField(blackFadeImageProp);
        EditorGUILayout.PropertyField(startCreditsDelayProp);
        EditorGUILayout.PropertyField(blackFadeInDurationProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Credits Sequence", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        
        for (int i = 0; i < creditsProp.arraySize; i++)
        {
            SerializedProperty stepProp = creditsProp.GetArrayElementAtIndex(i);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            SerializedProperty typeProp = stepProp.FindPropertyRelative("type");
            EndCreditsManager.CreditType cType = (EndCreditsManager.CreditType)typeProp.enumValueIndex;
            
            string labelName = cType == EndCreditsManager.CreditType.Text ? "Credit Text" : "Credit Image";
            EditorGUILayout.PropertyField(stepProp, new GUIContent($"{labelName} {i + 1}"), true);

            if (GUILayout.Button("Remove Credit", GUILayout.Width(120)))
            {
                if (cType == EndCreditsManager.CreditType.Text && ecm.credits[i].text != null)
                {
                    Undo.DestroyObjectImmediate(ecm.credits[i].text.gameObject);
                }
                else if (cType == EndCreditsManager.CreditType.Image && ecm.credits[i].image != null)
                {
                    Undo.DestroyObjectImmediate(ecm.credits[i].image.gameObject);
                }
                
                creditsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add new Credit Text"))
        {
            AddCreditObject(ecm, EndCreditsManager.CreditType.Text);
        }
        if (GUILayout.Button("Add new Credit Image"))
        {
            AddCreditObject(ecm, EndCreditsManager.CreditType.Image);
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void AddCreditObject(EndCreditsManager ecm, EndCreditsManager.CreditType cType)
    {
        // Find canvas
        Transform canvasTransform = ecm.transform.Find("EndCreditsCanvas(Clone)");
        if (canvasTransform == null) canvasTransform = ecm.transform.Find("EndCreditsCanvas");

        if (canvasTransform == null && ecm.transform.childCount > 0)
        {
            // Fallback attempt to find something mimicking the canvas
            canvasTransform = ecm.transform.GetChild(0); 
        }

        if (canvasTransform != null)
        {
            GameObject newObj = new GameObject($"Credit{cType}_" + ecm.credits.Count);
            newObj.transform.SetParent(canvasTransform, false);
            
            RectTransform rt = newObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            
            Undo.RegisterCreatedObjectUndo(newObj, $"Create Credit {cType}");

            EndCreditsManager.CreditStep newStep = new EndCreditsManager.CreditStep();
            newStep.type = cType;

            if (cType == EndCreditsManager.CreditType.Text)
            {
                rt.sizeDelta = new Vector2(800, 200);
                TMP_Text newText = newObj.AddComponent<TextMeshProUGUI>();
                newText.text = "New Credit line...";
                newText.alignment = TextAlignmentOptions.Center;
                newText.fontSize = 36;
                newText.alpha = 0f;
                newStep.text = newText;
            }
            else
            {
                rt.sizeDelta = new Vector2(400, 400); // Default placeholder size for an image
                UnityEngine.UI.Image newImage = newObj.AddComponent<UnityEngine.UI.Image>();
                Color c = newImage.color;
                newImage.color = new Color(c.r, c.g, c.b, 0f); // start invisible
                newStep.image = newImage;
            }
            
            Undo.RecordObject(ecm, "Added New Credit");
            ecm.credits.Add(newStep);
            
            EditorUtility.SetDirty(ecm);
            serializedObject.Update();
        }
        else
        {
            Debug.LogError($"Cannot add Credit {cType}. 'EndCreditsCanvas' was not found as a child of EndCreditsManager.");
        }
    }
}
