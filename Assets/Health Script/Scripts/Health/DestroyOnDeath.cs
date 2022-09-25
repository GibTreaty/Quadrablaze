using UnityEngine;
using System.Collections;

[AddComponentMenu("Health/Destroy On Death")]
public class DestroyOnDeath : MonoBehaviour {

	/// <summary>
	/// The object that will be destroyed on death
	/// </summary>
	public GameObject destroyThis;

	void Reset() {
		destroyThis = gameObject;
	}

	public void OnDeath(HealthEvent health) {
		//NetworkView networkView = GetComponent<NetworkView>();

		//if(networkView) {
		//	if(networkView.isMine) {
		//		Network.Destroy(destroyThis);

		//		Debug.Log("OnDeath");
		//	}
		//}
		//else
			Destroy(destroyThis);
	}
}
