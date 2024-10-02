Shader "Custom/CircleShader"
{
    Properties
    {
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.5
        _OuterRadius ("Outer Radius", Range(0, 1)) = 0.8
        _CircleColor ("Circle Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        AlphaTest Greater 0.5

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _InnerRadius;
            float _OuterRadius;
            fixed4 _CircleColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float distance = length(i.uv - 0.5); // 计算距离中心的距离
                if(distance > _InnerRadius && distance < _OuterRadius)
                {
                    return _CircleColor;
                }
                else
                {
                    discard;
                }
                return _CircleColor;
            }
            ENDCG
        }
    }
}