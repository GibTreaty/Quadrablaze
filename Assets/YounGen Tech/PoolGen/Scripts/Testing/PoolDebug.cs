using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.PoolGen.Debug {
    [AddComponentMenu("YounGen Tech/PoolGen/Debug/Pool Debug")]
    /// <summary>Displays a few useful object pool related things on screen using OnGUI</summary>
    public class PoolDebug : MonoBehaviour {
        public List<DebugInfo> debugPoolManagers = new List<DebugInfo>();

        void OnGUI() {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                for(int i = 0; i < debugPoolManagers.Count; i++) {
                    GUILayout.Label(debugPoolManagers[i].poolManager.PoolName);

                    foreach(var container in debugPoolManagers[i].poolManager.ObjectContainers.Values)
                        if(container.PoolPrefab.Prefab) {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayoutOption[] buttonOptions = new GUILayoutOption[] {
#if UNITY_ANDROID
                                    GUILayout.Width(100),
                                    GUILayout.Height(100)
#else
                                    GUILayout.Width(100),
                                    GUILayout.Height(50)
#endif
                                };

                                if(debugPoolManagers[i].showSpawnButton)
                                    if(GUILayout.Button("Spawn", buttonOptions))
                                        container.Spawn();

                                if(debugPoolManagers[i].showDespawnAllButton)
                                    if(GUILayout.Button("Despawn All", buttonOptions))
                                        container.DespawnAll();

                                GUILayout.Label(container.ToString());
                            }
                            GUILayout.EndHorizontal();
                        }

                    GUILayout.Space(10);
                }
            }
            GUILayout.EndVertical();
        }

        [System.Serializable]
        public struct DebugInfo {
            public PoolManager poolManager;
            public bool showSpawnButton;
            public bool showDespawnAllButton;
        }
    }
}