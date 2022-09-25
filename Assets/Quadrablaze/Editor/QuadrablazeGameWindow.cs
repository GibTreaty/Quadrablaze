using UnityEngine;
using UnityEditor;
using System.Collections;
using Quadrablaze.Skills;
using System.Collections.Generic;
using UnityEngine.Networking;
using Quadrablaze.Entities;
using Quadrablaze.SkillExecutors;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Quadrablaze {
    public class QuadrablazeGameWindow : EditorWindow {

        Vector2 scrollWindow;
        HashSet<GamePlayerInfo> players = new HashSet<GamePlayerInfo>();
        static bool entitiesFoldout = true;

        [MenuItem("Window/Quadrablaze Game Window")]
        static void Init() {
            var window = GetWindow<QuadrablazeGameWindow>("Quadrablaze");

            window.autoRepaintOnSceneChange = true;
        }

        void OnDisable() {
            players.Clear();
        }

        void OnGUI() {
            scrollWindow = EditorGUILayout.BeginScrollView(scrollWindow);
            string label = "Game Version `" + Application.version + "`";

            if(Application.isPlaying) {
                GUILayout.Label(label);

                if(RoundManager.RoundInProgress) {
                    GUI.backgroundColor = Color.black;
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUI.backgroundColor = Color.white;
                    {
                        EditorGUILayout.LabelField("Players");

                        EditorGUI.indentLevel++;
                        foreach(var item in GamePlayerInfo.PlayerInfos) {
                            EditorGUILayout.LabelField($"Username: {item.Username}");
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.ObjectField(item, typeof(GamePlayerInfo), false, GUILayout.ExpandWidth(false));

                                if(NetworkServer.active) {
                                    var gameObject = item.AttachedEntity?.CurrentGameObject;

                                    if(gameObject != null) {
                                        if(GUILayout.Button($"Select ActorObject {item.Username}", GUILayout.ExpandWidth(false)))
                                            Selection.activeObject = gameObject;

                                        if(GUILayout.Button($"Kill {item.Username}", GUILayout.ExpandWidth(false)))
                                            item.AttachedEntity.Kill();
                                    }

                                    EditorGUILayout.Space();

                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                            if(item.AttachedEntity != null) {
                                EditorGUI.indentLevel++;
                                if(item.AttachedEntity.CurrentUpgradeSet != null) {
                                    item.AttachedEntity.CurrentUpgradeSet.Lives = EditorGUILayout.IntField("Lives", item.AttachedEntity.CurrentUpgradeSet.Lives);

                                    int abilityCount = 0;

                                    foreach(var element in item.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout.Elements)
                                        if(element.CurrentExecutor != null)
                                            abilityCount++;

                                    GUILayout.Label($"Purchased abilities: {abilityCount}");
                                    GUILayout.Space(10);

                                    if(item.AttachedEntity.CurrentUpgradeSet.CurrentSkillLayout.GetFirstAvailableExecutor<Shield>() is Shield executor) {
                                        GUILayout.Label("Shield");
                                        EditorGUI.indentLevel++;
                                        GUILayout.Label($"Regeneration Time: {executor.RegenerationTimer.CurrentTime}");
                                        GUILayout.Label($"Time till next regen: {executor.LastHealTime} : {(executor.LastHealTime + 1) - Time.time}");
                                        EditorGUILayout.ObjectField("Collider", executor.ShieldCollider, typeof(Collider), true);
                                        EditorGUI.indentLevel--;
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndVertical();
                }

                if(GameManager.Current.Entities != null && GameManager.Current.Entities.Count > 0) {

                    string detail = "";

                    if(GameManager.Current.CurrentGameMode is GameModes.MainGameMode gameMode)
                        detail = $" Minion Count({gameMode.CurrentEnemyController.MinionCount}) Queue Count({gameMode.CurrentEnemyController.QueueCount})";

                    GUI.backgroundColor = new Color(.8f, .8f, .8f);
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUI.backgroundColor = Color.white;
                    {
                        entitiesFoldout = EditorGUILayout.Foldout(entitiesFoldout, $"Entities({GameManager.Current.Entities.Count})" + detail);

                        if(entitiesFoldout) {
                            EditorGUI.indentLevel++;
                            {
                                var labelStyle = new GUIStyle(GUI.skin.label);
                                labelStyle.richText = true;

                                foreach(var entity in GameManager.Current.Entities) {
                                    if(entity is ActorEntity actor) {
                                        EditorGUILayout.BeginVertical();
                                        {
                                            EditorGUI.indentLevel--;
                                            EditorGUILayout.LabelField(
                                                (actor.OriginalScriptableObject != null ? $"<b><size=18><color=#97b9e6>{actor.OriginalScriptableObject.name}</color></size></b> : " : "") +
                                                $"<b>{actor.GetType().Name}</b>",
                                                labelStyle,
                                                GUILayout.Height(24)
                                            );

                                            EditorGUI.indentLevel++;

                                            GUI.backgroundColor = new Color(.4f, .4f, .4f);
                                            EditorGUILayout.BeginVertical(GUI.skin.box);
                                            GUI.backgroundColor = Color.white;
                                            {
                                                EditorGUILayout.LabelField($"Entity Id:{actor.Id}");
                                                EditorGUILayout.LabelField($"Invincibility Time: {actor.InvincibilityTime}");
                                                EditorGUILayout.LabelField($"Invincibility Time Remaining: {actor.InvincibilityTimeRemaining}");

                                                if(actor.HealthSlots != null && actor.HealthSlots.Length > 0) {
                                                    EditorGUILayout.LabelField("Health");

                                                    EditorGUI.indentLevel++;
                                                    for(int i = 0; i < actor.HealthSlots.Length; i++) {
                                                        EditorGUILayout.LabelField($"{i}: {actor.HealthSlots[i].Value}/{actor.HealthSlots[i].MaxValue}");
                                                    }
                                                    EditorGUI.indentLevel--;
                                                }

                                                if(actor.CurrentUpgradeSet != null) {
                                                    EditorGUI.indentLevel++;
                                                    {
                                                        EditorGUILayout.LabelField($"Upgrade Set");

                                                        EditorGUI.indentLevel++;
                                                        {
                                                            if(!string.IsNullOrEmpty(actor.CurrentUpgradeSet.Id))
                                                                EditorGUILayout.LabelField($"Id:{actor.CurrentUpgradeSet.Id}");
                                                        }
                                                        EditorGUI.indentLevel--;
                                                    }
                                                    EditorGUI.indentLevel--;
                                                }
                                            }
                                            EditorGUILayout.EndVertical();
                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();

                if(EnemyProxy.Targets != null) {
                    EditorGUILayout.Space();
                    GUI.backgroundColor = Color.black;
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    GUI.backgroundColor = Color.white;
                    {
                        EditorGUILayout.LabelField("EnemyProxy Targets");

                        EditorGUI.indentLevel++;
                        foreach(var item in EnemyProxy.Targets) {
                            if(item == null) continue;

                            EditorGUILayout.BeginHorizontal(GUI.skin.box);
                            {
                                EditorGUILayout.ObjectField(item, typeof(Transform), false);

                                if(item.root != item) {
                                    EditorGUILayout.ObjectField("Root", item.root, typeof(Transform), false);
                                    EditorGUILayout.Space();
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            else {
                label += "Game must be playing to view game information";

                GUILayout.Label(label);
            }

            EditorGUILayout.EndScrollView();
            Repaint();
        }
    }
}