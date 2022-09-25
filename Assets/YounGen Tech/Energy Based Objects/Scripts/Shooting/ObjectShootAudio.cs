using UnityEngine;
using System;
using YounGenTech.ComponentInterface;

namespace YounGenTech.EnergyBasedObjects {

	/// <summary>
	/// Plays audio when an <see cref="ObjectShootingPoint"/> is either shot or loaded
	/// </summary>
	[AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Effects/Audio Effect (ObjectShootAudio)")]
	public class ObjectShootAudio : MonoBehaviour, IComponentEventConnector {
		/// <summary>
		/// Will use the <see cref="AudioSource"/> that is located on this <see cref="GameObject"/> instead of the clips below.<br>
		/// If no <see cref="AudioSource"/> is found, one will be added to the <see cref="GameObject"/>.
		/// </summary>
		public bool playOnSelf;

		/// <summary>
		/// Will use the clip located on the <see cref="AudioSource"/> rather than the shootClip or loadClip
		/// </summary>
		public bool useAudioSourceClip;

		public float volume = 1;
		public float pitch = 1;

		public AudioClip shootClip;
		public AudioClip loadClip;

		public bool playOnShoot;
		public bool playOnLoad;

		public void PlayShootClip() {
			if(playOnSelf) {
				if(playOnShoot) {
					if(!GetComponent<AudioSource>()) gameObject.AddComponent<AudioSource>();

					gameObject.GetComponent<AudioSource>().volume = volume;
					gameObject.GetComponent<AudioSource>().pitch = pitch;

					if(useAudioSourceClip)
						GetComponent<AudioSource>().Play();
					else
						GetComponent<AudioSource>().PlayOneShot(shootClip);
				}
			}
			else {
				if(playOnShoot) shootClip.PlayClipAtPoint(transform.position, volume, 1, AudioRolloffMode.Linear);
			}
		}

		public void PlayLoadClip() {
			if(playOnSelf) {
				if(playOnLoad) {
					if(!GetComponent<AudioSource>()) gameObject.AddComponent<AudioSource>();

					gameObject.GetComponent<AudioSource>().volume = volume;
					gameObject.GetComponent<AudioSource>().pitch = pitch;

					if(useAudioSourceClip)
						GetComponent<AudioSource>().Play();
					else
						GetComponent<AudioSource>().PlayOneShot(loadClip);
				}
			}
			else {
				if(playOnLoad) loadClip.PlayClipAtPoint(transform.position, volume, 1, AudioRolloffMode.Linear);
			}
		}

		void OnEnable() {
			SendMessage("OnComponentWasEnabled", this, SendMessageOptions.DontRequireReceiver);
		}

		void OnDisable() {
			SendMessage("OnComponentWasDisabled", this, SendMessageOptions.DontRequireReceiver);
		}

		public void ConnectShootingPoint(ObjectShootingPoint point) {
			DisconnectShootingPoint(point);

			point.OnWeaponShoot_NoParameters += PlayShootClip;
			point.OnWeaponLoad_NoParameters += PlayLoadClip;
		}

		public void DisconnectShootingPoint(ObjectShootingPoint point) {
			point.OnWeaponShoot_NoParameters -= PlayShootClip;
			point.OnWeaponLoad_NoParameters -= PlayLoadClip;
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