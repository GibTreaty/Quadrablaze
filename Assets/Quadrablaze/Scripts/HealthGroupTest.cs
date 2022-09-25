using UnityEngine;
using System.Diagnostics;

namespace Quadrablaze {
    public class HealthGroupTest : MonoBehaviour {

        public HealthLayer targetLayer;

        HealthGroup group;
        HealthLayer mainLayer;

        void Awake() {
            group = GetComponent<HealthGroup>();
            mainLayer = GetComponent<HealthLayer>();
        }

        void Update() {
            if(Input.GetKeyDown(KeyCode.Space)) {
                //Stopwatch watch = Stopwatch.StartNew();

                //for(int i = 0; i < 10000; i++)
                    group.Damage(targetLayer, new HealthEvent(mainLayer.gameObject, targetLayer.gameObject, 1));

                //watch.Stop();
                //UnityEngine.Debug.Log("Time = " + watch.Elapsed.TotalMilliseconds);
            }
        }
    }
}