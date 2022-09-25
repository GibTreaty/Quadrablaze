using System;
using System.Collections;
using System.Collections.Generic;
using DG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class Startup : MonoBehaviour {

        [Header("Options")]
        public UIOptionsTab[] optionsTabs;

        public Camera mainCamera;

        public SteamManager steamManager;
        public ShipImporter shipImporter;
        public ShipSelectionRenderController shipRenderController;
        public SteamWorkshopRenderController steamWorkshopRenderController;
        public ShipSelectionUIManager shipSelectionUIManager;
        public ShipPresetManager shipPresetManager;
        public PlayerSpawnManager playerSpawnManager;
        public GameManager gameManager;
        public UIManager uiManager;
        public UIManagerTextMeshPro feedbackManager;
        public PauseManager pauseManager;
        public SpectatorHUD spectatorHUD;
        public BossInfoUI bossInfoUI;

        public QuadrablazeSteamNetworking quadrablazeSteamNetworking;
        public NetworkUpgradeManager networkUpgradeManager;
        public GameLobbyManager gameLobbyManager;
        public GameNetworkSelectionUI gameNetworkSelectionUI;

        public LevelUpOverscreenEffect levelUpOverscreenEffect;

        public UnityEvent onStartup;

        bool skipInDevMessage;
        bool startedFromInvite;
        ulong commandLineLobbyId = 0;
        void Awake() {
            if(onStartup == null) onStartup = new UnityEvent();

            //Shader.WarmupAllShaders();
        }

        IEnumerator StartSteam() {
            int tryAttempt = 0;

            if(steamManager.UseSteamManager) {
                while(true) {
                    if(steamManager.Initialize())
                        break;

#if DEVELOPMENT_BUILD
                    goto AttemptItAgain;
                    //break;
#endif
                    if(tryAttempt >= 5) {
#if UNITY_EDITOR
                        Debug.LogError("SteamAPI could not be initialized");
#endif

                        Application.Quit();
                        yield break;
                    }

                    tryAttempt++;

#if DEVELOPMENT_BUILD
                    AttemptItAgain:
#endif

                    yield return new WaitForSecondsRealtime(.5f);
                }

                if(!Application.isEditor && !startedFromInvite)
                    yield return StartCoroutine(InDevMessage());


                //if(!SteamManager.Initialized) {
                //    Debug.LogError("SteamAPI could not be initialized");
                //    QuitApplication.QuitApp();
                //}
            }
        }

        IEnumerator Start() {
            CheckCommandLine();

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            yield return StartSteam();
            //            if(steamManager.UseSteamManager) {
            //                while(true) {
            //                    if(steamManager.Initialize())
            //                        break;

            //#if DEVELOPMENT_BUILD
            //                    goto AttemptItAgain;
            //                    //break;
            //#endif
            //                    if(tryAttempt >= 5) {
            //#if UNITY_EDITOR
            //                        Debug.LogError("SteamAPI could not be initialized");
            //#endif

            //                        Application.Quit();
            //                        yield break;
            //                    }

            //                    tryAttempt++;

            //#if DEVELOPMENT_BUILD
            //                    AttemptItAgain:
            //#endif

            //                    yield return new WaitForSecondsRealtime(.5f);
            //                }

            //                if(!Application.isEditor && !startedFromInvite)
            //                    yield return StartCoroutine(InDevMessage());


            //                //if(!SteamManager.Initialized) {
            //                //    Debug.LogError("SteamAPI could not be initialized");
            //                //    QuitApplication.QuitApp();
            //                //}
            //            }

            gameManager.PreInitialize();

            HealthProxy.Initialize();
            ShieldProxy.Initialize();
            PlayerProxy.Initialize();
            EnemyProxy.Initialize();
            RoundManager.Initialize();

            //gameManager.Initialize();
            //quadrablazeNetworkManager.Initialize();
            quadrablazeSteamNetworking.Initialize();
            feedbackManager.Initialize();
            pauseManager.Initialize();
            uiManager.Initialize();
            gameLobbyManager.Initialize();
            spectatorHUD.Initialize();
            bossInfoUI.Initialize();

            shipImporter.Initialize();

            shipSelectionUIManager.Initialize();
            shipPresetManager.Initialize();
            shipRenderController.Initialize();
            steamWorkshopRenderController.Initialize();

            shipSelectionUIManager.SetSelectedShip(PlayerPrefs.GetInt("Selected Ship", 0));
            shipSelectionUIManager.SetSelectedPreset(PlayerPrefs.GetInt("Selected Preset", 0));

            networkUpgradeManager.Initialize();
            playerSpawnManager.Initialize();

            levelUpOverscreenEffect.Initialize();

            gameManager.Initialize();
            gameNetworkSelectionUI.Initialize();

            mainCamera.enabled = false;

            foreach(var pool in PoolManager.GetPools()) {
                //pool.InitializePools();

                for(int i = 0; i < pool.PoolGenPrefabs.Count; i++) {
                    switch(pool.PoolName) {
                        case "Built-In Ships":
                        case "Synced Ships":
                        case "Enemy":
                            continue;
                    }

                    var farAwayPosition = new Vector3(10000, 10000, 0);

                    var poolGenPrefab = pool.PoolGenPrefabs[i];
                    var gameObject = Instantiate(poolGenPrefab.Prefab, farAwayPosition, Quaternion.identity);

                    gameObject.SetActive(false);

                    foreach(var renderer in gameObject.GetComponentsInChildren<Renderer>(true)) {
                        var meshFilter = renderer.GetComponent<MeshFilter>();

                        var types = new List<Type>() { renderer.GetType() };
                        if(meshFilter) types.Add(typeof(MeshFilter));

                        var clone = new GameObject("Preload", types.ToArray());

                        clone.transform.position = farAwayPosition;
                        clone.GetComponent<Renderer>().sharedMaterials = renderer.sharedMaterials;
                        clone.hideFlags = HideFlags.None;

                        if(meshFilter)
                            clone.GetComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;

                        switch(renderer) {
                            case LineRenderer _:
                                var lineRenderer = clone.GetComponent<LineRenderer>();

                                lineRenderer.useWorldSpace = false;

                                for(int x = 0; x < lineRenderer.positionCount; x++)
                                    lineRenderer.SetPosition(x, farAwayPosition);

                                break;

                            case SpriteRenderer _:
                                var spriteRenderer = clone.GetComponent<SpriteRenderer>();

                                spriteRenderer.sprite = (renderer as SpriteRenderer).sprite;
                                break;

                            case TrailRenderer _:
                                var trailRenderer = clone.GetComponent<TrailRenderer>();

                                trailRenderer.Clear();
                                break;
                        }

                        Destroy(clone, .5f);
                    }

                    Destroy(gameObject, .5f);
                }

                yield return new WaitForEndOfFrame();
            }

            mainCamera.enabled = true;

            yield return new WaitForEndOfFrame();

            foreach(var options in optionsTabs) {
                options.gameObject.SetActive(true);
                options.Initialize();
                options.LoadPrefs();
                options.gameObject.SetActive(false);

                //    GameDebug.Log("Initializing Options Tab - " + options.name);
            }

            uiManager.loadingText.StopAnimation();
            onStartup.Invoke();
            uiManager.title.SetActive(true);
            uiManager.startMenuTitle.SetActive(false);

            yield return new WaitForEndOfFrame();

            if(startedFromInvite) {
                QuadrablazeSteamNetworking.Current.JoinSteamLobby(new Steamworks.CSteamID(commandLineLobbyId));
                uiManager.DeactivateTitleInstant();
            }
        }

        IEnumerator InDevMessage() {
            uiManager.title.SetActive(false);
            uiManager.inDevMessage.gameObject.SetActive(true);

            float stopTime = Time.realtimeSinceStartup + 6;

            var player = Rewired.ReInput.players.GetPlayer(0);

            while(Time.realtimeSinceStartup < stopTime) {
                if(player.GetButtonDown("Cancel") || player.GetButtonDown("Submit"))
                    break;

                yield return new WaitForEndOfFrame();
            }

            uiManager.inDevMessage.gameObject.SetActive(false);
            uiManager.loadingText.StartAnimation();
            yield return new WaitForEndOfFrame();
        }

        void CheckCommandLine() {
            string[] args = Environment.GetCommandLineArgs();

            #region Steam Command Line
            {
                string input = "";

                for(int i = 0; i < args.Length; i++)
                    if(args[i] == "+connect_lobby" && args.Length > i + 1)
                        input = args[i + 1];

                if(!string.IsNullOrEmpty(input)) {
                    commandLineLobbyId = 0;
                    startedFromInvite = ulong.TryParse(input, out commandLineLobbyId);
                }
            }
            #endregion
        }
    }
}