using UnityEngine;
using System.Collections;

namespace YounGenTech {

	/// <summary>
	/// A timer that relies on FixedUpdate via Time.deltaTime and calls a function when the time has passed : Useful for delaying things like gun shots
	/// </summary>
	[AddComponentMenu("YounGen Tech/Scripts/Timers/Timer")]
	public class Timer : MonoBehaviour {

		/// <summary>
		/// Optional name to find a specific timer on a game object with multiple timers
		/// </summary>
		public string timerName;
		public float time;

		/// <summary>
		/// Speed at which the time goes to 0
		/// </summary>
		public float speed;

		/// <summary>
		/// The function that gets called when the time has passed
		/// </summary>
		TimerFunction timerFunction;

		public static Timer AddTimer(GameObject gameObject, TimerFunction methodToCall, float time) {
			return AddTimer(gameObject, methodToCall, time, 1, "");
		}
		public static Timer AddTimer(GameObject gameObject, TimerFunction methodToCall, float time, string name) {
			return AddTimer(gameObject, methodToCall, time, 1, name);
		}
		public static Timer AddTimer(GameObject gameObject, TimerFunction methodToCall, float time, float speed) {
			return AddTimer(gameObject, methodToCall, time, speed, "");
		}
		public static Timer AddTimer(GameObject gameObject, TimerFunction methodToCall, float time, float speed, string name) {
			Timer timer = null;

			if(gameObject != null) {
				timer = gameObject.AddComponent<Timer>();
				timer.timerName = name;
				timer.time = time;
				timer.speed = speed;
				timer.timerFunction = methodToCall;
			}

			return timer;
		}

		void Update() {
			time -= Time.deltaTime * speed;
			if(time <= 0) { timerFunction.Invoke(); Destroy(this); }
		}

		public delegate void TimerFunction();
	}
}