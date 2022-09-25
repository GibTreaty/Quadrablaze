using UnityEngine;
using System.Collections;

namespace YounGenTech {
	[AddComponentMenu("YounGen Tech/Scripts/Timers/FPS Display")]
	public class FramesPerSecondDisplay : MonoBehaviour {

		public float frequency = .5f;
		public int fps;

		float accum;
		int frames;

		void Start() {
			StartCoroutine("FPSCounter");
		}

		void OnGUI() {
			GUI.Box(new Rect(Screen.width - 85, 5, 90, 24), "<color=" + (fps >= 30 ? "green" : (fps > 10 ? "yellow" : "red")) + ">FPS " + fps + "</color>");
		}

		void Update() {
			accum += Time.timeScale / Time.deltaTime;
			++frames;
		}

		IEnumerator FPSCounter() {
			while(true) {
				float fpsOutput = accum / frames;

				fps = Mathf.RoundToInt(fpsOutput);
				accum = 0;
				frames = 0;

				yield return new WaitForSeconds(frequency);
			}
		}
	}
}