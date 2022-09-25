using UnityEngine;
using System.Collections;

namespace YounGenTech.EnergyBasedObjects {
	/// <summary>
	/// Stops an <see cref="ObjectShootingPoint"/> from shooting if the weapon becomes hot. Connects to all child <see cref="ObjectShootingPoint"/>s
	/// </summary>
	[AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Weapon Heat")]
	public class ObjectWeaponHeat : MonoBehaviour {

		[SerializeField]
		float _heat;
		public float heat {
			get { return _heat; }
			set {
				float previousValue = _heat;
				float tempHeat = Mathf.Clamp(value, 0, maxHeat);

				if(tempHeat - previousValue < 0) {
					_heat = tempHeat;
				}
				else if(!hot) {
					_heat = Mathf.Clamp(value, 0, maxHeat);

					if(_heat == maxHeat) hot = true;
					if(_heat - previousValue > 0)
						cooldownTime = cooldownDelay;
				}
			}
		}
		public float maxHeat;

		/// <summary>
		/// Heat must be under this amount to shoot
		/// </summary>
		public float enabledShootingHeat = 1;

		/// <summary>
		/// The amount this will heat up
		/// </summary>
		public float heatup = 1;

		/// <summary>
		/// The amount this will cool down
		/// </summary>
		public float cooldown = 1;

		/// <summary>
		/// The delay before cooling down
		/// </summary>
		public float cooldownDelay = 1;

		/// <summary>
		/// The current cooldown time
		/// </summary>
		public float cooldownTime;

		/// <summary>
		/// Indicates that the weapon is hot
		/// </summary>
		public bool hot;

		void Update() {
			if(cooldownTime > 0) {
				cooldownTime -= Time.deltaTime;

				if(cooldownTime < 0) cooldownTime = 0;
			}

			if(heat > 0 && cooldownTime == 0) {
				heat -= cooldown * Time.deltaTime;

				if(hot && heat < enabledShootingHeat) hot = false;
			}
		}

		public bool CanShoot() {
			return !hot || heat <= enabledShootingHeat;
		}

		public void HeatUp() {
			heat += heatup;
		}
		public void HeatUp(float amount) {
			heat += Mathf.Abs(amount);
		}

		void OnEnable() {
			ObjectShootingPoint point = GetComponentInChildren<ObjectShootingPoint>();

			if(point) ConnectShootingPoint(point);
		}

		void OnDisable() {
			ObjectShootingPoint point = GetComponentInChildren<ObjectShootingPoint>();

			if(point) DisonnectShootingPoint(point);
		}

		void ConnectShootingPoint(ObjectShootingPoint point) {
			DisonnectShootingPoint(point);

			point.OnWeaponLoad_NoParameters += HeatUp;
			point.CanShoot += CanShoot;
		}

		void DisonnectShootingPoint(ObjectShootingPoint point) {
			point.OnWeaponLoad_NoParameters -= HeatUp;
			point.CanShoot -= CanShoot;
		}

		void OnComponentEnabled(Component component) {
			if(!enabled) return;

			ObjectShootingPoint point = component as ObjectShootingPoint;

			if(point) ConnectShootingPoint(point);
		}

		void OnComponentDisabled(Component component) {
			if(!enabled) return;

			ObjectShootingPoint point = component as ObjectShootingPoint;

			if(point) DisonnectShootingPoint(point);
		}
	}
}