using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace YounGenTech.PoolGen {
    [AddComponentMenu("YounGen Tech/PoolGen/Test/Pool Test")]
    /// <summary>Used to test the pool system</summary>
    public class PoolTest : MonoBehaviour {

        /// <summary>Direct reference to a <see cref="PoolManager"/></summary>
        public PoolManager pool_MouseButtonRight;
        /// <summary>Name of a <see cref="PoolManager"/></summary>
        public string poolNameMouseButtonLeft = "";

        /// <summary>How many objects to spawn</summary>
        public int spawnCount = 1;

        /// <summary>Shoot timer delay for the left mouse button</summary>
        public float shootDelayLeft = .1f;
        /// <summary>Shoot timer delay for the right mouse button</summary>
        public float shootDelayRight = .1f;

        float timeLeft;
        float timeRight;

        void Update() {
            if(timeLeft > 0)
                timeLeft = Mathf.Max(timeLeft - Time.deltaTime, 0);

            if(timeRight > 0)
                timeRight = Mathf.Max(timeRight - Time.deltaTime, 0);

            //Spawns a random object from a pool using the pool's name rather than a direct reference
            if(Input.GetMouseButton(0))
                if(timeLeft == 0) {
                    for(int i = 0; i < spawnCount; i++)
                        PoolManager.Spawn(poolNameMouseButtonLeft, transform.position + new Vector3(Random.insideUnitCircle.x, 0, 0) * 2, Quaternion.identity);

                    timeLeft = shootDelayLeft;
                }

            //Spawns a random object from a pool using a direct reference to it
            if(Input.GetMouseButton(1))
                if(timeRight == 0) {
                    for(int i = 0; i < spawnCount; i++)
                        pool_MouseButtonRight.Spawn(transform.position + new Vector3(Random.insideUnitCircle.x, 0, 0) * 2, Quaternion.identity);

                    timeRight = shootDelayRight;
                }
        }
    }
}