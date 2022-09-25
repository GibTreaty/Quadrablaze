using UnityEngine;
using System.Collections;

namespace YounGenTech {
	public static class PlayAudio {
		/// <summary>
		/// Works just like AudioSource.PlayClipAtPoint but you can specify pitch and it returns the new AudioSource
		/// </summary>
		public static AudioSource PlayClipAtPoint(this AudioClip clip, Vector3 position, float volume, float pitch) {
			GameObject go = new GameObject("One Shot Audio", typeof(AudioSource));
			
			go.transform.position = position;

			go.GetComponent<AudioSource>().clip = clip;
			go.GetComponent<AudioSource>().volume = volume;
			go.GetComponent<AudioSource>().pitch = pitch;
			go.GetComponent<AudioSource>().Play();

			GameObject.Destroy(go, clip.length / pitch);

			return go.GetComponent<AudioSource>();
		}

		public static AudioSource PlayClipAtPoint(this AudioClip clip, Vector3 position, float volume, float pitch, AudioRolloffMode rollOffMode) {
			AudioSource audio = PlayClipAtPoint(clip, position, volume, pitch);

			audio.rolloffMode = rollOffMode;

			return audio;
		}
	}
}