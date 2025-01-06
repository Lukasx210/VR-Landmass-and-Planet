using UnityEngine;
using UnityEditor;

// Custom editor for the Planet component in the Unity Inspector
[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor {

    // References to the Planet component and its specific editors for shape and color settings
    Planet planet;
    Editor shapeEditor;
    Editor colourEditor;

    // Override the default Inspector GUI for the Planet component
    public override void OnInspectorGUI()
    {
        // Track changes in the Inspector using a ChangeCheckScope
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            // Draw the default Inspector GUI for the Planet component
            base.OnInspectorGUI();

            // If any changes are detected, regenerate the planet
            if (check.changed)
            {
                planet.GeneratePlanet();
            }
        }

        // Button to manually trigger planet generation
        if (GUILayout.Button("Generate Planet"))
        {
            planet.GeneratePlanet();
        }

        // Draw and handle foldout sections for shape and color settings
        DrawSettingsEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated, ref planet.shapeSettingsFoldout, ref shapeEditor);
        DrawSettingsEditor(planet.colourSettings, planet.OnColourSettingsUpdated, ref planet.colourSettingsFoldout, ref colourEditor);
    }

    // Helper method to draw custom settings editors for shape and color settings
    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            // Display a foldout for the settings section
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                // If the foldout is open, create and display the specific editor for the settings
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    // If any changes are detected in the settings, trigger the update callback
                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

    // Initialize the editor by getting a reference to the Planet component
    private void OnEnable()
    {
        planet = (Planet)target;
    }
}