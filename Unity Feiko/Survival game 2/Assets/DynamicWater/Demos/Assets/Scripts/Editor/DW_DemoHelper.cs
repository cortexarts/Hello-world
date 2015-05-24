using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DW_DemoHelper : MonoBehaviour {
    private static readonly string[] DemoScenes = 
        new string[] {"DW_Menu", "DW_Waterfall", "DW_Buoyancy", "DW_BuoyancyMobile", "DW_Pool", "DW_PoolMobile", "DW_Boat", "DW_BoatMobile", "DW_Character", "DW_Obstruction"};

    static DW_DemoHelper() {
        EditorApplication.hierarchyWindowChanged += CheckLayers;
    }

    private static void CheckLayers() {
        // Is the scene present if the list?
        string scene = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
        bool flag = false;
        foreach (string x in DemoScenes) {
            if (x == scene) {
                flag = true;
                break;
            }
        }

        if (flag) {
            DW_LayerTagChecker.ShowMissingTagsAndLayersDialog(DW_LayerTagChecker.RequiredTags, DW_LayerTagChecker.RequiredLayers);
        }
    }
}