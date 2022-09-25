// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:True,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:2865,x:32687,y:32575,varname:node_2865,prsc:2|emission-173-OUT;n:type:ShaderForge.SFN_Color,id:6665,x:31357,y:32573,ptovrint:False,ptlb:PrimaryColor HDR,ptin:_PrimaryColorHDR,varname:_PrimaryColorHDR,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.7529412,c2:0.7529412,c3:0.7529412,c4:1;n:type:ShaderForge.SFN_VertexColor,id:5044,x:30921,y:32718,varname:node_5044,prsc:2;n:type:ShaderForge.SFN_Lerp,id:266,x:31661,y:32589,varname:node_266,prsc:2|A-459-RGB,B-6665-RGB,T-5044-R;n:type:ShaderForge.SFN_Color,id:459,x:31357,y:32734,ptovrint:False,ptlb:SecondaryColor,ptin:_SecondaryColor,varname:_SecondaryColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.1529412,c2:0.09803922,c3:0.01960784,c4:1;n:type:ShaderForge.SFN_Color,id:4299,x:31634,y:33065,ptovrint:False,ptlb:GlowColor,ptin:_GlowColor,varname:_GlowColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:1,c3:0.04827595,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:9029,x:31430,y:32924,ptovrint:False,ptlb:Glow,ptin:_Glow,varname:_Glow,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1;n:type:ShaderForge.SFN_Power,id:5569,x:31634,y:32906,varname:node_5569,prsc:2|VAL-5044-G,EXP-9029-OUT;n:type:ShaderForge.SFN_Multiply,id:6940,x:31825,y:32995,varname:node_6940,prsc:2|A-5569-OUT,B-4299-RGB;n:type:ShaderForge.SFN_FragmentPosition,id:3731,x:30557,y:33525,varname:node_3731,prsc:2;n:type:ShaderForge.SFN_Subtract,id:1226,x:30742,y:33590,varname:node_1226,prsc:2|A-3731-XYZ,B-405-XYZ;n:type:ShaderForge.SFN_Color,id:952,x:31474,y:33214,ptovrint:False,ptlb:Hit Color,ptin:_HitColor,varname:node_952,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:0;n:type:ShaderForge.SFN_Vector4Property,id:3350,x:30738,y:33075,ptovrint:False,ptlb:Hit Direction,ptin:_HitDirection,varname:node_3350,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Dot,id:5750,x:30923,y:33075,varname:node_5750,prsc:2,dt:1|A-3350-XYZ,B-1226-OUT;n:type:ShaderForge.SFN_Multiply,id:6493,x:31693,y:33359,varname:node_6493,prsc:2|A-952-RGB,B-7379-OUT,C-4672-OUT;n:type:ShaderForge.SFN_Vector1,id:8647,x:31276,y:33751,varname:node_8647,prsc:2,v1:0;n:type:ShaderForge.SFN_Length,id:8778,x:31078,y:33576,varname:node_8778,prsc:2|IN-1226-OUT;n:type:ShaderForge.SFN_Subtract,id:8078,x:31276,y:33621,varname:node_8078,prsc:2|A-8778-OUT,B-3557-OUT;n:type:ShaderForge.SFN_Max,id:4672,x:31449,y:33643,varname:node_4672,prsc:2|A-8078-OUT,B-8647-OUT;n:type:ShaderForge.SFN_Add,id:6055,x:32013,y:33042,varname:node_6055,prsc:2|A-6940-OUT,B-6493-OUT;n:type:ShaderForge.SFN_Vector4Property,id:405,x:30557,y:33681,ptovrint:False,ptlb:Root Position,ptin:_RootPosition,varname:node_405,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Vector4Property,id:5316,x:30742,y:33420,ptovrint:False,ptlb:HitLeftDirection,ptin:_HitLeftDirection,varname:node_5316,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-0.77,v2:0,v3:1,v4:0;n:type:ShaderForge.SFN_Vector4Property,id:6964,x:30742,y:33253,ptovrint:False,ptlb:HitRightDirection,ptin:_HitRightDirection,varname:_HitLeftDirection_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.77,v2:0,v3:1,v4:0;n:type:ShaderForge.SFN_Dot,id:2650,x:30923,y:33255,varname:node_2650,prsc:2,dt:1|A-6964-XYZ,B-1226-OUT;n:type:ShaderForge.SFN_Dot,id:7186,x:30923,y:33420,varname:node_7186,prsc:2,dt:1|A-5316-XYZ,B-1226-OUT;n:type:ShaderForge.SFN_Multiply,id:7379,x:31137,y:33235,varname:node_7379,prsc:2|A-5750-OUT,B-2650-OUT,C-7186-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3557,x:31078,y:33712,ptovrint:False,ptlb:Hole,ptin:_Hole,varname:node_3557,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_NormalVector,id:8765,x:31577,y:32407,prsc:2,pt:False;n:type:ShaderForge.SFN_ViewVector,id:345,x:31577,y:32258,varname:node_345,prsc:2;n:type:ShaderForge.SFN_Cross,id:5716,x:31752,y:32307,varname:node_5716,prsc:2|A-345-OUT,B-8765-OUT;n:type:ShaderForge.SFN_Length,id:5520,x:31915,y:32326,varname:node_5520,prsc:2|IN-5716-OUT;n:type:ShaderForge.SFN_OneMinus,id:6011,x:32104,y:32315,varname:node_6011,prsc:2|IN-5520-OUT;n:type:ShaderForge.SFN_Multiply,id:1961,x:32186,y:32642,varname:node_1961,prsc:2|A-6011-OUT,B-266-OUT;n:type:ShaderForge.SFN_Add,id:173,x:32466,y:32762,varname:node_173,prsc:2|A-1961-OUT,B-6055-OUT;proporder:6665-459-4299-9029-952-3557-3350-405-5316-6964;pass:END;sub:END;*/

