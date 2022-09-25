using UnityEngine;
using System;

namespace YounGenTech.EnergyBasedObjects {
	/// <summary>
	/// Holds energy and automatically regains/drains energy overtime
	/// </summary>
	[AddComponentMenu("YounGen Tech/Energy Based Objects/Energy/Energy")]
	public class Energy : MonoBehaviour {

		/// <summary>
		/// Called when energy is automatically modified in Update 
		/// float is the amount changed
		/// </summary>
		public event Action<Energy, float> OnModifiedEnergy;
		/// <summary>
		/// Called when energy automatically reaches minAmount 
		/// float is the amount changed
		/// </summary>
		public event Action<Energy, float> OnMinEnergy;
		/// <summary>
		/// Called when energy automatically reaches maxAmount 
		/// float is the amount changed
		/// </summary>
		public event Action<Energy, float> OnMaxEnergy;

		[SerializeField]
		float _amount = 10;
		/// <summary>
		/// Amount of energy
		/// </summary>
		public float amount {
			get { return _amount; }
			set {
				_amount = value;

				ResetModifyDelay();
			}
		}

		/// <summary>
		/// Get how much positive energy there is. 
		/// This value is offset by the minAmount to give the total available amount.
		/// 
		/// So if you're minAmount is 1 and your amount is 10, you would really have 9 energy.
		/// </summary>
		public float positiveAmount {
			get { return Mathf.Max(0, amount - Mathf.Max(0, minAmount)); }
		}

		/// <summary> *Test*
		/// Get how much negative energy there is. 
		/// This value is offset by the maxAmount to give the total available amount.
		/// 
		/// So if you're maxAmount is -1 and your amount is -5, you would really have -4 energy.
		/// </summary>
		public float negativeAmount {
			get { return Mathf.Min(0, amount - Mathf.Min(0, maxAmount)); }
		}

		/// <summary>
		/// The amount of energy should not go below this amount
		/// </summary>
		public float minAmount;
		/// <summary>
		/// The amount of energy should not go above this amount
		/// </summary>
		public float maxAmount = 10;

		/// <summary>
		/// Multiply Time.deltaTime to the modifierAmount when modifying the amount. It effectively turns the modifierAmount into seconds so it won't be frame-dependant.
		/// </summary>
		public bool multiplyDeltaTime = true;
		/// <summary>
		/// The amount of energy will be changed by this amount. 
		/// Useful for automatically adding/subtracting energy. 
		/// </summary>
		public float modifierAmount = 1;

		/// <summary>
		/// The delay before the amount of energy begins modifying. 
		/// Used when the energy amount property is changed.
		/// </summary>
		public float modifyDelayAmount = 1;

		/// <summary>
		/// The time left until the amount will be modified again
		/// </summary>
		public float modifyTime;

		void Update() {
			if(modifyTime > 0) {
				modifyTime -= Time.deltaTime;

				if(modifyTime < 0) modifyTime = 0;
			}

			if(modifyTime == 0 && modifierAmount != 0) {
				bool onMinMax = _amount == minAmount || _amount == maxAmount;

				int x = (int)Mathf.Sign(modifierAmount);
				float previousAmount = _amount;

				x = (int)Mathf.Sign(modifierAmount); //Subtracted / Added
				_amount += modifierAmount * (multiplyDeltaTime ? Time.deltaTime : 1);

				if(x == -1) {
					if(_amount < minAmount) _amount = minAmount;
				}
				else if(x == 1) {
					if(_amount > maxAmount) _amount = maxAmount;
				}

				float difference = _amount - previousAmount;

				if(difference != 0) {
					if(OnModifiedEnergy != null) OnModifiedEnergy(this, difference);
				}

				if(!onMinMax && (_amount == minAmount || _amount == maxAmount))
					if(x == -1) {
						if(OnMinEnergy != null) OnMinEnergy(this, difference);
					}
					else if(x == 1) {
						if(OnMaxEnergy != null) OnMaxEnergy(this, difference);
					}
			}
		}

		/// <summary>
		/// Resets the timer for when the modifier should be applied
		/// </summary>
		public void ResetModifyDelay() {
			modifyTime = modifyDelayAmount;
		}

		/// <summary>
		/// Takes the amount of energy if not at minAmount
		/// </summary>
		/// <param name="amount"></param>
		/// <returns>Amount left after the energy was taken</returns>
		public float TakeEnergy(float amount) {
			float previousAmount = this.amount;

			this.amount = Mathf.Clamp(this.amount -= amount, minAmount, maxAmount);

			if(previousAmount != this.amount)
				if(OnModifiedEnergy != null) OnModifiedEnergy(this, previousAmount - this.amount);

			return amount - (previousAmount - this.amount);
		}

		/*
		/// <summary>
		/// Gives the amount of energy if not at maxAmount
		/// </summary>
		/// <param name="amount"></param>
		/// <returns>Actual amount given</returns>
		public float GiveEnergy(float amount) {
			float previousAmount = this.amount;

			this.amount += amount;

			if(this.amount < minAmount) this.amount = minAmount;

			return (previousAmount - this.amount) - amount;
		}*/
	}
}