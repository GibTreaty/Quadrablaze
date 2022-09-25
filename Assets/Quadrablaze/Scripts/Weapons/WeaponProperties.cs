using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Quadrablaze.Skills {
    public class WeaponProperties : MonoBehaviour {

        public static List<WeaponProperties> instances = new List<WeaponProperties>();

        [SerializeField]
        List<Properties> _weaponProperties = new List<Properties>();

        public int Count {
            get { return _weaponProperties.Count; }
        }

        static WeaponProperties() {
            instances.Clear();
        }

        public WeaponProperties() {
            instances.Add(this);
        }

        ~WeaponProperties() {
            instances.Remove(this);
        }

        public T GetValueFromDisplayName<T>(string displayName) {
            T value = default(T);

            for(int i = 0; i < _weaponProperties.Count; i++)
                if(_weaponProperties[i].DisplayName == displayName)
                    value = (T)System.Convert.ChangeType(_weaponProperties[i].Value, typeof(T));

            return value;
        }

        public string GetValue(int index) {
            return _weaponProperties[index].Value;
        }

        public Properties GetWeaponProperty(int index) {
            return _weaponProperties[index];
        }

        [System.Serializable]
        public struct Properties {

            [SerializeField]
            string _displayName;

            [SerializeField]
            Object _target;

            [SerializeField]
            string _propertyName;

            #region Properties
            public string DisplayName {
                get { return _displayName; }
                set { _displayName = value; }
            }

            public string PropertyName {
                get { return _propertyName; }
            }

            public Object Target {
                get { return _target; }
            }

            public string Value {
                get {
                    PropertyInfo property = _target.GetType().GetProperty(_propertyName);

                    return property != null ? property.GetValue(_target, null).ToString() : "";
                }
            }
            #endregion
        }
    }
}