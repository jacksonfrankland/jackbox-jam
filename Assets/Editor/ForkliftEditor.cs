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
            // script.Level.LogForkliftDetails("Before Move Forward");
            script.MoveForward();
            // script.Level.LogForkliftDetails("After Move Forward");
        }
        if (GUILayout.Button("Move Backwards"))
        {
            // script.Level.LogForkliftDetails("Before Move Backwards");
            script.MoveForward(-1);
            // script.Level.LogForkliftDetails("After Move Backwards");
        }
        if (GUILayout.Button("Rotate Clockwise"))
        {
            // script.Level.LogForkliftDetails("Before Rotate Clockwise");
            script.RotateClockwise();
            // script.Level.LogForkliftDetails("After Rotate Clockwise");
        }
        if (GUILayout.Button("Rotate Anti-Clockwise"))
        {
            // script.Level.LogForkliftDetails("Before Rotate Anti-Clockwise");
            script.RotateAnticlockwise();
            // script.Level.LogForkliftDetails("After Rotate Anti-Clockwise");
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Deal Hand"))
        {
            script.Hand.Deal();
        }
        if (GUILayout.Button("Mulligan All Cards"))
        {
            script.Hand.Mulligan(new[] { 0, 1, 2, 3, 4 });
        }
        if (GUILayout.Button("Log Hand"))
        {
            Debug.Log($"[{script.name}] Hand: {string.Join(", ", script.Hand.Cards)} (mulliganed: {script.Hand.HasMulliganed})");
        }
    }
}
