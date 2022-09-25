// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:1,cusa:True,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:True,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:1873,x:33229,y:32719,varname:node_1873,prsc:2|emission-1749-OUT,alpha-603-OUT;n:type:ShaderForge.SFN_Tex2d,id:4805,x:32551,y:32729,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:True,tagnsco:False,tagnrm:False,tex:b34fa5638ae41e84fb9e52d6e6d462af,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:1086,x:32812,y:32818,cmnt:RGB,varname:node_1086,prsc:2|A-4805-RGB,B-5983-RGB,C-5376-RGB;n:type:ShaderForge.SFN_Color,id:5983,x:32551,y:32915,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color_copy,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_VertexColor,id:5376,x:32551,y:33079,varname:node_5376,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1749,x:33025,y:32818,cmnt:Premultiply Alpha,varname:node_1749,prsc:2|A-1086-OUT,B-603-OUT;n:type:ShaderForge.SFN_Multiply,id:603,x:32812,y:32992,cmnt:A,varname:node_603,prsc:2|A-4805-A,B-5983-A,C-5376-A,D-2502-OUT;n:type:ShaderForge.SFN_Slider,id:5881,x:31441,y:33341,ptovrint:False,ptlb:Fade,ptin:_Fade,varname:node_5881,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_TexCoord,id:425,x:31134,y:33245,varname:node_425,prsc:2,uv:0;n:type:ShaderForge.SFN_OneMinus,id:3011,x:31982,y:33324,varname:node_3011,prsc:2|IN-4602-OUT;n:type:ShaderForge.SFN_Subtract,id:3683,x:32241,y:33226,varname:node_3683,prsc:2|A-9874-OUT,B-3011-OUT;n:type:ShaderForge.SFN_Multiply,id:4602,x:31772,y:33324,varname:node_4602,prsc:2|A-5881-OUT,B-2527-OUT,C-7656-OUT;n:type:ShaderForge.SFN_Vector1,id:2527,x:31598,y:33424,varname:node_2527,prsc:2,v1:2;n:type:ShaderForge.SFN_ToggleProperty,id:1653,x:31734,y:32959,ptovrint:False,ptlb:Invert Fade,ptin:_InvertFade,varname:node_1653,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False;n:type:ShaderForge.SFN_Clamp01,id:2502,x:32599,y:33220,varname:node_2502,prsc:2|IN-8775-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7656,x:31598,y:33518,ptovrint:False,ptlb:Max Fade,ptin:_MaxFade,varname:node_7656,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.4;n:type:ShaderForge.SFN_If,id:9874,x:31982,y:33112,varname:node_9874,prsc:2|A-1653-OUT,B-9435-OUT,GT-9656-OUT,EQ-425-V,LT-425-V;n:type:ShaderForge.SFN_Vector1,id:9435,x:31734,y:33014,varname:node_9435,prsc:2,v1:0.5;n:type:ShaderForge.SFN_OneMinus,id:9656,x:31734,y:33086,varname:node_9656,prsc:2|IN-425-V;n:type:ShaderForge.SFN_OneMinus,id:221,x:31492,y:33639,varname:node_221,prsc:2|IN-425-U;n:type:ShaderForge.SFN_Multiply,id:8572,x:31728,y:33606,varname:node_8572,prsc:2|A-425-U,B-221-OUT,C-248-OUT;n:type:ShaderForge.SFN_OneMinus,id:248,x:31492,y:33779,varname:node_248,prsc:2|IN-425-V;n:type:ShaderForge.SFN_Multiply,id:9115,x:32172,y:33710,varname:node_9115,prsc:2|A-8572-OUT,B-7126-OUT;n:type:ShaderForge.SFN_Subtract,id:8775,x:32419,y:33313,varname:node_8775,prsc:2|A-3683-OUT,B-9115-OUT;n:type:ShaderForge.SFN_Slider,id:7126,x:31819,y:33785,ptovrint:False,ptlb:A Thing,ptin:_AThing,varname:node_7126,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:100;proporder:4805-5983-5881-7656-1653-7126;pass:END;sub:END;*/

Shader "YounGen Tech/Recoil Bar" {
    Properties {
        [PerRendererData]_MainTex ("MainTex", 2D) = "white" {}
        [HDR]_Color ("Color", Color) = (1,1,1,1)
        _Fade ("Fade", Range(0, 1)) = 0
        _MaxFade ("Max Fade", Float ) = 0.4
        [MaterialToggle] _InvertFade ("Invert Fade", Float ) = 0
        _AThing ("A Thing", Range(0, 100)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _Color;
            uniform float _Fade;
            uniform fixed _InvertFade;
            uniform float _MaxFade;
            uniform float _AThing;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                #ifdef PIXELSNAP_ON
                    o.pos = UnityPixelSnap(o.pos);
                #endif
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_9874_if_leA = step(_InvertFade,0.5);
                float node_9874_if_leB = step(0.5,_InvertFade);
                float node_603 = (_MainTex_var.a*_Color.a*i.vertexColor.a*saturate(((lerp((node_9874_if_leA*i.uv0.g)+(node_9874_if_leB*(1.0 - i.uv0.g)),i.uv0.g,node_9874_if_leA*node_9874_if_leB)-(1.0 - (_Fade*2.0*_MaxFade)))-((i.uv0.r*(1.0 - i.uv0.r)*(1.0 - i.uv0.g))*_AThing)))); // A
                float3 emissive = ((_MainTex_var.rgb*_Color.rgb*i.vertexColor.rgb)*node_603);
                float3 finalColor = emissive;
                return fixed4(finalColor,node_603);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
