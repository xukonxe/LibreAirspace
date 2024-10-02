Shader "Custom/模糊与描边3" {
    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {} //消除警告
    }
    SubShader {
        Tags {
            "Queue" = "Transparent" //消除警告
            "IgnoreProjector" = "True" //消除警告
            "RenderType" = "Transparent" //消除警告
            "PreviewType" = "Plane" //消除警告
            "CanUseSpriteAtlas" = "True" //消除警告
        }
        GrabPass { Tags { "LightMode" = "Always" }}//用于后续自动分配：_GrabTexture
        Pass {
            //Blend SrcAlpha OneMinusSrcAlpha //让透明度有效
            CGPROGRAM
            #pragma vertex Transform //格式化函数引用CMKZ的格式化函数
            #pragma fragment HBlur //效果函数引用CMKZ的水平模糊
            #include "CMKZ.cginc"
            ENDCG
        }
        GrabPass { Tags { "LightMode" = "Always" }}//用于后续自动分配：_GrabTexture
        Pass {
            //Blend SrcAlpha OneMinusSrcAlpha //让透明度有效
            CGPROGRAM
            #pragma vertex Transform
            #pragma fragment VBlurAndOutLine //垂直模糊与边框
            #include "CMKZ.cginc"
            ENDCG
        }
    }
}