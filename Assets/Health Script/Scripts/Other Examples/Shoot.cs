using UnityEngine;
using System.Collections;

public class Shoot : MonoBehaviour {

	public GameObject crosshairs;
	public float damage = 1;
	[Range(0, 100)]
	public int criticalHitChance = 20;
	public float criticalHitMultiplier = 2;

	public KeyCode shootButton = KeyCode.Mouse0;

	void Update() {
		RaycastHit hit;

		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
			if(!crosshairs.activeSelf) crosshairs.SetActive(true);

			crosshairs.transform.position = hit.point;
		}
		else
			if(crosshairs.activeSelf) crosshairs.SetActive(false);

		if(Input.GetKeyDown(shootButton))
			if(crosshairs.activeSelf) {
				float deal = damage;

				if(criticalHitChance > 0 && Random.value <= criticalHitChance / 100f)
					deal *= criticalHitMultiplier;

				if(hit.collider.CompareTag("Enemy"))
					hit.collider.SendMessage("Damage", new HealthEvent(gameObject, deal), SendMessageOptions.DontRequireReceiver);
				else if(hit.collider.CompareTag("Friendly"))
					hit.collider.SendMessage("Heal", new HealthEvent(gameObject, deal), SendMessageOptions.DontRequireReceiver);
			}
	}

}
