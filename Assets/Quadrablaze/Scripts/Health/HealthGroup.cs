using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using YounGenTech;
using YounGenTech.Entities;
using Quadrablaze.Entities;

namespace Quadrablaze {
    public class HealthGroup : MonoBehaviour, IActorEntityObjectInitialize {

        [SerializeField]
        bool _setupOnAwake = true;

        [SerializeField]
        HealthLayer _mainHealthLayer;

        [SerializeField]
        HealthTier[] _healthTiers;

        public UnityEvent OnDamaged;

        HealthLayer[][] _healthLayers;
        string[] _tierNames;

        bool layersInitialized;
        //SortedList<int, List<HealthLayer>> sortedHealthLayers = null;

        #region Properties
        public uint EntityId { get; private set; }
        public HealthLayer MainHealthLayer => _mainHealthLayer;
        public bool SetupOnAwake => _setupOnAwake;
        public int TierCount => _healthLayers.Length;
        #endregion

        public void ActorEntityObjectInitialize(ActorEntity entity) {
            EntityId = entity.Id;
        }

        void Awake() {
            if(SetupOnAwake) BuildLayerArray();
        }

        public void BuildLayerArray() {
            if(layersInitialized) return;

            BuildLayerArray(_healthTiers);

            _healthTiers = null;
        }
        public void BuildLayerArray(HealthTier[] healthTiers) {
            var orderedArray = healthTiers.OrderBy(s => s.Order).ToArray();

            _healthLayers = new HealthLayer[orderedArray.Length][];
            _tierNames = new string[orderedArray.Length];

            for(int i = 0; i < orderedArray.Length; i++) {
                _healthLayers[i] = orderedArray[i].HealthLayers;
                _tierNames[i] = orderedArray[i].Name;

                for(int l = 0; l < orderedArray[i].HealthLayers.Length; l++)
                    orderedArray[i].HealthLayers[l].Group = this;
            }

            if(!_mainHealthLayer)
                _mainHealthLayer = _healthLayers.Last().Last();

            layersInitialized = true;
        }

        public bool ContainsLayer(int tierIndex, HealthLayer layer) {
            return _healthLayers[tierIndex].Contains(layer);
        }
        public bool ContainsLayer(HealthLayer layer) {
            return _healthLayers.Any(s => s.Contains(layer));
        }

        //public bool ContainsLayer(HealthLayer layer) {
        //    foreach(var list in sortedHealthLayers.Values)
        //        if(list.Contains(layer)) return true;

        //    return false;
        //}

        //public bool ContainsLayer(HealthLayer layer) {
        //    for(int i = 0; i < _healthTiers.Length; i++)
        //        if(_healthTiers[i].Contains(layer)) return true;

        //    return false;
        //}

        /// <summary>
        /// Damage bottom/outside layers before damaging the inputted layer.
        /// </summary>
        public bool Damage(HealthLayer targetLayer, HealthEvent healthEvent) {
            var entity = GameManager.Current.GetActorEntity(EntityId);

            if(entity != null)
                if(!entity.IsInvincible)
                    if(healthEvent.targetGameObject) {
                        int layerTier = GetTier(targetLayer);

                        for(int i = 0; i < TierCount && i <= layerTier; i++)
                            if(i == layerTier) {
                                healthEvent.targetGameObject = targetLayer.gameObject;

                                return targetLayer.HealthStatusAlive() ? targetLayer.Damage(healthEvent) : false;
                            }
                            else {
                                for(int n = 0; n < _healthLayers[i].Length; n++) {
                                    var healthLayer = _healthLayers[i][n];

                                    if(healthLayer.HealthStatusAlive()) {
                                        healthEvent.targetGameObject = healthLayer.gameObject;

                                        if(healthLayer.Damage(healthEvent))
                                            return true;
                                    }
                                }
                            }
                    }

            return false;
        }
        public bool Damage(int tierIndex, HealthEvent healthEvent) {
            var entity = GameManager.Current.GetActorEntity(EntityId);

            if(!entity.IsInvincible)
                for(int n = 0; n < _healthLayers[tierIndex].Length; n++) {
                    var healthLayer = _healthLayers[tierIndex][n];

                    if(healthLayer.HealthStatusAlive()) {
                        healthEvent.targetGameObject = healthLayer.gameObject;

                        if(healthLayer.Damage(healthEvent))
                            return true;
                    }
                }

            return false;
        }

        public HealthLayer GetHealthLayer(int tierIndex, int layerIndex) {
            return _healthLayers[tierIndex][layerIndex];
        }

        public (int tierIndex, int layerIndex) GetHealthLayerIndex(HealthLayer layer) {
            var data = (-1, -1);

            for(int tier = 0; tier < TierCount; tier++)
                for(int index = 0; index < _healthLayers[tier].Length; index++) {
                    if(_healthLayers[tier][index] == layer) {
                        data = (tier, index);
                        break;
                    }
                }

            return data;
        }

