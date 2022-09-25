using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

namespace YounGenTech.PoolGen.Effects {
    [AddComponentMenu("YounGen Tech/PoolGen/Effects/Timed Despawn")]
    /// <summary>Starts a despawn timer when spawned. An alternative to using <see cref="PoolPrefab.TimeToDespawn"/>.</summary>
    public class TimedDespawn : MonoBehaviour, IPoolInstantiate {
        public float time;

        [Tooltip("Uses the Invoke function if true. Uses Update for the timer if false.")]
        public bool useInvoke = true;

        float _timeLeft = 0;

        PoolUser UserComponent { get; set; }

        public void PoolInstantiate(PoolUser user) {
            UserComponent = user;

            UserComponent.OnSpawn.AddListener(OnSpawn);
            UserComponent.OnDespawn.AddListener(OnDespawn);
        }

        void Despawn() {
            UserComponent.Despawn();
        }

        void OnDespawn() {
            if(NetworkServer.active)
                if(useInvoke) CancelInvoke("Despawn");
                else _timeLeft = 0;
        }

        void OnSpawn() {
            if(NetworkServer.active)
                if(useInvoke) StartTimer();
                else _timeLeft = time;
        }

        void StartTimer() {
            Invoke("Despawn", time);
        }

        void Update() {
            if(!NetworkServer.active) return;
            if(useInvoke) return;

            if(_timeLeft > 0) {
                _timeLeft = Mathf.Max(_timeLeft - Time.deltaTime, 0);

                if(_timeLeft == 0) Despawn();
            }
        }
    }
}