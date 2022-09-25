using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Quadrablaze {
    [System.Serializable]
    public class ShipPresetList {

        //[SerializeField]
        List<ShipPreset> _presetList = new List<ShipPreset>();

        #region Properties
        [XmlArray("PresetList")]
        public List<ShipPreset> PresetList {
            get { return _presetList; }
        }
        #endregion

        public ShipPresetList() {
            _presetList = new List<ShipPreset>();
        }
    }
}