        public int GetTier(HealthLayer layer) {
            for(int i = 0; i < TierCount; i++)
                if(_healthLayers[i].Contains(layer)) return i;

            return -1;
        }
        public int GetTier(string name) {
            for(int i = 0; i < _tierNames.Length; i++)
                if(_tierNames[i] == name) return i;

            return -1;
        }

        public HealthLayer[] GetLayers(int tier) {
            return _healthLayers[tier];
        }

        //public void Sort() {
        //    _healthTiers = _healthTiers.OrderBy(s => { return s.Order; }).ToArray();
        //}

        #region Old Code
        /*
        public bool GetLayerList(int layer, out List<HealthLayer> list) {
            return sortedHealthLayers.TryGetValue(layer, out list);
        }

        public bool GetHighestLayerList(out HealthLayerData list) {
            list = new HealthLayerData(sortedHealthLayers.Values[sortedHealthLayers.Count - 1], sortedHealthLayers.Keys[sortedHealthLayers.Count - 1]);

            return list.healthLayers != null;
        }

        public bool GetLowestLayerList(out HealthLayerData list) {
            list = new HealthLayerData(sortedHealthLayers.Values[0], sortedHealthLayers.Keys[0]);

            return list.healthLayers != null;
        }

        public bool GetLowestLiveLayerList(out HealthLayerData list, int maxLayer = int.MaxValue) {
            foreach(var layers in sortedHealthLayers)
                if(layers.Key <= maxLayer)
                    foreach(var layer in layers.Value)
                        if(layer.HealthStatusAlive()) {
                            list = new HealthLayerData(layers.Value, layer.Order);
                            return true;
                        }

            list = HealthLayerData.Empty;

            return list.healthLayers != null;
        }
        public bool GetLowestLiveLayerList(out HealthLayerData list, int startLayer, int maxLayer = int.MaxValue) {
            foreach(var layers in sortedHealthLayers)
                if(layers.Key >= startLayer && layers.Key <= maxLayer)
                    foreach(var layer in layers.Value)
                        if(layer.HealthStatusAlive()) {
                            list = new HealthLayerData(layers.Value, layer.Order);
                            return true;
                        }

            list = HealthLayerData.Empty;

            return list.healthLayers != null;
        }

        public bool GetNextLayerList(out HealthLayerData list, int layer) {
            layer = sortedHealthLayers.IndexOfKey(layer);

            if(layer > -1)
                if(++layer < sortedHealthLayers.Count) {
                    list = new HealthLayerData(sortedHealthLayers.Values[layer], sortedHealthLayers.Keys[layer]);
                    return true;
                }

            list = HealthLayerData.Empty;

            return false;
        }

        public bool GetPreviousLayerList(out HealthLayerData list, int layer) {
            layer = sortedHealthLayers.IndexOfKey(layer);

            if(layer < sortedHealthLayers.Count)
                if(--layer > sortedHealthLayers.Count) {
                    list = new HealthLayerData(sortedHealthLayers.Values[layer], sortedHealthLayers.Keys[layer]);
                    return true;
                }

            list = HealthLayerData.Empty;

            return false;
        }

        public void RebuildHealthLayers() {
            return;

            if(sortedHealthLayers == null) sortedHealthLayers = new SortedList<int, List<HealthLayer>>();
            else sortedHealthLayers.Clear();

            foreach(var layer in GetComponentsInChildren<HealthLayer>(true))
                if(sortedHealthLayers.ContainsKey(layer.Order))
                    sortedHealthLayers[layer.Order].Add(layer);
                else
                    sortedHealthLayers[layer.Order] = new List<HealthLayer>() { layer };
        }
        */
        #endregion

        public struct HealthLayerData {
            public static HealthLayerData Empty {
                get { return new HealthLayerData(null, 0); }
            }

            public List<HealthLayer> healthLayers;
            public int order;

            public HealthLayerData(List<HealthLayer> healthLayers, int order) {
                this.healthLayers = healthLayers;
                this.order = order;
            }
        }

        [System.Serializable]
        public class HealthTier {

            [SerializeField]
            int _order;

            [SerializeField]
            string _name;

            [SerializeField]
            HealthLayer[] _healthLayers;

            #region Properties
            public HealthLayer[] HealthLayers {
                get { return _healthLayers; }
                set { _healthLayers = value; }
            }

            public int Order {
                get { return _order; }
                set { _order = value; }
            }

            public string Name {
                get { return _name; }
                set { _name = value; }
            }
            #endregion

            public HealthTier(int order, string name, HealthLayer[] healthLayers) {
                HealthLayers = healthLayers;
                Order = order;
                Name = name;
            }

            //public bool Contains(HealthLayer layer) {
            //    return HealthLayers.Contains(layer);
            //}

            //public HealthLayer GetLiveLayer() {
            //    return HealthLayers.First(layer => layer.HealthStatusAlive());
            //}
        }
    }
}