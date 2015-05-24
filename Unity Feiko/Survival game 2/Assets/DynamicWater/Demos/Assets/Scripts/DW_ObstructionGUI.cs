using UnityEngine;
using LostPolygon.DynamicWaterSystem;

/// <summary>
/// GUI used for Obstruction demo. 
/// </summary>
public class DW_ObstructionGUI : DW_DemoGUI {
    public DynamicWater Water = null;
    public SplashZone RainZone;

    public Texture2D[] ObstructionMasks;

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        const float initWidth = 275f;
        const float initHeight = 370f;
        const float initItemHeight = 30f;

        DW_GUILayout.itemWidth = initWidth - 20f;
        DW_GUILayout.itemHeight = initItemHeight;
        DW_GUILayout.yPos = 0f;
        DW_GUILayout.hovered = false;

        if (DW_GUILayout.IsRuntimePlatformMobile())
        {
            DW_GUILayout.UpdateScaleMobile();
        }
        else
        {
            DW_GUILayout.UpdateScaleDesktop(initHeight + 30f);
        }

        GUI.BeginGroup(new Rect(15f, 15f, initWidth, initHeight), "Obstruction geometry Demo", "Window");
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleLeft;

        DW_GUILayout.yPos = 20f;

        const string text = "This demo demonstrates more advanced usage of obstruction geometry. " +
                            "Here, a barell-like object is constructed with a combination of two obstruction meshes.\n" +
                            "This demo also uses\"Bake obstruction into mesh\" option, which in combination " +
                            "with special shader allows " +
                            "to create non-rectangular shapes with holes (note how the corners of water plane are " +
                            "not sticking out from the cylinder).";

        DW_GUILayout.itemHeight = centeredStyle.CalcHeight(new GUIContent(text, ""), DW_GUILayout.itemWidth);
        DW_GUILayout.Label(text);
        DW_GUILayout.itemHeight = initItemHeight;

        DW_GUILayout.Box("Simulation");

        bool toggleChanged;
        DW_GUILayout.tooltip = "Enables or disables raindrops. Raindrops are controlled by SplashZone component.";
        RainZone.IsRaining = DW_GUILayout.Toggle(RainZone.IsRaining, "Raindrops", out toggleChanged);

        DW_GUILayout.Label("Select one of the obstruction masks to see how they affect the wave propagation:", true);
        DW_GUILayout.Space(5f);

        DW_GUILayout.itemHeight = 45f;
        for (int i = 0; i < ObstructionMasks.Length; i++) {
            Texture2D obstructionMask = ObstructionMasks[i];
            Rect rect = new Rect(DW_GUILayout.paddingLeft + i * 64f, DW_GUILayout.yPos, 60f, DW_GUILayout.itemHeight);

            if (GUI.Button(rect, obstructionMask)) {
                Water.ObstructionMask = obstructionMask;
            }
        }
        DW_GUILayout.yPos += DW_GUILayout.itemHeight;
        DW_GUILayout.itemHeight = initItemHeight;

        GUI.color = new Color(1f, 0.6f, 0.6f, 1f);
        if (GUI.Button(new Rect(DW_GUILayout.paddingLeft, initHeight - 40f, DW_GUILayout.itemWidth, 30f), "Back to Main Menu"))
        {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Menu"));
        }

        GUI.EndGroup();

        GUI.color = Color.white;
        DW_GUILayout.DrawLogo(Logo);
    }
}