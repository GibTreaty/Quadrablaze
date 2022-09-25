using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using LapinerTools.Steam;
using LapinerTools.Steam.Data.Internal;
using Quadrablaze.Effects;
using Quadrablaze.Entities;
using Quadrablaze.Skills;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using YounGenTech.PoolGen;

namespace Quadrablaze {
    public class ShipImporter : MonoBehaviour {
        public static Encoding EncodingType { get { return Encoding.UTF8; } }
        public const string settingsFileName = "ShipSettings.json";
        public static ShipImportSettings defaultShipSettings;

        public static ShipImporter Current { get; private set; }
        public static Material defaultMaterial;

        public float maxSize = 2;

        public CantTouchThis crypter;
        public GameObject defaultPlayerMesh;
        public GameObject playerShell;
        public ScriptableUpgradeSet defaultUpgradeSet;
        public GameObject jetParticlePrefab;
        public GameObject recoilBarPrefab;
        public PoolManager poolManager;
        public PoolManager syncedShipPoolManager;

        public Dictionary<int, ImportedShipData> localShipData = new Dictionary<int, ImportedShipData>();
        public Dictionary<int, ImportedShipData> syncedShipData = new Dictionary<int, ImportedShipData>();
        public Dictionary<int, ShipImportSettings> localSettings = new Dictionary<int, ShipImportSettings>();

        [SerializeField]
        UnityEvent _onImport;

        HashSet<ShipInfoObject> shipInfoObjects = new HashSet<ShipInfoObject>();

        #region Properties
        public static string CustomShipFolderPath {
            get { return Application.persistentDataPath + "/Custom Ships"; }
        }

        public UnityEvent OnImport {
            get { return _onImport; }
            private set { _onImport = value; }
        }
        #endregion

        //void Awake() {
        //    if(!defaultMaterial) {
        //        defaultMaterial = GetDefaultMaterial();
        //    }

        //    CreateCustomShipFolder();

        //    //Import("Test Ship");
        //}

        //void OnEnable() {
        //    //Current = this;
        //    Debug.Log("Hey",gameObject);
        //}

        public void Initialize() {
            Current = this;

            if(!defaultMaterial)
                defaultMaterial = GetDefaultMaterial();

            if(OnImport == null) OnImport = new UnityEvent();

            CreateCustomShipFolder();
            ImportAll();
            InitializeShipPools();
        }

        void AddImportedShipData(string folderPath, int id, GameObject shipObject, GameObject playerShell, Material material, bool builtinShip, bool customShip, bool workshopShip, bool temporaryShip, ShipImportSettings settings) {
            var networkIdentity = playerShell.AddComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;

            //TODO: Remove this
            //if(false) {
            //var playerActor = playerShell.AddComponent<PlayerActor>();
            //playerActor.ActorType = ActorTypes.Player;
            //playerActor.Initialization = InitializationType.Pooled;
            //playerActor.Size = 1.2f;
            //playerActor.DamageMultiplier = 1;
            //playerActor.DefaultUpgrades = defaultUpgradeSet;
            //playerActor.MeshTransform = shipObject.transform;
            //playerActor.InvincibilityLength = 1;
            //playerActor.ShipID = id;

            playerShell.AddComponent<PlayerInput>();
            playerShell.AddComponent<NetworkSkillController>();
            //}

            var transformSync = playerShell.AddComponent<QuadrablazeTransformSync>();
            transformSync.SnapThreshold = 8;
            transformSync.SendInterval = 0.05555556f;
            transformSync.InterpolateRotation = 10;

            var newShipObject = Instantiate(shipObject);

            material.name = shipObject.name;

            newShipObject.name = newShipObject.name.Remove(newShipObject.name.Length - 7, 7);
            newShipObject.name = newShipObject.name.Replace('_', ' ');
            newShipObject.SetActive(false);

            var originalMesh = Instantiate(newShipObject);
            originalMesh.name = newShipObject.name;

            string assetHashName = GenerateAssetHashName(id, playerShell, builtinShip, customShip, workshopShip, temporaryShip);

            var shipData = new ImportedShipData() {
                folderPath = folderPath,
                workshopPublishedFileID = settings.workshopPublishedFileID,
                originalMeshObject = originalMesh,
                rootMeshObject = newShipObject,
                playerShellObject = playerShell,
                defaultMaterial = new Material(material),
                currentMaterial = material,
                shipPreset = new ShipPreset("Default", true, settings),
                isBuiltinShip = builtinShip,
                isCustomShip = customShip,
                isTemporaryShip = temporaryShip,
                isWorkshopShip = workshopShip,
                poolPrefabID = id,
                assetHashName = assetHashName
            };

            if(temporaryShip)
                syncedShipData[id] = shipData;
            else {
                localShipData[id] = shipData;
                localSettings[id] = settings;
            }

            if(!newShipObject.GetComponent<ShipInfoObject>()) {
                var shipInfoObject = newShipObject.AddComponent<ShipInfoObject>();
                var weaponPivots = new List<Transform>();
                var root = shipInfoObject.transform.root;

                shipInfoObjects.Add(shipInfoObject);

                GameHelper.GetAllChildrenRecurvisely("Weapon Pivot", root, weaponPivots);
                GameHelper.GetAllChildrenRecurvisely("Weapon_Pivot", root, weaponPivots);

                if(settings.weaponPivots != null)
                    foreach(var childPath in settings.weaponPivots) {
                        var meshTransform = root.Find(childPath);

                        if(meshTransform)
                            weaponPivots.Add(meshTransform);
                    }

                shipInfoObject.WeaponPivots = new List<Transform>(weaponPivots.GetRange(0, Mathf.Min(weaponPivots.Count, 8)));
            }
        }

