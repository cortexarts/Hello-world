using UnityEditor;
using UnityEngine;
using LostPolygon.DynamicWaterSystem;
using LostPolygon.DynamicWaterSystem.EditorExtensions;

[CustomEditor(typeof (DynamicWater))]
public class DW_DynamicWaterEditor : DW_FluidVolumeEditor {
    protected override void OnInspectorGUIDraw() {
        base.OnInspectorGUIDraw();

        DynamicWater _objectDW = _object as DynamicWater;
        if (_objectDW == null) {
            return;
        }

        // Quality
        _objectDW.Quality = EditorGUILayout.IntSlider(
            new GUIContent(
                "Quality",
                "Simulation grid resolution. " +
                "The higher the value - the more detailed the simulation will look"
                ),
            _objectDW.Quality,
            4,
            256
            );

        // Damping
        _objectDW.Damping =
            EditorGUILayout.Slider(
                new GUIContent(
                    "Damping",
                    "The higher the value, the faster the waves will dissipate. " +
                    "Optimal value for water is around 0.03. " +
                    "Value of 0 corresponds to absence of any damping, " +
                    "which could lead to simulation instability"
                    ),
                _objectDW.Damping,
                0f,
                1f
                );

        // Speed
        _objectDW.Speed =
            EditorGUILayout.Slider(
                new GUIContent(
                    "Speed",
                    "Wave propagation speed"
                    ),
                _objectDW.Speed,
                0f,
                _objectDW.MaxSpeed()
                );

        // UsePlaneCollider
        _objectDW.UsePlaneCollider =
            EditorGUILayout.Toggle(
                new GUIContent(
                    "Use plane collider",
                    "Whether to generate the static plane collider on initialization. " +
                    "Can be used for easier interaction."
                    ),
                _objectDW.UsePlaneCollider
                );

        // UpdateWhenNotVisible
        _objectDW.UpdateWhenNotVisible =
            EditorGUILayoutExtensions.ToggleFixedWidth(
                new GUIContent(
                    "Update when not visible",
                    "If checked, the simulation would be running even if the water is not visible"
                    ),
                _objectDW.UpdateWhenNotVisible
                );

        /* Obstructions */
        EditorGUILayout.HelpBox("Obstructions", MessageType.None, true);

        // UseObstructions
        _objectDW.UseObstructions =
            EditorGUILayoutExtensions.ToggleFixedWidth(
                new GUIContent(
                    "Use obstruction geometry",
                    "Whether to calculate where the simulation field is obstructed with GameObjects with tag DynamicWaterObstruction. " +
                    "This can be used to simulate complex shapes such as pond banks"
                    ),
                _objectDW.UseObstructions
                );

        // ObstructionMask
        Texture2D oldTexture = _objectDW.ObstructionMask;
        _objectDW.ObstructionMask =
            (Texture2D) EditorGUILayout.ObjectField(
                new GUIContent(
                    "Obstruction mask",
                    ""
                    ),
                _objectDW.ObstructionMask,
                typeof (Texture2D),
                false
                            );
        Texture2D newTexture = _objectDW.ObstructionMask;
        if (oldTexture != newTexture) {
            if (!TextureImporterHelper.GetTextureIsReadable(newTexture)) {
                if (EditorUtility.DisplayDialog(
                    "Texture not readable",
                    "This texture is not readable, which is required to use it as obstruction mask.\n" +
                    "Make the texture readable?",
                    "OK",
                    "Cancel"
                    )) {
                    TextureImporterHelper.SetTextureIsReadable(newTexture, true);
                } else {
                    _objectDW.ObstructionMask = null;
                }
            }
        }

        /* Advanced */
        GUILayout.Label("Advanced", EditorStylesInternal.helpBox, GUILayout.Width(130));

        // ObstructionDataErosion
        _objectDW.ObstructionDataErosion =
            EditorGUILayoutExtensions.IntSliderFixedWidth(
                new GUIContent(
                    "Obstruction data erosion",
                    "Indicates how much the obstruction field data will eroded (expanded or shrinked). " +
                    "This can be used for dealing with edge artifacts that can occur when using obstruction data"
                    ),
                _objectDW.ObstructionDataErosion,
                -10,
                10
                       );

        // MeshBakeObstructionData
        _objectDW.MeshBakeObstructionData =
            EditorGUILayoutExtensions.ToggleFixedWidth(
                new GUIContent(
                    "Bake obstruction data into mesh",
                    "Whether the obstruction field data will be baked into water mesh vertex colors.\n" +
                    "In the vertex color, red channel corresponds to the additinal dampening in that point," +
                    "where value of 255 means zero dampening and value of 1 means maximum dampening.\n" +
                    "0 is a special value that corresponds to the situation when the vertex " +
                    "is fully obstructed by obstruction geometry. In this case, a value of 255" +
                    "is additionaly written to the blue channel.\n" +
                    "You can use this data for more advanced shading, for example, discard fragments that are" +
                    "fully obstructed"
                    ),
                _objectDW.MeshBakeObstructionData
                );

        /* Rendering */
        EditorGUILayout.HelpBox("Rendering", MessageType.None, true);

        // CalculateNormals
        _objectDW.CalculateNormals =
            EditorGUILayout.Toggle(
                new GUIContent(
                    "Calculate normals",
                    "Whether the water mesh normals should be calculated"
                    ),
                _objectDW.CalculateNormals
                );

        GUI.enabled = _objectDW.CalculateNormals;
        EditorGUI.indentLevel++;

        // UseFastNormals
        _objectDW.UseFastNormals =
            EditorGUILayout.Toggle(
                new GUIContent(
                    "Fast normalization",
                    "Indicates whether the fast approximate method of calculating water mesh normals should be used.\n" +
                    "Enabling this could provide a huge performance boost with the cost of a bit degraded quality. " +
                    "Especially useful on mobile devices"
                    ),
                _objectDW.UseFastNormals
                );

        EditorGUI.indentLevel--;
        GUI.enabled = true;

        // SetTangents
        _objectDW.SetTangents =
            EditorGUILayout.Toggle(
                new GUIContent(
                    "Set mesh tangents",
                    "Whether the water mesh tangents must be set (usually for bump-mapped shaders).\n" +
                    "Enabling this may sometimes result in performance drop on high " +
                    "Quality levels. It is better to turn it off in case " +
                    "your shader doesn't uses tangents"
                    ),
                _objectDW.SetTangents
                );
    }
}