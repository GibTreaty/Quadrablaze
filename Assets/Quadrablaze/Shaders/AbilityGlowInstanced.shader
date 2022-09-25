// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Quadrablaze/Ability Glow Instanced" {
	Properties{
		[HDR]_TintColor("Color", Color) = (1,1,1,1)
		_OutsideThickness("Outside Thickness", Range(0, 1)) = 0.1
		_Thickness("Thickness", Range(0, 1)) = 0.3
		_InnerFade("Inner Fade", Range(0, 1)) = 0.2
	}
		SubShader{
			Tags {
				"IgnoreProjector" = "True"
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
			}
			Pass {
				Name "FORWARD"
				Blend One One
				ZWrite Off

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#pragma multi_compile_instancing
		//uniform float4 _TintColor;
		//uniform float _Thickness;
		//uniform float _OutsideThickness;
		//uniform float _InnerFade;

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _TintColor)
#define _TintColor_arr Props
			UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
#define _Thickness_arr Props
			UNITY_DEFINE_INSTANCED_PROP(float, _OutsideThickness)
#define _OutsideThickness_arr Props
			UNITY_DEFINE_INSTANCED_PROP(float, _InnerFade)
#define _InnerFade_arr Props
		UNITY_INSTANCING_BUFFER_END(Props)

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
		VertexOutput vert(VertexInput v) {
			VertexOutput o = (VertexOutput)0;
			o.uv0 = v.texcoord0;
			o.vertexColor = v.vertexColor;
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;
		}
		float4 frag(VertexOutput i) : COLOR {
			float2 node_1120 = (i.uv0*2.0 + -1.0);
			float2 node_5827 = (node_1120*node_1120).rg;
			float node_5064 = (node_5827.r + node_5827.g);
			float node_725 = (1.0 - node_5064);
			float InnerFadeAmount = UNITY_ACCESS_INSTANCED_PROP(_Thickness_arr, _Thickness);
			float3 emissive = (UNITY_ACCESS_INSTANCED_PROP(_TintColor_arr, _TintColor).rgb*saturate(((node_5064*ceil((node_725 - InnerFadeAmount))*UNITY_ACCESS_INSTANCED_PROP(_InnerFade_arr, _InnerFade)) + saturate(((-1.0 + (node_725 * 2) / UNITY_ACCESS_INSTANCED_PROP(_OutsideThickness_arr, _OutsideThickness))*(1.0 - saturate(pow((-0.9 + (node_725 * 1.8) / UNITY_ACCESS_INSTANCED_PROP(_Thickness_arr, _Thickness)),2.0)))))))*UNITY_ACCESS_INSTANCED_PROP(_TintColor_arr, _TintColor).a*i.vertexColor.rgb*i.vertexColor.a);
			fixed4 finalRGBA = fixed4(emissive,1);

			return finalRGBA;
		}
		ENDCG
	}
	}
}