        void AddShipToObjectPool(GameObject shipObject, int id, bool builtinShip, bool customShip, bool workshopShip, bool temporary) {
            if(poolManager) {
                var shipInfo = temporary ? syncedShipData[id] : localShipData[id];

                var prefab = new PoolPrefab() {
                    Active = true,
                    ID = id,
                    InitializationType = PoolManager.PoolInitializationType.Manual,
                    PoolSize = builtinShip ? 4 : 1,
                    Prefab = shipObject,
                    CanExpand = !temporary,
                    IsNetworked = true,
                    CustomAssetHashName = shipInfo.assetHashName
                };

                if(temporary)
                    syncedShipPoolManager.PoolGenPrefabs.Add(prefab);
                else
                    poolManager.PoolGenPrefabs.Add(prefab);

                shipObject.SetActive(false);
            }
        }

        void ApplyControlSettings(GameObject playerShellObject, ShipImportSettings settings) {
            playerShellObject.GetComponentInChildren<BaseMovementController>().MovementStyle = settings.controlSettings.movementStyle;
        }

        public Material ApplyDefaultMaterial(GameObject root, ShipImportSettings settings) {
            var material = new Material(defaultMaterial);
            //var material = Instantiate(defaultMaterial);

            material.SetColor("_PrimaryColor", settings.primaryColor);
            material.SetColor("_SecondaryColor", settings.secondaryColor);
            material.SetColor("_AccessoryPrimaryColor", settings.accessorySecondaryColor);
            material.SetColor("_AccessorySecondaryColor", settings.accessorySecondaryColor);
            material.SetColor("_GlowColor", settings.glowColor);

            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                renderer.sharedMaterial = material;

            return material;
        }

        public void ApplyJetParticles(Transform root, ShipImportSettings settings, JetParticles.JetControlType jetControl = JetParticles.JetControlType.Auto) {
            if(!jetParticlePrefab) return;
            if(root.GetComponentInChildren<JetParticles>(false)) return;

            var jetPivots = new HashSet<Transform>();
            GameHelper.GetAllChildrenRecurvisely("Jet Pivot", root, jetPivots);
            GameHelper.GetAllChildrenRecurvisely("Jet_Pivot", root, jetPivots);

            if(settings.jetPivots != null)
                foreach(var childPath in settings.jetPivots) {
                    var meshTransform = root.Find(childPath);

                    if(meshTransform)
                        jetPivots.Add(meshTransform);
                }

            foreach(Transform transform in jetPivots) {
                var gameObject = Instantiate(jetParticlePrefab, transform, false);
                var jetParticles = gameObject.GetComponent<JetParticles>();

                jetParticles.Speed = 2;
            }
        }

