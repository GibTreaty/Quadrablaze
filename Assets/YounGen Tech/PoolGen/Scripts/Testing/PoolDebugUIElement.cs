using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace YounGenTech.PoolGen.Debug {
    [AddComponentMenu("YounGen Tech/PoolGen/Debug/Pool Debug UI Element")]
    public class PoolDebugUIElement : MonoBehaviour {

        public Text originalTextObject;
        public Text poolNameText;

        public Button spawnButton;
        public bool showSpawnButton;

        public Button despawnAllButton;
        public bool showDespawnAllButton;

        [SerializeField]
        PoolManager _pool;

        Dictionary<ObjectPool, Text> createdTextObjects;

        public PoolManager Pool {
            get { return _pool; }
            set {
                _pool = value;

                enabled = Pool;

                if(Pool) {
                    poolNameText.text = Pool.PoolName;
                    spawnButton.onClick.AddListener(() => Pool.Spawn());
                    despawnAllButton.onClick.AddListener(() => Pool.DespawnAll());
                }
            }
        }

        void Awake() {
            createdTextObjects = new Dictionary<ObjectPool, Text>();
        }

        void Update() {
            if(!Pool) {
                enabled = false;
                return;
            }

            UpdateText();
        }

        public void CreateText() {
            foreach(var value in Pool.ObjectContainers)
                if(!createdTextObjects.ContainsKey(value.Value)) {
                    var gameObject = Instantiate(originalTextObject.gameObject, transform.parent, false);
                    var text = gameObject.GetComponent<Text>();

                    gameObject.SetActive(true);
                    createdTextObjects[value.Value] = text;
                }
        }

        public void UpdateText() {
            if(NetworkManager.singleton && !NetworkServer.active) {
                if(spawnButton.gameObject.activeSelf)
                    spawnButton.gameObject.SetActive(false);
                
                if(despawnAllButton.gameObject.activeSelf)
                    despawnAllButton.gameObject.SetActive(false);
            }
            else {
                if(spawnButton.gameObject.activeSelf != showSpawnButton)
                    spawnButton.gameObject.SetActive(showSpawnButton);

                if(despawnAllButton.gameObject.activeSelf != showDespawnAllButton)
                    despawnAllButton.gameObject.SetActive(showDespawnAllButton);
            }

            foreach(var value in Pool.ObjectContainers)
                createdTextObjects[value.Value].text = "    " + value.Value.ToString();
        }
    }
}