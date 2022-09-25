using System.Collections.Generic;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Networking;
using YounGenTech.PoolGen;

namespace Quadrablaze.SkillExecutors { //TODO: Make the turret shoot
    [CreateAssetMenu(fileName = "Turret Skill", menuName = "Skill Layout/Executors/Turret")]
    public class ScriptableTurretSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        ScriptableTurretEntity _originalTurretEntity;

        [SerializeField]
        string _poolManagerName;

        [SerializeField]
        string _poolPrefabName;

        [SerializeField]
        float _deployedDuration = 12;

        [SerializeField]
        float _cooldownDuration = 12;

        [SerializeField]
        float _deployPushForce = 1;

        [SerializeField]
        Vector3 _deployPushDirection;

        int deployedLevel;

        List<TurretEntity> turretEntities;

        public float CooldownDuration => _cooldownDuration;
        public float DeployedDuration => _deployedDuration;
        public float DeployPushForce => _deployPushForce;
        public ScriptableTurretEntity OriginalTurretEntity => _originalTurretEntity;
        public override bool Passive => false;
        public string PoolManagerName => _poolManagerName;
        public string PoolPrefabName => _poolPrefabName;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Turret(actorEntity, element);
        }
    }
}