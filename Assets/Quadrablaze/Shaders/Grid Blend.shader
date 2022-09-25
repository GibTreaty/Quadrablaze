// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:True,qofs:0,qpre:3,rntp:4,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:33558,y:32578,varname:node_4795,prsc:2|emission-1739-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:32323,y:32350,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ac6298635e0d19648a91dc2b665dc0fd,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Add,id:1739,x:32913,y:32471,varname:node_1739,prsc:2|A-6729-RGB,B-9688-OUT;n:type:ShaderForge.SFN_SceneColor,id:6729,x:32524,y:32304,varname:node_6729,prsc:2;n:type:ShaderForge.SFN_Multiply,id:9688,x:32683,y:32541,varname:node_9688,prsc:2|A-6729-RGB,B-6074-A,C-8951-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8951,x:32418,y:32569,ptovrint:False,ptlb:Add,ptin:_Add,varname:_Add,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Time,id:3679,x:29048,y:32475,varname:node_3679,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:9810,x:30478,y:32557,varname:node_9810,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_ComponentMask,id:1142,x:31126,y:32572,varname:node_1142,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-1301-OUT;n:type:ShaderForge.SFN_RemapRange,id:3168,x:30683,y:32561,varname:node_3168,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-9810-UVOUT;n:type:ShaderForge.SFN_Multiply,id:1301,x:30956,y:32576,varname:node_1301,prsc:2|A-3168-OUT,B-3168-OUT;n:type:ShaderForge.SFN_Add,id:6451,x:31305,y:32581,varname:node_6451,prsc:2|A-1142-R,B-1142-G,C-399-OUT;n:type:ShaderForge.SFN_OneMinus,id:4394,x:31483,y:32582,varname:node_4394,prsc:2|IN-6451-OUT;n:type:ShaderForge.SFN_Multiply,id:4267,x:31665,y:32710,varname:node_4267,prsc:2|A-4394-OUT,B-1973-OUT;n:type:ShaderForge.SFN_Slider,id:1973,x:31330,y:32782,ptovrint:False,ptlb:Transparency,ptin:_Transparency,varname:_Transparency,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_ValueProperty,id:8859,x:32395,y:32996,ptovrint:False,ptlb:TransparencyScale,ptin:_TransparencyScale,varname:_TransparencyScale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:9988,x:32586,y:32821,varname:node_9988,prsc:2|A-3043-OUT,B-8859-OUT;n:type:ShaderForge.SFN_Add,id:2893,x:31623,y:32949,varname:node_2893,prsc:2|A-4267-OUT,B-6270-OUT;n:type:ShaderForge.SFN_Subtract,id:7358,x:32159,y:32781,varname:node_7358,prsc:2|A-4267-OUT,B-2896-OUT;n:type:ShaderForge.SFN_Clamp01,id:2896,x:31993,y:32945,varname:node_2896,prsc:2|IN-6462-OUT;n:type:ShaderForge.SFN_Slider,id:6270,x:31240,y:33020,ptovrint:False,ptlb:node_6270,ptin:_node_6270,varname:_node_6270,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-2,cur:0,max:2;n:type:ShaderForge.SFN_Clamp01,id:3043,x:32366,y:32784,varname:node_3043,prsc:2|IN-7358-OUT;n:type:ShaderForge.SFN_Multiply,id:6462,x:31885,y:33110,varname:node_6462,prsc:2|A-2893-OUT,B-5911-OUT;n:type:ShaderForge.SFN_Slider,id:5911,x:31495,y:33231,ptovrint:False,ptlb:node_5911,ptin:_node_5911,varname:_node_5911,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_ValueProperty,id:9270,x:29068,y:32683,ptovrint:False,ptlb:Glow Speed,ptin:_GlowSpeed,varname:_GlowSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:5;n:type:ShaderForge.SFN_Divide,id:8375,x:29300,y:32556,varname:node_8375,prsc:2|A-3679-T,B-9270-OUT;n:type:ShaderForge.SFN_Sin,id:9553,x:30067,y:32757,varname:node_9553,prsc:2|IN-9121-OUT;n:type:ShaderForge.SFN_RemapRange,id:8628,x:30388,y:33102,varname:node_8628,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-9553-OUT;n:type:ShaderForge.SFN_Add,id:399,x:30489,y:32820,varname:node_399,prsc:2|A-9553-OUT,B-1388-OUT;n:type:ShaderForge.SFN_Vector1,id:1388,x:30298,y:32960,varname:node_1388,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:9121,x:29860,y:32905,varname:node_9121,prsc:2|A-1548-OUT,B-1491-OUT;n:type:ShaderForge.SFN_Vector1,id:1491,x:29334,y:32908,varname:node_1491,prsc:2,v1:360;n:type:ShaderForge.SFN_Divide,id:6047,x:29482,y:32684,varname:node_6047,prsc:2|A-8375-OUT,B-1491-OUT;n:type:ShaderForge.SFN_Frac,id:1548,x:29635,y:32729,varname:node_1548,prsc:2|IN-6047-OUT;proporder:6074-8951-1973-8859-6270-5911-9270;pass:END;sub:END;*/

Shader "Quadrablaze/Grid Blend" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _Add ("Add", Float ) = 1
        _Transparency ("Transparency", Range(0, 1)) = 1
        _TransparencyScale ("TransparencyScale", Float ) = 0
        _node_6270 ("node_6270", Range(-2, 2)) = 0
        _node_5911 ("node_5911", Range(0, 2)) = 0
        _GlowSpeed ("Glow Speed", Float ) = 5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Background"
        }
        GrabPass{ "Refraction" }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D Refraction;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Add;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 projPos : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float2 sceneUVs = (i.projPos.xy / i.projPos.w);
                float4 sceneColor = tex2D(Refraction, sceneUVs);
////// Lighting:
////// Emissive:
                float4 node_6729 = sceneColor;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = (node_6729.rgb+(node_6729.rgb*_MainTex_var.a*_Add));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
