using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Sloane
{
    [CustomEditor(typeof(SloanePixelartPalette))]
    public class SloanePixelartPaletteEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SloanePixelartPalette palette = (SloanePixelartPalette)target;

            if (GUILayout.Button("Generate Texture2D"))
            {
                Texture2D generatedTexture = palette.GenerateTexture2D();
                if (generatedTexture != null)
                {
                    string path = EditorUtility.SaveFilePanelInProject("Save Texture as PNG", "NewTexture", "png", "Please enter a filename to save the texture to");
                    if (!string.IsNullOrEmpty(path))
                    {
                        byte[] bytes = generatedTexture.EncodeToPNG();
                        File.WriteAllBytes(path, bytes);
                        AssetDatabase.Refresh();
                    }
                }
            }

            if (GUILayout.Button("Load Colors from Hex File"))
            {
                string path = EditorUtility.OpenFilePanel("Load Hex Color File", "", "hex");
                if (!string.IsNullOrEmpty(path))
                {
                    palette.Colors.Clear();
                    string[] lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        if (ColorUtility.TryParseHtmlString($"#{line}", out Color newColor))
                        {
                            palette.Colors.Add(newColor);
                        }
                    }
                    EditorUtility.SetDirty(palette);
                }
            }
        }
    }
}
