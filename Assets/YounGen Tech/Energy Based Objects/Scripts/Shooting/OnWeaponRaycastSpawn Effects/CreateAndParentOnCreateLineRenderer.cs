using UnityEngine;
using System.Collections;

namespace YounGenTech.EnergyBasedObjects {
	/// <summary>
	/// Create(optional) a GameObject and parent it to a LineRenderer that was created with <see cref="OnWeaponRaycastSpawn"/>
	/// </summary>
	[AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Effects/OnWeaponRaycastSpawn Effects/Create And Parent OnCreateLineRenderer")]
	public class CreateAndParentOnCreateLineRenderer : OnCreateLineRendererBase {

		/// <summary>
		/// GameObject that will be parented to the LineRenderer GameObject
		/// </summary>
		public GameObject gameObjectToParent;

		/// <summary>
		/// Should gameObjectToParent be instantiated before parenting? This should be true if it is a prefab.
		/// </summary>
		public bool instantiate = true;

		protected override void GetLineRenderer(LineRenderer lineRenderer) {
			if(!gameObjectToParent) return;

			if(instantiate)
				gameObjectToParent = Instantiate(gameObjectToParent) as GameObject;

			gameObjectToParent.transform.parent = lineRenderer.transform;
			gameObjectToParent.transform.localPosition = Vector3.zero;
			gameObjectToParent.transform.localRotation = Quaternion.identity;
		}
	}
}