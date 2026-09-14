using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var level = (Level)target;
        if (GUILayout.Button("Log Element Details"))
        {
            level.LogForkliftDetails("Manual");
        }
        if (GUILayout.Button("Run Conveyer Belts"))
        {
            level.RunConveyerBelts();
        }
    }
}
