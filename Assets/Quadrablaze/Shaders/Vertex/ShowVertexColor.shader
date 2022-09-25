Shader "Vertex/ShowVertexColor"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Standard

		#include "UnityCG.cginc"

		struct Input {
			float4 vertexColor : COLOR;
			float3 viewDir;
		};
						
		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = IN.vertexColor;
			//o.Albedo = 0;
			o.Metallic = .5f;
			o.Smoothness = .2f;
		}

		ENDCG
	}
}
