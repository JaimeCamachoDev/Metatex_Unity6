using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

public enum Generator
{
    SolidColor,
    LinearGradient,
    RadialGradient,
    Checkerboard,
    Shader,
    Material
}

[ScriptedImporter(1, "metatex")]
public class CustomMetatexImporter : ScriptedImporter
{
    [SerializeField] Vector2Int dimensions = new Vector2Int(512, 512);
    [SerializeField] Generator generator = Generator.Checkerboard;
    [SerializeField] Color color = Color.white;
    [SerializeField] Color color2 = Color.black;
    [SerializeField] Gradient gradient = new Gradient();
    [SerializeField] int checkerSize = 8;

    [SerializeField] Shader shader = null;
    [SerializeField] Material material = null;

    [SerializeField] TextureWrapMode wrapMode = TextureWrapMode.Repeat;
    [SerializeField] FilterMode filterMode = FilterMode.Bilinear;
    [SerializeField, Range(0, 16)] int anisoLevel = 1;
    [SerializeField] bool compression = false;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        Texture2D texture = new Texture2D(dimensions.x, dimensions.y, TextureFormat.RGBA32, false);

        switch (generator)
        {
            case Generator.SolidColor:
                FillSolidColor(texture, color);
                break;
            case Generator.LinearGradient:
                FillLinearGradient(texture, gradient);
                break;
            case Generator.RadialGradient:
                FillRadialGradient(texture, gradient);
                break;
            case Generator.Checkerboard:
                FillCheckerboard(texture, color, color2, checkerSize);
                break;
            case Generator.Shader:
                if (shader != null)
                {
                    var mat = new Material(shader);
                    UpdateMaterial(mat);
                    BakeMaterial(texture, mat, 0);
                    Object.DestroyImmediate(mat);
                }
                else
                    FillSolidColor(texture, Color.magenta);
                break;
            case Generator.Material:
                if (material != null)
                {
                    UpdateMaterial(material);
                    BakeMaterial(texture, material, 0);
                }
                else
                    FillSolidColor(texture, Color.magenta);
                break;
        }

        if (compression)
        {
            texture.Compress(true);
            texture.Apply(true, true);
        }
        else
        {
            texture.Apply();
        }

        texture.wrapMode = wrapMode;
        texture.filterMode = filterMode;
        texture.anisoLevel = anisoLevel;

        ctx.AddObjectToAsset("texture", texture);
        ctx.SetMainObject(texture);
    }

    void UpdateMaterial(Material mat)
    {
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Color2")) mat.SetColor("_Color2", color2);
        if (mat.HasProperty("_Dimensions")) mat.SetVector("_Dimensions", new Vector4(dimensions.x, dimensions.y, 0, 0));
        if (mat.HasProperty("_Scale")) mat.SetVector("_Scale", new Vector2(1, 1));
    }

    void FillSolidColor(Texture2D tex, Color c)
    {
        Color[] fill = new Color[tex.width * tex.height];
        for (int i = 0; i < fill.Length; i++) fill[i] = c;
        tex.SetPixels(fill);
    }

    void FillLinearGradient(Texture2D tex, Gradient grad)
    {
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
                tex.SetPixel(x, y, grad.Evaluate((float)x / (tex.width - 1)));
    }

    void FillRadialGradient(Texture2D tex, Gradient grad)
    {
        Vector2 center = new Vector2(tex.width / 2f, tex.height / 2f);
        float maxDist = Vector2.Distance(Vector2.zero, center);
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                tex.SetPixel(x, y, grad.Evaluate(dist));
            }
    }

    void FillCheckerboard(Texture2D tex, Color c1, Color c2, int count)
    {
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                int cx = x * count / tex.width;
                int cy = y * count / tex.height;
                bool isEven = (cx + cy) % 2 == 0;
                tex.SetPixel(x, y, isEven ? c1 : c2);
            }
        }
    }

    void BakeMaterial(Texture2D tex, Material mat, int pass)
    {
        RenderTexture rt = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
        rt.Create();

        RenderTexture prev = RenderTexture.active;
        Graphics.Blit(null, rt, mat, pass);
        RenderTexture.active = rt;

        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = prev;
        rt.Release();
    }

    [MenuItem("Assets/Create/Metatex", false, 1000)]
    public static void CreateAsset()
    {
        ProjectWindowUtil.CreateAssetWithContent("New Metatex.metatex", "");
    }
}