        public void ApplyRotatingObjects(Transform root, ShipImportSettings settings) {
            var rotatingObjects = new HashSet<Transform>();
            GameHelper.GetAllChildrenRecurvisely("Rotating Object", root, rotatingObjects);
            GameHelper.GetAllChildrenRecurvisely("Rotating_Object", root, rotatingObjects);

            if(settings.rotatingObjects != null)
                foreach(var childPath in settings.rotatingObjects) {
                    var meshTransform = root.Find(childPath);

                    if(meshTransform)
                        rotatingObjects.Add(meshTransform);
                }

            foreach(Transform transform in rotatingObjects)
                transform.gameObject.AddComponent<RotateTransform>();
        }

        public void ApplyWeapons(GameObject playerShell, Transform root, ShipImportSettings settings) {
            var weaponPivots = new List<Transform>();
            var playerActor = playerShell.GetComponent<PlayerActor>();

            GameHelper.GetAllChildrenRecurvisely("Weapon Pivot", root, weaponPivots);
            GameHelper.GetAllChildrenRecurvisely("Weapon_Pivot", root, weaponPivots);

            if(weaponPivots.Count == 0) return;

            if(settings.weaponPivots != null)
                foreach(var childPath in settings.weaponPivots) {
                    var meshTransform = root.Find(childPath);

                    if(meshTransform)
                        weaponPivots.Add(meshTransform);
                }

            for(int i = 0; i < 8; i++)
                if(i < weaponPivots.Count) {
                    var recoilBarGameObject = Instantiate(recoilBarPrefab, weaponPivots[i], false);
                    var recoilBar = recoilBarGameObject.GetComponent<RecoilBar>();

                    recoilBar.WeaponIndex = i;
                }
        }

        public static void CreateCustomShipFolder() {
            Directory.CreateDirectory(CustomShipFolderPath);
        }

        bool DoBoundsCheck(GameObject root) {
            var maxObjectBounds = new Bounds(Vector3.zero, new Vector3(maxSize, maxSize, maxSize));

            foreach(var renderer in root.GetComponentsInChildren<Renderer>())
                if(!(maxObjectBounds.Contains(maxObjectBounds.min) &&
                     maxObjectBounds.Contains(new Vector3(maxObjectBounds.max.x, maxObjectBounds.min.y, maxObjectBounds.min.z)) &&
                     maxObjectBounds.Contains(new Vector3(maxObjectBounds.min.x, maxObjectBounds.min.y, maxObjectBounds.max.z)) &&
                     maxObjectBounds.Contains(new Vector3(maxObjectBounds.max.x, maxObjectBounds.min.y, maxObjectBounds.max.z)) &&
                     maxObjectBounds.Contains(new Vector3(maxObjectBounds.min.x, maxObjectBounds.max.y, maxObjectBounds.min.z)) &&
                     maxObjectBounds.Contains(new Vector3(maxObjectBounds.max.x, maxObjectBounds.max.y, maxObjectBounds.min.z)) &&
                     maxObjectBounds.Contains(new Vector3(maxObjectBounds.min.x, maxObjectBounds.max.y, maxObjectBounds.max.z)) &&
                     maxObjectBounds.Contains(maxObjectBounds.max)))
                    return false;

            return true;
        }

        public static string GenerateAssetHashName(int id, GameObject playerShell, bool builtinShip, bool customShip, bool workshopShip, bool temporaryShip) {
            string assetHashName = "Asset:";

            if(builtinShip) assetHashName += "Builtin:";
            if(customShip) assetHashName += "Custom:";
            if(workshopShip) assetHashName += "Workshop:";
            if(temporaryShip) assetHashName += "Temporary:" + id + ":";

            assetHashName += playerShell.name;

            return assetHashName;
        }

        public Material GetDefaultMaterial() {
            return Resources.Load<Material>("Custom Ship");
        }

        public ImportedShipData GetShipData(GamePlayerInfo playerInfo) {
            ImportedShipData shipInfo;

            if(playerInfo.hasAuthority) {
                shipInfo = localShipData[playerInfo.SelectedShipID];
            }
            else {
                if(playerInfo.IsBuiltinShip) {
                    shipInfo = localShipData.FirstOrDefault(data => data.Value.assetHashName == playerInfo.ShipAssetHashName).Value;
                }
                else {
                    shipInfo = syncedShipData[playerInfo.ShipID];
                }
            }

            return shipInfo;
        }
        public ImportedShipData GetPlayerActorShipData(PlayerEntity entity) {
            ImportedShipData shipInfo;

            if(entity.Owner) {
                shipInfo = localShipData[entity.PlayerInfo.SelectedShipID];
            }
            else {
                if(entity.PlayerInfo.IsBuiltinShip) {
                    shipInfo = localShipData.FirstOrDefault(data => data.Value.assetHashName == entity.PlayerInfo.ShipAssetHashName).Value;
                }
                else {
                    foreach(var key in syncedShipData.Keys)
                        Debug.Log(key);

                    shipInfo = syncedShipData[entity.PlayerInfo.ShipID];
                }
            }

            return shipInfo;
        }

