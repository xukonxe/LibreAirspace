using LeTai.TrueShadow.PluginInterfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LeTai.TrueShadow
{
[ExecuteAlways]
[RequireComponent(typeof(TrueShadow))]
public class DisableShadowCache : MonoBehaviour, ITrueShadowCustomHashProvider
{
    TrueShadow  shadow;
    public bool everyFrame;

    void OnEnable()
    {
        shadow = GetComponent<TrueShadow>();
        Dirty();
    }

    void Update()
    {
        if (everyFrame)
            Dirty();
    }

    void Dirty()
    {
        shadow.CustomHash = Random.Range(int.MinValue, int.MaxValue);
        shadow.SetTextureDirty();
    }

    void OnDisable()
    {
        shadow.CustomHash = 0;
        shadow.SetTextureDirty();
    }
}
}
