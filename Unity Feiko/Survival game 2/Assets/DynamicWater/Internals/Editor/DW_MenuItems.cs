#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#  define PRE_UNITY_4_3
#endif

using System;
using System.IO;
using LostPolygon.DynamicWaterSystem;
using LostPolygon.DynamicWaterSystem.EditorExtensions;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DW_MenuItems : MonoBehaviour {

    private static GameObject CreateGameObject<T>() where T : Component {
#       if PRE_UNITY_4_3
        Undo.RegisterSceneUndo("Create_" + typeof(T).Name);
#       endif

        GameObject go = new GameObject(typeof(T).Name);
        go.AddComponent<T>();

#       if !PRE_UNITY_4_3
        Undo.RegisterCreatedObjectUndo(go, "Create_" + typeof(T).Name);
#       endif

        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);

        return go;
    }

    [MenuItem("GameObject/Create Other/Dynamic Water")]
    private static void CreateDynamicWater() {
        GameObject water = CreateGameObject<DynamicWater>();
        water.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));

        DW_LayerTagChecker.ShowMissingTagsAndLayersDialog(DW_LayerTagChecker.RequiredTags, DW_LayerTagChecker.RequiredLayers);
    }

    [MenuItem("GameObject/Create Other/Fluid Volume")]
    private static void CreateFluidVolume() {
        CreateGameObject<FluidVolume>();

        DW_LayerTagChecker.ShowMissingTagsAndLayersDialog(DW_LayerTagChecker.RequiredTags, DW_LayerTagChecker.RequiredLayers);
    }

    [MenuItem("GameObject/Create Other/Splash Zone")]
    private static void CreateSplashZone() {
        CreateGameObject<SplashZone>();

        DW_LayerTagChecker.ShowMissingTagsAndLayersDialog(DW_LayerTagChecker.RequiredTags, DW_LayerTagChecker.RequiredLayers);
    }

    [MenuItem("Tools/Lost Polygon/Dynamic Water System/Relocate for JS support")]
    private static void EnableJsSupport() {
        try {
            string[] dirs = Directory.GetDirectories(@"Assets/", "DynamicWater");
            string root = dirs.Length > 0 ? dirs[0] : "";
            if (root == "")
                throw new Exception("Dynamic Water System directory not found");

            const string destMain = @"Assets/Standard Assets/DynamicWater/";
            string sourceMain = root + @"/";
            string sourceEditorMain = root + @"/Internals/Editor/";
            string sourceEditorMisc = root + @"/Demos/Assets/Scripts/Editor/";
            const string destEditor = @"Assets/Editor/DynamicWater/";
            
            AssetDatabaseHelper.MoveAssetDirectory(sourceEditorMain, destEditor);
            AssetDatabaseHelper.MoveAssetDirectory(sourceEditorMisc, destEditor);
            AssetDatabaseHelper.MoveAssetDirectory(sourceMain, destMain);
        } catch (Exception) {
            throw;
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Lost Polygon/Dynamic Water System/Open Online documentation")]
    private static void OpenOnlineDocumentation() {
        Application.OpenURL("http://cdn.lostpolygon.com/dynamicwater/");
    }
}