        public string GetShipImportPath(string shipName) {
            return CustomShipFolderPath + "/" + shipName;
        }

        public ShipImportSettings GetShipImportSettings(int index) {
            localSettings.TryGetValue(index, out ShipImportSettings settings);

            return settings;
        }

        public void ImportFullPath(string shipModelPath, bool workshopShip) {
            string shipFolderPath = Path.GetDirectoryName(shipModelPath);
            string shipName = Path.GetFileNameWithoutExtension(shipModelPath);
            string shipSettingsPath = shipFolderPath + "/" + settingsFileName;
            string workshopSettingsPath = shipFolderPath + "/WorkshopItemInfo.xml";

            if(Directory.Exists(shipFolderPath)) {
                if(File.Exists(shipModelPath)) {
                    string colladaString;

                    using(var reader = File.OpenText(shipModelPath))
                        colladaString = reader.ReadToEnd();

                    GameObject gameObject = null;
                    ShipFile shipFile = null;

                    try {
                        if(workshopShip) {
                            try {
                                shipFile = ShipFile.LoadFromFile(shipModelPath, crypter);

                                gameObject = shipFile.CreateGameObject();
                            }
                            catch {
                                throw new ShipImportException(ShipImportException.ErrorTypes.MeshImportFailed);
                            }
                        }
                        else
                            gameObject = ColladaImporter.Import(colladaString, true);
                    }
                    catch {
                        Destroy(gameObject);

                        throw new ShipImportException(ShipImportException.ErrorTypes.MeshImportFailed);
                    }

                    if(gameObject == null)
                        throw new ShipImportException(ShipImportException.ErrorTypes.MeshImportFailed);

                    if(gameObject.name == "ImportedColladaScene") {
                        Destroy(gameObject);

                        throw new ShipImportException(ShipImportException.ErrorTypes.MultipleObjects);
                    }

                    ResizeShip(gameObject);

                    ShipImportSettings settings;

                    if(workshopShip)
                        settings = shipFile.ImportSettings;
                    else if(File.Exists(shipSettingsPath)) { //Read settings
                        string jsonSettings = File.ReadAllText(shipSettingsPath, EncodingType);

                        try {
                            settings = JsonUtility.FromJson<ShipImportSettings>(jsonSettings);
                        }
                        catch {
                            //Destroy(playerShellObject);
                            Destroy(gameObject);

                            throw new ShipImportException(ShipImportException.ErrorTypes.ShipImportSettingsFailed);
                        }
                    }
                    else { //Write settings
                        settings = new ShipImportSettings() {
                            primaryColor = defaultMaterial.GetColor("_PrimaryColor"),
                            secondaryColor = defaultMaterial.GetColor("_SecondaryColor"),
                            accessoryPrimaryColor = defaultMaterial.GetColor("_AccessoryPrimaryColor"),
                            accessorySecondaryColor = defaultMaterial.GetColor("_AccessorySecondaryColor"),
                            glowColor = defaultMaterial.GetColor("_GlowColor"),
                            controlSettings = new Controls() { movementStyle = BaseMovementController.MovementType.Directional },
                            jetPivots = new List<string>() { "Example/Pivot", "Example/Pivot2" },
                            rotatingObjects = new List<string>() { "Example/Pivot", "Example/Pivot2" },
                            weaponPivots = new List<string>() { "Example/Pivot", "Example/Pivot2" }
                        };

                        File.WriteAllText(shipSettingsPath, JsonUtility.ToJson(settings, true));
                    }

                    //Write workshop published file id to the ship import settings
                    if(File.Exists(workshopSettingsPath)) {
                        WorkshopItemInfo itemInfo;

                        using(var fileReader = File.OpenRead(workshopSettingsPath))
                        using(var xmlReader = XmlReader.Create(fileReader))
                            itemInfo = (WorkshopItemInfo)new XmlSerializer(typeof(WorkshopItemInfo)).Deserialize(xmlReader);

                        settings.workshopPublishedFileID = (PublishedFileId_t)itemInfo.PublishedFileId;
                    }

                    var playerShellObject = InstantiatePlayerShell();

                    playerShellObject.name = shipName;

                    var material = ApplyDefaultMaterial(gameObject, settings);
                    AddImportedShipData(shipFolderPath, localShipData.Count, gameObject, playerShellObject, material, false, !workshopShip, workshopShip, false, settings);

                    gameObject.transform.SetParent(playerShellObject.transform, false);

                    ApplyControlSettings(playerShellObject, settings);
                    ApplyJetParticles(gameObject.transform, settings);
                    ApplyRotatingObjects(gameObject.transform, settings);
                    ApplyWeapons(playerShellObject, gameObject.transform, settings);

                    AddShipToObjectPool(playerShellObject, localShipData.Count - 1, false, !workshopShip, workshopShip, false);

                }
            }
            else {
                Debug.Log("Ship '" + shipName + "' not found");
            }
        }
        public void ImportDefaultPlayer(GameObject defaultShipMesh) {
            var gameObject = Instantiate(defaultShipMesh, Vector3.zero, Quaternion.identity);
            var playerShellObject = InstantiatePlayerShell();

            defaultShipSettings = default;

            defaultShipSettings.primaryColor = defaultMaterial.GetColor("_PrimaryColor");
            defaultShipSettings.secondaryColor = defaultMaterial.GetColor("_SecondaryColor");
            defaultShipSettings.accessoryPrimaryColor = defaultMaterial.GetColor("_AccessoryPrimaryColor");
            defaultShipSettings.accessorySecondaryColor = defaultMaterial.GetColor("_AccessorySecondaryColor");
            defaultShipSettings.glowColor = defaultMaterial.GetColor("_GlowColor");

            //gameObject.name = gameObject.name.Remove(gameObject.name.Length - 7, 7);
            //gameObject.name = gameObject.name.Replace('_', ' ');
            gameObject.name = "Ship Mesh";
            playerShellObject.name = defaultShipMesh.name + ": Shell";

            var material = ApplyDefaultMaterial(gameObject, defaultShipSettings);
            AddImportedShipData("", localShipData.Count, gameObject, playerShellObject, material, true, false, false, false, defaultShipSettings);

            gameObject.transform.SetParent(playerShellObject.transform, false);

            ApplyJetParticles(gameObject.transform, defaultShipSettings);
            ApplyRotatingObjects(gameObject.transform, defaultShipSettings);
            ApplyWeapons(playerShellObject, gameObject.transform, defaultShipSettings);

            AddShipToObjectPool(playerShellObject, localShipData.Count - 1, true, false, false, false);
        }
        public void ImportGamePlayer(GamePlayerInfo gamePlayerInfo, ShipImportSettings settings) {
            int id = (int)gamePlayerInfo.netId.Value;

            gamePlayerInfo.ShipSettings = settings;

            GameObject gameObject;

            if(gamePlayerInfo.hasAuthority) {
                var shipInfo = localShipData[gamePlayerInfo.SelectedShipID];

                gameObject = Instantiate(shipInfo.originalMeshObject);
                gameObject.name = shipInfo.originalMeshObject.name;
                gameObject.SetActive(true);
            }
            else {
                gameObject = gamePlayerInfo.DownloadedShip.CreateGameObject();
            }

            var playerShellObject = InstantiatePlayerShell();

            gamePlayerInfo.PlayerShipPrefab = playerShellObject;

            //Debug.LogError(string.Format("PrimaryColor <color=#" + ColorUtility.ToHtmlStringRGB(settings.PrimaryColor) + ">" + '\u25AE' + " </color> {0}", settings.PrimaryColor));
            //Debug.LogError(string.Format("SecondaryColor <color=#" + ColorUtility.ToHtmlStringRGB(settings.SecondaryColor) + ">" + '\u25AE' + " </color> {0}", settings.SecondaryColor));
            //Debug.LogError(string.Format("AccessoryPrimaryColor <color=#" + ColorUtility.ToHtmlStringRGB(settings.AccessoryPrimaryColor) + ">" + '\u25AE' + " </color> {0}", settings.AccessoryPrimaryColor));
            //Debug.LogError(string.Format("AccessorySecondaryColor <color=#" + ColorUtility.ToHtmlStringRGB(settings.AccessorySecondaryColor) + ">" + '\u25AE' + " </color> {0}", settings.AccessorySecondaryColor));
            //Debug.LogError(string.Format("GlowColor <color=#" + ColorUtility.ToHtmlStringRGB(settings.GlowColor) + ">" + '\u25AE' + " </color> {0}", settings.GlowColor));

            var material = ApplyDefaultMaterial(gameObject, settings);

            playerShellObject.name = gameObject.name;

            AddImportedShipData("", id, gameObject, playerShellObject, material, false, false, false, true, settings);

            gameObject.transform.SetParent(playerShellObject.transform, false);

            ApplyControlSettings(playerShellObject, settings);
            ApplyJetParticles(gameObject.transform, settings);
            ApplyRotatingObjects(gameObject.transform, settings);
            ApplyWeapons(playerShellObject, gameObject.transform, settings);

            Debug.Log("Add ship to pool " + id);
            AddShipToObjectPool(playerShellObject, id, false, false, false, true);

            syncedShipPoolManager.InitializePool(syncedShipPoolManager.IndexFromPrefabID(id));
        }

