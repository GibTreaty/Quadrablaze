using System.Collections;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;
using StatSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class HitManager : MonoBehaviour {

        public static HitManager Current { get; private set; }

        [SerializeField]
        AudioSource _oneHitSound;

        [SerializeField]
        AudioSource _continuousHitSound;

        [SerializeField]
        AudioSource _playerDamagedOneHitSound;

        [SerializeField]
        AudioSource _shieldHitSound;

        [SerializeField]
        AudioSource _continuousShieldHitSound;

        [SerializeField]
        AudioSource _continuousShieldHealSound;

        [SerializeField]
        EventTimer _continuousHitTimer = new EventTimer(.5f);

        [SerializeField]
        float _continuousHitFade = .5f;

        [SerializeField]
        EventTimer _continuousShieldHitTimer = new EventTimer(.5f);

        [SerializeField]
        float _continuousHealFade = 2;

        [SerializeField]
        EventTimer _continuousShieldHealTimer = new EventTimer(.5f);

        Coroutine continuousHitRoutine = null;
        Coroutine continuousShieldHitRoutine = null;
        Coroutine continuousShieldHealRoutine = null;

        void OnEnable() {
            Current = this;
        }

        void Awake() {
            _continuousHitTimer.OnElapsed.AddListener(() => continuousHitRoutine = StartCoroutine(FadeOutContinuousSound(_continuousHitFade, _continuousHitSound)));
            _continuousHitTimer.OnHigh.AddListener(() => {
                if(continuousHitRoutine != null) {
                    StopCoroutine(continuousHitRoutine);
                    continuousHitRoutine = null;
                }

                _continuousHitSound.volume = 1;
                _continuousHitTimer.Start();
            });

            _continuousShieldHitTimer.OnElapsed.AddListener(() => continuousShieldHitRoutine = StartCoroutine(FadeOutContinuousSound(_continuousHitFade, _continuousShieldHitSound)));
            _continuousShieldHitTimer.OnHigh.AddListener(() => {
                if(continuousShieldHitRoutine != null) {
                    StopCoroutine(continuousShieldHitRoutine);
                    continuousShieldHitRoutine = null;
                }

                _continuousShieldHitSound.volume = 1;
                _continuousShieldHitTimer.Start();
            });

            _continuousShieldHealTimer.OnElapsed.AddListener(() => continuousShieldHealRoutine = StartCoroutine(FadeOutContinuousSound(_continuousHealFade, _continuousShieldHealSound)));
            _continuousShieldHealTimer.OnHigh.AddListener(() => {
                if(continuousShieldHealRoutine != null) {
                    StopCoroutine(continuousShieldHealRoutine);
                    continuousShieldHealRoutine = null;
                }

                _continuousShieldHealSound.volume = 1;
                _continuousShieldHealTimer.Start();
            });

            //TODO: Implement Player continuous heal sound
        }

        void Update() {
            _continuousHitTimer.Update(true);
            _continuousShieldHitTimer.Update(true);
            _continuousShieldHealTimer.Update(true);
        }

        public void OneHit() {
            _oneHitSound.Play();
        }

        public void ContinuousHit() {
            _continuousHitTimer.Reset();
        }

        public void ContinuousShieldHeal() {
            _continuousShieldHealTimer.Reset();
        }

        public void ContinuousShieldHit() {
            _continuousShieldHitTimer.Reset();
        }

        IEnumerator FadeOutContinuousSound(float time, AudioSource source) {
            float startTime = Time.unscaledTime;
            //float startTime = time;

            while(time > 0) {
                var currentTime = Time.unscaledTime;
                var volume = Mathf.InverseLerp(startTime, startTime + time, currentTime);

                source.volume = 1 - volume;

                //time = Mathf.Max(time - Time.deltaTime, 0);
                //source.volume = time / startTime;

                yield return null;
            }
        }

        public void PlayerDamagedOneHitSound() {
            _playerDamagedOneHitSound.Play();
        }

        public void ShieldHit() {
            _shieldHitSound.Play();
        }

        public void FilterEvent(StatEvent statEvent) {
            if(statEvent.AffectedObject == null) return;

            bool isShield = statEvent.AffectedObject is Shield;

            if(!isShield)
                if(statEvent.AffectedObject == statEvent.SourceObject) return;

            if(statEvent.ContinuousEffect) {
                if(isShield) {
                    var message = statEvent.GetMessage<StatChangeValueMessage>();

                    if(message.AmountChanged < 0)
                        FilterEvent(HitType.ShieldHitContinuous);
                    else if(message.AmountChanged > 0)
                        FilterEvent(HitType.ShieldHealContinuous);
                }
                else
                    FilterEvent(HitType.HitContinuous);
            }
            else {
                if(isShield)
                    FilterEvent(HitType.ShieldHit);
                else if(statEvent.AffectedObject is PlayerEntity)
                    FilterEvent(HitType.PlayerHit);
                else if(statEvent.AffectedObject is Entities.EnemyEntity)
                    FilterEvent(HitType.EnemyHit);
            }
        }

        void FilterEvent(HitType hitType) {
            switch(hitType) {
                case HitType.PlayerHit: PlayerDamagedOneHitSound(); break;
                case HitType.HitContinuous: ContinuousHit(); break;
                case HitType.ShieldHit: ShieldHit(); break;
                case HitType.ShieldHitContinuous: ContinuousShieldHit(); break;
                case HitType.ShieldHealContinuous: ContinuousShieldHeal(); break;
                case HitType.EnemyHit: OneHit(); break;
            }

            if(NetworkServer.active) {
                var writer = new NetworkWriter();

                writer.StartMessage(NetMessageType.Client_HitEffect);
                writer.Write((byte)hitType);
                writer.FinishMessage();

                QuadrablazeSteamNetworking.SendWriterToAll(writer, false);
            }
        }

        [RegisterNetworkHandlers]
        public static void RegisterHandlers() {
            QuadrablazeSteamNetworking.RegisterClientHandler(NetMessageType.Client_HitEffect, NetworkFilterHit);
        }

        static void NetworkFilterHit(NetworkMessage message) {
            var reader = message.reader;
            var hitTypeData = reader.ReadByte();
            var hitType = (HitType)System.Enum.Parse(typeof(HitType), hitTypeData.ToString());

            HitManager.Current.FilterEvent(hitType);
        }

        enum HitType : byte {
            PlayerHit = 0,
            EnemyHit = 1,
            HitContinuous = 2,
            ShieldHit = 3,
            ShieldHitContinuous = 4,
            ShieldHealContinuous = 5
            //TODO: PlayerHeal sound. Turret heals the player
        }
    }
}