using UnityEngine;
using System.Collections;

namespace YounGenTech.EnergyBasedObjects {
	/// <summary>
	/// The base class that that you can extend to create scripts that grab and modify the LineRenderer returned in the event from OnWeaponRaycastSpawn.OnCreateLineRenderer.
	/// </summary>
	[RequireComponent(typeof(OnWeaponRaycastSpawn)), AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Effects/OnWeaponRaycastSpawn Effects/OnCreateLineRendererBase")]
	public class OnCreateLineRendererBase : MonoBehaviour {

		OnWeaponRaycastSpawn raycastSpawn;

		protected virtual void OnEnable() {
			if(!raycastSpawn)
				raycastSpawn = GetComponent<OnWeaponRaycastSpawn>();

			raycastSpawn.OnCreateLineRenderer += GetLineRenderer;
		}
		protected virtual void OnDisable() { raycastSpawn.OnCreateLineRenderer -= GetLineRenderer; }

		/// <summary>
		/// Override this function in your class
		/// </summary>
		protected virtual void GetLineRenderer(LineRenderer lineRenderer) { }
	}
}