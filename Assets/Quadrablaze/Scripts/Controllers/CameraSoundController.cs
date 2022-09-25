using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Quadrablaze {
    public class CameraSoundController : MonoBehaviour, ISerializationCallbackReceiver {

        public static CameraSoundController Current { get; private set; }

        [SerializeField]
        SoundInfo[] sounds;

        Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();
        AudioSource _attachedAudioSource;

        #region Properties
        AudioSource AttachedAudioSource {
            get {
                if(!_attachedAudioSource) _attachedAudioSource = GetComponent<AudioSource>();
                return _attachedAudioSource;
            }
        }
        #endregion

        void Awake() {
            Current = this;
        }

        public void OnAfterDeserialize() {
            foreach(SoundInfo info in sounds)
                soundDictionary.Add(info.name, info.clip);
        }

        public void OnBeforeSerialize() { }

        public void PlaySound(AudioClip clip) {
            AttachedAudioSource.PlayOneShot(clip);
        }
        public void PlaySound(string name, bool playOverNetwork = false) {
            if(soundDictionary.ContainsKey(name))
                if(playOverNetwork && NetworkServer.active)
                    SendNetworkSound(name);
                else
                    PlaySound(soundDictionary[name]);
        }

        void SendNetworkSound(string name) {
            NetworkServer.SendToAll(NetMessageType.Client_PlayCameraSound, new StringMessage(name));
        }

        public void StopSounds() {
            AttachedAudioSource.Stop();
        }

        [System.Serializable]
        public struct SoundInfo {
            public string name;
            public AudioClip clip;

            public SoundInfo(string name, AudioClip clip) {
                this.name = name;
                this.clip = clip;
            }
        }

        public static void NetworkPlayCameraSound(NetworkMessage networkMessage) {
            string name = networkMessage.ReadMessage<StringMessage>().value;

            Current.PlaySound(name);
        }
    }
}