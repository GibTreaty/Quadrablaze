using UnityEngine;

namespace Quadrablaze {
    public class EnemySpawnHover : MonoBehaviour {

        float speed;
        float time;
        Transform child;

        void Awake() {
            speed = Random.value * .35f;
            time = (Random.value * 2) - 1;
            child = transform.GetChild(0);
        }

        void Update() {
            time = (time + Time.unscaledDeltaTime * speed) % 2;

            child.localPosition = new Vector3(0, ((Mathf.PingPong(time, 1) * 2) - 1) * .5f, 0);
        }
    }
}