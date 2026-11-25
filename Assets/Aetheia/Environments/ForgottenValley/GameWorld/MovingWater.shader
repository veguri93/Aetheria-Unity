Shader "Aetheia/Playable Valley/Moving water" {
Properties { _BaseColor("Deep water",Color)=(.05,.14,.12,1) }
SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
Pass { Cull Off
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
struct V{float4 p:POSITION;float2 uv:TEXCOORD0;};struct F{float4 p:SV_POSITION;float3 w:TEXCOORD0;float2 uv:TEXCOORD1;float fog:TEXCOORD2;};
F vert(V v){F o;o.w=TransformObjectToWorld(v.p.xyz);o.p=TransformWorldToHClip(o.w);o.uv=v.uv;o.fog=ComputeFogFactor(o.p.z);return o;}
float hash21(float2 q){return frac(sin(dot(q,float2(41.7,127.1)))*43758.54);}
float noise21(float2 p){float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash21(q),hash21(q+float2(1,0)),f.x),lerp(hash21(q+float2(0,1)),hash21(q+1),f.x),f.y);}
half4 frag(F i):SV_Target{float2 p=i.w.xz+float2(150,95);float t=_Time.y;float rx=sin(p.x*2.1+p.y*.44+t*1.5+sin(p.y*.8))*.06;
float rz=cos(p.y*3.4+p.x*.94+t*2.3+sin(p.x*.7))*.08;
rx+=.14*(noise21(p*2+float2(0,t*.4))-.5);rz+=.18*(noise21(p*float2(2,4)+t*.45)-.5);half3 n=normalize(half3(rx,1,rz));half3 eye=SafeNormalize(GetWorldSpaceViewDir(i.w));half fres=pow(1-saturate(dot(n,eye)),3);
Light sun=GetMainLight(TransformWorldToShadowCoord(i.w));half glint=pow(saturate(dot(n,normalize(eye+sun.direction))),180);
half3 c=lerp(half3(.026,.085,.073),half3(.22,.30,.29),.06+.52*fres)+glint*sun.color*.6;
c*=.62+.38*sun.shadowAttenuation;float falls=smoothstep(380,383,p.y)*(1-smoothstep(385,387,p.y));
float streak=noise21(float2(p.x*2.5+sin(i.w.y*.7)*.21,i.w.y*.58+t*2.8))*.55+noise21(float2(p.x*9,i.w.y*.8+t*3.6))*.45;
float foam=falls*(.11+.82*smoothstep(.22,.76,streak))+.38*exp(-pow((p.y-381)/1.2,2))*(.5+.5*sin(p.x*4+p.y*3+t*4));
return half4(MixFog(lerp(c,half3(.56,.65,.63),saturate(foam)),i.fog),1);}
ENDHLSL } } }