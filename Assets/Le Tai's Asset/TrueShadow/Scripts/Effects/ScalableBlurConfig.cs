using UnityEngine;
using static UnityEngine.Mathf;

namespace LeTai.Effects
{
public class ScalableBlurConfig : BlurConfig
{
    [SerializeField]                 float radius    = 4;
    [SerializeField]                 int   iteration = 4;
    [SerializeField]                 int   maxDepth  = 6;
    [SerializeField] [Range(0, 256)] float strength;

    /// <summary>
    /// Distance between the base texel and the texel to be sampled.
    /// </summary>
    public float Radius
    {
        get { return radius; }
        set { radius = Max(0, value); }
    }

    /// <summary>
    /// Half the number of time to process the image. It is half because the real number of iteration must alway be even. Using half also make calculation simpler
    /// </summary>
    /// <value>
    /// Must be non-negative
    /// </value>
    public int Iteration
    {
        get { return iteration; }
        set { iteration = Max(0, value); }
    }

    /// <summary>
    /// Clamp the minimum size of the intermediate texture. Reduce flickering and blur
    /// </summary>
    /// <value>
    /// Must larger than 0
    /// </value>
    public int MaxDepth
    {
        get { return maxDepth; }
        set { maxDepth = Max(1, value); }
    }

    public override int MinExtent => (Iteration + 1) * (Iteration + 1);

    /// <summary>
    /// User friendly property to control the amount of blur
    /// </summary>
    ///<value>
    /// Must be non-negative
    /// </value>
    public override float Strength
    {
        get { return strength = radius * (3 * (1 << iteration) - 2) / UNIT_VARIANCE; }
        set
        {
            strength = Max(0, value);
            SetAdvancedFieldFromSimple();
        }
    }

    // With the "correct" unit variance, the edge of the shadow at higher stddev go below 8bit fixed point resolution
    // We "wastes" processing power on these.
    // TODO: optimize that:
    // The maximum distance that will show up is:
    // e^(-D^2 / 2R^2) < .5/256
    // => D < 3*sqrt(2*log(2)) * R ~ 3.53223*R
    // Can probably stop sooner than that
    static readonly float UNIT_VARIANCE = 1f + Sqrt(2f) / 2f;

    /// <summary>
    /// Calculate size and iteration from strength
    /// </summary>
    protected virtual void SetAdvancedFieldFromSimple()
    {
        strength = Clamp(strength, 0, (1 << 14) * (1 << 14));

        var scaledStrength = strength * .66f;

        radius    = Sqrt(scaledStrength);
        iteration = 0;
        while ((1 << iteration) < radius)
            iteration++;
        radius = scaledStrength / (1 << iteration);
    }

    void OnValidate()
    {
        SetAdvancedFieldFromSimple();
    }
}
}
