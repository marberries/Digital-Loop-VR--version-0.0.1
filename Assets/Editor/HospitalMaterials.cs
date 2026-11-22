using System.IO;
using UnityEngine;
using UnityEditor;

public static class HospitalMaterials
{
    public static Shader GetLitShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        return shader != null ? shader : Shader.Find("Standard");
    }

    public static Material CreateMaterial(string name, Color color, float smoothness = 0.2f, float metallic = 0.0f)
    {
        EnsureFolders();
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(GetLitShader());
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        return mat;
    }

    public static Material CreateEmissiveMaterial(string name, Color baseColor, Color emissionColor, float intensity = 2.0f)
    {
        Material mat = CreateMaterial(name, baseColor, 0.4f, 0.0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor * intensity);
        return mat;
    }

    public static Material CreateTiledFloorMaterial()
    {
        EnsureFolders();
        string texPath = "Assets/Materials/Hospital_Floor_Tiles.png";
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        Color tileLight = new Color(0.68f, 0.72f, 0.74f);
        Color tileAlt = new Color(0.64f, 0.68f, 0.70f);
        Color grout = new Color(0.42f, 0.46f, 0.49f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isBorder = (x < 3 || y < 3 || (x >= 127 && x <= 129) || (y >= 127 && y <= 129));
                if (isBorder)
                {
                    pixels[y * size + x] = grout;
                }
                else
                {
                    bool checker = ((x < 128) ^ (y < 128));
                    pixels[y * size + x] = checker ? tileLight : tileAlt;
                }
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (importedTex != null) importedTex.wrapMode = TextureWrapMode.Repeat;

        Material mat = CreateMaterial("Hospital_Floor_Mat", Color.white, 0.52f, 0.02f);
        mat.SetTexture("_BaseMap", importedTex);
        mat.SetTexture("_MainTex", importedTex);
        mat.SetTextureScale("_BaseMap", new Vector2(10f, 10f));
        mat.SetTextureScale("_MainTex", new Vector2(10f, 10f));
        return mat;
    }

    public static Material CreateECGScreenMaterial()
    {
        EnsureFolders();
        string texPath = "Assets/Materials/Medical_ECG_Screen.png";
        int width = 512;
        int height = 340;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color bg = new Color(0.02f, 0.06f, 0.07f);
        Color grid = new Color(0.05f, 0.16f, 0.14f);
        Color trace = new Color(0.10f, 0.98f, 0.48f);
        Color warn = new Color(0.95f, 0.35f, 0.25f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isGrid = (x % 32 == 0) || (y % 32 == 0);
                tex.SetPixel(x, y, isGrid ? grid : bg);
            }
        }

        int midY = height / 2;
        for (int x = 20; x < width - 20; x++)
        {
            int cycle = x % 140;
            int offsetY = 0;
            if (cycle > 45 && cycle <= 52) offsetY = (cycle - 45) * 4;
            else if (cycle > 52 && cycle <= 58) offsetY = 28 - (cycle - 52) * 5;
            else if (cycle > 62 && cycle <= 66) offsetY = -(cycle - 62) * 6;
            else if (cycle > 66 && cycle <= 73) offsetY = -24 + (cycle - 66) * 16;
            else if (cycle > 73 && cycle <= 81) offsetY = 88 - (cycle - 73) * 15;
            else if (cycle > 81 && cycle <= 86) offsetY = -32 + (cycle - 81) * 6;
            else if (cycle > 96 && cycle <= 106) offsetY = (int)(Mathf.Sin((cycle - 96) / 10f * Mathf.PI) * 18f);

            int py = Mathf.Clamp(midY + offsetY, 4, height - 5);
            for (int dy = -2; dy <= 2; dy++)
            {
                tex.SetPixel(x, py + dy, trace);
            }
        }

        for (int x = 20; x < 160; x++)
            for (int y = height - 34; y < height - 22; y++)
                tex.SetPixel(x, y, trace);

        for (int x = width - 150; x < width - 20; x++)
            for (int y = height - 34; y < height - 22; y++)
                tex.SetPixel(x, y, warn);

        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        Material mat = CreateEmissiveMaterial("Medical_ECG_Screen_Mat", Color.white, Color.white, 1.45f);
        mat.SetTexture("_BaseMap", importedTex);
        mat.SetTexture("_MainTex", importedTex);
        mat.SetTexture("_EmissionMap", importedTex);
        return mat;
    }

    public static Material CreatePaintingMaterial(string name, int variant)
    {
        EnsureFolders();
        string texPath = $"Assets/Materials/{name}_Tex.png";
        int w = 512;
        int h = 384;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            float ny = (float)y / h;
            for (int x = 0; x < w; x++)
            {
                float nx = (float)x / w;
                Color c;

                if (variant == 1)
                {
                    Color skyTop = new Color(0.48f, 0.65f, 0.78f);
                    Color skyHorizon = new Color(0.86f, 0.82f, 0.74f);
                    c = Color.Lerp(skyHorizon, skyTop, Mathf.Clamp01((ny - 0.35f) / 0.65f));

                    float farRidge = 0.52f + Mathf.Sin(nx * 6.5f) * 0.08f + Mathf.Cos(nx * 14f) * 0.03f;
                    float nearRidge = 0.42f + Mathf.Cos(nx * 5.0f + 1.2f) * 0.06f + Mathf.Sin(nx * 11f) * 0.025f;

                    if (ny < farRidge && ny >= 0.32f)
                        c = new Color(0.28f, 0.40f, 0.48f);
                    if (ny < nearRidge && ny >= 0.32f)
                        c = new Color(0.16f, 0.28f, 0.32f);
                    if (ny < 0.32f)
                    {
                        float ripple = Mathf.Sin(ny * 180f + nx * 12f) * 0.02f;
                        c = new Color(0.24f + ripple, 0.42f + ripple, 0.50f);
                    }
                }
                else
                {
                    Color twilightTop = new Color(0.14f, 0.12f, 0.25f);
                    Color twilightBottom = new Color(0.82f, 0.52f, 0.32f);
                    c = Color.Lerp(twilightBottom, twilightTop, Mathf.Clamp01((ny - 0.25f) / 0.75f));

                    float dx = Mathf.Abs(nx - 0.5f);
                    float tentRoofTop = 0.76f - dx * 1.15f;
                    if (ny < 0.28f)
                    {
                        c = new Color(0.12f, 0.14f, 0.18f);
                    }
                    else if (dx < 0.26f && ny < tentRoofTop)
                    {
                        bool stripe = ((int)(nx * 24f) % 2 == 0);
                        c = stripe ? new Color(0.74f, 0.14f, 0.18f) : new Color(0.88f, 0.80f, 0.62f);
                    }
                }

                if (x < 14 || x >= w - 14 || y < 14 || y >= h - 14)
                {
                    c = new Color(0.92f, 0.91f, 0.87f);
                }

                pixels[y * w + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        Material mat = CreateMaterial(name, Color.white, 0.25f, 0.0f);
        mat.SetTexture("_BaseMap", importedTex);
        mat.SetTexture("_MainTex", importedTex);
        return mat;
    }

    public static Material CreateCertificateMaterial(string name, int variant)
    {
        EnsureFolders();
        string texPath = $"Assets/Materials/{name}_Tex.png";
        int w = 384;
        int h = 512;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[w * h];

        Color paper = new Color(0.94f, 0.93f, 0.89f);
        Color borderAccent = variant == 1 ? new Color(0.14f, 0.34f, 0.48f) : new Color(0.55f, 0.42f, 0.18f);
        Color textDark = new Color(0.22f, 0.24f, 0.26f);
        Color sealRed = new Color(0.75f, 0.18f, 0.18f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = paper;

                bool outerFrame = (x >= 18 && x <= 23 && y >= 18 && y <= h - 18) ||
                                  (x >= w - 23 && x <= w - 18 && y >= 18 && y <= h - 18) ||
                                  (y >= 18 && y <= 23 && x >= 18 && x <= w - 18) ||
                                  (y >= h - 23 && y <= h - 18 && x >= 18 && x <= w - 18);

                bool innerFrame = (x >= 30 && x <= 32 && y >= 30 && y <= h - 30) ||
                                  (x >= w - 32 && x <= w - 30 && y >= 30 && y <= h - 30) ||
                                  (y >= 30 && y <= 32 && x >= 30 && x <= w - 30) ||
                                  (y >= h - 32 && y <= h - 30 && x >= 30 && x <= w - 30);

                if (outerFrame || innerFrame) c = borderAccent;

                float crestDist = Vector2.Distance(new Vector2(x, y), new Vector2(w * 0.5f, h - 76f));
                if (crestDist < 22f) c = borderAccent;

                if (y >= h - 132 && y <= h - 122 && x >= 85 && x <= w - 85) c = borderAccent;
                if (y >= h - 165 && y <= h - 159 && x >= 65 && x <= w - 65) c = textDark;
                if (y >= h - 195 && y <= h - 190 && x >= 75 && x <= w - 75) c = textDark;
                if (y >= h - 220 && y <= h - 215 && x >= 60 && x <= w - 60) c = textDark;
                if (y >= h - 245 && y <= h - 240 && x >= 80 && x <= w - 80) c = textDark;
                if (y >= h - 270 && y <= h - 265 && x >= 70 && x <= w - 70) c = textDark;

                if (y >= 88 && y <= 92 && x >= 60 && x <= 185) c = textDark;
                float sealDist = Vector2.Distance(new Vector2(x, y), new Vector2(w - 105f, 95f));
                if (sealDist < 24f) c = sealRed;

                pixels[y * w + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        Material mat = CreateMaterial(name, Color.white, 0.55f, 0.05f);
        mat.SetTexture("_BaseMap", importedTex);
        mat.SetTexture("_MainTex", importedTex);
        return mat;
    }

    public static Material CreateClockFaceMaterial()
    {
        EnsureFolders();
        string texPath = "Assets/Materials/Hospital_Clock_Tex.png";
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                Color c = dist > 120f ? new Color(0.22f, 0.25f, 0.28f) : new Color(0.92f, 0.94f, 0.94f);

                if (dist >= 96f && dist <= 112f)
                {
                    float angle = Mathf.Atan2(y - center.y, x - center.x) * Mathf.Rad2Deg;
                    if (angle < 0f) angle += 360f;
                    if (Mathf.Abs((angle % 30f) - 15f) > 13.5f)
                    {
                        c = new Color(0.15f, 0.18f, 0.20f);
                    }
                }

                Vector2 rel = new Vector2(x, y) - center;
                if (rel.magnitude < 58f && Mathf.Abs(rel.x * 0.5f + rel.y * 0.86f) < 3.0f && rel.x < 5f)
                    c = new Color(0.12f, 0.14f, 0.16f);
                if (rel.magnitude < 84f && Mathf.Abs(rel.x * 0.5f - rel.y * 0.86f) < 2.2f && rel.x > -5f)
                    c = new Color(0.12f, 0.14f, 0.16f);
                if (dist < 7f)
                    c = new Color(0.75f, 0.18f, 0.18f);

                pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        Material mat = CreateMaterial("Hospital_Clock_Mat", Color.white, 0.6f, 0.1f);
        mat.SetTexture("_BaseMap", importedTex);
        mat.SetTexture("_MainTex", importedTex);
        return mat;
    }

    public static Material CreateVentLouversMaterial()
    {
        EnsureFolders();
        string texPath = "Assets/Materials/Hospital_Vent_Tex.png";
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color metal = new Color(0.70f, 0.73f, 0.75f);
        Color slot = new Color(0.14f, 0.16f, 0.18f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool border = x < 10 || x >= size - 10 || y < 10 || y >= size - 10;
                bool louver = !border && (y % 12 < 5) && (x < 58 || x > 70);
                tex.SetPixel(x, y, louver ? slot : metal);
            }
        }
        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        Material mat = CreateMaterial("Hospital_Vent_Mat", Color.white, 0.55f, 0.45f);
        mat.SetTexture("_BaseMap", importedTex);
        mat.SetTexture("_MainTex", importedTex);
        return mat;
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
    }
}