        public void ImportAll() {
            localSettings.Clear();

            foreach(var shipInfoObject in shipInfoObjects)
                if(shipInfoObject)
                    Destroy(shipInfoObject.gameObject);

            shipInfoObjects.Clear();

            foreach(var data in localShipData.Values) {
                // TODO Make sure this is clearing all mesh objects
                Destroy(data.rootMeshObject);
                Destroy(data.defaultMaterial);
                Destroy(data.currentMaterial);
            }

            localShipData.Clear();

            foreach(var pool in poolManager.ObjectContainers.Values) {
                Destroy(pool.PoolPrefab.Prefab);
                pool.ClearAll();
            }

            poolManager.PoolGenPrefabs.Clear();
            poolManager.ObjectContainers = new Dictionary<int, ObjectPool>();

            ImportDefaultPlayer(defaultPlayerMesh);

            //if(false) {
            // TODO: Avoiding importing custom ships here

            if(Directory.Exists(CustomShipFolderPath)) {
                var filePaths = Directory.GetFiles(CustomShipFolderPath, "*.dae", SearchOption.AllDirectories);

                foreach(var filePath in filePaths)
                    try {
                        ImportFullPath(filePath, false);
                    }
                    catch(ShipImportException e) {
                        Debug.LogError("Ship Import Error: " + e.Message + " FilePath: " + filePath);

                        UIManager.Current.mainMenuGroup.interactable = false;
                        UIManager.Current.mainMenuGroup.alpha = .5f;

                        UIManager.Current.DoPopup("Ship Import Error", e.Message + " FilePath: " + filePath, null,
                            (LapinerTools.uMyGUI.uMyGUI_PopupManager.BTN_OK, null));
                    }
            }

            if(SteamManager.Instance.UseSteamManager)
                ImportSteamWorkshop();
            //}

            OnImport.Invoke();
        }

