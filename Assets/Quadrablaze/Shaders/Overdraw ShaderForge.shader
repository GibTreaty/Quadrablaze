// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:14,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:32836,y:32595,varname:node_4795,prsc:2|emission-6797-OUT;n:type:ShaderForge.SFN_Multiply,id:2393,x:32181,y:32591,varname:node_2393,prsc:2|A-4696-OUT,B-1857-OUT,C-4880-RGB;n:type:ShaderForge.SFN_Color,id:4880,x:31985,y:32831,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.1372549,c2:0.3019608,c3:0.08235294,c4:1;n:type:ShaderForge.SFN_Rotator,id:1572,x:30298,y:32715,varname:_rotate,prsc:2|UVIN-8378-OUT,ANG-1997-OUT;n:type:ShaderForge.SFN_Frac,id:4166,x:30935,y:32654,varname:_lineFrac,prsc:2|IN-7128-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7389,x:29838,y:32618,ptovrint:False,ptlb:Lines,ptin:_Lines,varname:_Lines,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:17;n:type:ShaderForge.SFN_Multiply,id:8378,x:30103,y:32579,varname:node_8378,prsc:2|A-7248-UVOUT,B-7389-OUT;n:type:ShaderForge.SFN_OneMinus,id:9969,x:31174,y:32690,varname:node_9969,prsc:2|IN-4166-OUT;n:type:ShaderForge.SFN_ScreenPos,id:7248,x:29831,y:32450,varname:node_7248,prsc:2,sctp:2;n:type:ShaderForge.SFN_RemapRange,id:7579,x:30489,y:32718,varname:node_7579,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-1572-UVOUT;n:type:ShaderForge.SFN_Slider,id:2858,x:29781,y:32765,ptovrint:False,ptlb:Angle,ptin:_Angle,varname:_Angle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:313.8462,max:360;n:type:ShaderForge.SFN_ComponentMask,id:6205,x:31397,y:32706,varname:node_6205,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-9969-OUT;n:type:ShaderForge.SFN_Vector1,id:5314,x:29865,y:32911,cmnt:Rad2Deg,varname:node_5314,prsc:2,v1:0.01745329;n:type:ShaderForge.SFN_Multiply,id:1997,x:30108,y:32808,varname:node_1997,prsc:2|A-2858-OUT,B-5314-OUT;n:type:ShaderForge.SFN_Add,id:7128,x:30724,y:32651,varname:node_7128,prsc:2|A-8960-OUT,B-7579-OUT;n:type:ShaderForge.SFN_Clamp01,id:1857,x:31986,y:32621,varname:node_1857,prsc:2|IN-1534-OUT;n:type:ShaderForge.SFN_Add,id:1534,x:31772,y:32716,varname:node_1534,prsc:2|A-6205-OUT,B-559-OUT;n:type:ShaderForge.SFN_Vector1,id:559,x:31564,y:32757,varname:node_559,prsc:2,v1:-0.5;n:type:ShaderForge.SFN_Add,id:1908,x:32413,y:32631,varname:node_1908,prsc:2|A-2393-OUT,B-4880-RGB;n:type:ShaderForge.SFN_Clamp01,id:6797,x:32592,y:32628,varname:node_6797,prsc:2|IN-1908-OUT;n:type:ShaderForge.SFN_Noise,id:8708,x:31770,y:32476,varname:_lineNoise,prsc:2|XY-5422-OUT;n:type:ShaderForge.SFN_Multiply,id:4696,x:31966,y:32416,varname:node_4696,prsc:2|A-8592-OUT,B-8708-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8592,x:31767,y:32373,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:_Noise,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:77,x:31360,y:32409,ptovrint:False,ptlb:NoiseScale,ptin:_NoiseScale,varname:_NoiseScale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:5422,x:31539,y:32468,varname:_lineMultiply,prsc:2|A-77-OUT,B-4166-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8960,x:30488,y:32591,ptovrint:False,ptlb:LineTime,ptin:_LineTime,varname:_LineTime,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:4880-7389-2858-8592-77-8960;pass:END;sub:END;*/

Shader "Quadrablaze/Overdraw" {
    Properties {
        _Color ("Color", Color) = (0.1372549,0.3019608,0.08235294,1)
        _Lines ("Lines", Float ) = 17
        _Angle ("Angle", Range(0, 360)) = 313.8462
        _Noise ("Noise", Float ) = 0
        _NoiseScale ("NoiseScale", Float ) = 1
        _LineTime ("LineTime", Float ) = 1
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
            Cull Off
            ZWrite Off
            ColorMask RGB
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _Lines;
            uniform float _Angle;
            uniform float _Noise;
            uniform float _NoiseScale;
            uniform float _LineTime;
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float2 sceneUVs = (i.projPos.xy / i.projPos.w);
////// Lighting:
////// Emissive:
                float _rotate_ang = (_Angle*0.01745329);
                float _rotate_spd = 1.0;
                float _rotate_cos = cos(_rotate_spd*_rotate_ang);
                float _rotate_sin = sin(_rotate_spd*_rotate_ang);
                float2 _rotate_piv = float2(0.5,0.5);
                float2 _rotate = (mul((sceneUVs.rg*_Lines)-_rotate_piv,float2x2( _rotate_cos, -_rotate_sin, _rotate_sin, _rotate_cos))+_rotate_piv);
                float2 _lineFrac = frac((_LineTime+(_rotate*0.5+0.5)));
                float2 _lineMultiply = (_NoiseScale*_lineFrac);
                float2 _lineNoise_skew = _lineMultiply + 0.2127+_lineMultiply.x*0.3713*_lineMultiply.y;
                float2 _lineNoise_rnd = 4.789*sin(489.123*(_lineNoise_skew));
                float _lineNoise = frac(_lineNoise_rnd.x*_lineNoise_rnd.y*(1+_lineNoise_skew.x));
                float3 emissive = saturate((((_Noise*_lineNoise)*saturate(((1.0 - _lineFrac).r+(-0.5)))*_Color.rgb)+_Color.rgb));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
