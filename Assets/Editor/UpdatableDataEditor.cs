using UnityEngine;
using UnityEditor;

// Custom editor for the UpdatableData component, extends the editor functionality for derived types as well
[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor {

    // Override the default Inspector GUI to add custom functionality
    public override void OnInspectorGUI ()
    {
        // Draw the default inspector for the UpdatableData component
        base.OnInspectorGUI ();

        // Cast the target object to UpdatableData to access its properties and methods
        UpdatableData data = (UpdatableData)target;

        // Button to trigger the update of data values
        if (GUILayout.Button ("Update")) {
            // Notify the object of updated values and mark the object as dirty
            data.NotifyOfUpdatedValues ();

            // Mark the target object as dirty to ensure changes are serialized and saved
            EditorUtility.SetDirty(target);
        }
    }
}
