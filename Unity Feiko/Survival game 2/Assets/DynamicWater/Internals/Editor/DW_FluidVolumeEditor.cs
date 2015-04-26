using UnityEditor;
using UnityEngine;
using LostPolygon.DynamicWaterSystem;
using LostPolygon.DynamicWaterSystem.EditorExtensions;

[CustomEditor(typeof (FluidVolume))]
public class DW_FluidVolumeEditor : UndoEditor<FluidVolume> {

    protected override void OnInspectorGUIDraw() {
        /* Obstructions */
        EditorGUILayout.HelpBox("Simulation", MessageType.None, true);

        // Size.x
        float sizeX =
            Mathf.Clamp(
                EditorGUILayout.FloatField(
                    new GUIContent("Length",
                                   "Length of the water plane"),
                    _object.Size.x),
                0f,
                float.PositiveInfinity
                );

        // Size.y
        float sizeY =
            Mathf.Clamp(
                EditorGUILayout.FloatField(
                    new GUIContent("Width",
                                   "Width of the water plane"),
                    _object.Size.y),
                0f,
                float.PositiveInfinity
                );

        _object.Size = new Vector2(sizeX, sizeY);

        // Depth
        _object.Depth =
            Mathf.Clamp(
                EditorGUILayout.FloatField(
                    new GUIContent(_object.GetType() == typeof (FluidVolume) ? "Height" : "Depth",
                                   _object.GetType() == typeof (FluidVolume) ? "Height of fluid volume" : "Depth of fluid volume"),
                    _object.Depth),
                0f,
                float.PositiveInfinity
                );

        // Density
        _object.Density =
            Mathf.Clamp(
                EditorGUILayout.FloatField(
                    new GUIContent("Density",
                                   "Fluid density in kg/m^3"),
                    _object.Density),
                0f,
                10000f
                );
    }

    protected override void OnSceneGUIDraw() {
        Vector2 sizeOld = _object.Size;

        // Size.x
        Vector3 pos = _object.transform.TransformPoint(_object.transform.InverseTransformDirection(_object.transform.right) * _object.Size.x);
        Handles.DrawLine(_object.transform.position, pos);
        float sizeX =
            Mathf.Clamp(
                _object.transform.InverseTransformPoint(
                    Handles.Slider(pos,
                                   _object.transform.right,
                                   HandleUtility.GetHandleSize(pos) *
                                   0.15f,
                                   Handles.CubeCap,
                                   1f)).x,
                0.001f,
                float.PositiveInfinity);

        // Size.y
        pos = _object.transform.TransformPoint(_object.transform.InverseTransformDirection(_object.transform.forward * _object.Size.y));
        Handles.DrawLine(_object.transform.position, pos);
        float sizeY = Mathf.Clamp(
            _object.transform.InverseTransformPoint(
                Handles.Slider(pos,
                               _object.transform.forward,
                               HandleUtility.GetHandleSize(pos) *
                               0.15f, Handles.CubeCap,
                               1f)).z, 0.001f,
            float.PositiveInfinity);

        Vector2 sizeNew = new Vector2(sizeX, sizeY);

        // Apply Size
        if (!float.IsPositiveInfinity(sizeNew.x) && !float.IsPositiveInfinity(sizeNew.y) && sizeOld != sizeNew) {
            _undoManager.RegisterUndo();
            _object.Size = sizeNew;
        }
    }

    protected override void OnSceneGUIUndo() {
        Vector2 size = _object.Size;
        _object.Size = Vector2.one;
        _object.Size = size;
    }
}