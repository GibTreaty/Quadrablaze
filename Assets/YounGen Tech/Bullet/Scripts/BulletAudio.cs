using UnityEngine;
using System.Collections;
using YounGenTech.ComponentInterface;

namespace YounGenTech.BulletPhysics {
	/// <summary>
	/// Plays audio when a bullet collides
	/// </summary>
	[AddComponentMenu("YounGen Tech/Bullet Physics/Bullet Audio")]
	public class BulletAudio : MonoBehaviour, IComponentEventConnector {
		/// <summary>
		/// Will use the <see cref="AudioSource"/> that is located on this <see cref="GameObject"/> instead of the collideClip.<br>
		/// If no <see cref="AudioSource"/> is found, one will be added to the <see cref="GameObject"/>.
		/// </summary>
		public bool playOnSelf;
		/// <summary>
		/// Will use the clip located on the <see cref="AudioSource"/> rather than the collideClip
		/// </summary>
		public bool useAudioSourceClip;

		public float volume = 1;
		public float pitch = 1;

		/// <summary>
		/// The audio played when the <see cref="Bullet"/> collides
		/// </summary>
		public AudioClip collideClip;

		void OnEnable() {
			SendMessage("OnComponentWasEnabled", this, SendMessageOptions.DontRequireReceiver);
		}

		void OnDisable() {
			SendMessage("OnComponentWasDisabled", this, SendMessageOptions.DontRequireReceiver);
		}

		void Play(BulletHit bullet, float velocity) {
			if(collideClip)
				collideClip.PlayClipAtPoint(transform.position, Mathf.Min(volume, velocity), pitch);
		}

		void ConnectBullet(Bullet bullet) {
			if(bullet) {
				DisconnectBullet(bullet);

				bullet.OnBulletCollided += Play;
			}
		}

		void DisconnectBullet(Bullet bullet) {
			if(bullet) bullet.OnBulletCollided -= Play;
		}

		void OnComponentWasEnabled(Component component) {
			this.ConnectComponentEventTo(component);
		}

		void OnComponentWasDisabled(Component component) {
			this.DisconnectComponentEventFrom(component);
		}

		public void ConnectComponentEvent(Component component) {
			Bullet bullet = component as Bullet;

			if(bullet) ConnectBullet(bullet);
		}

		public void DisconnectComponentEvent(Component component) {
			Bullet bullet = component as Bullet;

			if(bullet) ConnectBullet(bullet);
		}
	}
}