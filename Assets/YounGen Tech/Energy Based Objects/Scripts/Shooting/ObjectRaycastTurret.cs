using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YounGenTech.ComponentInterface;

namespace YounGenTech.EnergyBasedObjects {
	/// <summary>
	/// Fires multiple raycasts
	/// </summary>
	[AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Raycast Turret")]
	public class ObjectRaycastTurret : ObjectShootingPoint {

		public float maxDistance = 30;
		public int projectiles = 1;
		public Vector3 randomRelativeDirection = Vector3.zero;
		public LayerMask raycastMask = -1;

		public bool useRaycastAll = true;

		public Ray[] rays = null;
		public RaycastHitInfo[] raycastHitInfo = null;

		void Awake() {
			OnWeaponLoad_NoParameters += CreateRays;
			OnWeaponLoad_NoParameters += FireRaycasts;

			OnWeaponShoot_NoParameters += ClearAll;
		}

		public void CreateRays() {
			List<Ray> list = new List<Ray>();

			for(int i = 0; i < projectiles; i++) {
				Vector3 hitPoint = transform.forward + transform.TransformDirection(GetRandomDirection(randomRelativeDirection));
				Vector3 direction = hitPoint.normalized;

				list.Add(new Ray(transform.position, direction));
			}

			rays = list.ToArray();
		}

		public void FireRaycasts() {
			raycastHitInfo = rays.Raycast(maxDistance, raycastMask, useRaycastAll);
		}

		/// <summary>
		/// Clears the rays and raycastHitInfo arrays
		/// </summary>
		public void ClearAll() {
			rays = null;
			raycastHitInfo = null;
		}
	}
}