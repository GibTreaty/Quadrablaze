using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze.Skills {
    public class WeaponDisplayNames : ScriptableObject {

        public static WeaponDisplayNames Current { get; private set; }

        public List<string> displayNames = new List<string>();

        void OnEnable() {
            //if(Current && Current != this) DestroyImmediate(this, true);

            Current = this;
        }

        public void UpdateDisplayNames() {
            foreach(WeaponProperties weaponProperty in WeaponProperties.instances)
                if(weaponProperty != null)
                    for(int i = 0; i < weaponProperty.Count; i++) {
                        WeaponProperties.Properties property = weaponProperty.GetWeaponProperty(i);
                        string displayName = property.DisplayName.Trim();

                        if(!string.IsNullOrEmpty(displayName) && !displayNames.Contains(displayName))
                            displayNames.Add(displayName);
                    }

            displayNames.Sort();
        }
    }
}