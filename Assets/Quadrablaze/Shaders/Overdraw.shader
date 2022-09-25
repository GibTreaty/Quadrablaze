Shader "Overdraw" {
	Properties{
		_MainTex("Base", 2D) = "white" {}
		_Color("Main Color", Color) = (0.15,0.0,0.0,0.0)
	}

	SubShader{
		//Tags { "RenderType" = "Transparent" }
		Fog { Mode Off }
		ZWrite Off
		ZTest Always
		Blend One One // additive blending

		CGPROGRAM
		#pragma surface surf Standard

		sampler _MainTex;
		float4 _Color;
		
		struct Input {
			float4 vertexColor : COLOR;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = _Color.rgb;
		}
		
		ENDCG
		/*Pass {
			SetTexture[_MainTex] {
				constantColor [_Color]
				combine constant, texture
			}
		}*/
	}
}
