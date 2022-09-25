using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class GameManager : MonoBehaviour {

        public static GameManager Current { get; private set; }

        [SerializeField]
        ScriptableShooterPlayerEntity _originalPlayer;

        [SerializeField]
        ScriptableGameEntity _target;

        [SerializeField]
        Transform _targetPivot;

        public ShooterPlayerEntity CurrentPlayer { get; private set; }

        public GameEntity CurrentTarget { get; private set; }

        public HashSet<Entity> Entities { get; private set; }

        public HashSet<IEntityUpdate> UpdateEntities { get; private set; }

        System.Action<Entity> entityCreatedMethod;
        System.Action<Entity> entityDestroyedMethod;

        bool initialized;

        void Start() {
            CurrentPlayer = _originalPlayer.CreateInstance() as ShooterPlayerEntity;
            CurrentPlayer.CreateGameObject();

            CurrentTarget = _target.CreateInstance() as GameEntity;
            CurrentTarget.CreateGameObject(new Vector3(2, 0, 0), Quaternion.identity, _targetPivot);

            initialized = true;
        }

        void DeinitializeEntityEvents() {
            //Entity.Proxy.Subscribe(EntityActions.Created, 
            //Entity.OnEntityCreated -= entityCreatedMethod;
            //Entity.OnEntityDestroyed -= entityDestroyedMethod;

            entityCreatedMethod = null;
            entityDestroyedMethod = null;
        }

        void InitializeEntityEvents() {
            entityCreatedMethod = OnEntityCreated;
            entityDestroyedMethod = OnEntityDestroyed;

            //Entity.OnEntityCreated += entityCreatedMethod;
            //Entity.OnEntityDestroyed += entityDestroyedMethod;
        }

        void OnDisable() {
            DeinitializeEntityEvents();
        }

        void OnEnable() {
            Current = this;
            Entities = new HashSet<Entity>();
            UpdateEntities = new HashSet<IEntityUpdate>();

            InitializeEntityEvents();
        }

        void OnEntityCreated(Entity entity) {
            Entities.Add(entity);

            if(entity is IEntityUpdate) UpdateEntities.Add(entity as IEntityUpdate);
        }

        void OnEntityDestroyed(Entity entity) {
            Entities.Remove(entity);

            if(entity is IEntityUpdate) UpdateEntities.Remove(entity as IEntityUpdate);
        }

        /*void OnGUI() {
            if(CurrentPlayer != null)
                if(CurrentPlayer.CurrentWeapon != null) {
                    GUILayout.Label($"Shots Per Second: {CurrentPlayer.CurrentWeapon.ShotsPerSecond}");
                    CurrentPlayer.CurrentWeapon.ShotsPerSecond = Mathf.RoundToInt(GUILayout.HorizontalSlider(CurrentPlayer.CurrentWeapon.ShotsPerSecond, 0, 50, GUILayout.ExpandWidth(false), GUILayout.Width(300)));

                    GUILayout.Label($"Projectiles Per Shot: {CurrentPlayer.CurrentWeapon.ProjectilesPerShot}");
                    CurrentPlayer.CurrentWeapon.ProjectilesPerShot = Mathf.RoundToInt(GUILayout.HorizontalSlider(CurrentPlayer.CurrentWeapon.ProjectilesPerShot, 0, 20, GUILayout.ExpandWidth(false), GUILayout.Width(300)));

                    GUILayout.Label($"Spread Type: {CurrentPlayer.CurrentWeapon.CurrentSpreadType}");
                    var spreadType = Mathf.RoundToInt(GUILayout.HorizontalSlider((int)CurrentPlayer.CurrentWeapon.CurrentSpreadType, 0, 3, GUILayout.ExpandWidth(false), GUILayout.Width(300)));

                    CurrentPlayer.CurrentWeapon.CurrentSpreadType = (SpreadType)System.Enum.Parse(typeof(SpreadType), spreadType.ToString());

                    GUILayout.Label($"Spread: {CurrentPlayer.CurrentWeapon.Spread}");
                    CurrentPlayer.CurrentWeapon.Spread = GUILayout.HorizontalSlider(CurrentPlayer.CurrentWeapon.Spread, 0, 20, GUILayout.ExpandWidth(false), GUILayout.Width(300));

                    GUILayout.Label($"Spread Speed: {CurrentPlayer.CurrentWeapon.SpreadSpeed}");
                    CurrentPlayer.CurrentWeapon.SpreadSpeed = GUILayout.HorizontalSlider(CurrentPlayer.CurrentWeapon.SpreadSpeed, 0, 50, GUILayout.ExpandWidth(false), GUILayout.Width(300));

                    GUILayout.Label($"Projectile Angular Velocity: {CurrentPlayer.CurrentWeapon.AngularVelocity}");
                    CurrentPlayer.CurrentWeapon.AngularVelocity = GUILayout.HorizontalSlider(CurrentPlayer.CurrentWeapon.AngularVelocity, -360, 360, GUILayout.ExpandWidth(false), GUILayout.Width(300));
                }
        }*/

        void Update() {
            if(!initialized) return;

            foreach(var entity in UpdateEntities.ToList())
                entity.EntityUpdate();
        }
    }
}