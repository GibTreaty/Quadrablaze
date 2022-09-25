using System.Collections.Generic;
using UnityEngine;

namespace Quadrablaze {
    public class ShipInfoObject : MonoBehaviour {

        [SerializeField]
        List<Transform> weaponPivots;

        #region Properties
        public List<Transform> WeaponPivots {
            get { return weaponPivots; }
            set { weaponPivots = value; }
        } 
        #endregion
    }
}