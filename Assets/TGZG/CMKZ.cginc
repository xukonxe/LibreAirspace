#include "UnityCG.cginc" //引用unity自带的函数，下面调用
sampler2D _MainTex;//自动赋值，没用，消除警告
sampler2D _GrabTexture;//自动获得屏幕各坐标的颜色。未渲染此物体之前
float4 _GrabTexture_TexelSize;//自动赋值。分辨率的倒数
float4 _MainTex_ST;//自动获得

float2 Rect;//C#里脚本函数里会设置，矩形宽度
float4 BorderC;//脚本设置，边框颜色
float BorderW;//脚本设置，边框宽度
float4 SelfColor;//脚本设置，本色
float Size;//脚本设置，模糊强度。默认强度应当为5
int Mix;//脚本设置，是否混合本色

struct In { //输入结构体。是屏幕上每个点的原状态
    float4 vertex : POSITION; //位置，范围从-1到1
    float2 uv : TEXCOORD0;//位置
    float4 color : COLOR;//颜色
};
struct Out { //输出结构体。是屏幕上每个点的原状态的格式化版本
    float4 vertex : SV_POSITION;//位置，范围从-1到1
    float2 uv : TEXCOORD0;//位置
    float4 uvgrab : TEXCOORD1;//位置，范围从0到1
    float4 color : COLOR;//颜色
};
Out Transform(In v) { //格式化函数
    Out o;
    o.vertex = UnityObjectToClipPos(v.vertex);//格式化
    o.uv = v.uv;
    #if UNITY_UV_STARTS_AT_TOP //适应不同的设备。有的原点在上，有的原点在下
        float scale = -1.0;
    #else
        float scale = 1.0;
    #endif
    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;//从-1到1 归入0到1。格式化
    o.uvgrab.zw = o.vertex.zw;
    o.color = v.color;
    return o;
}
float4 ClearAlpha(float4 X) { //工具函数，清除透明度
    return float4(X.x, X.y, X.z, 0.5);
}
#define PI 3.141592
#define TAU (PI * 2.0)
float Gaussian(float d, float sigma) { //距离，最大半径。高斯模糊函数
	return 1.0 / (sigma * sqrt(TAU)) * exp(-(d*d) / (2.0 * sigma * sigma));
}
float4 HBlur(Out i) : SV_Target { //水平模糊
    float4 color = float4(0.0, 0.0, 0.0, 0.0);
    float H = 0;
    for (int j = 0; j < 2*Size+1; j++) { //遍历模糊半径中的圆形范围的每一个像素，根据高斯模糊函数计算
        float4 A = tex2D(_GrabTexture, i.uvgrab.xy + float2(0, (j-Size)*_GrabTexture_TexelSize.y));
        float G = Gaussian(abs(j-Size),Size);
        color += A * G;//A是颜色，G是权重
        H += G;
    }
    return color/H;//颜色是周围所有像素的颜色按权重加起来、除以所有像素的权重之和
}
float4 VBlurAndOutLine(Out i) : SV_Target { //垂直模糊和描边
    if (i.uv.x * Rect.x < BorderW || i.uv.y * Rect.y < BorderW || (1.0 - i.uv.x) * Rect.x < BorderW || (1.0 - i.uv.y) * Rect.y < BorderW){
        return BorderC; //如果在边缘，则描边
    }
    float4 color = float4(0.0, 0.0, 0.0, 0.0);
    float H = 0;
    for (int j = 0; j < 2*Size+1; j++) { //垂直模糊
        if(j>100)break; //可以删掉。之前测试时其他代码写错，避免死循环卡死
        float4 A = tex2D(_GrabTexture, i.uvgrab.xy + float2((j-Size)*_GrabTexture_TexelSize.x, 0));
        if (Mix == 1) { //Mix表示是否携带image本身的颜色，在C#中设置。1是true
            A = SelfColor.w*SelfColor + (1 - SelfColor.w) * A;
            //从_MainTex采样并混合
            //A=lerp(tex2D(_MainTex, i.uv),A,A.w);
        }
        float G = Gaussian(abs(j-Size),Size);
        color += A * G;
        H += G;
    }
    return color/H;
}