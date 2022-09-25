using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities.Weapon {
    public class GameEntity : Entity {

        static HashSet<GameEntity> GameEntities { get; set; }
        static bool GameEntitiesInitialized { get; set; }

        public Vector3 Position {
            get {
                return CurrentGameObject != null ?
                    CurrentTransform.position :
                    lastKnownPosition;
            }

            set {
                if(CurrentGameObject != null)
                    CurrentTransform.position = value;

                lastKnownPosition = value;
            }
        }


        public string Tag { get; set; }

        Vector3 lastKnownPosition { get; set; }

        public GameEntity(uint id, GameObject gameObject) : base(id, gameObject) {
            InitializeGameEntities();
            GameEntities.Add(this);
        }

        public override GameObject CreateGameObject(Vector3 position = default, Quaternion rotation = default, Transform parent = null) {
            Position = position;

            return base.CreateGameObject(position, rotation, parent);
        }

        public override void DestroyEntity() {
            GameEntities.Remove(this);
            base.DestroyEntity();
        }

        public static GameEntity FindWithTag(string tag) {
            foreach(var entity in GameEntities)
                if(entity.Tag == tag)
                    return entity;

            return null;
        }

        static void InitializeGameEntities() {
            if(GameEntitiesInitialized) return;

            GameEntities = new HashSet<GameEntity>();
            GameEntitiesInitialized = true;
        }
    }
}