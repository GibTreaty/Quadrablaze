// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.34 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.34;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:4,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33683,y:32658,varname:node_4013,prsc:2|emission-9468-OUT;n:type:ShaderForge.SFN_Time,id:8285,x:30675,y:32916,varname:node_8285,prsc:2;n:type:ShaderForge.SFN_Color,id:8286,x:32411,y:32217,ptovrint:True,ptlb:Primary,ptin:_PrimaryColor,varname:_PrimaryColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:1,c3:0.934,c4:1;n:type:ShaderForge.SFN_Multiply,id:9468,x:33424,y:32698,varname:node_9468,prsc:2|A-2140-OUT,B-4410-OUT,C-7345-OUT,D-2087-OUT,E-8281-OUT;n:type:ShaderForge.SFN_Append,id:3121,x:32269,y:33066,varname:node_3121,prsc:2|A-9706-OUT,B-6028-OUT;n:type:ShaderForge.SFN_TexCoord,id:2812,x:30782,y:32753,varname:node_2812,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:7039,x:31599,y:32778,varname:node_7039,prsc:2|A-4813-OUT,B-2812-V,C-8285-TSL;n:type:ShaderForge.SFN_Multiply,id:3263,x:31156,y:32901,varname:node_3263,prsc:2|A-8285-T,B-4582-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4582,x:30969,y:32926,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:_Speed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.77;n:type:ShaderForge.SFN_Multiply,id:7673,x:31037,y:32600,varname:node_7673,prsc:2|A-8517-OUT,B-2812-U;n:type:ShaderForge.SFN_Sin,id:4813,x:31436,y:32600,varname:node_4813,prsc:2|IN-9057-OUT;n:type:ShaderForge.SFN_Add,id:9057,x:31240,y:32600,varname:node_9057,prsc:2|A-7673-OUT,B-3263-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8517,x:30848,y:32600,ptovrint:False,ptlb:Frequency,ptin:_Frequency,varname:_Frequency,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:69.1125;n:type:ShaderForge.SFN_ValueProperty,id:71,x:31599,y:32555,ptovrint:False,ptlb:Amplitude,ptin:_Amplitude,varname:_Amplitude,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Sin,id:1884,x:31334,y:33081,varname:node_1884,prsc:2|IN-3885-OUT;n:type:ShaderForge.SFN_Multiply,id:3885,x:31156,y:33081,varname:node_3885,prsc:2|A-8285-T,B-8729-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8729,x:30969,y:33101,ptovrint:False,ptlb:Amplitude Speed,ptin:_AmplitudeSpeed,varname:_AmplitudeSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Distance,id:9706,x:32058,y:33028,varname:node_9706,prsc:2|A-2812-U,B-1988-OUT;n:type:ShaderForge.SFN_RemapRange,id:8257,x:32434,y:33066,varname:node_8257,prsc:2,frmn:0,frmx:0.5,tomn:1,tomx:0|IN-3121-OUT;n:type:ShaderForge.SFN_Distance,id:6028,x:32058,y:33163,varname:node_6028,prsc:2|A-2812-V,B-1988-OUT;n:type:ShaderForge.SFN_Vector1,id:1988,x:31751,y:32925,varname:node_1988,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Frac,id:2666,x:32053,y:32778,varname:node_2666,prsc:2|IN-1514-OUT;n:type:ShaderForge.SFN_Distance,id:6915,x:32233,y:32778,varname:node_6915,prsc:2|A-2666-OUT,B-1988-OUT;n:type:ShaderForge.SFN_ComponentMask,id:9329,x:32269,y:33274,varname:node_9329,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-8257-OUT;n:type:ShaderForge.SFN_Multiply,id:3261,x:32461,y:33274,varname:node_3261,prsc:2|A-9329-R,B-9329-G;n:type:ShaderForge.SFN_Vector1,id:6852,x:32459,y:33411,varname:node_6852,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:8565,x:32459,y:33481,varname:node_8565,prsc:2,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:8147,x:32459,y:33567,ptovrint:False,ptlb:Edge Fade,ptin:_EdgeFade,varname:_EdgeFade,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:7016,x:32711,y:33274,varname:node_7016,prsc:2|IN-3261-OUT,IMIN-6852-OUT,IMAX-8147-OUT,OMIN-6852-OUT,OMAX-8565-OUT;n:type:ShaderForge.SFN_Clamp01,id:7345,x:32872,y:33274,varname:node_7345,prsc:2|IN-7016-OUT;n:type:ShaderForge.SFN_RemapRange,id:3919,x:32398,y:32778,varname:node_3919,prsc:2,frmn:0,frmx:0.5,tomn:0,tomx:1|IN-6915-OUT;n:type:ShaderForge.SFN_Power,id:9413,x:32632,y:32778,varname:node_9413,prsc:2|VAL-3919-OUT,EXP-4336-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4336,x:32447,y:32972,ptovrint:False,ptlb:Line Pinch,ptin:_LinePinch,varname:_LinePinch,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Color,id:6375,x:32411,y:32416,ptovrint:True,ptlb:Background,ptin:_BackgroundColor,varname:_BackgroundColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:2140,x:32738,y:32373,varname:node_2140,prsc:2|A-6375-RGB,B-8286-RGB,T-9413-OUT;n:type:ShaderForge.SFN_DepthBlend,id:4410,x:32504,y:32644,varname:node_4410,prsc:2|DIST-6011-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6011,x:32333,y:32644,ptovrint:False,ptlb:Blend,ptin:_Blend,varname:_Blend,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Multiply,id:1514,x:31812,y:32778,varname:node_1514,prsc:2|A-6637-OUT,B-7039-OUT;n:type:ShaderForge.SFN_Multiply,id:6637,x:31789,y:32555,varname:node_6637,prsc:2|A-71-OUT,B-3689-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3601,x:31265,y:33247,ptovrint:False,ptlb:Amplitude Offset,ptin:_AmplitudeOffset,varname:_AmplitudeOffset,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Add,id:6946,x:31517,y:33181,varname:node_6946,prsc:2|A-1884-OUT,B-3601-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:3689,x:31548,y:33028,varname:node_3689,prsc:2,min:-1,max:1|IN-6946-OUT;n:type:ShaderForge.SFN_InverseLerp,id:131,x:31364,y:32160,varname:node_131,prsc:2|A-601-OUT,B-1466-OUT,V-2812-V;n:type:ShaderForge.SFN_Vector1,id:601,x:30996,y:32139,varname:node_601,prsc:2,v1:1;n:type:ShaderForge.SFN_InverseLerp,id:8847,x:31364,y:32384,varname:node_8847,prsc:2|A-9422-OUT,B-7805-OUT,V-2812-V;n:type:ShaderForge.SFN_Vector1,id:9422,x:31198,y:32355,varname:node_9422,prsc:2,v1:0;n:type:ShaderForge.SFN_Multiply,id:2087,x:31657,y:32249,varname:node_2087,prsc:2|A-131-OUT,B-8847-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7805,x:30851,y:32241,ptovrint:False,ptlb:Height Fade,ptin:_HeightFade,varname:node_7805,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;n:type:ShaderForge.SFN_Subtract,id:1466,x:31165,y:32186,varname:node_1466,prsc:2|A-601-OUT,B-7805-OUT;n:type:ShaderForge.SFN_TexCoord,id:6978,x:32911,y:33023,varname:node_6978,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Slider,id:7155,x:32710,y:32950,ptovrint:False,ptlb:HorizontalFade,ptin:_HorizontalFade,varname:node_7155,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:1,max:1;n:type:ShaderForge.SFN_Add,id:3480,x:33137,y:32989,varname:node_3480,prsc:2|A-7155-OUT,B-6978-U;n:type:ShaderForge.SFN_Clamp01,id:8281,x:33313,y:32989,varname:node_8281,prsc:2|IN-3480-OUT;proporder:8286-6375-6011-4582-8517-71-3601-8729-8147-7805-4336-7155;pass:END;sub:END;*/

