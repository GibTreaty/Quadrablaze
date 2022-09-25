using UnityEngine;
using System;
using System.Collections.Generic;
using YounGenTech.ComponentInterface;

namespace YounGenTech.EnergyBasedObjects {

	/// <summary>
	/// Receives the raycast info from the ObjectRaycastTurret and instantiates LineRenderers accordingly
	/// </summary>
	[AddComponentMenu("YounGen Tech/Energy Based Objects/Shooting/Effects/Spawn LineRenderers ~ Connects to ObjectRaycastTurret (OnWeaponRaycastSpawn)")]
	public class OnWeaponRaycastSpawn : MonoBehaviour, IComponentEventConnector {
		/// <summary>
		/// Called when a LineRenderer has been created and properties are set
		/// </summary>
		public event Action<LineRenderer> OnCreateLineRenderer;

		/// <summary>
		/// The limit of how many objects are checked when hit from a RaycastAll
		/// </summary>
		public int maxObjectsToHit = 1;

		/// <summary>
		/// Spawns effect and sends messages to the collider.attachedRigidbody if it exists.<br>
		/// Otherwise it defaults to the collider that was hit.
		/// </summary>
		public bool useAttachedRigidbody;

		public LineRendererOptions lineRendererOptions = new LineRendererOptions();
		public MoveLineRendererOptions moveLineRendererOptions = new MoveLineRendererOptions();
		public EffectOptions effectOptions = new EffectOptions();
		public MessageOptions messageOptions = new MessageOptions();

		void OnEnable() {
			SendMessage("OnComponentWasEnabled", this, SendMessageOptions.DontRequireReceiver);
		}

		void OnDisable() {
			SendMessage("OnComponentWasDisabled", this, SendMessageOptions.DontRequireReceiver);
		}

		void OnComponentWasEnabled(Component component) {
			this.ConnectComponentEventTo(component);
		}

		void OnComponentWasDisabled(Component component) {
			this.DisconnectComponentEventFrom(component);
		}

		public void ConnectComponentEvent(Component component) {
			ObjectRaycastTurret raycaster = component as ObjectRaycastTurret;

			if(raycaster) {
				raycaster.OnWeaponLoad -= OnWeaponRaycast;
				raycaster.OnWeaponLoad += OnWeaponRaycast;
			}
		}

		public void DisconnectComponentEvent(Component component) {
			ObjectRaycastTurret raycaster = component as ObjectRaycastTurret;

			if(raycaster) raycaster.OnWeaponLoad -= OnWeaponRaycast;
		}

