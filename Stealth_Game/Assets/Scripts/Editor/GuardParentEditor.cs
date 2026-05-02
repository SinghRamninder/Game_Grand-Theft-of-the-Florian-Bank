using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GuardParent))]
public class GuardParentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GuardParent guardParent = (GuardParent)target;

        if (guardParent.securityOfficerScriptChild == null)
        {
            guardParent.securityOfficerScriptChild = guardParent.GetComponentInChildren<SecurityOfficerScript>();
        }

        if (guardParent.securityOfficerScriptChild != null)
        {
            Editor childEditor = CreateEditor(guardParent.securityOfficerScriptChild);
            childEditor.OnInspectorGUI();
        }
    }
}
