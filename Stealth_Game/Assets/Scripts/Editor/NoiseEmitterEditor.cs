using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseEmitter))]
[CanEditMultipleObjects]
public class NoiseEmitterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Create No Noise Ring Collider"))
        {
            SpawnCollider();
        }
    }

    private void SpawnCollider()
    {
        GameObject noNoiseRing = new GameObject("No Noise Ring");

        noNoiseRing.AddComponent<BoxCollider2D>();

        noNoiseRing.tag = "No Sound";
    }
}
