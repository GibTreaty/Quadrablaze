using UnityEngine;
using UnityEngine.Serialization;
using YounGenTech.PoolGen;

// TODO: Remove WeaponDamage script
namespace Quadrablaze {
    public class WeaponDamage : MonoBehaviour, IDamageMultiplier {

        [field: SerializeField, FormerlySerializedAs("_damage")]
        public int Damage { get; set; } = 1;

        [field: SerializeField, FormerlySerializedAs("_damageMultiplier")]
        public float DamageMultiplier { get; set; } = 1;

        public void SetDamageOnObject(GameObject gameObject) {
            var projectile = GetComponent<ProjectileBase>();

            if(projectile) projectile.Damage = Mathf.FloorToInt(Damage * DamageMultiplier);
        }

        public void SetDamageOnObject(PoolManager poolManager, GameObject gameObject) {
            SetDamageOnObject(gameObject);
        }
    }
}