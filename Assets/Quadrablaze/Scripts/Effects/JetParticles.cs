using System;
using Quadrablaze.Entities;
using UnityEngine;
using YounGenTech.Entities;

namespace Quadrablaze.Effects {
    public class JetParticles : MonoBehaviour, IActorEntityObjectInitialize {

        [SerializeField]
        float _speed;

        [SerializeField]
        ParticleSystem _particles;

        [SerializeField]
        AudioSource _audio;

        [SerializeField]
        GameAudioSource _gameAudio;

        [SerializeField]
        Light _attachedLight;

        [SerializeField]
        JetControlType _jetControl = JetControlType.Auto;

        float _lightIntensity;

        #region Properties
        public Light AttachedLight {
            get { return _attachedLight; }
            set { _attachedLight = value; }
        }

        public AudioSource Audio {
            get { return _audio; }
            set { _audio = value; }
        }

        BaseMovementController BaseMovementControllerComponent { get; set; }

        public bool ForceSimulate { get; set; }

        public GameAudioSource GameAudio {
            get { return _gameAudio; }
            set { _gameAudio = value; }
        }

        public JetControlType JetControl {
            get { return _jetControl; }
            set { _jetControl = value; }
        }

        public float JetPower { get; set; }

        public bool Opened { get; private set; }

        public ParticleSystem Particles {
            get { return _particles; }
            set { _particles = value; }
        }

        public PlayerInput PlayerInputComponent { get; private set; }

        public float Speed {
            get { return _speed; }
            set { _speed = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            BaseMovementControllerComponent = entity.BaseMovementControllerComponent;
            PlayerInputComponent = (entity as PlayerEntity).PlayerInputComponent;

            _lightIntensity = AttachedLight.intensity;
            AttachedLight.intensity = 0;
        }

        //void OnGUI() {
        //    GUILayout.Label("");
        //}

        void Update() {
            if((PlayerInputComponent != null) || JetControl == JetControlType.Manual)
                UpdateJet();
        }

        void UpdateJet() {
            switch(JetControl) {
                case JetControlType.Auto:
                    Vector3 movement = PlayerInputComponent.MovementInput;

                    if(BaseMovementControllerComponent.MovementStyle == BaseMovementController.MovementType.Directional) {
                        JetPower = Mathf.Clamp01(Vector3.Dot(movement, -transform.forward));
                    }
                    else {
                        float horizontalAngularVelocity = movement.x;
                        Vector3 directionFromCenter = transform.position - transform.root.position;
                        Vector3 angularVelocity = Vector3.Cross(directionFromCenter, transform.root.up);
                        //
                        if(horizontalAngularVelocity != 0) {
                            Vector3 direction = Vector3.Cross(angularVelocity, transform.right * horizontalAngularVelocity);

                            direction = transform.root.rotation * direction;
                            JetPower = direction.y;
                        }
                        else
                            JetPower = 0;

                        var tempDot = Mathf.Clamp01(Vector3.Dot(new Vector3(0, 0, movement.z), Quaternion.Inverse(transform.root.rotation) * -transform.forward));

                        if(tempDot > Mathf.Abs(JetPower))
                            JetPower = tempDot;
                    }

                    break;
            }


            var info = Particles.main;

            info.startSpeed = Speed * JetPower;

            AttachedLight.intensity = JetPower > .1f ? JetPower * _lightIntensity : 0;

            if(Particles.isPlaying) {
                if(JetPower <= .1f) {
                    Opened = false;

                    //if(!ForceSimulate)
                    Particles.Stop();

                    Audio.Stop();
                }
            }
            else {
                if(JetPower > .1f) {
                    Opened = true;

                    //if(!ForceSimulate)
                    Particles.Play();

                    Audio.Play();
                }
            }

            if(Opened && ForceSimulate && Time.timeScale == 0 && Particles.isPlaying)
                Particles.Simulate(Time.unscaledDeltaTime, false, false, true);
        }

        public enum JetControlType {
            Auto = 0,
            Manual = 1
        }
    }
}