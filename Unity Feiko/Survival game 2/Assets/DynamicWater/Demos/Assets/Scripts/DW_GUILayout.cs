using UnityEngine;

// VERY dumb, but fast, GUILayout substitute. Only for demo use
public class DW_GUILayout {
    public static string tooltip;
    public static string tooltipActive;
    public static bool hovered;
    public static float yPos;
    public static float itemWidth;
    public static float itemHeight;
    public static float scaleFactor = 1f;
    public const float paddingLeft = 10f;

    private static Rect GetCurrentRect() {
        return new Rect(new Rect(paddingLeft, yPos, itemWidth, itemHeight));
    }

    private static void CheckTooltipRect(Rect rect) {
        if (IsRuntimePlatformMobile() || !rect.Contains(Event.current.mousePosition)) {
            return;
        }

        tooltipActive = tooltip;
        hovered = true;
    }

    public static void UpdateScaleDesktop(float baseHeight) {
        scaleFactor = Mathf.Min(Screen.height / baseHeight, 1f);

        Vector3 scale;
        scale.x = scaleFactor;
        scale.y = scaleFactor;
        scale.z = 1f;

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
    }

    public static void UpdateScaleMobile() {
        scaleFactor = (Screen.height * 0.85f - 20f) / 460f;
        if (scaleFactor < 1f) {
            scaleFactor = 1f;
        }

        Vector3 scale;
        scale.x = scaleFactor;
        scale.y = scaleFactor;
        scale.z = 1f;

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
    }

    public static bool IsRuntimePlatformMobile() {
        return Application.platform == RuntimePlatform.Android
               || Application.platform == RuntimePlatform.IPhonePlayer
#if UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5 || UNITY_5_0
               || Application.platform == RuntimePlatform.WP8Player
               #endif
            ;
    }

    public static void Space(float value) {
        yPos += value;
    }

    public static void Label(string text, bool autoHeight = false) {
        float initItemHeight = itemHeight;
        if (autoHeight) {
            itemHeight = GUI.skin.GetStyle("Label").CalcHeight(new GUIContent(text, string.Empty), itemWidth);
        }
            
        Rect rect = GetCurrentRect();
        GUI.Label(rect, text);

        yPos += itemHeight;

        if (autoHeight)
        {
            itemHeight = initItemHeight;
        }
    }

    public static void Box(string text) {
        float oldItemHeight = itemHeight;
        itemHeight = 23f;
        Rect rect = GetCurrentRect();
        GUI.Box(rect, text);
        itemHeight = oldItemHeight;

        yPos += itemHeight;
    }

    public static bool Button(GUIContent content, float width = 0f)
    {
        bool value;

        float oldItemitemWidth = itemWidth;
        itemWidth = width;

        Rect rect = GetCurrentRect();
        if (width != 0f)
        {
            rect.width = width;
            rect.x = 275f / 2f - width / 2f;
            value = GUI.Button(rect, content);
        }
        else
        {
            value = GUI.Button(rect, content);
        }

        itemWidth = oldItemitemWidth;

        yPos += itemHeight;

        return value;
    }

    public static bool Button(string text, float width = 0f) {
        return Button(new GUIContent(text), width);
    }

    public static bool Toggle(bool value, string text, out bool changed) {
        changed = false;

        Rect rect = GetCurrentRect();
        Rect toggleRect = rect;
        toggleRect.width = 50f;
        toggleRect.height -= 8f;
        toggleRect.x = itemWidth - toggleRect.width + paddingLeft;
        toggleRect.y += 4f;

        CheckTooltipRect(rect);
        GUI.Label(rect, text);

        if (value) {
            if (GUI.Button(toggleRect, "ON")) {
                value = false;
                changed = true;
            }
        } else {
            if (GUI.Button(toggleRect, "OFF")) {
                value = true;
                changed = true;
            }
        }

        yPos += itemHeight;

        return value;
    }

    public static float Slider(float value, string text, float min, float max) {
        const float minTextWidth = 60f;
        float textWidth = Mathf.Max(GUI.skin.GetStyle("Label").CalcSize(new GUIContent(text)).x + 7f, minTextWidth);
        float sliderWidth = 198f - textWidth;

        CheckTooltipRect(GetCurrentRect());
        GUI.Label(GetCurrentRect(), text);

        value = GUI.HorizontalSlider(new Rect(textWidth + paddingLeft, yPos + 10f, sliderWidth, itemHeight), value, min, max);

        float totalWidth = textWidth + sliderWidth + paddingLeft + 7f;
        GUI.Label(new Rect(totalWidth, yPos + 5f, itemWidth - totalWidth + paddingLeft, 20f), value - (int)value > 0.01 ? value.ToString("0.000") : value.ToString("0"), "TextArea");

        yPos += itemHeight;

        return value;
    }

    public static float Slider(int value, string text, float min, float max) {
        return Slider((float) value, text, min, max);
    }

    public static void DrawLogo(Texture2D texture) {
        float width = texture.width;
        float height = texture.height;

        GUI.DrawTexture(
            new Rect(Screen.width / scaleFactor - width, Screen.height / scaleFactor - height, width, height), texture,
            ScaleMode.ScaleAndCrop);
    }
}