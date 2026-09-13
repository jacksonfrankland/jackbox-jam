using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Forklift))]
public class ForkliftEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (Forklift)target;
        if (GUILayout.Button("Move Forward"))
        {
            script.MoveForward();
        }
        if (GUILayout.Button("Move Backwards"))
        {
            script.MoveForward(-1);
        }
        if (GUILayout.Button("Rotate Clockwise"))
        {
            script.RotateClockwise();
        }
        if (GUILayout.Button("Rotate Anti-Clockwise"))
        {
            script.RotateAnticlockwise();
        }

    }
}
