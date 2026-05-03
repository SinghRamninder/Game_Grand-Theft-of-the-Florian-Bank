using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VisionConeParent))]
public class VisionConeParentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VisionConeParent parent = (VisionConeParent)target;

        if (parent.visionConeParent == null)
        {
            parent.visionConeParent = parent.GetComponentInChildren<VisionCone2D>();
        }

        if (parent.visionConeParent != null)
        {
            Editor childEditor = CreateEditor(parent.visionConeParent);
            childEditor.OnInspectorGUI();
        }
    }
}
