using UnityEditor;
using UnityEngine;
using LostPolygon.DynamicWaterSystem;
using LostPolygon.DynamicWaterSystem.EditorExtensions;

[CustomEditor(typeof (BuoyancyForce))]
public class DW_BuoyancyForceEditor : UndoEditor<BuoyancyForce> {
    protected override void OnInspectorGUIDraw() {
        // Density
        _object.Density =
            Mathf.Clamp(
                EditorGUILayout.FloatField(
                    new GUIContent(
                        "Density",
                        "The object density in kg/m^3"
                        ),
                    _object.Density),
                0.1f,
                float.PositiveInfinity);

        // CalculateMassFromDensity
        _object.CalculateMassFromDensity =
            EditorGUILayoutExtensions.ToggleFixedWidth(
                new GUIContent(
                    "Calculate mass from density",
                    "If checked, the density value will be used, otherwise density will be approximated from the objects volume and mass"
                    ),
                _object.CalculateMassFromDensity
                );

        // Resolution
        _object.Resolution =
            EditorGUILayout.IntSlider(
                new GUIContent(
                    "Quality",
                    "The number of subdivisions to approximate the object volume with voxels. " +
                    "Value of 1 is usually enough for cube-shaped object. " +
                    "Value of 2-3 is good for most objects with regular shape. " +
                    "You may want to set this value high enough if your " +
                    "object has an irregular shape (i.e. a boat). "
                    ),
                _object.Resolution,
                1,
                15);

        // ProcessChildren
        _object.ProcessChildren =
            EditorGUILayout.Toggle(
                new GUIContent(
                    "Process children",
                    "If checked, children colliders will be included in calculations"
                    ),
                _object.ProcessChildren
                );

        _object.DragInFluid =
            Mathf.Clamp(
                EditorGUILayout.FloatField(
                    new GUIContent(
                        "Drag in fluid",
                        "The additional drag for when the object is in contact with the fluid"
                        ),
                    _object.DragInFluid
                    ),
                0f,
                float.PositiveInfinity
                );

        // AngularDragInFluid
        _object.AngularDragInFluid =
            Mathf.Clamp(
                EditorGUILayout.FloatField(
                    new GUIContent(
                        "Angular drag in fluid",
                        "The additional angular drag for when the object is in contact with the fluid"
                        ),
                    _object.AngularDragInFluid),
                0f,
                float.PositiveInfinity
                );

        // SplashForceFactor
        _object.SplashForceFactor =
            EditorGUILayout.Slider(
                new GUIContent(
                    "Splash force factor",
                    "Force multiplie factor that will be attached to the waves produced by the floating object. " +
                    "For an object of relatively small size, do not set this value high, " +
                    "as the object will bounce on his own waves endlessly. "
                    ),
                _object.SplashForceFactor,
                0f,
                50f
                );

        // MaxSplashForce
        _object.MaxSplashForce =
            EditorGUILayout.Slider(
                new GUIContent(
                    "Max splash force",
                    "The absolute maximum value of force applied to the water when creating splashes"
                    ),
                _object.MaxSplashForce,
                0f,
                50f
                );
    }
}