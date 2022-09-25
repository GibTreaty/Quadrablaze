// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:1,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:33093,y:32427,varname:node_4795,prsc:2|emission-2393-OUT;n:type:ShaderForge.SFN_Multiply,id:2393,x:32895,y:32544,varname:node_2393,prsc:2|A-797-RGB,B-4395-OUT,C-797-A,D-2070-RGB,E-2070-A;n:type:ShaderForge.SFN_Color,id:797,x:32665,y:32470,ptovrint:True,ptlb:Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_TexCoord,id:5523,x:29748,y:32375,varname:node_5523,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRange,id:1120,x:29914,y:32375,varname:node_1120,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-5523-UVOUT;n:type:ShaderForge.SFN_Multiply,id:9044,x:30073,y:32375,varname:node_9044,prsc:2|A-1120-OUT,B-1120-OUT;n:type:ShaderForge.SFN_ComponentMask,id:5827,x:30239,y:32375,varname:node_5827,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-9044-OUT;n:type:ShaderForge.SFN_Add,id:5064,x:30427,y:32375,varname:node_5064,prsc:2|A-5827-R,B-5827-G;n:type:ShaderForge.SFN_OneMinus,id:725,x:30660,y:32357,varname:node_725,prsc:2|IN-5064-OUT;n:type:ShaderForge.SFN_Clamp01,id:3215,x:32298,y:32691,varname:node_3215,prsc:2|IN-529-OUT;n:type:ShaderForge.SFN_Power,id:3214,x:31567,y:32733,varname:node_3214,prsc:2|VAL-8008-OUT,EXP-4645-OUT;n:type:ShaderForge.SFN_Vector1,id:4645,x:31415,y:32793,varname:node_4645,prsc:2,v1:2;n:type:ShaderForge.SFN_Clamp01,id:3373,x:31736,y:32723,varname:node_3373,prsc:2|IN-3214-OUT;n:type:ShaderForge.SFN_OneMinus,id:5626,x:31921,y:32723,varname:node_5626,prsc:2|IN-3373-OUT;n:type:ShaderForge.SFN_Multiply,id:529,x:32108,y:32691,varname:node_529,prsc:2|A-5998-OUT,B-5626-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:8008,x:31275,y:32733,varname:node_8008,prsc:2|IN-1010-OUT,IMIN-7649-OUT,IMAX-3913-OUT,OMIN-6062-OUT,OMAX-8640-OUT;n:type:ShaderForge.SFN_Vector1,id:7649,x:31074,y:32767,varname:node_7649,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:6062,x:31074,y:32910,varname:node_6062,prsc:2,v1:-0.9;n:type:ShaderForge.SFN_Vector1,id:8640,x:31074,y:32970,varname:node_8640,prsc:2,v1:0.9;n:type:ShaderForge.SFN_Slider,id:3913,x:30917,y:32840,ptovrint:False,ptlb:Thickness,ptin:_Thickness,varname:_Thickness,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3,max:1;n:type:ShaderForge.SFN_Slider,id:5252,x:30917,y:32508,ptovrint:False,ptlb:Outside Thickness,ptin:_OutsideThickness,varname:_OutsideThickness,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1,max:1;n:type:ShaderForge.SFN_Vector1,id:3140,x:31074,y:32438,varname:node_3140,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:2735,x:31074,y:32581,varname:node_2735,prsc:2,v1:-1;n:type:ShaderForge.SFN_Vector1,id:4487,x:31074,y:32641,varname:node_4487,prsc:2,v1:1;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:5998,x:31293,y:32378,varname:node_5998,prsc:2|IN-3814-OUT,IMIN-3140-OUT,IMAX-5252-OUT,OMIN-2735-OUT,OMAX-4487-OUT;n:type:ShaderForge.SFN_Relay,id:3814,x:30917,y:32378,varname:node_3814,prsc:2|IN-725-OUT;n:type:ShaderForge.SFN_Relay,id:1010,x:30917,y:32688,varname:node_1010,prsc:2|IN-725-OUT;n:type:ShaderForge.SFN_RemapRange,id:597,x:30866,y:32581,varname:node_597,prsc:2,frmn:0,frmx:1,tomn:-0.1,tomx:0.1|IN-5064-OUT;n:type:ShaderForge.SFN_Add,id:1000,x:32517,y:32619,varname:node_1000,prsc:2|A-4816-OUT,B-3215-OUT;n:type:ShaderForge.SFN_Multiply,id:4816,x:31903,y:32149,varname:node_4816,prsc:2|A-9826-OUT,B-7677-OUT,C-6036-OUT;n:type:ShaderForge.SFN_Ceil,id:7677,x:31402,y:32208,varname:node_7677,prsc:2|IN-7373-OUT;n:type:ShaderForge.SFN_Relay,id:7867,x:31007,y:32231,varname:node_7867,prsc:2|IN-725-OUT;n:type:ShaderForge.SFN_Relay,id:9826,x:30987,y:32129,varname:node_9826,prsc:2|IN-5064-OUT;n:type:ShaderForge.SFN_Slider,id:6036,x:31540,y:32276,ptovrint:False,ptlb:Inner Fade,ptin:_InnerFade,varname:_InnerFade,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.2,max:1;n:type:ShaderForge.SFN_Subtract,id:7373,x:31196,y:32187,varname:node_7373,prsc:2|A-7867-OUT,B-4752-OUT;n:type:ShaderForge.SFN_Get,id:4752,x:31007,y:32298,varname:node_4752,prsc:2|IN-4915-OUT;n:type:ShaderForge.SFN_Set,id:4915,x:31285,y:32869,varname:InnerFadeAmount,prsc:2|IN-3913-OUT;n:type:ShaderForge.SFN_Clamp01,id:4395,x:32665,y:32619,varname:node_4395,prsc:2|IN-1000-OUT;n:type:ShaderForge.SFN_VertexColor,id:2070,x:32665,y:32759,varname:node_2070,prsc:2;proporder:797-5252-3913-6036;pass:END;sub:END;*/

Shader "Quadrablaze/AbilityGlow" {
    Properties {
        [HDR]_TintColor ("Color", Color) = (1,1,1,1)
        _OutsideThickness ("Outside Thickness", Range(0, 1)) = 0.1
        _Thickness ("Thickness", Range(0, 1)) = 0.3
        _InnerFade ("Inner Fade", Range(0, 1)) = 0.2
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
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
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform float4 _TintColor;
            uniform float _Thickness;
            uniform float _OutsideThickness;
            uniform float _InnerFade;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float2 node_1120 = (i.uv0*2.0+-1.0);
                float2 node_5827 = (node_1120*node_1120).rg;
                float node_5064 = (node_5827.r+node_5827.g);
                float node_725 = (1.0 - node_5064);
                float InnerFadeAmount = _Thickness;
                float node_3140 = 0.0;
                float node_2735 = (-1.0);
                float node_7649 = 0.0;
                float node_6062 = (-0.9);
                float3 emissive = (_TintColor.rgb*saturate(((node_5064*ceil((node_725-InnerFadeAmount))*_InnerFade)+saturate(((node_2735 + ( (node_725 - node_3140) * (1.0 - node_2735) ) / (_OutsideThickness - node_3140))*(1.0 - saturate(pow((node_6062 + ( (node_725 - node_7649) * (0.9 - node_6062) ) / (_Thickness - node_7649)),2.0)))))))*_TintColor.a*i.vertexColor.rgb*i.vertexColor.a);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