        public void ImportSteamWorkshop() {
            QueryAllItems();
        }

        void InitializeShipPools() {
            //foreach(var pool in poolManager.ObjectContainers)
            //    pool.Value.PoolPrefab.Prefab.SetActive(false);

            poolManager.InitializePools();
        }

        public GameObject InstantiatePlayerShell() {
            return Instantiate(playerShell, Vector3.zero, Quaternion.identity);
        }

        public byte[] PrepareShipForMuliplayerUpload(int index) {
            var shipData = localShipData[index];
            var shipFile = new ShipFile(shipData.rootMeshObject.transform);

            return shipFile.SaveToBytes();
        }

        void QueryAllItems() {
            uint subscriptionsCount = SteamUGC.GetNumSubscribedItems();

            if(subscriptionsCount > 0) {
                // create a query to get subscribed items information
                PublishedFileId_t[] subscriptions = new PublishedFileId_t[subscriptionsCount];
                subscriptionsCount = System.Math.Min(subscriptionsCount, SteamUGC.GetSubscribedItems(subscriptions, subscriptionsCount));
                UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(subscriptions, subscriptionsCount);

                SteamWorkshopMain.Instance.Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(queryHandle), OnAvailableItemsToImport);
            }
        }

        void OnAvailableItemsToImport(SteamUGCQueryCompleted_t p_callback, bool bIOFailure) {
            for(uint i = 0; i < p_callback.m_unNumResultsReturned; i++) {
                if(SteamUGC.GetQueryUGCResult(p_callback.m_handle, i, out SteamUGCDetails_t itemDetails)) {
                    var itemState = (EItemState)SteamUGC.GetItemState(itemDetails.m_nPublishedFileId);
                    //bool isOwned = itemDetails.m_ulSteamIDOwner == SteamUser.GetSteamID().m_SteamID;
                    //itemDetails.

                    bool installed = ((itemState & EItemState.k_EItemStateInstalled) == EItemState.k_EItemStateInstalled);

                    if(!installed) continue;

                    //DateTime timestampParsed = DateTime.MinValue;

                    if(SteamUGC.GetItemInstallInfo(itemDetails.m_nPublishedFileId, out ulong sizeOnDisk, out string localFolder, 260, out uint timestamp)) {
                        //timestampParsed = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        //timestampParsed = timestampParsed.AddSeconds(timestamp).ToLocalTime();

                        Debug.Log(string.Format("Steam Workshop Item Import\nFolder {0}\nSize {1}mb", localFolder, ((sizeOnDisk / 1024d) / 1024d).ToString("F2")));

                        var filePaths = Directory.GetFiles(localFolder, "*.qship", SearchOption.AllDirectories);
                        //var filePaths = Directory.GetFiles(localFolder, "*.dae", SearchOption.AllDirectories);

                        foreach(var filePath in filePaths)
                            ImportFullPath(filePath, true);
                    }
                }
            }
        }

