// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:1,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:4,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33164,y:32693,varname:node_4013,prsc:2|emission-9468-OUT;n:type:ShaderForge.SFN_Time,id:8285,x:30512,y:32731,varname:node_8285,prsc:2;n:type:ShaderForge.SFN_Color,id:8286,x:32228,y:32398,ptovrint:True,ptlb:Primary,ptin:_PrimaryColor,varname:_PrimaryColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:1,c3:0.934,c4:1;n:type:ShaderForge.SFN_Multiply,id:9468,x:32959,y:32768,varname:node_9468,prsc:2|A-2140-OUT,B-4410-OUT,C-7345-OUT;n:type:ShaderForge.SFN_Append,id:3121,x:31594,y:33149,varname:node_3121,prsc:2|A-9706-OUT,B-6028-OUT;n:type:ShaderForge.SFN_TexCoord,id:2812,x:30512,y:32578,varname:node_2812,prsc:2,uv:0;n:type:ShaderForge.SFN_Add,id:7039,x:31251,y:32661,varname:node_7039,prsc:2|A-4813-OUT,B-2832-TSL;n:type:ShaderForge.SFN_Multiply,id:3263,x:30689,y:32802,varname:node_3263,prsc:2|A-8285-T,B-4582-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4582,x:30512,y:32893,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:_Speed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.77;n:type:ShaderForge.SFN_Multiply,id:7673,x:30689,y:32561,varname:node_7673,prsc:2|A-8517-OUT,B-2812-V;n:type:ShaderForge.SFN_Sin,id:4813,x:31058,y:32678,varname:node_4813,prsc:2|IN-9057-OUT;n:type:ShaderForge.SFN_Add,id:9057,x:30871,y:32678,varname:node_9057,prsc:2|A-7673-OUT,B-3263-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8517,x:30512,y:32514,ptovrint:False,ptlb:Frequency,ptin:_Frequency,varname:_Frequency,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:69.1125;n:type:ShaderForge.SFN_ValueProperty,id:71,x:31085,y:32525,ptovrint:False,ptlb:Amplitude,ptin:_Amplitude,varname:_Amplitude,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Sin,id:1884,x:30713,y:32194,varname:node_1884,prsc:2|IN-3885-OUT;n:type:ShaderForge.SFN_Multiply,id:3885,x:30545,y:32181,varname:node_3885,prsc:2|A-2644-T,B-8729-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8729,x:30355,y:32264,ptovrint:False,ptlb:Amplitude Speed,ptin:_AmplitudeSpeed,varname:_AmplitudeSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Distance,id:9706,x:31383,y:33149,varname:node_9706,prsc:2|A-6339-V,B-1988-OUT;n:type:ShaderForge.SFN_RemapRange,id:8257,x:31759,y:33149,varname:node_8257,prsc:2,frmn:0,frmx:0.5,tomn:1,tomx:0|IN-3121-OUT;n:type:ShaderForge.SFN_Distance,id:6028,x:31383,y:33284,varname:node_6028,prsc:2|A-2671-V,B-9125-OUT;n:type:ShaderForge.SFN_Vector1,id:1988,x:31161,y:33246,varname:node_1988,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Frac,id:2666,x:31667,y:32670,varname:node_2666,prsc:2|IN-1514-OUT;n:type:ShaderForge.SFN_Distance,id:6915,x:31839,y:32764,varname:node_6915,prsc:2|A-2666-OUT,B-1950-OUT;n:type:ShaderForge.SFN_ComponentMask,id:9329,x:31934,y:33149,varname:node_9329,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-8257-OUT;n:type:ShaderForge.SFN_Multiply,id:3261,x:32126,y:33149,varname:node_3261,prsc:2|A-9329-R,B-9329-G;n:type:ShaderForge.SFN_Vector1,id:6852,x:32124,y:33276,varname:node_6852,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:8565,x:32124,y:33412,varname:node_8565,prsc:2,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:8147,x:32124,y:33357,ptovrint:False,ptlb:Edge Fade,ptin:_EdgeFade,varname:_EdgeFade,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:7016,x:32376,y:33149,varname:node_7016,prsc:2|IN-3261-OUT,IMIN-6852-OUT,IMAX-8147-OUT,OMIN-6852-OUT,OMAX-8565-OUT;n:type:ShaderForge.SFN_Clamp01,id:7345,x:32553,y:33149,varname:node_7345,prsc:2|IN-7016-OUT;n:type:ShaderForge.SFN_RemapRange,id:3919,x:32004,y:32764,varname:node_3919,prsc:2,frmn:0,frmx:0.5,tomn:0,tomx:1|IN-6915-OUT;n:type:ShaderForge.SFN_Power,id:9413,x:32228,y:32751,varname:node_9413,prsc:2|VAL-3919-OUT,EXP-4336-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4336,x:32004,y:32959,ptovrint:False,ptlb:Line Pinch,ptin:_LinePinch,varname:_LinePinch,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Color,id:6375,x:32228,y:32573,ptovrint:True,ptlb:Background,ptin:_BackgroundColor,varname:_BackgroundColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:2140,x:32555,y:32554,varname:node_2140,prsc:2|A-6375-RGB,B-8286-RGB,T-9413-OUT;n:type:ShaderForge.SFN_DepthBlend,id:4410,x:32724,y:32788,varname:node_4410,prsc:2|DIST-6011-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6011,x:32555,y:32788,ptovrint:False,ptlb:Blend,ptin:_Blend,varname:_Blend,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Multiply,id:1514,x:31473,y:32670,varname:node_1514,prsc:2|A-6637-OUT,B-7039-OUT;n:type:ShaderForge.SFN_Multiply,id:6637,x:31251,y:32525,varname:node_6637,prsc:2|A-3689-OUT,B-71-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3601,x:30713,y:32336,ptovrint:False,ptlb:Amplitude Offset,ptin:_AmplitudeOffset,varname:_AmplitudeOffset,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Add,id:6946,x:30907,y:32286,varname:node_6946,prsc:2|A-1884-OUT,B-3601-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:3689,x:31067,y:32286,varname:node_3689,prsc:2,min:-1,max:1|IN-6946-OUT;n:type:ShaderForge.SFN_Time,id:2832,x:31058,y:32822,varname:node_2832,prsc:2;n:type:ShaderForge.SFN_Time,id:2644,x:30355,y:32108,varname:node_2644,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:6339,x:31161,y:33101,varname:node_6339,prsc:2,uv:0;n:type:ShaderForge.SFN_TexCoord,id:2671,x:31161,y:33311,varname:node_2671,prsc:2,uv:0;n:type:ShaderForge.SFN_Vector1,id:9125,x:31161,y:33446,varname:node_9125,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Vector1,id:1950,x:31667,y:32799,varname:node_1950,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Vector1,id:2561,x:32282,y:33055,varname:node_2561,prsc:2,v1:1;proporder:8286-6375-6011-4582-8517-71-3601-8729-8147-4336;pass:END;sub:END;*/

