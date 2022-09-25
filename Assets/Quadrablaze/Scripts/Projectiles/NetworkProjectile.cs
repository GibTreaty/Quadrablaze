using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Quadrablaze {
    public class NetworkProjectile : NetworkBehaviour {

        [SerializeField]
        float _velocityThreshold = .1f;

        Movement _movementData = new Movement();
        NetworkInstanceId _ownerID;
        Rigidbody _rigidbodyComponent = null;

        #region Properties
        public NetworkInstanceId OwnerID {
            get { return _ownerID; }
            set { _ownerID = value; }
        }

        public Rigidbody RigidbodyComponent {
            get {
                if(!_rigidbodyComponent) _rigidbodyComponent = GetComponent<Rigidbody>();

                if(!_rigidbodyComponent) {
                    enabled = false;
                    Debug.LogError("No Rigidbody found for Network Projectile", gameObject);
                }

                return _rigidbodyComponent;
            }
        }
        #endregion

        void OnDisable() {
            OwnerID = NetworkInstanceId.Invalid;
            GetComponent<Owner>().OwnerObject = null;
        }

        public override void OnStartClient() {
            if(!RigidbodyComponent) {
                enabled = false;
                Debug.Log("NetworkProjectile: No Rigidbody found", this);
            }

            System.Array.ForEach(GetComponentsInChildren<Collider>(true), collider => collider.enabled = hasAuthority);

            if(NetworkServer.active)
                StartCoroutine(DelayedCall(SetStartValues));
            //SetValues();
        }

        IEnumerator DelayedCall(System.Action call) {
            yield return new WaitForEndOfFrame();
            call();
        }

        void OnMovementSet(Movement movement) {
            _movementData = movement;

            GetComponentInChildren<TrailRenderer>()?.Clear();

            transform.position = movement.position;
            RigidbodyComponent.velocity = movement.velocity;
        }

        void OnOwnerIDSet(NetworkInstanceId netId) {
            OwnerID = netId;

            var owner = GetComponent<Owner>();

            owner.OwnerObject = ClientScene.FindLocalObject(OwnerID);

            // TODO Non-host Clients should ignore projectiles by default
            //if(owner.OwnerObject) {
            //    var actor = owner.OwnerObject.GetComponent<Actor>();

            //    if(actor.ActorType == ActorTypes.Player) {
            //        foreach(var player in PlayerProxy.Players)

            //            //player.WeaponControllerComponent.NetworkProjectileIgnore(this);
            //    }
            //    else {
            //        //actor.WeaponControllerComponent.NetworkProjectileIgnore(this);
            //    }
            //}
        }

        [ClientRpc]
        public void Rpc_SetOwnerId(NetworkInstanceId netId) {
            OnOwnerIDSet(netId);
        }

        void SetStartValues() {
            _movementData = new Movement() { position = RigidbodyComponent.position, velocity = RigidbodyComponent.velocity };

            foreach(var connection in QuadrablazeSteamNetworking.Current.SteamConnections)
                if(connection.connectionId != 0) {
                    Target_SetRotation(connection, RigidbodyComponent.rotation.eulerAngles.y);
                    Target_SetValues(connection, _movementData);
                }
        }

        public void SetValues() {
            SetValues(RigidbodyComponent.position, RigidbodyComponent.velocity);
        }

        [Server]
        void SetValues(Vector3 position, Vector3 velocity) {
            _movementData = new Movement() { position = position, velocity = velocity };

            foreach(var connection in QuadrablazeSteamNetworking.Current.SteamConnections)
                if(connection.connectionId != 0)
                    Target_SetValues(connection, _movementData);
        }

        [TargetRpc]
        void Target_SetValues(NetworkConnection connection, Movement movement) {
            OnMovementSet(movement);
        }

        [TargetRpc]
        void Target_SetRotation(NetworkConnection connection, float angle) {
            var angles = transform.eulerAngles;
            angles.y = angle;
            transform.eulerAngles = angles;
        }

        [ServerCallback]
        void Update() {
            if(RigidbodyComponent.velocity.magnitude - _movementData.velocity.magnitude >= _velocityThreshold)
                SetValues();
        }

        [System.Serializable]
        public struct Movement {
            public Vector3 position;
            public Vector3 velocity;
        }
    }
}