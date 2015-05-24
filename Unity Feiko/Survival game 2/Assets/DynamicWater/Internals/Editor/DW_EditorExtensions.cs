#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#  define PRE_UNITY_4_3
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LostPolygon.DynamicWaterSystem.EditorExtensions {
    public static class EditorGUILayoutExtensions {
        private const float IndentationWidth = 9f;
        private const float LeftPaddingWidth = 4f;
        //FixedWidthLabel class. Extends IDisposable, so that it can be used with the "using" keyword.
        private class FixedWidthLabel : IDisposable {
            private readonly ZeroIndent _indentReset; // Helper class to reset and restore indentation

            public FixedWidthLabel(GUIContent label) {
#             if PRE_UNITY_4_3
                float indentation = IndentationWidth * EditorGUI.indentLevel + LeftPaddingWidth;
#             else
                float indentation = IndentationWidth * EditorGUI.indentLevel;
#             endif

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indentation);
                float width = Mathf.Max(EditorGUIUtilityInternal.labelWidth - indentation,
                                        GUI.skin.label.CalcSize(label).x);
                GUILayout.Label(label, GUILayout.Width(width));

                _indentReset = new ZeroIndent();
            }

            public FixedWidthLabel(string label) : this(new GUIContent(label)) {
            }

            public void Dispose() {
                _indentReset.Dispose();
                EditorGUILayout.EndHorizontal();
            }
        }

        private class ZeroIndent : IDisposable //helper class to clear indentation
        {
            private readonly int _originalIndent; //the original indentation value before we change the GUI state

            public ZeroIndent() {
                _originalIndent = EditorGUI.indentLevel; //save original indentation
                EditorGUI.indentLevel = 0; //clear indentation
            }

            public void Dispose() {
                EditorGUI.indentLevel = _originalIndent; //restore original indentation
            }
        }

        public static bool ToggleFixedWidth(GUIContent label, bool value) {
            using (new FixedWidthLabel(label)) {
                Rect controlRect =
                    EditorGUILayoutInternal.GetControlRect(
                        false,
                        16f,
                        EditorStyles.toggle,
                        null
                        );

                value =
                    GUI.Toggle(
                        controlRect,
                        value,
                        ""
                        );

                return value;
            }
        }

        public static bool ToggleFixedWidth(string label, bool value) {
            return ToggleFixedWidth(new GUIContent(label), value);
        }

        public static int IntSliderFixedWidth(GUIContent label, int value, int leftValue, int rightValue) {
            using (new FixedWidthLabel(label)) {
                value =
                    EditorGUI.IntSlider(
                        EditorGUILayoutInternal.GetControlRect(
                            false,
                            16f,
                            EditorStyles.toggle,
                            null
                            ),
                        "",
                        value,
                        leftValue,
                        rightValue
                        );

                return value;
            }
        }

        public static int IntSliderFixedWidth(string label, int value, int leftValue, int rightValue) {
            return IntSliderFixedWidth(new GUIContent(label), value, leftValue, rightValue);
        }
    }

    public static class EditorGUIInternal {
        public const float kNumberW = 40f;
    }

    public static class EditorGUIUtilityInternal {
        public static float labelWidth {
            get {
#             if PRE_UNITY_4_3
                Type type = typeof (EditorGUIUtility);
                FieldInfo info = type.GetField("labelWidth", BindingFlags.Static | BindingFlags.NonPublic);
                if (info != null) {
                    object value = info.GetValue(null);
                    return (float) value;
                }

                return 0f;
#             else
                return EditorGUIUtility.labelWidth;
#             endif
            }
        }
    }

    public static class EditorStylesInternal
    {
        public static GUIStyle helpBox
        {
            get
            {
                Type type = typeof(EditorStyles);
                PropertyInfo info = type.GetProperty("helpBox", BindingFlags.Static | BindingFlags.NonPublic);
                if (info != null)
                {
                    object value = info.GetValue(type, null);
                    return (GUIStyle)value;
                }

                return EditorStyles.label;
            }
        }
    }

    public static class EditorGUILayoutInternal {
        public const float kLabelFloatMinW = 80f + EditorGUIInternal.kNumberW + 5f;
        public const float kLabelFloatMaxW = 80f + EditorGUIInternal.kNumberW + 5f;
        public const float kPlatformTabWidth = 30f;

        public static Rect GetControlRect(bool hasLabel, float height, GUIStyle style, params GUILayoutOption[] options) {
            Rect rect = GUILayoutUtility.GetRect(!hasLabel ? EditorGUIInternal.kNumberW : kLabelFloatMinW,
                                                 kLabelFloatMaxW, height, height, style, options);
            
#         if PRE_UNITY_4_3
            rect.yMin -= 2f;
#         endif

            return rect;
        }
    }

    public static class TagManager {
        private const string TagManagerPath = "ProjectSettings/TagManager.asset";
        private static SerializedObject _tagManager;
        private static SerializedProperty _tags;

        static TagManager() {
            UpdateManager();
        }

        private static void UpdateManager() {
            _tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(TagManagerPath)[0]);
            _tags = _tagManager.FindProperty("tags");
        }

        private static void SaveManager() {
            _tagManager.UpdateIfDirtyOrScript();
            _tagManager.ApplyModifiedProperties();
        }

        public static bool IsTagExists(string tag) {
            UpdateManager();

            IEnumerator it = _tags.GetEnumerator();

            while (it.MoveNext()) {
                SerializedProperty prop = it.Current as SerializedProperty;
                if (prop == null || prop.type != "string") {
                    continue;
                }

                if (prop.stringValue == tag) {
                    return true;
                }
            }

            return false;
        }

        public static void AddTag(string tag) {
            UpdateManager();

            if (IsTagExists(tag)) {
                return;
            }

            int newIndex = _tags.arraySize - 1;
            _tags.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newTag = _tags.GetArrayElementAtIndex(newIndex);
            newTag.stringValue = tag.Trim();

            SaveManager();
        }

        public static string GetLayer(int layerNumber) {
            UpdateManager();

            SerializedProperty layer =
                _tagManager.FindProperty("User Layer " + layerNumber.ToString(CultureInfo.InvariantCulture));

            return layer != null ? layer.stringValue : null;
        }

        public static bool SetLayer(int layerNumber, string layerName) {
            UpdateManager();

            SerializedProperty layer =
                _tagManager.FindProperty("User Layer " + layerNumber.ToString(CultureInfo.InvariantCulture));

            if (layer == null) {
                return false;
            }

            layer.stringValue = layerName.Trim();
            SaveManager();

            return true;
        }

        public static bool IsLayerExists(string layerName) {
            for (int i = 8; i <= 32; i++) {
                if (GetLayer(i) == layerName) {
                    return true;
                }
            }

            return false;
        }

        public enum LayerSearchDirection {
            LastToFirst,
            FirstToLast,
        }

        public static int GetFreeLayer(LayerSearchDirection searchDirection) {
            if (searchDirection == LayerSearchDirection.FirstToLast) {
                for (int i = 8; i <= 32; i++) {
                    if (GetLayer(i) == "") {
                        return i;
                    }
                }
            } else {
                for (int i = 32; i >= 8; i--) {
                    if (GetLayer(i) == "") {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static void AddLayer(string layerName,
                                    LayerSearchDirection searchDirection = LayerSearchDirection.LastToFirst) {
            if (IsLayerExists(layerName)) {
                return;
            }

            int freeLayer = GetFreeLayer(searchDirection);
            SetLayer(freeLayer, layerName);
        }
    }

    public static class TextureImporterHelper {
        public static void SetTextureIsReadable(Texture2D texture, bool readable) {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null) {
                textureImporter.isReadable = readable;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        public static bool GetTextureIsReadable(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            return textureImporter == null || textureImporter.isReadable;
        }
    }

    public static class AssetDatabaseHelper {
        private class Folders
        {
            public string Source { get; private set; }
            public string Target { get; private set; }
        
            public Folders(string source, string target)
            {
                Source = source;
                Target = target;
            }
        }

        private static void MoveDirectoryEx(string source, string target)
        {
            var stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));
        
            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                     string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                     if (File.Exists(targetFile)) File.Delete(targetFile);
                     File.Move(file, targetFile);
                }
        
                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }
            Directory.Delete(source, true);
        }

        public static void MoveAssetDirectory(string sourceDir, string destDir) {
            destDir = Path.GetFullPath(destDir);
            sourceDir = Path.GetFullPath(sourceDir);

            DirectoryInfo sourceInfo = new DirectoryInfo(sourceDir);
            DirectoryInfo destInfo = new DirectoryInfo(destDir);

            if (Directory.Exists(destDir) && Directory.GetFiles(destDir).Length == 0)
                Directory.Delete(destDir);

            Directory.CreateDirectory(destInfo.Parent.ToString());

            MoveDirectoryEx(sourceDir, destDir);

            string slash = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

            string sourceMeta = sourceInfo.Parent + slash + sourceInfo.Name + ".meta";
            string destMeta = destInfo.Parent + slash + destInfo.Name + ".meta";

            if (File.Exists(destMeta))
                File.Delete(destMeta);

            File.Move(sourceMeta, destMeta);
        }
    }
}