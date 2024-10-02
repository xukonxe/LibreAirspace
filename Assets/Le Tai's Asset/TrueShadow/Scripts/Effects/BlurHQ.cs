using UnityEngine;
using UnityEngine.Rendering;

namespace LeTai.Effects
{
public class BlurHQConfig : BlurConfig
{
    [SerializeField] float strength;

    static readonly float SQRT05 = Mathf.Sqrt(2f);

    internal readonly float[] rawWeights = new float[128];
    internal readonly float[] weights    = new float[64];
    internal readonly float[] offsets    = new float[64];

    public int Extent    { get; private set; }
    public int NumWeight { get; private set; }

    public override int MinExtent => 0;

    public override float Strength
    {
        get => strength;
        set
        {
            var newStrength = Mathf.Min(value, 126);
            if (Mathf.Approximately(strength, newStrength))
                return;

            strength = newStrength;
            Extent   = Mathf.CeilToInt(strength);
            var sigma       = .5f * strength;
            var scaleFactor = 1f / sigma * SQRT05;

            var sumWeight = rawWeights[0] = 1f;
            for (int i = 1; i <= Extent; i++)
            {
                float w;
                if (sigma < .8f)
                {
                    var p1 = Erf((i + 0.5f) * scaleFactor);
                    var p2 = Erf((i - 0.5f) * scaleFactor);
                    w = (p1 - p2) / 2f;
                }
                else
                {
                    w = Mathf.Exp(-i * i / sigma / sigma);
                }

                rawWeights[i] =  w;
                sumWeight     += w * 2;
            }

            var norm = 1f / sumWeight;
            for (int i = 0; i <= Extent; i++)
            {
                rawWeights[i] *= norm;
            }

            weights[0] = rawWeights[0];
            NumWeight  = Mathf.CeilToInt(Extent / 2f) + 1;
            for (int i = 1; i < NumWeight; i++)
            {
                var ir = i * 2;
                var il = ir - 1;

                var w0 = rawWeights[il];
                var w1 = ir <= Extent ? rawWeights[ir] : 0;
                var w  = w0 + w1;

                weights[i] = w;
                offsets[i] = il + w1 / w;
            }
        }
    }

    static float Erf(float x)
    {
        const float a1 = 0.254829592f;
        const float a2 = -0.284496736f;
        const float a3 = 1.421413741f;
        const float a4 = -1.453152027f;
        const float a5 = 1.061405429f;
        const float p  = 0.3275911f;

        int sign = x < 0 ? -1 : 1;
        x = Mathf.Abs(x);

        float t = 1.0f / (1.0f + p * x);
        float y = 1.0f - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Mathf.Exp(-x * x);

        return sign * y;
    }
}

public class BlurHQ : IBlurAlgorithm
{
    public const float MAX_RADIUS = 126;

    BlurHQConfig config;
    Material     _material;

    Material Material
    {
        get
        {
            if (_material == null)
            {
                _material = new Material(Shader.Find("Hidden/TrueShadow/GenerateHQ"));
            }

            return _material;
        }
    }

    public void Configure(BlurConfig config)
    {
        this.config = (BlurHQConfig)config;
    }

    public void Blur(CommandBuffer cmd, RenderTargetIdentifier src, Rect srcCropRegion, RenderTexture target)
    {
        cmd.GetTemporaryRT(ShaderId.TMP_TEX, target.width, target.height, 0, FilterMode.Bilinear, target.format);

        Material.DisableKeyword("MAX_TAPS_64");
        Material.DisableKeyword("MAX_TAPS_16");
        Material.DisableKeyword("MAX_TAPS_4");
        if (config.NumWeight <= 8)
            Material.EnableKeyword("MAX_TAPS_8");
        else if (config.NumWeight <= 32)
            Material.EnableKeyword("MAX_TAPS_32");
        else
            Material.EnableKeyword("MAX_TAPS_64");

        Material.SetInt(ShaderId.EXTENT, config.Extent);
        Material.SetFloatArray(ShaderId.WEIGHTS, config.weights);
        Material.SetFloatArray(ShaderId.OFFSETS, config.offsets);

        Material.SetTexture(ShaderId.BLUE_NOISE, Resources.BLUE_NOISE);
        Material.SetVector(ShaderId.TARGET_SIZE, new Vector4(target.width, target.height));

        cmd.Blit(src,              ShaderId.TMP_TEX, Material, 0);
        cmd.Blit(ShaderId.TMP_TEX, target,           Material, 1);

        cmd.ReleaseTemporaryRT(ShaderId.TMP_TEX);
    }
}
}
