Shader "Particles/Vertex Color Particle" {
	Properties{
		_TintColor("Main Color", Color) = (1,1,1,1)
		_Gloss("Gloss", Range(0,1)) = 0.8
		_Metallic("Metallic", Range(0,1)) = 0.0
		_GlowPower("Glow Power", Range(0,10)) = 0
		_RimColor("Rim Color", Color) = (1,1,1,0.0)
		_RimPower("Rim Power", Range(0.5,8.0)) = 5.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM

			#pragma surface surf Standard
			//#pragma target 3.0

			sampler2D _MainTex;

			struct Input {
				float4 vertexColor : COLOR;
				float3 viewDir;
			};

			half _Gloss;
			half _Metallic;
			float4 _RimColor;
			float _RimPower;
			float _GlowPower;

			fixed4 _TintColor;

			void surf(Input IN, inout SurfaceOutputStandard o) {
				float3 emissive = IN.vertexColor.rgb * _GlowPower;
				o.Albedo = IN.vertexColor.rgb * _TintColor.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Gloss;

				half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
				o.Emission = emissive + _RimColor * pow(rim, _RimPower);
			}
			ENDCG
	}
		FallBack "Diffuse"
}
