Shader "Aetheia/Botanical Groundcover"
{
 Properties
 {
    _BaseMap("Color and coverage",2D)="white"{}
    _NormalMap("Bark normal",2D)="bump"{}
    _RoughMap("Bark roughness",2D)="white"{}
    _Cutoff("Alpha cutoff",Range(0,1))=.32
    _WindResponse("Ground vegetation wind response",Range(0,2))=1
    [Toggle(_LEAVES)]_Leaves("Foliage",Float)=0
    [Enum(UnityEngine.Rendering.CullMode)]_Cull("Culling",Float)=2
 }
 SubShader
 {
 Tags {"RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout"}
 Cull Off
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
 TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
 TEXTURE2D(_RoughMap); SAMPLER(sampler_RoughMap);
 CBUFFER_START(UnityPerMaterial)
 float _Cutoff,_Leaves,_Cull,_WindResponse;
 CBUFFER_END
 float4 _AetheiaWind,_AetheiaGust;
 float3 _LightDirection,_LightPosition;
 struct A {float4 p:POSITION; float3 n:NORMAL; float4 tangent:TANGENT; float2 uv:TEXCOORD0; float4 mask:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 pos:SV_POSITION; float3 world:TEXCOORD0; half3 n:TEXCOORD1; float2 uv:TEXCOORD2; half4 tangent:TEXCOORD3; half fog:TEXCOORD4;};
 V PlantVertex(A v)
 {
    UNITY_SETUP_INSTANCE_ID(v);
    V o; float3 p=TransformObjectToWorld(v.p.xyz);
    float3 root=TransformObjectToWorld(float3(0,0,0));
    float height=max(0,p.y-root.y);
    float anchored=1.0;
    float time=_Time.y*_AetheiaWind.w;
    float phase=dot(root.xz,float2(.131,.083));
    float travel=dot(p.xz,_AetheiaWind.xy)*.057;
    float gust=pow(saturate(.50+.31*sin(.31*time-travel)+.19*sin(.17*time-.57*travel)),3);
    float wave=.58*sin(.89*time-travel+.35*sin(phase))+.42*sin(1.42*time-.55*travel+.58*phase);
    float bend=(v.mask.r*.026)*_AetheiaGust.y;
    float breeze=_WindResponse*anchored*_AetheiaWind.z*(1+gust*_AetheiaGust.x);
    float flutter=sin(4.2*time+v.mask.b*12+dot(p,float3(.74,1.06,.42)))*v.mask.g*.006*_AetheiaGust.z;
    p.xz+=breeze*(_AetheiaWind.xy*wave*bend+float2(-_AetheiaWind.y,_AetheiaWind.x)*flutter);
    p.y+=breeze*flutter*.48;
    o.world=p;o.pos=TransformWorldToHClip(p);o.n=TransformObjectToWorldNormal(v.n);
    o.tangent=half4(TransformObjectToWorldDir(v.tangent.xyz),v.tangent.w*GetOddNegativeScale());
    o.uv=v.uv;o.fog=ComputeFogFactor(o.pos.z);return o;
 }
 half4 SamplePlant(float2 uv) {half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv);
 clip(c.a-_Cutoff);
 return c;}
 half3 PlantNormal(V i,half signFace) {
    half3 n=normalize(i.n);
    half3 t=normalize(i.tangent.xyz);half3 b=cross(n,t)*i.tangent.w;
    half3 tn=UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.uv),.58h);
    return normalize(signFace*(t*tn.x+b*tn.y+n*tn.z)+half3(0,.45,0));
 }
 ENDHLSL
 Pass {
 Name "Forward" Tags {"LightMode"="UniversalForwardOnly"}
 HLSLPROGRAM
 #pragma target 3.5
 #pragma vertex PlantVertex
 #pragma fragment Forward
 #pragma multi_compile_instancing
 #pragma shader_feature_local _LEAVES
 #pragma multi_compile_fog
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 half4 Forward(V i, FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target {
    half3 albedo=SamplePlant(i.uv).rgb;half3 n=PlantNormal(i,IS_FRONT_VFACE(face,1,-1));
    float4 shadow=TransformWorldToShadowCoord(i.world);
 #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
    shadow=ComputeScreenPos(TransformWorldToHClip(i.world));
 #endif
    Light sun=GetMainLight(shadow);
    half nl=dot(n,sun.direction);half3 ambient=max(SampleSH(n),half3(.06,.065,.06));
    half3 view=SafeNormalize(GetWorldSpaceViewDir(i.world));
    half rough=SAMPLE_TEXTURE2D(_RoughMap,sampler_RoughMap,i.uv).r;
    half tint=sin(i.world.x*.57+i.world.z*.39)*.045;
    albedo*=half3(1+tint,1+tint*.35,1-tint);
    half forwardScatter=.16*saturate(-nl)*(.55+.45*saturate(dot(-sun.direction,view)));
    half spec=.075*(1-rough)*pow(saturate(dot(n,SafeNormalize(view+sun.direction))),lerp(65.h,7.h,rough));
    half3 lit=albedo*ambient+sun.color*(albedo*(.18+.82*saturate(nl)+forwardScatter)+spec)*lerp(.12h,1.h,sun.shadowAttenuation);
    return half4(MixFog(lit,i.fog),1);
 }
 ENDHLSL
 }
 Pass {
 Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ColorMask 0
 HLSLPROGRAM
 #pragma vertex Shadow
 #pragma fragment Depth
 #pragma multi_compile_instancing
 #pragma shader_feature_local _LEAVES
 #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
 V Shadow(A v){V o=PlantVertex(v);float3 d=_LightDirection;
 #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
 d=normalize(_LightPosition-o.world);
 #endif
 o.pos=ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(o.world,o.n,d)));return o;}
 half4 Depth(V i):SV_Target{SamplePlant(i.uv);return 0;}
 ENDHLSL
 }
 Pass {
 Name "DepthOnly" Tags {"LightMode"="DepthOnly"} ZWrite On ColorMask R
 HLSLPROGRAM
 #pragma vertex PlantVertex
 #pragma fragment Depth
 #pragma multi_compile_instancing
 #pragma shader_feature_local _LEAVES
 half4 Depth(V i):SV_Target{SamplePlant(i.uv);return i.pos.z;}
 ENDHLSL
 }
 Pass {
 Name "DepthNormals" Tags {"LightMode"="DepthNormalsOnly"}
 HLSLPROGRAM
 #pragma vertex PlantVertex
 #pragma fragment Normals
 #pragma multi_compile_instancing
 #pragma shader_feature_local _LEAVES
 #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
 half4 Normals(V i, FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target{SamplePlant(i.uv);half3 n=PlantNormal(i,IS_FRONT_VFACE(face,1,-1));
 #if defined(_GBUFFER_NORMALS_OCT)
 return half4(PackFloat2To888(saturate(PackNormalOctQuadEncode(n)*.5+.5)),0);
 #else
 return half4(n,0);
 #endif
 }
 ENDHLSL
 }
 }
}


