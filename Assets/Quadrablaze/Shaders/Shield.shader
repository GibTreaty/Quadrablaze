// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|emission-3399-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32265,y:32769,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_NormalVector,id:9472,x:31301,y:32867,prsc:2,pt:False;n:type:ShaderForge.SFN_Dot,id:3859,x:31474,y:32931,varname:node_3859,prsc:2,dt:1|A-9472-OUT,B-714-OUT;n:type:ShaderForge.SFN_ViewVector,id:714,x:31301,y:33032,varname:node_714,prsc:2;n:type:ShaderForge.SFN_OneMinus,id:6555,x:31642,y:32931,varname:node_6555,prsc:2|IN-3859-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3510,x:31909,y:32853,ptovrint:False,ptlb:Multiplier,ptin:_Multiplier,varname:node_3510,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:1601,x:32083,y:32874,varname:node_1601,prsc:2|A-3510-OUT,B-9846-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7820,x:32083,y:33021,ptovrint:False,ptlb:Adder,ptin:_Adder,varname:node_7820,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:2145,x:32265,y:32943,varname:node_2145,prsc:2|A-1601-OUT,B-7820-OUT;n:type:ShaderForge.SFN_Multiply,id:3399,x:32468,y:32847,varname:node_3399,prsc:2|A-7241-RGB,B-2145-OUT;n:type:ShaderForge.SFN_ViewVector,id:4048,x:31586,y:33102,varname:node_4048,prsc:2;n:type:ShaderForge.SFN_ComponentMask,id:9846,x:31851,y:33052,varname:node_9846,prsc:2,cc1:2,cc2:-1,cc3:-1,cc4:-1|IN-7739-OUT;n:type:ShaderForge.SFN_ViewReflectionVector,id:7739,x:31612,y:33263,varname:node_7739,prsc:2;proporder:7241-3510-7820;pass:END;sub:END;*/

Shader "Quadrablaze/Shield" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Multiplier ("Multiplier", Float ) = 1
        _Adder ("Adder", Float ) = 0
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
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _Multiplier;
            uniform float _Adder;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
////// Lighting:
////// Emissive:
                float3 emissive = (_Color.rgb*((_Multiplier*viewReflectDirection.b)+_Adder));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
