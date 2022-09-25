using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using YounGenTech.PoolGen;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;

namespace Quadrablaze {
    [RequireComponent(typeof(ParticleSystem))]
    public class XPParticleManager : NetworkBehaviour {

        public static XPParticleManager Current { get; private set; }

        public float pullForce = 1;
        public float pullDistance = 1;
        public float drag = 1;
        public float collectDistance = 1;

        public PoolManager xpRingPool;

        public AudioClip collectSound;
        public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;

        void OnEnable() {
            Current = this;
        }

        public void PlayEffect(Vector3 position) {
            var audioObject = new GameObject("Collect XP", typeof(AudioSource));
            var audioSource = audioObject.GetComponent<AudioSource>();

            audioObject.transform.position = position;

            audioSource.clip = collectSound;
            audioSource.volume = Random.Range(.5f, .8f);
            audioSource.pitch = Random.Range(.7f, 1.5f);
            audioSource.Play();
            audioSource.outputAudioMixerGroup = audioMixerGroup;

            Destroy(audioObject, audioSource.clip.length / audioSource.pitch);
        }

        IEnumerable<PlayerEntity> GetAttractionPoints() {
            foreach(var entity in GameManager.Current.Entities)
                if(entity is PlayerEntity playerEntity)
                    yield return playerEntity;
        }

        public Vector3 GetNearestAttractionPoint(Vector3 position) {
            Vector3 nearestPosition = position;
            float nearestDistance = Mathf.Infinity;

            foreach(var playerEntity in GetAttractionPoints()) {
                float distance = (transform.position - position).sqrMagnitude;

                if(distance < nearestDistance) {
                    nearestDistance = distance;
                    nearestPosition = playerEntity.CurrentTransform.position;
                }
            }

            return nearestPosition;
        }

        public PlayerEntity GetNearestAttractionTransform(Vector3 position) {
            return GetNearestAttractionTransform(position, Mathf.Infinity);
        }
        public PlayerEntity GetNearestAttractionTransform(Vector3 position, float minimumDistance) {
            PlayerEntity nearestEntity = null;
            float nearestDistance = minimumDistance;

            foreach(var playerEntity in GetAttractionPoints()) {
                float distance = (playerEntity.CurrentTransform.position - position).magnitude;
                float checkDistance = nearestDistance;

                if(playerEntity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<XPCollectionRange>() is var executor)
                    checkDistance += executor.GetRange();

                if(distance <= checkDistance) {
                    nearestDistance = distance;
                    nearestEntity = playerEntity;
                }
            }

            return nearestEntity;
        }

        public PlayerEntity GetFirstNearestAttractionTransform(Vector3 position, float minimumDistance) {
            PlayerEntity nearestEntity = null;
            float nearestDistance = Mathf.Infinity;

            foreach(var playerEntity in GetAttractionPoints()) {
                if(playerEntity.CurrentTransform == null) continue;

                float distance = (playerEntity.CurrentTransform.position - position).magnitude;
                float checkDistance = minimumDistance;

                if(playerEntity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<XPCollectionRange>() is XPCollectionRange executor)
                    checkDistance += executor.GetRange();

                if(distance < nearestDistance && distance <= checkDistance) {
                    nearestDistance = distance;
                    nearestEntity = playerEntity;

                    return nearestEntity;
                }
            }

            return null;
        }

        public void SpawnXP(uint amount, Vector3 position) {
            position.y = 0;

            var poolUser = xpRingPool.Spawn(position, Quaternion.identity);
            var xpRing = poolUser.GetComponent<XPRing>();

            xpRing.SetPosition(position);
            xpRing.XP = amount;
        }

        public void SpawnRandomXP(uint amount) {
            var circle = Random.insideUnitCircle * GameManager.Current.ArenaRadius;

            SpawnXP(amount, new Vector3(circle.x, 0, circle.y));
        }
        public void SpawnRandomXP(int amount) {
            SpawnRandomXP((uint)amount);
        }
    }
}