using UnityEngine;

/// <summary>
/// Base GUI used for demos. 
/// </summary>
public abstract class DW_DemoGUI : MonoBehaviour {
    public Texture2D Logo;
    protected bool visible = true;

    protected virtual void OnLevelWasLoaded(int level)
    {
        DW_CameraFade.StartAlphaFade(Color.black, true, 0.5f, 0.5f);
    }

    protected virtual void Start()
    {
        useGUILayout = false;
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            DW_CameraFade.StartAlphaFade(Color.black, false, 0.5f, 0f, () => Application.LoadLevel("DW_Menu"));
        }

        if (Input.GetKeyDown(KeyCode.Menu) || Input.GetKeyDown(KeyCode.Return)) {
            visible = !visible;
        }
    }
}