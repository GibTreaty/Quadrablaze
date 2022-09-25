using UnityEngine;
using System.Collections;

/// <summary>
/// Stores a GameObject that acts as an owner
/// Also re-routes health events to the owner which "should" have a Health component
/// </summary>
public class Owner : MonoBehaviour {

	[SerializeField]
	GameObject _ownerObject;

	public GameObject OwnerObject {
		get { return _ownerObject;}
		set { _ownerObject = value; }
	}

	public void Heal(HealthEvent health) {
		OwnerObject.SendMessage("Heal", health, SendMessageOptions.DontRequireReceiver);
	}

	public void Damage(HealthEvent health) {
		OwnerObject.SendMessage("Damage", health, SendMessageOptions.DontRequireReceiver);
	}
}
