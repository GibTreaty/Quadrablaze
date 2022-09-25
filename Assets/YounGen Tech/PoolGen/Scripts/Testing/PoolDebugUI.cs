using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.PoolGen.Debug {
    [AddComponentMenu("YounGen Tech/PoolGen/Debug/Pool Debug UI")]
    public class PoolDebugUI : MonoBehaviour {

        public PoolDebugUIElement originalElement;
        public RectTransform elementContainer;
        public List<DebugInfo> poolManagers = new List<DebugInfo>();

        void Start() {
            for(int i = 0; i < poolManagers.Count; i++) {
                var gameObject = Instantiate(originalElement.gameObject, elementContainer, false);
                var elementUI = gameObject.GetComponent<PoolDebugUIElement>();
                
                elementUI.Pool = poolManagers[i].poolManager;
                gameObject.SetActive(true);
                elementUI.CreateText();

                elementUI.showSpawnButton = poolManagers[i].showSpawnButton;
                elementUI.showDespawnAllButton = poolManagers[i].showDespawnAllButton;
            }
        }

        [System.Serializable]
        public struct DebugInfo {
            public PoolManager poolManager;
            public bool showSpawnButton;
            public bool showDespawnAllButton;
        }
    }
}