using UnityEngine;
using UnityEditor;

// Custom editor for the MapPreview component in the Unity Inspector
[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor {

    // Override the default GUI for the MapPreview component in the Inspector
    public override void OnInspectorGUI() {
        // Get the MapPreview instance (the target object being edited)
        MapPreview mapPreview = (MapPreview)target;

        // Draw the default inspector GUI elements
        if (DrawDefaultInspector()) {
            // If auto-update is enabled, render the map preview automatically in the editor
            if (mapPreview.enableautoUpdate) {
                mapPreview.RenderMapInEditor();
            }
        }

        // Button to manually trigger map rendering in the editor
        if (GUILayout.Button("Generate")) {
            mapPreview.RenderMapInEditor(); // Render the map when the button is pressed
        }
    }
}