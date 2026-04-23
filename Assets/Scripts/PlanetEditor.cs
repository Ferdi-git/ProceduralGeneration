using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{

    Planet planet;
    Editor shapeEditor;
    Editor colourEditor;


    public override void OnInspectorGUI()
    {
        using(var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                planet.GeneratePlanet();
            }

        }

        if(GUILayout.Button("Generate Planet"))
        {
            planet.GeneratePlanet();
        }

        DrawSettingsEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated , ref  planet.shapeSettingsFaldout, ref shapeEditor);
        DrawSettingsEditor(planet.colorSettings, planet.OnColourSettingsUpdated, ref planet.colorSettingsFaldout, ref colourEditor);
    }


    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings == null) return;
        
        foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            if (!foldout) return;
                
            CreateCachedEditor(settings, null, ref editor);
            editor.OnInspectorGUI();

            if (check.changed)
            {
                if (onSettingsUpdated != null)
                {
                    onSettingsUpdated();
                }
            }
                
        }
        
    }

    private void OnEnable()
    {
        planet = (Planet)target;
    }

}