Shader "Quadrablaze/Shield Drainer" {
    Properties {
        [HDR]_PrimaryColor ("Primary", Color) = (0.5,1,0.934,1)
        [HDR]_BackgroundColor ("Background", Color) = (0.5,0.5,0.5,1)
        _Blend ("Blend", Float ) = 2
        _Speed ("Speed", Float ) = 0.77
        _Frequency ("Frequency", Float ) = 69.1125
        _Amplitude ("Amplitude", Float ) = 2
        _AmplitudeOffset ("Amplitude Offset", Float ) = 1
        _AmplitudeSpeed ("Amplitude Speed", Float ) = 0
        _EdgeFade ("Edge Fade", Float ) = 0
        _LinePinch ("Line Pinch", Float ) = 2
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Overlay"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _CameraDepthTexture;
            uniform float4 _TimeEditor;
            uniform float4 _PrimaryColor;
            uniform float _Speed;
            uniform float _Frequency;
            uniform float _Amplitude;
            uniform float _AmplitudeSpeed;
            uniform float _EdgeFade;
            uniform float _LinePinch;
            uniform float4 _BackgroundColor;
            uniform float _Blend;
            uniform float _AmplitudeOffset;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 projPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
////// Lighting:
////// Emissive:
                float4 node_2644 = _Time + _TimeEditor;
                float4 node_8285 = _Time + _TimeEditor;
                float4 node_2832 = _Time + _TimeEditor;
                float2 node_9329 = (float2(distance(i.uv0.g,0.5),distance(i.uv0.g,0.5))*-2.0+1.0).rg;
                float node_6852 = 0.0;
                float3 emissive = (lerp(_BackgroundColor.rgb,_PrimaryColor.rgb,pow((distance(frac(((clamp((sin((node_2644.g*_AmplitudeSpeed))+_AmplitudeOffset),-1,1)*_Amplitude)*(sin(((_Frequency*i.uv0.g)+(node_8285.g*_Speed)))+node_2832.r))),0.5)*2.0+0.0),_LinePinch))*saturate((sceneZ-partZ)/_Blend)*saturate((node_6852 + ( ((node_9329.r*node_9329.g) - node_6852) * (1.0 - node_6852) ) / (_EdgeFade - node_6852))));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
