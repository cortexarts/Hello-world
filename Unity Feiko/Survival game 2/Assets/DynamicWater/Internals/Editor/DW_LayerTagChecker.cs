using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using LostPolygon.DynamicWaterSystem.EditorExtensions;

/// <summary>
/// Helper class to allow easier adding required tags and layers.
/// </summary>
public class DW_LayerTagChecker : MonoBehaviour {
    public static readonly string[] RequiredTags = new string[] { "DynamicWater", "DynamicWaterObstruction", "DynamicWaterObstructionInverted", "DynamicWaterPlaneCollider" };
    public static readonly string[] RequiredLayers = new string[] { "DynamicWaterPlaneCollider" };

    /// <summary>
    /// Check the array of tags and returns a list of missing ones.
    /// </summary>
    /// <param name="tags">
    /// The tags array.
    /// </param>
    /// <returns>
    /// The <see cref="List{T}"/> of missing tags.
    /// </returns>
    public static List<string> CheckMissingTags(string[] tags) {
        List<string> missingTags = new List<string>();

        // Make a list of missing tags
        foreach (string tag in tags)
        {
            if (!TagManager.IsTagExists(tag))
                missingTags.Add(tag);
        }

        return missingTags;
    }

    /// <summary>
    /// Check the array of layers and returns a list of missing ones.
    /// </summary>
    /// <param name="layers">
    /// The array of layer names.
    /// </param>
    /// <returns>
    /// The <see cref="List{T}"/> of missing layers.
    /// </returns>
    public static List<string> CheckMissingLayers(string[] layers) {
        List<string> missingLayers = new List<string>();

        // Make a list of missing tags
        foreach (string layer in layers)
        {
            if (!TagManager.IsLayerExists(layer))
                missingLayers.Add(layer);
        }

        return missingLayers;
    }

    public static void ShowMissingTagsAndLayersDialog(string[] tags, string[] layers) {
        List<string> missingTags = CheckMissingTags(tags);
        List<string> missingLayers = CheckMissingLayers(layers);

        StringBuilder sb = new StringBuilder();
        string title;

        // No missing tags and layers, nothing to do
        if (missingTags.Count == 0 && missingLayers.Count == 0) {
            return;
        }

        // Constructing the dialog message
        sb.Append("In order to use Dynamic Water System, you have to add following ");
        if (missingTags.Count != 0 && missingLayers.Count == 0) {
            sb.Append("tags");
            title = "Missing tags";
        } else if (missingTags.Count == 0 && missingLayers.Count != 0) {
            sb.Append("layers");
            title = "Missing layers";
        } else {
            sb.Append("tags and layers");
            title = "Missing tags and layers";
        }

        sb.Append(":\n\n");

        if (missingTags.Count != 0) {
            sb.Append("Tags:\n");
            foreach (string missingTag in missingTags) {
                sb.AppendFormat("- {0}\n", missingTag);
            }

            sb.Append("\n");
        }

        if (missingLayers.Count != 0) {
            sb.Append("Layers:\n");
            foreach (string missingLayer in missingLayers) {
                sb.AppendFormat("- {0}\n", missingLayer);
            }

            sb.Append("\n");
        }

        string consoleMessage = sb.ToString();

        sb.Append("Do you want to add them automatically?");

        if (EditorUtility.DisplayDialog(title, sb.ToString(), "Add", "Cancel")) {
            foreach (string missingTag in missingTags) {
                TagManager.AddTag(missingTag);
            }

            foreach (string missingLayer in missingLayers) {
                TagManager.AddLayer(missingLayer);
            }
        } else {
            Debug.LogWarning(consoleMessage);
        }
    }
}