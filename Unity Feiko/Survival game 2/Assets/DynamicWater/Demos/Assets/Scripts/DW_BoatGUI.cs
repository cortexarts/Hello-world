using UnityEngine;

/// <summary>
/// GUI used for Buoyancy demo. 
/// </summary>
public class DW_BoatGUI : DW_DemoGUI
{
    private void OnGUI() {
        if (!visible) {
            return;
        }

        const float initWidth = 275f;
        const float initHeight = 205f;
        const float initItemHeight = 30f;

        DW_GUILayout.itemWidth = initWidth - 20f;
        DW_GUILayout.itemHeight = initItemHeight;
        DW_GUILayout.yPos = 0f;
        DW_GUILayout.hovered = false;

        if (DW_GUILayout.IsRuntimePlatformMobile()) {
            DW_GUILayout.UpdateScaleMobile();
        } else {
            DW_GUILayout.UpdateScaleDesktop(initHeight + 30f);
        }

        GUI.BeginGroup(new Rect(15f, 15f, initWidth, initHeight), "Boat Demo Scene", "Window");
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleLeft;

        DW_GUILayout.yPos = 20f;

        string solverName = Application.loadedLevelName == "DW_Boat" ? "Advanced Ambient" : "Simple Ambient";

        string text = "This demo shows how a simple boat controller can be implemented and demonstrates " +
                      "a practical use of " + solverName + " Solver component.\n\n" +
                      "Controls:\n";
        if (DW_GUILayout.IsRuntimePlatformMobile()) {
            text += "Tilt your device to control the boat.";
        } else {
            text += "Movement - Arrow keys or WSAD\n" +
                    "Camera: use wheel to zoom, use right mouse button to orbit camera";
        }

        DW_GUILayout.itemHeight = centeredStyle.CalcHeight(new GUIContent(text, ""), DW_GUILayout.itemWidth);
        DW_GUILayout.Label(text);
        DW_GUILayout.itemHeight = initItemHeight;

        GUI.color = new Color(1f, 0.6f, 0.6f, 1f);
        if (GUI.Button(new Rect(DW_GUILayout.paddingLeft, initHeight - 40f, DW_GUILayout.itemWidth, 30f), "Back to Main Menu")) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Menu"));
        }

        GUI.EndGroup();

        GUI.color = Color.white;
        DW_GUILayout.DrawLogo(Logo);
    }
}