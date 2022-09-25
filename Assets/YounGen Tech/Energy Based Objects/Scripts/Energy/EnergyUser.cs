using UnityEngine;
using System;
using System.Collections.Generic;
using YounGenTech.ComponentInterface;

namespace YounGenTech.EnergyBasedObjects {
	/// <summary>
	/// Use up <see cref="Energy"/> when a <see cref="ObjectShootingPoint"/> is shot
	/// </summary>
	[AddComponentMenu("YounGen Tech/Energy Based Objects/Energy/Energy User")]
	public class EnergyUser : MonoBehaviour, IComponentEventConnector {
		/// <summary>
		/// The object holding the <see cref="Energy"/>
		/// </summary>
		public GameObject energyContainer;

		/// <summary>
		/// Minimum amount of Energy that is required for this weapon/object
		/// </summary>
		public float useAmount = 1;
		
		/// <summary>
		/// When energy is taken, the actual amount taken will be multiplied by Time.deltaTime. Use this for weapons/objects that should take energy per-frame rather than per-shot.
		/// </summary>
		public bool multiplyDeltaTime = true;
		
		/// <summary>
		/// Check the Energy component that is on this object
		/// </summary>
		public bool checkSelfEnergy = true;

		/// <summary>
		/// Check the Energy components that are on this object's and the energyContainer's children
		/// </summary>
		public bool checkChildrenEnergy = true;

		/// <summary>
		/// Takes the required amount of energy from specified sources if available
		/// </summary>
		public void UseEnergy() {
			List<Energy> list = new List<Energy>();
			float amount = 0;
			float actualUseAmount = useAmount;

			if(multiplyDeltaTime) actualUseAmount *= Time.deltaTime;

			foreach(Energy a in GetNonEmptyEnergy()) {
				amount += a.amount;
				list.Add(a);

				if(amount >= actualUseAmount) break;
			}

			if(amount >= actualUseAmount) {
				amount = actualUseAmount;

				foreach(Energy a in list) {
					amount = a.TakeEnergy(amount);

					if(amount <= 0) break;
				}
			}
		}

		/// <summary>
		/// Checks if the specified Energy sources have enough Energy
		/// </summary>
		/// <returns>Has enough Energy</returns>
		public bool CheckEnergy() {
			float amount = 0;
			float actualUseAmount = useAmount;

			foreach(Energy a in GetNonEmptyEnergy()) {
				amount += a.positiveAmount;

				if(amount >= actualUseAmount) return true;
			}

			return amount >= actualUseAmount;
		}

		/// <summary>
		/// Looks for available Energy sources and returns them
		/// </summary>
		/// <returns>Available Energy sources</returns>
		public Energy[] GetEnergy() {
			List<Energy> list = new List<Energy>();

			if(checkChildrenEnergy) {
				if(energyContainer)
					list.AddRange(energyContainer.GetComponentsInChildren<Energy>());

				if(checkSelfEnergy)
					list.AddRange(GetComponentsInChildren<Energy>());
			}
			else {
				if(energyContainer)
					list.AddRange(energyContainer.GetComponents<Energy>());

				if(checkSelfEnergy)
					list.AddRange(GetComponents<Energy>());
			}

			return list.ToArray();
		}

		/// <summary>
		/// Looks for available Energy sources that aren't empty and returns them
		/// </summary>
		/// <returns>Available Energy sources that aren't empty</returns>
		public Energy[] GetNonEmptyEnergy() {
			List<Energy> list = new List<Energy>();

			if(checkChildrenEnergy) {
				if(energyContainer)
					foreach(Energy a in energyContainer.GetComponentsInChildren<Energy>())
						if(a.positiveAmount > 0) list.Add(a);

				if(checkSelfEnergy)
					foreach(Energy a in GetComponentsInChildren<Energy>())
						if(a.positiveAmount > 0) list.Add(a);
			}
			else {
				if(energyContainer)
					foreach(Energy a in energyContainer.GetComponents<Energy>())
						if(a.positiveAmount > 0) list.Add(a);

				if(checkSelfEnergy)
					foreach(Energy a in GetComponents<Energy>())
						if(a.positiveAmount > 0) list.Add(a);
			}

			return list.ToArray();
		}

		void OnEnable() {
			SendMessage("OnComponentWasEnabled", this, SendMessageOptions.DontRequireReceiver);
		}

		void OnDisable() {
			SendMessage("OnComponentWasDisabled", this, SendMessageOptions.DontRequireReceiver);
		}

		public void ConnectShootingPoint(ObjectShootingPoint point) {
			DisconnectShootingPoint(point);

			point.CanShoot += CheckEnergy;
			point.OnWeaponLoad_NoParameters += UseEnergy;
		}

		public void DisconnectShootingPoint(ObjectShootingPoint point) {
			point.CanShoot -= CheckEnergy;
			point.OnWeaponLoad_NoParameters -= UseEnergy;
		}

		void OnComponentWasEnabled(Component component) {
			this.ConnectComponentEventTo(component);
		}

		void OnComponentWasDisabled(Component component) {
			this.DisconnectComponentEventFrom(component);
		}

		public void ConnectComponentEvent(Component component) {
			ObjectShootingPoint point = component as ObjectShootingPoint;

			if(point) ConnectShootingPoint(point);
		}

		public void DisconnectComponentEvent(Component component) {
			ObjectShootingPoint point = component as ObjectShootingPoint;

			if(point) DisconnectShootingPoint(point);
		}
	}
}