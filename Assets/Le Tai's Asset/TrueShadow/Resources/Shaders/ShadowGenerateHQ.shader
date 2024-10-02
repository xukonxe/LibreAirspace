Shader "Hidden/TrueShadow/GenerateHQ"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    #pragma enable_d3d11_debug_symbols
    #pragma multi_compile_local_fragment MAX_TAPS_64 MAX_TAPS_32 MAX_TAPS_8

    sampler2D _MainTex;
    float4    _MainTex_TexelSize;
    float4    _MainTex_ST;

    #define MAX_TAPS 64

    #ifdef MAX_TAPS_64
    #define MAX_TAPS 64
    #elif MAX_TAPS_32
    #define MAX_TAPS 32
    #elif MAX_TAPS_8
    #define MAX_TAPS 8
    #endif

    float _Weights[MAX_TAPS];
    float _Offsets[MAX_TAPS];
    int   _Extent;

    sampler2D _BlueNoise;
    half4     _BlueNoise_TexelSize;
    half2     _TargetSize;

    struct v2f
    {
        half4 vertex : SV_POSITION;
        half2 texcoord : TEXCOORD0;
    };

    v2f vert(appdata_img v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = v.texcoord.xy;
        return o;
    }

    half4 blur(float2 centerUv, uint axis)
    {
        half4 o = 0.;

        int middle = ceil(_Extent / 2.);
        for (int i = 0; i <= _Extent; ++i)
        {
            int   j = abs(middle - i);
            float sign = i < middle ? -1 : 1;

            float texelSize = axis == 0 ? _MainTex_TexelSize.x : _MainTex_TexelSize.y;

            float  offset = sign * _Offsets[j] * texelSize;
            float2 sampleUv = centerUv;
            if (axis == 0)
                sampleUv.x += offset;
            else
                sampleUv.y += offset;

            o += tex2D(_MainTex, sampleUv) * _Weights[j];
        }

        half2 noiseUv = (centerUv - .5) * _TargetSize;
        half  noise = tex2D(_BlueNoise, noiseUv * _BlueNoise_TexelSize.xy).r;
        noise -= .5;
        noise *= 1. / 255.;

        o += noise;

        return o;
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag(v2f IN) : SV_Target
            {
                return blur(IN.texcoord, 0);
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag(v2f i) : SV_Target
            {
                return blur(i.texcoord, 1);
            }
            ENDCG
        }
    }

    FallBack Off
}