Shader "Quadrablaze/Force Field 2" {
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
        _HeightFade ("Height Fade", Float ) = 0.2
        _LinePinch ("Line Pinch", Float ) = 2
        _HorizontalFade ("HorizontalFade", Range(-1, 1)) = 1
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Overlay"
            "RenderType"="Transparent"
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
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
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
            uniform float _HeightFade;
            uniform float _HorizontalFade;
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
                float4 node_8285 = _Time + _TimeEditor;
                float node_1988 = 0.5;
                float2 node_9329 = (float2(distance(i.uv0.r,node_1988),distance(i.uv0.g,node_1988))*-2.0+1.0).rg;
                float node_6852 = 0.0;
                float node_601 = 1.0;
                float3 emissive = (lerp(_BackgroundColor.rgb,_PrimaryColor.rgb,pow((distance(frac(((_Amplitude*clamp((sin((node_8285.g*_AmplitudeSpeed))+_AmplitudeOffset),-1,1))*(sin(((_Frequency*i.uv0.r)+(node_8285.g*_Speed)))+i.uv0.g+node_8285.r))),node_1988)*2.0+0.0),_LinePinch))*saturate((sceneZ-partZ)/_Blend)*saturate((node_6852 + ( ((node_9329.r*node_9329.g) - node_6852) * (1.0 - node_6852) ) / (_EdgeFade - node_6852)))*(((i.uv0.g-node_601)/((node_601-_HeightFade)-node_601))*((i.uv0.g-0.0)/(_HeightFade-0.0)))*saturate((_HorizontalFade+i.uv0.r)));
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
