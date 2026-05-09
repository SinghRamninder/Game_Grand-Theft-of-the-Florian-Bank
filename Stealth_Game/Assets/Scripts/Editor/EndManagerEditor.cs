using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(EndManager))]
public class EndManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EndManager endManager = (EndManager)target;

        endManager.afterEnding = EditorGUILayout.Popup(new GUIContent("After Ending options"), endManager.afterEnding, endManager.afterEndingOptions);

        if (endManager.afterEndingOptions[endManager.afterEnding] == "New Scene")
        {
            endManager.selectedScene = (SceneAsset)EditorGUILayout.ObjectField("Scene", endManager.selectedScene, typeof(SceneAsset), false);
        }

        EditorGUILayout.Space();
        endManager.delayBeforeSceneLoad = EditorGUILayout.FloatField("Delay Before Scene Load", endManager.delayBeforeSceneLoad);
    }
}
