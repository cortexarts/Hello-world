using UnityEngine;

/// <summary>
/// GUI used for Buoyancy demo. 
/// </summary>
public class DW_BuoyancyGUI : DW_DemoGUI {
    private string _sceneName;

    override protected void Start() {
        base.Start();

        _sceneName = Application.loadedLevelName;
    }

    private void OnGUI() {
        if (!visible) {
            return;
        }
        const float initWidth = 275f;
        const float initHeight = 395f;
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

        GUI.BeginGroup(new Rect(15f, 15f, initWidth, initHeight), "Buoyancy Demo Scene", "Window");
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleLeft;

        DW_GUILayout.yPos = 20f;

        string text;
        if (_sceneName == "DW_BuoyancyMobile") {
            text = "This demo shows how buoyancy force can be applied to objects of any shape and configuration." +
                   "Note that the boat actually consists of multiple child objects with BoxCollider, and with \"Process children\" enabled on parent.\n" +
                   "This demo also shows how to create and modify wave functions to get the desired effect " +
                   "(see DynamicWaterSolverAmbientSimple.cs for example).\n" +
                   "Also note that huge waves like the ones in this demo can be effectively created using " +
                   "very small quality level (this demo uses 25x25 grid, total 676 vertices, 1250 triangles). " +
                   "However, actual wave simulation is disabled in this demo, as it doesn't scales well for large surfaces.\n\n" +
                   "You can also drag objects to see how buoyancy is applied.";
        } else {
            text = "This demo shows how buoyancy force can be applied to objects of any shape and configuration. " +
                   "Note that the boat actually consists of three child objects with \"Process children\" enabled on parent.\n\n" +
                   "This demo also shows how to create and modify wave functions to get the desired effect " +
                   "(see DynamicWaterSolverAmbientSimple.cs for example).\n" +
                   "Also note that huge waves like in this demo can be effectively created using " +
                   "very small quality level (this demo uses 25x25 grid, total 676 vertices, 1250 triangles). " +
                   "However, actual wave simulation is disabled in this demo, as it doesn't scales well for large surfaces.\n\n" +
                   "You can also drag objects to see how buoyancy is applied.";
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