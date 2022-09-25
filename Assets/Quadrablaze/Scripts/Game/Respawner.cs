using System.Collections;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class Respawner : MonoBehaviour {

        public PoolManager poolManager;
        public int prefabIndex;
        public GamePlayerInfo playerInfo;
        public bool isPlayer;

        void DoRespawn(RespawnData respawnData) {
            Spawn(respawnData.position);
            Exit();
        }

        public void Exit() {
            Destroy(gameObject);
        }

        public void Respawn(RespawnData respawnData) {
            if(respawnData.time <= 0)
                DoRespawn(respawnData);
            else
                StartCoroutine(RespawnCoroutine(respawnData));
        }

        IEnumerator RespawnCoroutine(RespawnData respawnData) {
            yield return new WaitForSeconds(respawnData.time);

            if(RoundManager.RoundInProgress)
                DoRespawn(respawnData);
        }

        void Spawn(Vector3 position) {
            if(isPlayer) {
                var gameObject = PlayerSpawnManager.Current.SpawnPlayerGameObject(playerInfo.connectionToClient, playerInfo, position);
                //var playerActor = gameObject.GetComponent<PlayerActor>();

                //playerActor.HasRespawned = true;
                //playerActor.TargetRpc_StartPlayerRespawned(playerInfo.connectionToClient);

                //TODO: Get rid of the Respawner

                //playerActor.OnRespawn.Invoke();
            }
            else {
                var user = poolManager.Spawn(prefabIndex, position, Quaternion.identity);

                if(user) {
                    //var actor = user.GetComponent<Actor>();

                    //actor.HasRespawned = true;
                    //actor.OnRespawn.Invoke();
                }
            }
        }

        public static Respawner Create(GamePlayerInfo playerInfo, PoolManager poolManager, int prefabIndex, float respawnDelay, Vector3 position) {
            var gameObject = new GameObject("Respawner", typeof(Respawner));
            var respawner = gameObject.GetComponent<Respawner>();

            respawner.poolManager = poolManager;
            respawner.prefabIndex = prefabIndex;
            respawner.playerInfo = playerInfo;
            respawner.isPlayer = playerInfo != null;

            respawner.Respawn(new RespawnData(respawnDelay, position));

            Debug.Log("Respawner Create " + (playerInfo != null), gameObject);

            return respawner;
        }

        public struct RespawnData {
            public float time;
            public Vector3 position;

            public RespawnData(float time, Vector3 position) {
                this.time = time;
                this.position = position;
            }
        }
    }
}