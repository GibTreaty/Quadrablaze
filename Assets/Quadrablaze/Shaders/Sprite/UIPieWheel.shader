// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/UI Pie Wheel"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Slices("Slices", int) = 4

		_InnerRadius("Inner Radius", Range(0, 1)) = 0
		_InnerAlpha("Inner Alpha", Range(0, .5)) = .02
		_OuterAlpha("Outer Alpha", Range(0, .5)) = .23

		_SliceGap("Slice Gap", Range(0, .5)) = .01
		_GapSoftness("Gap Softness", float) = 1
		_GapSoftnessOffset("Gap Softness Offset", float) = 0

		_AngleOffset("Angle Offset", float) = 0

		_HighlightColor("Highlight Color", Color) = (1,0,0,1)
		_HighlightSlice("Highlight Slice", int) = 0

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					half2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;
				int _Slices;
				float _InnerRadius;
				float _SliceGap;
				float _GapSoftness;
				float _GapSoftnessOffset;
				float _InnerAlpha;
				float _OuterAlpha;
				float _HighlightSlice;
				fixed4 _HighlightColor;
				float _AngleOffset;

				v2f vert(appdata_t IN) {
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
	#ifdef UNITY_HALF_TEXEL_OFFSET
					OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1,1);
	#endif
					OUT.color = IN.color * _Color;
					return OUT;
				}

				sampler2D _MainTex;

				float2x2 CalculateRotationMatrix(float angle) {
					float sinX = sin(angle);
					float cosX = cos(angle);

					return float2x2(cosX, -sinX, sinX, cosX);
				}

				float GetGapAngle(half2 texcoord) {
					float pi = 3.14159;
					float piMul2 = 2 * pi;

					int sliceCount = floor(_Slices);
					float2 uv = (texcoord - .5) * 2;
					float gapAngle = atan2(uv.y, uv.x);
					float remapAngle = (pi + gapAngle) / piMul2;

					//gapAngle = floor(remapAngle * sliceCount) / (sliceCount - 1);
					gapAngle = floor(remapAngle * sliceCount) / sliceCount;
					//gapAngle = gapAngle * piMul2;
					gapAngle = (gapAngle * piMul2) - pi;
					//gapAngle = (gapAngle * piMul2) - (pi / 2);

					return gapAngle;
				}

				float GetHighlightValue(float2 direction) {
					float angleSize = float(360) / float(_Slices);
					float angle = degrees(atan2(direction.y, direction.x)) + (angleSize / 2) + 180;
					float angleBefore = angle + (angleSize / 2);

					angle += (angleSize / 2);

					float angleModStep = fmod(angle / angleSize, 1);
					float angleStep = floor(angle / angleSize);
					float angleDistance = 1 - angleModStep;

					int currentSlice = (fmod(angleBefore / 360, 1) * floor(_Slices)) + 1;
					return floor(_HighlightSlice) == currentSlice ? 1 : 0;
				}

				float2 RotatePosition(float2 position, float2x2 rotationMatrix) {
					position.xy = position.xy - .5;
					position.xy = mul(position.xy, rotationMatrix);
					position.xy = position.xy + .5;

					return position;
				}

				float GetLineMask(float angle, float2 pixelPosition) {
					float2x2 rotationMatrix = CalculateRotationMatrix(angle);

					pixelPosition = RotatePosition(pixelPosition, rotationMatrix);

					float middleY = abs(pixelPosition.y - .5) * 2;
					float lineMask = (1 - step(middleY, _SliceGap)) + step(pixelPosition.x, .5);
					middleY = (middleY * _GapSoftness) - _GapSoftnessOffset;
					//return clamp(middleY, 0, 1);
					lineMask *= middleY;

					return clamp(lineMask, 0, 1);
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;

					float alpha = color.a;
					float distance = (1 - clamp(length(abs(IN.texcoord - float2(.5, .5))), 0 , 1)) - .5;

					if (distance < .01)
						color.a = min(distance * 200, 1);
					else
						color.a = ceil(distance);

					//color.a = distance > (1 - _InnerRadius) * .5 ? 0 : color.a;

					//color.a *= ((1 - _InnerRadius) * .5) / distance;

					if (distance > (1 - _InnerRadius) * .5)
						color.a = 0;

					color.a = lerp(color.a, color.a * .5, step(_InnerAlpha, distance));
					color.a = lerp(color.a, color.a / .25, step(_OuterAlpha, distance));

					if (_Slices > 1) {
						float2 position = float2(1 - IN.texcoord.y, IN.texcoord.x);
						//float2 position = float2(IN.texcoord.x, IN.texcoord.y);

						float2 direction = position - float2(.5, .5);
						//float2 direction = 1 - position - float2(.5, .5);
						//float2 direction = 1 - IN.texcoord - float2(.5, .5);
						float highlightValue = GetHighlightValue(direction);

						color.rgb = lerp(color.rgb, _HighlightColor, highlightValue);

						float angleSize = float(360) / float(_Slices);


						//color.a = 1;

						float gapAngle = GetGapAngle(position);
						float lineMask = GetLineMask(gapAngle, position);
						color.a *= lineMask;

						float gapAngle2 = GetGapAngle(position) + radians(angleSize);
						float lineMask2 = GetLineMask(gapAngle2, position);
						color.a *= lineMask2;
					}

					color.a = clamp(color.a, 0, 1);
					color.a *= alpha;

					clip(color.a - 0.01);

					return color;
				}

				ENDCG
				}
		}
}