		void OnWeaponRaycast(ObjectShootingPoint point) {
			if(maxObjectsToHit <= 0) return;

			ObjectRaycastTurret raycaster = point as ObjectRaycastTurret;

			foreach(RaycastHitInfo a in raycaster.raycastHitInfo) {
				RaycastHit hit = a.hits[0];
				GameObject useGameObject = hit.collider ? hit.collider.gameObject : null;

				if(useAttachedRigidbody && useGameObject && hit.collider.attachedRigidbody)
					useGameObject = hit.collider.attachedRigidbody.gameObject;

				List<RaycastHit> eventList2 = new List<RaycastHit>();
				Queue<RaycastHit> eventList = new Queue<RaycastHit>();
				GameObject go = new GameObject("LineRenderer", typeof(MoveLineRenderer));
				MoveLineRenderer move = go.GetComponent<MoveLineRenderer>();
				LineRenderer line = go.GetComponent<LineRenderer>();
				line.SetWidth(lineRendererOptions.startWidth, lineRendererOptions.endWidth);

				line.material = lineRendererOptions.lineRendererMaterial;
				line.SetColors(lineRendererOptions.startColor, lineRendererOptions.endColor);

				Destroy(go, lineRendererOptions.lineRendererLife);

				if(OnCreateLineRenderer != null) OnCreateLineRenderer(line);

				Vector3 lineEndPosition = hit.point;

				if(useGameObject) {
					int hitCount = 0;

					foreach(RaycastHit b in a.hits) {
						if(b.collider != null) {
							///Check Rotate effect
							if(hitCount < maxObjectsToHit) {
								eventList.Enqueue(b);
								eventList2.Add(b);
								lineEndPosition = b.point;
							}
							else
								break;

							hitCount++;
						}
					}

					Action<float> moveEvent = null;
					moveEvent = new Action<float>(dist => {
						List<RaycastHit> list = new List<RaycastHit>(eventList2);

						foreach(RaycastHit hitInfo in list)
							if(hitInfo.collider)
								if(dist >= hitInfo.distance) {
									GameObject goCollider = hitInfo.collider.gameObject;

									if(useAttachedRigidbody && goCollider && hitInfo.collider.attachedRigidbody)
										goCollider = hitInfo.collider.attachedRigidbody.gameObject;

									if(goCollider.GetComponent<Rigidbody>() && moveLineRendererOptions.relativeHitForce != Vector3.zero) {
										Vector3 direction = moveLineRendererOptions.endPosition - moveLineRendererOptions.startPosition;

										goCollider.GetComponent<Rigidbody>().AddForceAtPosition(Quaternion.LookRotation(direction) * moveLineRendererOptions.relativeHitForce, moveLineRendererOptions.endPosition);
									}

									messageOptions.SendMessage(goCollider.gameObject);

									if(effectOptions.effect) {
										GameObject instantiatedEffect = Instantiate(effectOptions.effect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal)) as GameObject;
										if(effectOptions.effectUseLife) Destroy(instantiatedEffect, effectOptions.effectLife);
									}

									eventList2.Remove(hitInfo);
								}


					});

					move.OnLineReachDistance += moveEvent;
				}

				moveLineRendererOptions.startPosition = a.startPosition;
				moveLineRendererOptions.endPosition = lineEndPosition;

				move.MoveTo(moveLineRendererOptions.startPosition, moveLineRendererOptions.endPosition, moveLineRendererOptions.startOffset, moveLineRendererOptions.endOffset, moveLineRendererOptions.moveStart, moveLineRendererOptions.moveEnd, moveLineRendererOptions.speed, moveLineRendererOptions.speedIsLineDistance);
			}
		}

		public enum SendMessageType {
			SendMessage, SendMessageUpwards, BroadcastMessage
		}

		[System.Serializable]
		public class EffectOptions {
			public GameObject effect;
			public float effectLife = 1;
			public bool effectUseLife = true;
		}

		[System.Serializable]
		public class LineRendererOptions {
			public Material lineRendererMaterial;

			public float startWidth = 2;
			public float endWidth = 2;

			public Color startColor = Color.white;
			public Color endColor = Color.white;

			public float lineRendererLife = 3;
		}

		[System.Serializable]
		public class MessageOptions {
			public float messageParameter = 0;
			public bool sendMessageParameter = false;
			public string messageToHitObjects = "";
			public SendMessageType messageType = SendMessageType.SendMessage;

			public void SendMessage(GameObject gameObject) {
				if(!gameObject) return;

				if(messageToHitObjects != "")
					switch(messageType) {
						case SendMessageType.SendMessage:
							if(sendMessageParameter) gameObject.SendMessage(messageToHitObjects, messageParameter, SendMessageOptions.DontRequireReceiver);
							else gameObject.SendMessage(messageToHitObjects, SendMessageOptions.DontRequireReceiver);
							break;
						case SendMessageType.SendMessageUpwards:
							if(sendMessageParameter) gameObject.SendMessageUpwards(messageToHitObjects, messageParameter, SendMessageOptions.DontRequireReceiver);
							else gameObject.SendMessageUpwards(messageToHitObjects, SendMessageOptions.DontRequireReceiver);
							break;
						case SendMessageType.BroadcastMessage:
							if(sendMessageParameter) gameObject.BroadcastMessage(messageToHitObjects, messageParameter, SendMessageOptions.DontRequireReceiver);
							else gameObject.BroadcastMessage(messageToHitObjects, SendMessageOptions.DontRequireReceiver);
							break;
					}
			}
		}

		[System.Serializable]
		public class MoveLineRendererOptions {
			[HideInInspector]
			public Vector3 startPosition;
			[HideInInspector]
			public Vector3 endPosition;

			public float startOffset;
			public float endOffset;

			public bool moveStart;
			public bool moveEnd;

			public float speed = 1;
			public bool speedIsLineDistance;

			public ForceMode forceMode = ForceMode.Force;
			public Vector3 relativeHitForce;
		}
	}
}