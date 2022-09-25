using System.Collections.Generic;
using UnityEngine;
using YounGenTech.Entities.Weapon;

namespace Quadrablaze.WeaponSystem {
    public static class WeaponDetails {

        public static List<(string, string)> GenerateWeaponDetails(this ScriptableWeapon weapon) {
            List<(string, string)> list = new List<(string, string)>();

            weapon.Properties.GenerateWeaponDetails(list);

            return list;
        }

        static void GenerateWeaponDetails(this WeaponProperties weaponProperties, List<(string, string)> details) {
            details.Add(("Projectiles", weaponProperties.projectilesPerShot.ToString()));
            details.Add(("Fire Rate", weaponProperties.shotsPerSecond.ToString()));

            foreach(var module in weaponProperties.originalProjectile.Modules) {
                switch(module) {
                    case ScriptableDamageModule info: { details.Add(("Damage", info.DamageAmount.ToString())); break; }
                    case ScriptableHomingModule info: { details.Add(("Homing", "*")); break; }
                }
            }
        }
    }
}