        void ResizeShip(GameObject root) {
            var objectBounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach(var renderer in root.GetComponentsInChildren<Renderer>())
                objectBounds.Encapsulate(renderer.bounds);

            GameHelper.ForEachChild(root.transform, s => objectBounds.Encapsulate(s.position));

            float highestAxis = Mathf.Max(objectBounds.extents.x, objectBounds.extents.y, objectBounds.extents.z);

            if(highestAxis > 0) {
                float rescaleSize = 1 / highestAxis;

                root.transform.localScale *= rescaleSize;
            }
        }

        public void SaveSettings(int shipIndex, ShipImportSettings settings) {
            var shipData = localShipData[shipIndex];

            if(Directory.Exists(shipData.folderPath)) {
                string shipSettingsPath = Path.Combine(shipData.folderPath, settingsFileName);

                if(File.Exists(shipSettingsPath)) {
                    string jsonSettings = JsonUtility.ToJson(settings, true);

                    File.WriteAllText(shipSettingsPath, jsonSettings, EncodingType);

                    localSettings[shipIndex] = settings;
                    shipData.shipPreset.SetValues(settings);

                    //var prefabIndex = poolManager.IndexFromPrefabID(shipData.poolPrefabID);
                    //var prefab = poolManager.PoolGenPrefabs[prefabIndex];

                    //poolManager.ObjectContainers
                    ApplyControlSettings(shipData.playerShellObject, settings);

                    var objectPool = poolManager.GetObjectPool(shipData.poolPrefabID);

                    foreach(var user in objectPool.PooledObjects)
                        ApplyControlSettings(user.gameObject, settings);
                }
            }
        }

        [Serializable]
        public struct ImportedShipData {
            public PublishedFileId_t workshopPublishedFileID;
            public GameObject originalMeshObject;
            public GameObject rootMeshObject;
            public GameObject playerShellObject;
            public Material defaultMaterial;
            public Material currentMaterial;
            public ShipPreset shipPreset;
            public string folderPath;
            public bool isBuiltinShip; // The ship comes built into the game
            public bool isCustomShip;
            public bool isWorkshopShip;
            public bool isTemporaryShip;
            public int poolPrefabID;
            public string assetHashName;

            //public Guid GetKey() {
            //    string value = playerShellObject.name;

            //    value += isBuiltinShip ? 1 : 0;
            //    value += isCustomShip ? 1 : 0;
            //    value += isWorkshopShip ? 1 : 0;
            //    value += isTemporaryShip ? 1 : 0;
            //    value += workshopPubilshedFileID.m_PublishedFileId;

            //    return new Guid(value);
            //}
        }
    }
}

public class ShipImportException : Exception {

    string _message = "";

    #region Properties
    public override string Message {
        get { return _message; }
    }
    #endregion

    public ShipImportException(ErrorTypes error) {
        _message = error.ToString();
    }

    public enum ErrorTypes {
        MultipleObjects,
        MeshImportFailed,
        ShipImportSettingsFailed
    }
}