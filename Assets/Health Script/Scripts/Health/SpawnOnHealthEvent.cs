using UnityEngine;
using System.Collections;

[AddComponentMenu("Health/Spawn On Health Event")]
public class SpawnOnHealthEvent : MonoBehaviour {

	public GameObject indicatorPrefab;
	public Transform spawnPosition;

	public bool spawnOnHealed = true;
	public bool spawnOnDamaged = true;
	public bool spawnOnDeath = true;

	void OnHealed(HealthEvent health) {
		if(spawnOnHealed) Spawn("OnHealed", health);
	}

	void OnDamaged(HealthEvent health) {
		if(spawnOnDamaged) Spawn("OnDamaged", health);
	}

	void OnDeath(HealthEvent health) {
		if(spawnOnDeath) Spawn("OnDeath", health);
	}

	void Spawn(string method, HealthEvent health) {
		if(indicatorPrefab) {
			GameObject go = Instantiate(indicatorPrefab, spawnPosition ? spawnPosition.position : transform.position, transform.rotation) as GameObject;

			go.SendMessage(method, health, SendMessageOptions.DontRequireReceiver);
		}
	}
}
