using UnityEngine;

public class DW_MainMenuGUI : MonoBehaviour {
    private void Start() {
        Application.targetFrameRate = 2000;
    }

    private void OnLevelWasLoaded(int level) {
        DW_CameraFade.StartAlphaFade(Color.black, true, 0.5f, 0.5f);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    private void OnGUI() {
        var centeredStyle = GUI.skin.GetStyle("Label");

        const float width = 250f;
        const float buttonHeight = 35f;

        float height = 370f + buttonHeight * 3f;
        if (Application.isWebPlayer) {
            height -= 20f + buttonHeight;
        }
        if (DW_GUILayout.IsRuntimePlatformMobile()) {
            height -= buttonHeight;
        }

        if (DW_GUILayout.IsRuntimePlatformMobile())
        {
            DW_GUILayout.UpdateScaleMobile();
        }
        else
        {
            DW_GUILayout.UpdateScaleDesktop(height);
        }

        GUILayout.BeginArea(
            new Rect(
                Screen.width / 2f / DW_GUILayout.scaleFactor - width / 2f,
                Screen.height / 2f / DW_GUILayout.scaleFactor - height / 2f, width, height
                ),
            "DynamicWater Demo Menu", "Window"
            );
        GUILayout.BeginVertical();

        GUILayout.Space(10);
        if (GUILayout.Button("Pool Scene", GUILayout.Height(buttonHeight))) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Pool"));
        }
        if (GUILayout.Button("Waterfall Scene", GUILayout.Height(buttonHeight))) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Waterfall"));
        }
        if (GUILayout.Button("Buoyancy Demo Scene", GUILayout.Height(buttonHeight))) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Buoyancy"));
        }
        if (GUILayout.Button("Boat Demo Scene", GUILayout.Height(buttonHeight))) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Boat"));
        }
        // Not showing Character demo for mobile, as it is unplayable for now
        if (!DW_GUILayout.IsRuntimePlatformMobile()) {
            if (GUILayout.Button("Character Scene", GUILayout.Height(buttonHeight))) {
                DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Character"));
            }
        }
        if (GUILayout.Button("Obstruction geometry Demo", GUILayout.Height(buttonHeight)))
        {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Obstruction"));
        }

        centeredStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Mobile Demos", GUILayout.Height(buttonHeight * 0.75f));

        if (GUILayout.Button("Pool Scene (Mobile)", GUILayout.Height(buttonHeight))) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_PoolMobile"));
        }
        if (GUILayout.Button("Buoyancy Demo Scene (Mobile)", GUILayout.Height(buttonHeight))) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_BuoyancyMobile"));
        }
        if (GUILayout.Button("Boat Demo Scene (Mobile)", GUILayout.Height(buttonHeight))) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_BoatMobile"));
        }

        if (!Application.isWebPlayer) {
            GUILayout.Space(20);
            GUI.color = new Color(1f, 0.6f, 0.6f, 1f);
            if (GUILayout.Button("Quit", GUILayout.Height(buttonHeight))) {
                DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, Application.Quit);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();

        GUI.color = Color.white;
        centeredStyle.alignment = TextAnchor.UpperLeft;

        if (!DW_GUILayout.IsRuntimePlatformMobile() && !Application.isEditor)
        {
            Screen.fullScreen = GUI.Toggle(
                new Rect((Screen.width / 2f - width / 2f - 100f) / DW_GUILayout.scaleFactor,
                    Screen.height / 2f / DW_GUILayout.scaleFactor - height / 2f,
                         100f, 400f), Screen.fullScreen, " Fullscreen");
        }
        GUI.Label(new Rect((Screen.width / 2f + width / 2f + 20f) / DW_GUILayout.scaleFactor,
            Screen.height / 2f / DW_GUILayout.scaleFactor - height / 2f, 200f, 400f),
                  "You can press Enter/Menu button to disable GUI (as Unity GUI may cause slowdown, especially on mobile)");
    }
}