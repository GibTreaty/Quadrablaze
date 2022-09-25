using UnityEngine;
using System.Collections;

namespace YounGenTech.EnergyBasedObjects {
	/// <summary>
	/// Add light to a LineRenderer that was created with <see cref="OnWeaponRaycastSpawn"/>
	/// </summary>
	[AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Effects/OnWeaponRaycastSpawn Effects/Add Light OnCreateLineRenderer")]
	public class AddLightOnCreateLineRenderer : OnCreateLineRendererBase {
		//I tried to keep these variables looking as close to the default Light inspector as I could
		//Some variables were not included, such as "Draw Halo" because it doesn't exist as a variable in the Light class

		public LightType type = LightType.Point;
		public float range = 10;
		public Color color = Color.white;
		[Range(0, 8)]
		public float intensity = 1;
		public float spotAngle = 30;
		public float cookieSize = 10;
		public Texture cookie;

		public LightShadows shadowType;
		public float shadowStrength = 1;
		public float shadowBias = .05f;
		public float shadowSoftness = 4;
		public float shadowSoftnessFade = 1;

		public Flare flare;
		public LightRenderMode renderMode;
		public LayerMask cullingMask = -1;

		/// <summary>
		/// Destroy the Light once the LineRenderer has reached the end set by MoveLineRenderer
		/// </summary>
		public bool destroyLightAtEnd = true;

		protected override void GetLineRenderer(LineRenderer lineRenderer) {
			Light light = lineRenderer.gameObject.AddComponent<Light>();

			light.type = type;
			light.range = range;
			light.color = color;
			light.intensity = intensity;
			light.spotAngle = spotAngle;
			light.cookieSize = cookieSize;
			light.cookie = cookie;

			light.shadows = shadowType;
			light.shadowStrength = shadowStrength;
			light.shadowBias = shadowBias;

			light.flare = flare;
			light.renderMode = renderMode;
			light.cullingMask = cullingMask;

			//Creates a delegate that
			//checks if the light exists and
			//destroys the light
			if(destroyLightAtEnd)
				lineRenderer.GetComponent<MoveLineRenderer>().OnLineReachEnd += () => {
					if(light) Destroy(light);
				};
		}
	}
}