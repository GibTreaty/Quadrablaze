// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Instanced/Vertex Color Map Instanced" {
	Properties{
		_PrimaryColor("PrimaryColor", Color) = (0.7529412,0.7529412,0.7529412,1)
		_SecondaryColor("SecondaryColor", Color) = (0.1529412,0.09803922,0.01960784,1)
		_AccessoryPrimaryColor("AccessoryPrimaryColor", Color) = (0,0,0)
		_AccessorySecondaryColor("AccessorySecondaryColor", Color) = (0,0,0)
		_Gloss("Gloss", Range(0,1)) = 0.8
		_Metallic("Metallic", Range(0,1)) = 0.0
		[HDR]_GlowColor("GlowColor", Color) = (0,1,0.04827595,1)
		_RimColor("Rim Color", Color) = (1,1,1,0.0)
		_RimPower("Rim Power", Range(0.5,8.0)) = 5.0
	}

	SubShader{
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM

		#pragma surface surf Standard
		//#pragma target 3.0
		#pragma multi_compile_instancing

		sampler2D _MainTex;

		struct Input {
			float4 vertexColor : COLOR;
			float3 viewDir;
		};

		half _Gloss;
		half _Metallic;
		float4 _RimColor;
		float _RimPower;

		// Declare instanced properties inside a cbuffer.
		// Each instanced property is an array of by default 500(D3D)/128(GL) elements. Since D3D and GL imposes a certain limitation
		// of 64KB and 16KB respectively on the size of a cubffer, the default array size thus allows two matrix arrays in one cbuffer.
		// Use maxcount option on #pragma instancing_options directive to specify array size other than default (divided by 4 when used
		// for GL).
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _PrimaryColor)
//#define _PrimaryColor_arr Props
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _SecondaryColor)
//#define _SecondaryColor_arr Props
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _AccessoryPrimaryColor)
//#define _AccessoryPrimaryColor_arr Props
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _AccessorySecondaryColor)
//#define _AccessorySecondaryColor_arr Props
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _GlowColor)
//#define _GlowColor_arr Props
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o) {
			//float3 primaryColor = lerp(UNITY_ACCESS_INSTANCED_PROP(_PrimaryColor_arr, _PrimaryColor).rgb, UNITY_ACCESS_INSTANCED_PROP(_AccessoryPrimaryColor_arr, _AccessoryPrimaryColor).rgb, IN.vertexColor.b);
			//float3 secondaryColor = lerp(UNITY_ACCESS_INSTANCED_PROP(_SecondaryColor_arr, _SecondaryColor).rgb, UNITY_ACCESS_INSTANCED_PROP(_AccessorySecondaryColor_arr, _AccessorySecondaryColor).rgb, IN.vertexColor.b);

			float3 primaryColor = lerp(UNITY_ACCESS_INSTANCED_PROP(Props, _PrimaryColor).rgb, UNITY_ACCESS_INSTANCED_PROP(Props, _AccessoryPrimaryColor).rgb, IN.vertexColor.b);
			float3 secondaryColor = lerp(UNITY_ACCESS_INSTANCED_PROP(Props, _SecondaryColor).rgb, UNITY_ACCESS_INSTANCED_PROP(Props, _AccessorySecondaryColor).rgb, IN.vertexColor.b);

			float3 diffuseColor = (1 - IN.vertexColor.g) * lerp(secondaryColor, primaryColor, IN.vertexColor.r);

			fixed4 c = fixed4(diffuseColor.r, diffuseColor.g, diffuseColor.b, 1);
			float3 emissive = IN.vertexColor.g * UNITY_ACCESS_INSTANCED_PROP(Props, _GlowColor).rgb;
			//float3 emissive = IN.vertexColor.g * UNITY_ACCESS_INSTANCED_PROP(_GlowColor_arr, _GlowColor).rgb;

			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Gloss;

			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Emission = emissive + _RimColor * pow(rim, _RimPower);
		}

		ENDCG
	}

	FallBack "Diffuse"
}
