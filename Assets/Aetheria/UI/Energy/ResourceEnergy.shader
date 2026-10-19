Shader "Aetheria/UI/Resource Energy"
{
 Properties
 {
  [PerRendererData] _MainTex ("Original bar artwork", 2D) = "white" {}
  _Color ("Tint", Color) = (1,1,1,1)
  [HDR] _EnergyColor ("Energy color", Color) = (1,.12,.08,1)
  _Speed ("Flow speed", Range(0,0.6)) = .16
  _Brightness ("Energy brightness", Range(0,2)) = .75
  _BarBrightness ("Overall bar brightness", Range(0.2,1.2)) = 1
  _Opacity ("Energy opacity", Range(0,1)) = .25
  [Enum(Crimson,0,Arcane,1,Golden,2)] _Style ("Flow character", Float) = 0
  _Phase ("Phase offset", Float) = 0
  _StencilComp ("Stencil Comparison", Float) = 8
  _Stencil ("Stencil ID", Float) = 0
  _StencilOp ("Stencil Operation", Float) = 0
  _StencilWriteMask ("Stencil Write Mask", Float) = 255
  _StencilReadMask ("Stencil Read Mask", Float) = 255
  _ColorMask ("Color Mask", Float) = 15
  [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
 }
 SubShader
 {
  Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
  Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask [_ColorMask]
  Pass
  {
   Name "ResourceEnergy"
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   struct appdata { float4 vertex:POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct v2f { float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 local:TEXCOORD1;half4 mask:TEXCOORD2;UNITY_VERTEX_OUTPUT_STEREO };
   sampler2D _MainTex;
   fixed4 _Color,_TextureSampleAdd;
   float4 _ClipRect,_MainTex_ST;
   float _UIMaskSoftnessX,_UIMaskSoftnessY;
   float4 _EnergyColor;
   float _Speed,_Brightness,_BarBrightness,_Opacity,_Style,_Phase,_AetheriaUIEnergyTime;
   v2f vert(appdata v)
   {
    v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;
    float2 pixelSize=o.vertex.w;pixelSize/=abs(mul((float2x2)UNITY_MATRIX_P,_ScreenParams.xy));
    float4 rect=clamp(_ClipRect,-2e10,2e10);
    o.mask=half4(v.vertex.xy*2-rect.xy-rect.zw,.25/(.25*half2(_UIMaskSoftnessX,_UIMaskSoftnessY)+abs(pixelSize.xy)));
    return o;
   }
   fixed4 frag(v2f i):SV_Target
   {
    // Slider Filled geometry already clips both artwork and energy to current value.
    half4 col=(tex2D(_MainTex,i.uv)+_TextureSampleAdd)*i.color;
    float t=_AetheriaUIEnergyTime*_Speed+_Phase;
    float x=i.uv.x,y=i.uv.y;
    float arcane=saturate(1-abs(_Style-1));float golden=saturate(_Style-1);
    float bend=lerp(.12,.035,golden)+arcane*.045;
    float centre=.5+bend*sin(x*11-t*3.1);
    float streak=exp2(-145*(y-centre)*(y-centre));
    float flow=.32+.68*pow(.5+.5*sin(x*19-t*6.283),4);
    float secondCentre=.28+bend*.7*sin(x*16-t*2.4+1.8);
    float secondary=exp2(-230*(y-secondCentre)*(y-secondCentre))*(.5+.5*sin(x*27-t*7.1));
    float shimmer=pow(.5+.5*sin(x*6.283-t*1.7),14)*exp2(-13*(y-.55)*(y-.55));
    float edge=smoothstep(0,.16,y)*smoothstep(0,.16,1-y);
    float sweep=exp2(-70*pow(frac(x-t*.42)-.5,2));
    float strength=saturate(_Brightness*_Opacity);
    float energy=saturate((streak*flow*.68+secondary*.3+shimmer*.25+sweep*.52)*edge*_Brightness*_Opacity)*_EnergyColor.a;
    float movingShade=(.5+.5*sin(x*13-t*6.283))*.20*strength*edge;
    col.rgb*=1-movingShade;
    // Screen-like light softly mixes over the original pixels; original alpha stays intact.
    col.rgb=lerp(col.rgb,saturate(col.rgb+_EnergyColor.rgb*.65),energy);
    col.rgb *= _BarBrightness;
    #ifdef UNITY_UI_CLIP_RECT
     half2 mask=saturate((_ClipRect.zw-_ClipRect.xy-abs(i.mask.xy))*i.mask.zw);col.a*=mask.x*mask.y;
    #endif
    #ifdef UNITY_UI_ALPHACLIP
     clip(col.a-.001);
    #endif
    return col;
   }
   ENDCG
  }
 }
}

