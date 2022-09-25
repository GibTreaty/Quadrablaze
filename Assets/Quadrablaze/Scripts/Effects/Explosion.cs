using UnityEngine;
using YounGenTech.PoolGen;

public class Explosion : MonoBehaviour, IPoolInstantiate {

	public float explosionSpeed;
	float size = 1;

    public void PoolInstantiate(PoolUser user) {
        user.OnSpawn.AddListener(UserPooled);
    }

    void Update() {
		size += Time.deltaTime * explosionSpeed;
		transform.localScale = Vector3.one * size;
	}

	void UserPooled() {
		transform.localScale = Vector3.one;
		size = 1;
		GetComponent<AudioSource>().pitch = Random.Range(.8f, 1.2f);
	}
}