Shader "Aetheia/Playable Valley/Ground" {
Properties { _BaseMap("Habitat mask",2D)="white"{} _Meadow("Woodland",2D)="white"{} _Dirt("Gravel",2D)="white"{} }
SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
Pass { Tags {"LightMode"="UniversalForward"}
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile_fragment _ _SHADOWS_SOFT
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
struct V {float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
struct F {float4 clip:SV_POSITION;float3 pos:TEXCOORD0;float3 n:TEXCOORD1;half fog:TEXCOORD2;};
F vert(V v){F o;o.pos=TransformObjectToWorld(v.vertex.xyz);o.clip=TransformWorldToHClip(o.pos);o.n=TransformObjectToWorldNormal(v.normal);o.fog=ComputeFogFactor(o.clip.z);return o;}
sampler2D _BaseMap,_Meadow,_Dirt;
half4 frag(F i):SV_Target {float2 p=i.pos.xz+float2(150,95);float3 mask=tex2D(_BaseMap,p/500).rgb;
half3 moss=tex2D(_Meadow,p/5.8).rgb*half3(.65,.66,.53);
half3 dirt=tex2D(_Dirt,p/3.3).rgb*half3(.79,.68,.55);
half3 rock=tex2D(_Dirt,(p+i.pos.y*.49)/7).rgb*half3(.72,.75,.77);
half3 col=lerp(lerp(moss,dirt,mask.r),rock,mask.g)*(.79+.31*mask.b);
half3 n=normalize(i.n);Light l=GetMainLight(TransformWorldToShadowCoord(i.pos));
return half4(MixFog(col*(SampleSH(n)+l.color*saturate(dot(n,l.direction))*l.shadowAttenuation),i.fog),1);}
ENDHLSL }
UsePass "Universal Render Pipeline/Lit/ShadowCaster"
UsePass "Universal Render Pipeline/Lit/DepthOnly"
} }