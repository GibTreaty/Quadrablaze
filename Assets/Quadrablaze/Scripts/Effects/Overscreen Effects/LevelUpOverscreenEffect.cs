using System;
using UnityEngine;

namespace Quadrablaze {
    public class LevelUpOverscreenEffect : MonoBehaviour {

        public RectTransform skillPointUITarget;

        ParticleSystem.Particle[] particles;

        #region Properties
        ParticleSystem ParticleSystemComponent { get; set; }

        RectTransform RectTransformComponent { get; set; }
        #endregion

        public void Initialize() {
            ParticleSystemComponent = GetComponent<ParticleSystem>();
            RectTransformComponent = GetComponentInParent<RectTransform>();
            particles = new ParticleSystem.Particle[ParticleSystemComponent.main.maxParticles];

            PlayerSpawnManager.Current.OnPlayerGainedSkillPoint.AddListener(FireEffect);
        }

        //void Awake() {
        //    ParticleSystemComponent = GetComponent<ParticleSystem>();
        //    RectTransformComponent = GetComponentInParent<RectTransform>();
        //    particles = new ParticleSystem.Particle[ParticleSystemComponent.main.maxParticles];
        //}

        //void Start() {
        //    //PlayerSpawnManager.Current.OnPlayerFirstSpawned.AddListener(PlayerSpawned);
        //    PlayerSpawnManager.Current.OnPlayerGainedSkillPoint.AddListener(FireEffect);

        //    //InvokeRepeating("TryFire", 0, 1);
        //}

        //void TryFire() {
        //    if(PlayerSpawnManager.IsPlayerAlive) {
        //        FireEffect();
        //    }
        //}

        void LateUpdate() {
            UpdateParticles();
        }

        void UpdateParticles() {
            //if(!PlayerSpawnManager.IsPlayerAlive) return;
            if(PlayerSpawnManager.Current.CurrentPlayerEntityId == 0) return;

            int count = ParticleSystemComponent.GetParticles(particles);
            var playerEntity = PlayerSpawnManager.Current.GetCurrentEntity();
            //Vector3 playerPosition = playerEntity.CurrentTransform.position;

            for(int i = 0; i < count; i++) {
                ParticleSystem.Particle particle = particles[i];

                particle.position = Vector3.Lerp(skillPointUITarget.position, particle.angularVelocity3D, particle.remainingLifetime / particle.startLifetime);
                //particle.position = Vector3.Lerp(skillPointUITarget.position, playerPosition, particle.remainingLifetime / particle.startLifetime);
                particles[i] = particle;
            }

            ParticleSystemComponent.SetParticles(particles, count);
        }

        void FireEffect() {
            if(PlayerSpawnManager.Current.CurrentPlayerEntityId > 0) {
                var playerEntity = PlayerSpawnManager.Current.GetCurrentEntity();

                AddParticle(playerEntity.CurrentTransform.position);
            }
        }

        public void AddParticle(Vector3 position, Vector3 velocity = default) {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams() {
                position = position,
                //startLifetime = 2,
                angularVelocity3D = position,
                startColor = ParticleSystemComponent.main.startColor.Evaluate(0),
            };

            ParticleSystemComponent.Emit(emitParams, 1);
        }

        public void ClearParticles() {
            ParticleSystemComponent.Clear();
        }
    }
}