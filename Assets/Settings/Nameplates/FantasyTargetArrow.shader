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
            float arrowDistance(float2 p)
            {
                // Broad, gently notched arrowhead with rounded lips and a narrow nose.
                // This is a filled plate, rather than a font stroke or solid triangle.
                float2 v[8];
                v[0]=float2(-.58,.64); v[1]=float2(-.30,.60);
                v[2]=float2(.64,.075); v[3]=float2(.70,0);
                v[4]=float2(.64,-.075); v[5]=float2(-.30,-.60);
                v[6]=float2(-.58,-.64); v[7]=float2(-.08,0);
                float d = dot(p-v[0],p-v[0]); float s=1;
                [unroll] for(int i=0;i<8;i++)
                {
                    int j=i==0?7:i-1;
                    float2 e=v[j]-v[i], w=p-v[i];
                    float2 b=w-e*saturate(dot(w,e)/dot(e,e));
                    d=min(d,dot(b,b));
                    bool3 c=bool3(p.y>=v[i].y,p.y<v[j].y,e.x*w.y>e.y*w.x);
                    if(all(c)||all(!c)) s=-s;
                }
                return s*sqrt(d)-_Roundness;
            }
            half4 frag(v2f i):SV_Target
            {
                float2 p=i.uv*2-1; p.x*=_Direction;
                float d=arrowDistance(p);
                float aa=max(fwidth(d),.004);
                float body=1-smoothstep(-aa,aa,d);
                float inner=1-smoothstep(-_BorderWidth-aa,-_BorderWidth+aa,d);
                // Stable neutral metal: no time-dependent color or opacity here.
                float light=saturate(.58+p.y*.38-p.x*.14);
                float lip=exp2(-1900*(d+.025)*(d+.025));
                float3 metal=lerp(_BorderColor.rgb*.72,_HighlightColor.rgb,light*.72+lip*.22);
                float t=_AetheriaTargetArrowTime*_Speed+_Phase;
                float bend=sin(p.y*7+t*1.4)*.35;
                float flow=pow(.5+.5*sin(p.x*12-p.y*4-t*5+bend),5);
                float filament=exp2(-100*(p.y-.13*sin(p.x*7-t*2))*(p.y-.13*sin(p.x*7-t*2)));
                float breath=1+_Breathing*sin(t*2.0);
                float energy=(.46+_Intensity*(.22+.24*flow+.13*filament))*breath;
                float3 interior=_EnergyColor.rgb*energy;
                // Fine, pale highlights read as energy under a polished surface.
                interior+=_EnergyColor.rgb*flow*.10;
                float3 rgb=lerp(metal,interior,inner);
                float halo=(1-body)*exp2(-max(d,0)*34)*_Glow*(.78+.08*sin(t*2));
                float coverage=saturate(body+halo);
                rgb=(rgb*body+_EnergyColor.rgb*halo)/max(coverage,.0001);
                return half4(rgb,coverage*_Opacity);
            }
            ENDCG
        }
    }
    Fallback Off
}
