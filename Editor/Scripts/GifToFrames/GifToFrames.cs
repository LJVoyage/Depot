using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;

namespace VoyageForge.Depot.Editor
{
    public class GifToFrames : EditorWindow
    {
        string gifPath;
        string saveFolder = "Assets/GifFrames";
        float fps = 12f;

        [MenuItem("VoyageForge/Depot/GIF To SpriteSheet Material")]
        static void Init() => GetWindow<GifToFrames>().Show();

        void OnGUI()
        {
            GUILayout.Label("GIF → SpriteSheet + Material", EditorStyles.boldLabel);

            gifPath = EditorGUILayout.TextField("GIF Path", gifPath);
            saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
            fps = EditorGUILayout.FloatField("FPS", fps);

            if (GUILayout.Button("Generate SpriteSheet Material"))
            {
                Generate();
            }
        }

        void Generate()
        {
            if (!File.Exists(gifPath))
            {
                Debug.LogError("GIF not found!");
                return;
            }

            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            // 取 GIF 文件名（不带扩展）
            string fileName = Path.GetFileNameWithoutExtension(gifPath);

            string sheetName = fileName + "_Sheet.png";
            string materialName = fileName + "_Mat.mat";

            // 1️⃣ 拆帧
            using (var gif = Image.FromFile(gifPath))
            {
                var dimension = new FrameDimension(gif.FrameDimensionsList[0]);
                int frameCount = gif.GetFrameCount(dimension);

                int frameW = gif.Width;
                int frameH = gif.Height;

                int cols = Mathf.CeilToInt(Mathf.Sqrt(frameCount));
                int rows = Mathf.CeilToInt(frameCount / (float)cols);

                Texture2D sheet = new Texture2D(cols * frameW, rows * frameH, TextureFormat.RGBA32, false);

                // 填充透明背景
                Color[] fillColor = new Color[cols * frameW * rows * frameH];
                for (int i = 0; i < fillColor.Length; i++)
                    fillColor[i] = Color.clear;
                sheet.SetPixels(fillColor);

                for (int i = 0; i < frameCount; i++)
                {
                    gif.SelectActiveFrame(dimension, i);
                    using (var bmp = new Bitmap(gif))
                    {
                        for (int y = 0; y < frameH; y++)
                        {
                            for (int x = 0; x < frameW; x++)
                            {
                                var c = bmp.GetPixel(x, frameH - y - 1);
                                sheet.SetPixel(i % cols * frameW + x, rows * frameH - (i / cols + 1) * frameH + y,
                                    new Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f));
                            }
                        }
                    }
                }

                sheet.Apply();

                // 保存 Sprite Sheet
                string sheetPath = Path.Combine(saveFolder, sheetName);
                File.WriteAllBytes(sheetPath, sheet.EncodeToPNG());
                AssetDatabase.ImportAsset(sheetPath);
                TextureImporter ti = AssetImporter.GetAtPath(sheetPath) as TextureImporter;
                ti.textureType = TextureImporterType.Default;
                ti.wrapMode = TextureWrapMode.Repeat;
                ti.filterMode = FilterMode.Bilinear;
                ti.SaveAndReimport();

                Debug.Log("SpriteSheet created at: " + sheetPath);

                // 2️⃣ 创建材质
                Shader shader = Shader.Find("VoyageForge/Builtin/UI/AnimatedSpriteSheet");
                if (!shader)
                {
                    Debug.LogError("Shader 'Custom/AnimatedSpriteSheet' not found! Please create it first.");
                    return;
                }

                Material mat = new Material(shader);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
                mat.SetTexture("_MainTex", tex);
                mat.SetFloat("_Cols", cols);
                mat.SetFloat("_Rows", rows);
                mat.SetFloat("_FrameCount", frameCount);
                mat.SetFloat("_FPS", fps);

                string matPath = Path.Combine(saveFolder, materialName);
                AssetDatabase.CreateAsset(mat, matPath);
                AssetDatabase.SaveAssets();

                Debug.Log($"Material created at: {matPath}\nCols: {cols}, Rows: {rows}, Frames: {frameCount}");
            }
        }
    }
}