Shader "Unlit/Vertex Color Map with Glow" {
    Properties {
        _PrimaryColorHDR ("PrimaryColor HDR", Color) = (0.7529412,0.7529412,0.7529412,1)
        _SecondaryColor ("SecondaryColor", Color) = (0.1529412,0.09803922,0.01960784,1)
        [HDR]_GlowColor ("GlowColor", Color) = (0,1,0.04827595,1)
        _Glow ("Glow", Float ) = 0.1
        [HDR]_HitColor ("Hit Color", Color) = (0,0,0,0)
        _Hole ("Hole", Float ) = 0.5
        _HitDirection ("Hit Direction", Vector) = (0,0,0,0)
        _RootPosition ("Root Position", Vector) = (0,0,0,0)
        _HitLeftDirection ("HitLeftDirection", Vector) = (-0.77,0,1,0)
        _HitRightDirection ("HitRightDirection", Vector) = (0.77,0,1,0)
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _PrimaryColorHDR;
            uniform float4 _SecondaryColor;
            uniform float4 _GlowColor;
            uniform float _Glow;
            uniform float4 _HitColor;
            uniform float4 _HitDirection;
            uniform float4 _RootPosition;
            uniform float4 _HitLeftDirection;
            uniform float4 _HitRightDirection;
            uniform float _Hole;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
////// Lighting:
////// Emissive:
                float3 node_266 = lerp(_SecondaryColor.rgb,_PrimaryColorHDR.rgb,i.vertexColor.r);
                float3 node_1961 = ((1.0 - length(cross(viewDirection,i.normalDir)))*node_266);
                float3 node_1226 = (i.posWorld.rgb-_RootPosition.rgb);
                float3 node_6055 = ((pow(i.vertexColor.g,_Glow)*_GlowColor.rgb)+(_HitColor.rgb*(max(0,dot(_HitDirection.rgb,node_1226))*max(0,dot(_HitRightDirection.rgb,node_1226))*max(0,dot(_HitLeftDirection.rgb,node_1226)))*max((length(node_1226)-_Hole),0.0)));
                float3 emissive = (node_1961+node_6055);
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
