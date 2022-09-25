using System;
using Quadrablaze.Entities;
using UnityEngine;
using UnityEngine.Events;
using YounGenTech.Entities;

namespace Quadrablaze {
    public class GameAudioSource : MonoBehaviour, IActorEntityObjectInitialize {

        [SerializeField]
        bool _changePause = true;

        [SerializeField]
        bool _changeSpatial = true;

        AudioSource[] _attachedAudioSources;

        UnityAction<bool> onPauseAudioAction = null;
        UnityAction<bool> onChangeAudioAction = null;

        #region Properties
        public AudioSource[] AttachedAudioSources {
            get { return _attachedAudioSources; }
            private set { _attachedAudioSources = value; }
        }
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            AttachedAudioSources = GetComponents<AudioSource>();

            if(_changePause) {
                onPauseAudioAction = PauseAudio;

                PauseManager.Current.onPauseChange.AddListener(onPauseAudioAction);
                PauseAudio(PauseManager.Current.IsPaused);
            }

            if(_changeSpatial) {
                onChangeAudioAction = s => ChangeAudio(!s);

                GameManager.Current.OverviewCameraComponent.OnChangedStatus.AddListener(onChangeAudioAction);
                ChangeAudio(!GameManager.Current.OverviewCameraComponent.Status);
            }
        }

        void ChangeAudio(bool value) {
            ChangeAudio(value ? 1 : 0);
        }
        void ChangeAudio(float value) {
            foreach(var source in AttachedAudioSources)
                source.spatialBlend = value;
        }

        void OnDestroy() {
            if(onPauseAudioAction != null)
                PauseManager.Current.onPauseChange.RemoveListener(onPauseAudioAction);

            if(onChangeAudioAction != null)
                GameManager.Current.OverviewCameraComponent.OnChangedStatus.RemoveListener(onChangeAudioAction);
        }

        void PauseAudio(bool enable) {
            foreach(var source in AttachedAudioSources)
                if(enable) source.Pause();
                else source.UnPause();
        }
    }
}