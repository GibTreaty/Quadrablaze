using Quadrablaze;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities {
    public class Entity {

        public static readonly ProxyListener<ProxyAction> Proxy = new ProxyListener<ProxyAction>();

        public static List<Entity> Entities { get; set; } = new List<Entity>();

        protected GameObject _currentGameObject;

        public event Action<GameObject> OnGameObjectSet;
        public event Action OnDestroyed;

        public object DeathReason { get; protected set; }
        public GameObject CurrentGameObject => _currentGameObject;
        public Transform CurrentTransform => _currentGameObject?.transform;
        public uint Id { get; set; }
        public ProxyListener<ProxyAction> Listener { get; private set; } = new ProxyListener<ProxyAction>();
        public GameObject OriginalGameObject { get; protected set; }
        public Rigidbody RigidbodyComponent { get; protected set; }

        public Entity(uint id, GameObject gameObject) {
            Id = id;
            OriginalGameObject = gameObject;
            Entities.Add(this);

            Listener.OnListenEvent += Proxy.RaiseEvent;
            Listener.RaiseEvent(EntityActions.Created, this.ToArgs());
        }

        public virtual GameObject CreateGameObject(Vector3 position = default, Quaternion rotation = default, Transform parent = null) {
            if(_currentGameObject == null) {
                _currentGameObject = GameObject.Instantiate(OriginalGameObject, position, rotation, parent);
                SetGameObject(_currentGameObject);
            }

            return _currentGameObject;
        }

        protected void InvokeDestroyEvents() {
            OnDestroyed?.Invoke();
            Entities.Remove(this);
        }

        public virtual void DestroyEntity() {
            DestroyGameObject();
            InvokeDestroyEvents();
            Listener.OnListenEvent -= Proxy.RaiseEvent;
        }

        public virtual void DestroyGameObject() {
            UnityEngine.Object.Destroy(CurrentGameObject);
            _currentGameObject = null;
        }

        protected virtual void OnDisable() {
            Listener.OnListenEvent -= Proxy.RaiseEvent;
        }

        protected virtual void OnEnable() {
            Listener.OnListenEvent += Proxy.RaiseEvent;
        }

        protected void RaiseGameObjectSetEvent(GameObject gameObject) {
            OnGameObjectSet?.Invoke(gameObject);
        }

        public virtual void SetGameObject(GameObject gameObject) {
            _currentGameObject = gameObject;

            RigidbodyComponent = _currentGameObject?.GetComponent<Rigidbody>();

            RaiseGameObjectSetEvent(_currentGameObject);
        }
    }

    public class EntityArgs : EventArgs {

        public uint Id { get; }

        public EntityArgs(uint id) {
            Id = id;
        }
        public EntityArgs(Entity entity) : this(entity.Id) { }
    }

    public static class EntityUtility {
        public static EntityArgs ToArgs(this Entity entity) {
            return new EntityArgs(entity);
        }

        public static Entity GetEntity(this EntityArgs args) {
            return GameManager.Current.GetEntity(args.Id);
        }

        public static T GetEntity<T>(this EntityArgs args) where T : Entity {
            return GameManager.Current.GetEntity(args.Id) as T;
        }
    }
}

public static partial class EntityActions {
    public static readonly ProxyAction Created = new ProxyAction();
    public static readonly ProxyAction Destroyed = new ProxyAction();
}