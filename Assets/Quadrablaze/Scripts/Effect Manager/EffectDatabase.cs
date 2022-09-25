using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quadrablaze {
    [CreateAssetMenu(menuName = "Effect Database/Effect Database")]
    public class EffectDatabase : ScriptableObject {

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("_soundList")]
        List<EffectInfo> _effectList;

        [SerializeField]
        Dictionary<uint, GameObject> _spawnedEffects;

        public uint GetUniqueId() {
            uint currentId = 1;
            var ids = _spawnedEffects.Keys;

            foreach(var id in ids)
                if(currentId == id)
                    currentId++;
                else break;

            return currentId;
        }

        public GameObject Play(string id, Transform parent, Action<object, string[]> callback = null, params string[] parameters) {
            var info = _effectList.FirstOrDefault(s => s.id == id);
            GameObject output = null;

            if(info.effect != null)
                output = info.effect.Play(id, parent, callback, parameters);

            return output;
        }

        [System.Serializable]
        public struct EffectInfo {
            public string id;
            public EffectBase effect;
        }
    }
}