// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.34 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.34;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33087,y:32693,varname:node_4013,prsc:2|emission-9468-OUT,clip-1835-A;n:type:ShaderForge.SFN_Time,id:8285,x:30681,y:32944,varname:node_8285,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:1835,x:32531,y:32928,ptovrint:True,ptlb:Main,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:634fa1b7c1fd33c45a4f08ba33bb2228,ntxv:0,isnm:False|UVIN-3121-OUT;n:type:ShaderForge.SFN_Color,id:8286,x:32461,y:32661,ptovrint:False,ptlb:Tint Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:9468,x:32723,y:32818,varname:node_9468,prsc:2|A-8286-RGB,B-1835-RGB;n:type:ShaderForge.SFN_Append,id:3121,x:32320,y:32833,varname:node_3121,prsc:2|A-2812-U,B-7039-OUT;n:type:ShaderForge.SFN_TexCoord,id:2812,x:30681,y:32727,varname:node_2812,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:7039,x:32107,y:32843,varname:node_7039,prsc:2|A-1347-OUT,B-2812-V;n:type:ShaderForge.SFN_Multiply,id:3263,x:31162,y:32884,varname:node_3263,prsc:2|A-8285-T,B-4582-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4582,x:30975,y:32973,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:_Speed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:7673,x:30960,y:32627,varname:node_7673,prsc:2|A-8517-OUT,B-2812-U;n:type:ShaderForge.SFN_Sin,id:4813,x:31370,y:32700,varname:node_4813,prsc:2|IN-9057-OUT;n:type:ShaderForge.SFN_Add,id:9057,x:31162,y:32700,varname:node_9057,prsc:2|A-7673-OUT,B-3263-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8517,x:30740,y:32603,ptovrint:False,ptlb:Frequency,ptin:_Frequency,varname:_Frequency,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_ValueProperty,id:71,x:31234,y:32468,ptovrint:False,ptlb:Amplitude,ptin:_Amplitude,varname:_Amplitude,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:1347,x:31984,y:32705,varname:node_1347,prsc:2|A-7510-OUT,B-4813-OUT;n:type:ShaderForge.SFN_Multiply,id:7510,x:31763,y:32524,varname:node_7510,prsc:2|A-71-OUT,B-2700-OUT,C-1884-OUT;n:type:ShaderForge.SFN_Sin,id:1884,x:31362,y:33057,varname:node_1884,prsc:2|IN-3885-OUT;n:type:ShaderForge.SFN_Multiply,id:3885,x:31162,y:33109,varname:node_3885,prsc:2|A-8285-T,B-8729-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8729,x:30975,y:33122,ptovrint:False,ptlb:Amplitude Speed,ptin:_AmplitudeSpeed,varname:_AmplitudeSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:1711,x:31299,y:32593,ptovrint:False,ptlb:Amplitude Offset,ptin:_AmplitudeOffset,varname:_AmplitudeOffset,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:2700,x:31510,y:32536,varname:node_2700,prsc:2|A-1711-OUT,B-4813-OUT;proporder:8286-1835-4582-8517-71-8729-1711;pass:END;sub:END;*/

Shader "Shader Forge/Force Field" {
    Properties {
        [HDR]_TintColor ("Tint Color", Color) = (1,1,1,1)
        _MainTex ("Main", 2D) = "white" {}
        _Speed ("Speed", Float ) = 1
        _Frequency ("Frequency", Float ) = 2
        _Amplitude ("Amplitude", Float ) = 1
        _AmplitudeSpeed ("Amplitude Speed", Float ) = 1
        _AmplitudeOffset ("Amplitude Offset", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform float _Speed;
            uniform float _Frequency;
            uniform float _Amplitude;
            uniform float _AmplitudeSpeed;
            uniform float _AmplitudeOffset;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 node_8285 = _Time + _TimeEditor;
                float node_4813 = sin(((_Frequency*i.uv0.r)+(node_8285.g*_Speed)));
                float2 node_3121 = float2(i.uv0.r,(((_Amplitude*(_AmplitudeOffset+node_4813)*sin((node_8285.g*_AmplitudeSpeed)))*node_4813)+i.uv0.g));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_3121, _MainTex));
                clip(_MainTex_var.a - 0.5);
////// Lighting:
////// Emissive:
                float3 emissive = (_TintColor.rgb*_MainTex_var.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Speed;
            uniform float _Frequency;
            uniform float _Amplitude;
            uniform float _AmplitudeSpeed;
            uniform float _AmplitudeOffset;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 node_8285 = _Time + _TimeEditor;
                float node_4813 = sin(((_Frequency*i.uv0.r)+(node_8285.g*_Speed)));
                float2 node_3121 = float2(i.uv0.r,(((_Amplitude*(_AmplitudeOffset+node_4813)*sin((node_8285.g*_AmplitudeSpeed)))*node_4813)+i.uv0.g));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_3121, _MainTex));
                clip(_MainTex_var.a - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
