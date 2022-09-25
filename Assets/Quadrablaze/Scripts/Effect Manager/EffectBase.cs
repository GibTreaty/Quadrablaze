using System;
using UnityEngine;

namespace Quadrablaze {
    public abstract class EffectBase : ScriptableObject {

        [SerializeField]
        GameObject _spawnEffectObject;

        protected GameObject CreateObject(Transform parent) {
            if(_spawnEffectObject != null)
                return Instantiate(_spawnEffectObject, parent, false);

            return null;
        }

        public virtual GameObject Play(string id, Transform parent, Action<object, string[]> callback = null, params string[] parameters) {
            var output = CreateObject(parent);

            if(output != null)
                callback?.Invoke(output, parameters);

            return output;
        }
    }
}