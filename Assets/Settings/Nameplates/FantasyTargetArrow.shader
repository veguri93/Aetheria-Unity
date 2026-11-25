Shader "Aetheria/Nameplates/Contained Fantasy Energy"
{
    Properties
    {
        _BorderColor ("Metallic border color", Color) = (0.26,0.28,0.31,1)
        _HighlightColor ("Silver bevel highlight", Color) = (0.68,0.71,0.74,1)
        _EnergyColor ("Inner energy color", Color) = (0.24,0.56,0.9,1)
        _Intensity ("Inner energy intensity", Range(0,2)) = 0.85
        _Speed ("Energy animation speed", Range(0,1.5)) = 0.28
        _Glow ("Soft outer glow", Range(0,1)) = 0.22
        _Breathing ("Interior breathing amount", Range(0,0.15)) = 0.055
        _BorderWidth ("Metal rim thickness", Range(0.025,0.12)) = 0.065
        _Roundness ("Soft corner radius", Range(0.015,0.1)) = 0.055
        [HideInInspector] _Opacity ("Opacity", Float) = 1
        [HideInInspector] _Direction ("Direction", Float) = 1
        [HideInInspector] _Phase ("Phase", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Name "FantasyArrowhead"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            float4 _BorderColor, _HighlightColor, _EnergyColor;
            float _Intensity, _Speed, _Glow, _Breathing, _BorderWidth, _Roundness;
            float _Opacity, _Direction, _Phase, _AetheriaTargetArrowTime;
            v2f vert(appdata v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o;
            }
            // The spur faces away from the name. Its opposite end is a small jewel.
            float arrowDistance(float2 p)
            {
                float x = p.x;
                float y = abs(p.y);
                float sweep = smoothstep(-0.52, 0.53, x);
                float ridge = 0.60 * sweep;
                float blade = (abs(y - ridge) - (0.014 + 0.072 * (1.0 - sweep))) * 0.76;
                blade = max(blade, max(-0.55 - x, x - 0.54));
                float spur = max(abs(x + 0.51) * 0.32 + y - 0.094, -0.80 - x);
                float diamond = (abs(x - 0.37) / 0.23 + y / 0.155 - 1.0) * 0.13;
                return min(min(blade, spur), diamond);
            }
            half4 frag(v2f i):SV_Target
            {
                float2 p = i.uv * 2.0 - 1.0;
                p.x *= _Direction;
                float d = arrowDistance(p);
                float aa = max(fwidth(d) * 0.55, 0.006);
                float body = 1.0 - smoothstep(-aa, aa, d);
                float bevel = smoothstep(-0.014, 0.069, -d);
                float facet = saturate(0.69 + p.y * 0.48 - p.x * 0.18);
                float3 metal = lerp(_BorderColor.rgb, _HighlightColor.rgb, bevel * facet);
                float halo = _Glow * exp2(-max(d, 0.0) * 24.0) * (1.0 - body);
                float coverage = body + halo;
                float3 rgb = (metal * body + _EnergyColor.rgb * halo) / max(coverage, 0.0001);
                return half4(rgb, coverage * _Opacity);
            }
            ENDCG
        }
    }
    Fallback Off
}
