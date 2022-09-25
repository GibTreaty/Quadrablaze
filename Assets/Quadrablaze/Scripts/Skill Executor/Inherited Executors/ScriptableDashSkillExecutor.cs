using System;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Quadrablaze.SkillExecutors {
    [CreateAssetMenu(fileName = "Dash Skill", menuName = "Skill Layout/Executors/Dash")]
    public class ScriptableDashSkillExecutor : ScriptableSkillExecutor {

        [SerializeField]
        string _graphicsRadiusTransformPath;

        [SerializeField]
        float _graphicsRadius = 1;

        [SerializeField]
        float _speed = 15;

        [SerializeField]
        float _accelerationMultiplierChange = 1;

        [SerializeField]
        DashDirectionType _dashDirection = DashDirectionType.TransformForward;

        [SerializeField]
        float _dashDuration = 1;

        [SerializeField]
        float _cooldownDuration = 1;

        [SerializeField]
        float _teleportDistance = 1;

        [SerializeField]
        LayerMask _teleportMask = -1;

        [SerializeField]
        int _maxDashes;

        public float AccelerationMultiplierChange => _accelerationMultiplierChange;
        public float CooldownDuration => _cooldownDuration;
        public DashDirectionType DashDirection => _dashDirection;
        public float DashDuration => _dashDuration;
        public float GraphicsRadius => _graphicsRadius;
        public string GraphicsRadiusTransformPath => _graphicsRadiusTransformPath;
        public int MaxDashes => _maxDashes;
        public override bool Passive => false;
        public float Speed => _speed;
        public float TeleportDistance => _teleportDistance;
        public LayerMask TeleportMask => _teleportMask;

        public override SkillExecutor CreateInstance(ActorEntity actorEntity, SkillLayoutElement element) {
            return new Dash(actorEntity, element);
        }

        public enum DashDirectionType {
            Direction = 1,
            TransformForward = 2,
            Velocity = 3
        }
    }
}