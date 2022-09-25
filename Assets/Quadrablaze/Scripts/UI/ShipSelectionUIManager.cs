using System;
using System.Collections;
using System.Linq;
using Rewired;
using UIWidgets;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Quadrablaze {
    public class ShipSelectionUIManager : MonoBehaviour {

        public static ShipSelectionUIManager Current { get; private set; }

        [SerializeField]
        CantTouchThis _crypter;

        [SerializeField]
        MenuSystem.ScriptableMenu _colorPickerMenu;

        [SerializeField]
        MenuSystem.ScriptableMenu _movementTypeMenu;

        [SerializeField]
        int _currentShipSelection;

        [SerializeField]
        int _currentShipPreset;

        [Space, Header("Background")]
        [SerializeField]
        Image _backgroundImage;

        [SerializeField]
        Color _backgroundDefaultShipColor;

        [SerializeField]
        Color _backgroundCustomShipColor;

        [SerializeField]
        Color _backgroundWorkshopShipColor;

        [Space, Header("Outline")]
        [SerializeField]
        Image _outlineImage;

        [SerializeField]
        Color _outlineDefaultShipColor;

        [SerializeField]
        Color _outlineCustomShipColor;

        [SerializeField]
        Color _outlineWorkshopShipColor;

        [Space]
        [SerializeField]
        GameObject _shipOptionsPanel;

        [SerializeField]
        GameObject _shipOptionsRoot;

        [SerializeField]
        GameObject _shipRenderRoot;

        [SerializeField]
        GameObject _shipNameInputRoot;

        [SerializeField]
        InputPopup _shipNamePopup;

        [SerializeField]
        InputPopup _shipDescriptionPopup;

        [SerializeField]
        Button _reimportButton;

        [SerializeField]
        Button _renameButton;

        [SerializeField]
        Button _leftShipArrowButton;

        [SerializeField]
        Button _rightShipArrowButton;

        [SerializeField]
        Button _leftPresetArrowButton;

        [SerializeField]
        Button _rightPresetArrowButton;

        [SerializeField]
        GameObject _glowSliderObject;

        [SerializeField]
        Slider _glowSlider;

        [SerializeField]
        TMPro.TMP_Text _shipNameText;

        [SerializeField]
        TMPro.TMP_Text _presetNameText;

        [SerializeField]
        ColorPicker _colorPickerUI;

        [SerializeField]
        GameObject _colorPickerPanel;

        [SerializeField]
        GameObject _colorHeader;

        [SerializeField]
        UIColorButton[] _colorButtons;

        [SerializeField]
        TMPro.TMP_Text _workshopIDText;

        [SerializeField]
        Button _uploadButton;

        [SerializeField]
        GameObject _saveAsDefaultRoot;

        [SerializeField]
        Button _saveAsDefaultButton;

        [Space, Header("Movement Type")]
        [SerializeField]
        Button _movementTypeButton;

        [SerializeField]
        TMPro.TMP_Text _movementTypeButtonText;

        [SerializeField]
        ListView _movementTypeListView;

        [Space]
        [SerializeField]
        Button _addButton;

        [SerializeField]
        Button _deleteButton;

        [SerializeField]
        Button _doneButton;

        [SerializeField]
        Button _applyColorPickerButton;

        [SerializeField]
        Button _cancelColorPickerButton;

        [SerializeField]
        Button _openCustomShipFolder;

        [SerializeField]
        Text _descriptionText;

        UIColorButton _selectedColorButton;

        Color _newColor;
        Color _oldColor;

        float _newBrightness;
        float _oldBrightness;

        ulong _workshopPublishedFileID;
        int uploadedShipSelected;

        #region Properties
        public CanvasGroup CanvasGroupComponent { get; private set; }

        public GameObject ColorPickerPanel {
            get { return _colorPickerPanel; }
            set { _colorPickerPanel = value; }
        }

        public ShipPreset CurrentPreset {
            get { return CurrentShipPreset > 0 ? ShipPresetManager.Current.PresetList[CurrentShipPreset - 1] : ShipImporter.Current.localShipData[CurrentShipSelection].shipPreset; }
        }

        public int CurrentShipPreset {
            get { return _currentShipPreset; }
            private set { _currentShipPreset = value; }
        }

        public int CurrentShipSelection {
            get { return _currentShipSelection; }
            private set { _currentShipSelection = value; }
        }

        public ListView MovementTypeListView {
            get { return _movementTypeListView; }
            set { _movementTypeListView = value; }
        }

        Player RewiredPlayer { get; set; }

        public InputPopup ShipDescriptionPopup {
            get { return _shipDescriptionPopup; }
            set { _shipDescriptionPopup = value; }
        }

        public static string ShipIconFilePath {
            get { return Path.Combine(ShipIconFolderPath, "ShipRender.png"); }
        }

        public static string ShipIconFolderPath {
            get { return Path.Combine(Application.persistentDataPath, "Last Ship Render"); }
        }

        public GameObject ShipNameInputRoot {
            get { return _shipNameInputRoot; }
            set { _shipNameInputRoot = value; }
        }

        public InputPopup ShipNamePopup {
            get { return _shipNamePopup; }
            set { _shipNamePopup = value; }
        }

        public GameObject ShipOptionsPanel {
            get { return _shipOptionsPanel; }
            set { _shipOptionsPanel = value; }
        }

        public static string TempUploadFolderPath {
            get { return Path.Combine(Application.persistentDataPath, "Temp Upload"); }
        }
        #endregion

        //public void 
        //void Awake() {
        //    _leftArrowButton.onClick.AddListener(ChangeShipLeft);
        //    _rightArrowButton.onClick.AddListener(ChangeShipRight);
        //}

        //IEnumerator Start() {
        //    yield return new WaitForEndOfFrame();
        //    SetSelectedShip(PlayerSpawnManager.Current.playerPoolPrefabID);
        //}

        void OnEnable() {
            Current = this;
        }

        public void Initialize() {
            Current = this;

            CanvasGroupComponent = GetComponent<CanvasGroup>();
            RewiredPlayer = ReInput.players.GetPlayer(0);

            _reimportButton.onClick.AddListener(ShipImporter.Current.ImportAll);

            ShipNamePopup.Initialize();
            _renameButton.onClick.AddListener(RenameShip);
            ShipNamePopup.OnNameSubmitted.AddListener(RenameSubmitted);
            ShipNamePopup.OnCancel.AddListener(RenameCanceled);

            ShipDescriptionPopup.Initialize();
            //AddListener(ChangeDescription);
            ShipDescriptionPopup.OnNameSubmitted.AddListener(ChangeDescriptionSubmitted);
            ShipDescriptionPopup.OnCancel.AddListener(ChangeDescriptionCanceled);

            _movementTypeButton.onClick.AddListener(OpenMovementTypeListView);
            MovementTypeListView.OnSelectString.AddListener(SaveMovementType);

            _leftShipArrowButton.onClick.AddListener(ChangeShipLeft);
            _rightShipArrowButton.onClick.AddListener(ChangeShipRight);
            _leftPresetArrowButton.onClick.AddListener(ChangePresetLeft);
            _rightPresetArrowButton.onClick.AddListener(ChangePresetRight);
            _applyColorPickerButton.onClick.AddListener(ApplyColorChange);
            _cancelColorPickerButton.onClick.AddListener(CancelColorChange);

            _uploadButton.onClick.AddListener(UploadShip);

            _saveAsDefaultButton.onClick.AddListener(SaveAsDefault);

            _addButton.onClick.AddListener(AddPreset);
            _deleteButton.onClick.AddListener(RemovePreset);
            _doneButton.onClick.AddListener(Done);

            _openCustomShipFolder.onClick.AddListener(OpenCustomShipFolder);

            for(int i = 0; i < _colorButtons.Length; i++) {
                var colorButton = _colorButtons[i];

                colorButton.ColorButton.onClick.AddListener(() => ColorButtonSelected(colorButton));
            }

            _colorPickerUI.OnChange.AddListener(ColorChanged);

            gameObject.SetActive(false);
            ShipOptionsPanel.SetActive(true);
            ColorPickerPanel.SetActive(false);

            ShipImporter.Current.OnImport.AddListener(() => SetSelectedShip(0));
        }

        void OpenCustomShipFolder() {
            ShipImporter.CreateCustomShipFolder();
            Application.OpenURL(ShipImporter.CustomShipFolderPath);
        }

        //void Update() {
        //    if(!UIManager.Current.trelloUIManager.gameObject.activeInHierarchy)
        //        if(!(ColorPickerPanel.activeSelf || MovementTypeListView.gameObject.activeSelf))
        //            if(RewiredPlayer != null)
        //                if(RewiredPlayer.GetNegativeButtonDown("Menu Tab Navigation"))
        //                    ChangeShipLeft();
        //                else if(RewiredPlayer.GetButtonDown("Menu Tab Navigation"))
        //                    ChangeShipRight();
        //}

        void AddPreset() {
            var preset = ShipPresetManager.Current.AddPreset(ShipImporter.Current.localShipData[CurrentShipSelection].shipPreset);
            var index = ShipPresetManager.Current.PresetList.IndexOf(preset);

            Debug.Log("Add Preset " + index);
            SetSelectedPreset(index + 1);
            ShipPresetManager.Current.Save();
        }

        public void ApplyColorChange() {
            if(_selectedColorButton) {
                _selectedColorButton.Brightness = _newBrightness;
                _selectedColorButton.SetImageColor(_newColor);
                _selectedColorButton.ApplyColor(ShipImporter.Current.localShipData[CurrentShipSelection].currentMaterial);
                _selectedColorButton.ApplyColor(CurrentPreset);
            }

            CloseColorPicker();
            ShipPresetManager.Current.Save();
        }

        public void BrightnessChanged(float brightness) {
            _newBrightness = brightness;

            if(_selectedColorButton.HDRColor)
                _selectedColorButton.Brightness = brightness;

            _selectedColorButton.ApplyColor(_newColor, ShipImporter.Current.localShipData[CurrentShipSelection].currentMaterial);
        }

        public void CancelColorChange() {
            if(_selectedColorButton) {
                if(_selectedColorButton.HDRColor)
                    _selectedColorButton.Brightness = _oldBrightness;

                _selectedColorButton.SetImageColor(_oldColor);
                _selectedColorButton.ApplyColor(ShipImporter.Current.localShipData[_currentShipSelection].currentMaterial);
            }

            CloseColorPicker();
        }

        void ChangeDescription() {
            if(CurrentShipPreset == 0) return;

            ShipDescriptionPopup.Open(CurrentPreset.Name);
            ToggleHide(false);
        }

        void ChangeDescriptionCanceled() {
            ToggleHide(true);
        }

        void ChangeDescriptionSubmitted(string name) {
            //CurrentPreset.Name = name;

            ToggleHide(true);

            //SetSelectedPreset(CurrentShipPreset);
            //ShipPresetManager.Current.Save();
        }

        public void ChangePreset(int direction) {
            if(direction == 0) return;

            int index = CurrentShipPreset;

            if(direction < 0) index--;
            else index++;

            index %= ShipPresetManager.Current.PresetList.Count + 1;

            if(index < 0) index += ShipPresetManager.Current.PresetList.Count + 1;

            SetSelectedPreset(index);
        }

        public void ChangePresetLeft() {
            ChangePreset(-1);
        }

        public void ChangePresetRight() {
            ChangePreset(1);
        }

        public void ChangeShip(int direction) {
            if(direction == 0) return;

            int index = CurrentShipSelection;

            if(direction < 0) index--;
            else index++;

            index %= ShipImporter.Current.localShipData.Count;

            if(index < 0) index += ShipImporter.Current.localShipData.Count;

            SetSelectedShip(index);
        }

        public void ChangeShipLeft() {
            ChangeShip(-1);
        }

        public void ChangeShipRight() {
            ChangeShip(1);
        }

        public void ClampPresetIndex() {
            if(ShipPresetManager.Current == null || ShipPresetManager.Current.PresetList == null)
                return;

            CurrentShipPreset = Mathf.Clamp(CurrentShipPreset, 0, ShipPresetManager.Current.PresetList.Count);
        }

        public void ClampShipIndex() {
            CurrentShipSelection = Mathf.Clamp(CurrentShipSelection, 0, ShipImporter.Current.localShipData.Count - 1);
        }

        public void Close() {
            SteamWorkshopRenderController.Current.RemoveModel();
            CancelColorChange();
            gameObject.SetActive(false);
        }

        public void CloseColorPicker() {
            UIManager.Current.GoToParentMenu();
            //ShipOptionsPanel.SetActive(true);
            //ColorPickerPanel.SetActive(false);

            //if(_selectedColorButton)
            //    EventSystem.current.SetSelectedGameObject(_selectedColorButton.gameObject);
        }

        public void CloseMovementTypeListView() {
            UIManager.Current.GoToParentMenu();
            MovementTypeListView.SelectedIndex = -1;

            EventSystem.current.SetSelectedGameObject(_movementTypeButton.gameObject);
        }

        void ColorButtonSelected(UIColorButton colorButton) {
            _oldColor = colorButton.ButtonColor;
            _oldBrightness = colorButton.Brightness;
            _selectedColorButton = colorButton;
            _colorPickerUI.Color = colorButton.ButtonColor;

            OpenColorPicker();
        }

        void ColorChanged(Color32 color) {
            _newColor = color;
            _selectedColorButton.ApplyColor(_newColor, ShipImporter.Current.localShipData[CurrentShipSelection].currentMaterial);

            if(CurrentShipPreset > 0)
                _selectedColorButton.ApplyColor(CurrentPreset);
        }

        void DeletePreset() {
            ClampPresetIndex();

            if(CurrentShipPreset > 0) {
                var preset = ShipPresetManager.Current.PresetList[CurrentShipPreset - 1];

                ShipPresetManager.Current.DeletePreset(preset);
                ClampPresetIndex();
                //ShipPresetManager.Current.Save();
            }
        }

        void Done() {
            ApplyColorChange();
            _selectedColorButton = null;

            var preset = CurrentPreset;

            foreach(var button in _colorButtons)
                button.ApplyColor(preset);

            PlayerPrefs.Save();
            //UIManager.Current.EnableShipSelectionMenu(false);
            UIManager.Current.GoToParentMenu();
            PlayerSpawnManager.Current.nextPlayerPrefabId = CurrentShipSelection;
        }

        void InitializeColors() {
            var material = ShipImporter.Current.localShipData[CurrentShipSelection].currentMaterial;

            foreach(var colorButton in _colorButtons) {
                //var color = material.GetColor(colorButton.MaterialProperty);
                var color = CurrentPreset.GetValue(colorButton.PresetColor);

                //if(CurrentShipPreset > 0)
                //colorButton.ApplyColor(CurrentPreset);

                if(colorButton.HDRColor) {
                    float h, s, v;

                    Color.RGBToHSV(color, out h, out s, out v);
                    colorButton.Brightness = v;
                }

                colorButton.ApplyColor(color, material);
                colorButton.SetImageColor(color);
            }
        }

        void OnAvailableItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure) {
            if(p_callback.m_unNumResultsReturned > 0) {
                SteamUGCDetails_t itemDetails;

                if(SteamUGC.GetQueryUGCResult(p_callback.m_handle, 0, out itemDetails)) {
                    var itemState = (EItemState)SteamUGC.GetItemState(itemDetails.m_nPublishedFileId);
                    bool isOwned = itemDetails.m_ulSteamIDOwner == SteamUser.GetSteamID().m_SteamID;
                    bool installed = ((itemState & EItemState.k_EItemStateInstalled) == EItemState.k_EItemStateInstalled);
                    bool legacyItem = ((itemState & EItemState.k_EItemStateLegacyItem) == EItemState.k_EItemStateLegacyItem);
                    bool subscribed = ((itemState & EItemState.k_EItemStateSubscribed) == EItemState.k_EItemStateSubscribed);
                    //var username = SteamFriends.GetFriendPersonaName();
                    var username = SteamFriends.GetFriendPersonaName((CSteamID)itemDetails.m_ulSteamIDOwner);
                    //Debug.Log(string.Format("Installed:{0}\nLegacyItem:{1}", installed, legacyItem));

                    if(installed)
                        //_workshopIDText.text = isOwned ? "Your Workshop Creation" : "Workshop Item";
                        _workshopIDText.text = "<smallcaps>Creator</smallcaps>: " + username;
                    else
                        _workshopIDText.text = "<smallcaps>Upload to the workshop</smallcaps>";

                    var shipData = ShipImporter.Current.localShipData[CurrentShipSelection];
                    bool showUploadButton = !shipData.isWorkshopShip;

                    UpdateWorkshopUIActivation(true, showUploadButton);

                    return;
                }
            }
        }

        public void Open() {
            InitializeColors();
            gameObject.SetActive(true);

            CloseMovementTypeListView();
        }

        public void OpenColorPicker() {
            UIManager.Current.GoToMenu(_colorPickerMenu);

            _glowSliderObject.SetActive(_selectedColorButton.HDRColor);

            if(_selectedColorButton.HDRColor) {
                var material = ShipImporter.Current.localShipData[_currentShipSelection].currentMaterial;
                float h, s, v;

                Color.RGBToHSV(material.GetColor(_selectedColorButton.MaterialProperty), out h, out s, out v);

                _selectedColorButton.Brightness = v;
                _glowSlider.value = v;
            }
        }

        public void OpenMovementTypeListView() {
            UIManager.Current.GoToMenu(_movementTypeMenu);

            var importSettings = ShipImporter.Current.localSettings[CurrentShipSelection];
            var selectable = MovementTypeListView.ScrollRect.content.GetChild((int)importSettings.controlSettings.movementStyle + 1).gameObject;

            EventSystem.current.SetSelectedGameObject(selectable);
            selectable.GetComponent<Selectable>()?.Select();
        }

        void QueryCreatedItem() {
            if(!SteamManager.Initialized) return;

            UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(
                new PublishedFileId_t[] { new PublishedFileId_t(_workshopPublishedFileID) },
                1
            );

            SteamWorkshopMain.Instance.Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(queryHandle), OnAvailableItemsCallCompleted);
        }

        void RemovePreset() {
            if(CurrentShipPreset == 0) return;

            ShipPresetManager.Current.RemovePreset(CurrentShipPreset - 1);

            ClampPresetIndex();
            SetSelectedPreset(CurrentShipPreset);
            ShipPresetManager.Current.Save();
        }

        public void RenameCanceled() {
            _shipRenderRoot.gameObject.SetActive(true);
            UIManager.Current.GoToParentMenu();
        }

        void RenameShip() {
            if(CurrentShipPreset == 0) return;

            ShipNamePopup.Open(CurrentPreset.Name);

            _shipRenderRoot.gameObject.SetActive(false);
            UIManager.Current.GoToMenu("Ship Selection - Rename Preset");
        }

        void RenameSubmitted(string name) {
            CurrentPreset.Name = name;


            SetSelectedPreset(CurrentShipPreset);
            ShipPresetManager.Current.Save();

            _shipRenderRoot.gameObject.SetActive(true);
            UIManager.Current.GoToParentMenu();

        }

        public void SetSelectedPreset(int index) {
            CurrentShipPreset = index;
            ClampPresetIndex();

            PlayerPrefs.SetInt("Selected Preset", CurrentShipPreset);
            PlayerPrefs.Save();
            ShipPreset preset;

            _colorHeader.SetActive(CurrentShipPreset != 0);
            _deleteButton.gameObject.SetActive(CurrentShipPreset != 0);
            _colorButtons.ForEach(button => button.gameObject.SetActive(CurrentShipPreset != 0));

            preset = CurrentPreset;

            SetColorButton("Primary Color", preset.PrimaryColor);
            SetColorButton("Secondary Color", preset.SecondaryColor);
            SetColorButton("Accessory Primary Color", preset.AccessoryPrimaryColor);
            SetColorButton("Accessory Secondary Color", preset.AccessorySecondaryColor);
            SetColorButton("Emissive Color", preset.GlowColor);

            _presetNameText.text = preset.Name;

            InitializeColors();

            var shipData = ShipImporter.Current.localShipData[CurrentShipSelection];

            _saveAsDefaultRoot.SetActive(shipData.isCustomShip && !shipData.isWorkshopShip && CurrentShipPreset != 0);
        }

        public void SetSelectedShip(int index) {
            CurrentShipSelection = index;
            ClampShipIndex();
            PlayerPrefs.SetInt("Selected Ship", CurrentShipSelection);
            PlayerPrefs.Save();

            var shipData = ShipImporter.Current.localShipData[CurrentShipSelection];
            var importSettings = ShipImporter.Current.localSettings[CurrentShipSelection]; // TODO KeyNotFoundException
            var shipGameObject = shipData.rootMeshObject;

            _shipNameText.text = shipGameObject.name;
            InitializeColors();

            _workshopPublishedFileID = shipData.workshopPublishedFileID.m_PublishedFileId;

            UpdateWorkshopUIActivation(false);

            if(shipData.isCustomShip || shipData.isWorkshopShip)
                QueryCreatedItem();

            _saveAsDefaultRoot.SetActive(shipData.isCustomShip && !shipData.isWorkshopShip && CurrentShipPreset != 0);

            SteamWorkshopRenderController.Current.RemoveModel();

            if(shipData.isBuiltinShip) {
                _backgroundImage.color = _backgroundDefaultShipColor;
                _outlineImage.color = _outlineDefaultShipColor;
                _movementTypeButton.interactable = false;
                //_movementTypeButton.enabled = false;

                var navigation = _movementTypeButton.navigation;
                navigation.mode = Navigation.Mode.None;
                _movementTypeButton.navigation = navigation;
            }
            else if(shipData.isCustomShip) {
                _backgroundImage.color = _backgroundCustomShipColor;
                _outlineImage.color = _outlineCustomShipColor;
                _movementTypeButton.interactable = true;

                var navigation = _movementTypeButton.navigation;
                navigation.mode = Navigation.Mode.Automatic;
                _movementTypeButton.navigation = navigation;

                //_movementTypeButton.enabled = true;
                SteamWorkshopRenderController.Current.UpdateSelectedShip();
            }
            else if(shipData.isWorkshopShip) {
                _backgroundImage.color = _backgroundWorkshopShipColor;
                _outlineImage.color = _outlineWorkshopShipColor;
                _movementTypeButton.interactable = false;

                var navigation = _movementTypeButton.navigation;
                navigation.mode = Navigation.Mode.None;
                _movementTypeButton.navigation = navigation;

                //_movementTypeButton.enabled = false;
                SteamWorkshopRenderController.Current.UpdateSelectedShip();
            }

            SetMovementTypeButtonText(importSettings.controlSettings.movementStyle.ToString());

            ShipSelectionRenderController.Current.UpdateSelectedShip();
        }

        //void GetWorkshopItems() {
        //    SteamWorkshopMain.Instance.GetItemList(1, ReceiveWorkshopItems);
        //}

        //void ReceiveWorkshopItems(WorkshopItemListEventArgs obj) {

        //}

        UIColorButton GetColorButton(string name) {
            return _colorButtons.First(s => s.name == name);
        }

        void SaveAsDefault() {
            var shipData = ShipImporter.Current.localShipData[CurrentShipSelection];
            var importSettings = ShipImporter.Current.localSettings[CurrentShipSelection];

            if(Directory.Exists(shipData.folderPath)) {
                string shipSettingsPath = Path.Combine(shipData.folderPath, ShipImporter.settingsFileName);

                if(File.Exists(shipSettingsPath)) {
                    var preset = CurrentPreset;

                    importSettings.primaryColor = preset.PrimaryColor;
                    importSettings.secondaryColor = preset.SecondaryColor;
                    importSettings.accessoryPrimaryColor = preset.AccessoryPrimaryColor;
                    importSettings.accessorySecondaryColor = preset.AccessorySecondaryColor;
                    importSettings.glowColor = preset.GlowColor;

                    ShipImporter.Current.SaveSettings(CurrentShipSelection, importSettings);

                    SetSelectedPreset(0);
                }
            }
        }

        void SaveMovementType(int index, string value) {
            SetMovementTypeButtonText(value);

            var shipData = ShipImporter.Current.localShipData[CurrentShipSelection];
            var importSettings = ShipImporter.Current.localSettings[CurrentShipSelection];

            if(Directory.Exists(shipData.folderPath)) {
                string shipSettingsPath = Path.Combine(shipData.folderPath, ShipImporter.settingsFileName);

                if(File.Exists(shipSettingsPath)) {
                    var movementType = (BaseMovementController.MovementType)Enum.Parse(typeof(BaseMovementController.MovementType), value);
                    var controlSettings = importSettings.controlSettings;

                    controlSettings.movementStyle = movementType;
                    importSettings.controlSettings = controlSettings;

                    ShipImporter.Current.SaveSettings(CurrentShipSelection, importSettings);
                }
            }

            CloseMovementTypeListView();
        }

        void SetColorButton(string name, Color color, float brightness = 0) {
            var colorButton = GetColorButton(name);

            if(colorButton) {
                colorButton.ButtonColor = color;
                colorButton.Brightness = brightness;
            }
        }

        void SetMovementTypeButtonText(string value) {
            _movementTypeButtonText.text = string.Format("{0} Movement", value);
        }

        public void SetOpen(bool enable) {
            if(enable) Open();
            else Close();
        }

        void UploadShip() {
            UIManager.Current.DoPopup("Uploading", "Please wait", null);

            uploadedShipSelected = CurrentShipSelection;

            var shipData = ShipImporter.Current.localShipData[CurrentShipSelection];
            var shipImportSettings = ShipImporter.Current.localSettings[CurrentShipSelection];

            if(!Directory.Exists(shipData.folderPath)) {
                Debug.LogError("Upload error: Ship path doesn't exist. Path:" + shipData.folderPath);
                return;
            }

            string tempUploadPath = TempUploadFolderPath;

            //var securityRules = new DirectorySecurity();
            //securityRules.AddAccessRule(new FileSystemAccessRule(@"Domain\account1", FileSystemRights.FullControl, AccessControlType.Allow));

            Directory.CreateDirectory(tempUploadPath);

            var shipFile = new ShipFile(shipData.rootMeshObject.transform) {
                ImportSettings = shipImportSettings
            };
            shipFile.SaveToFile(Path.Combine(tempUploadPath, shipData.rootMeshObject.name + ".qship"), _crypter);

            var workshopItem = shipData.workshopPublishedFileID != PublishedFileId_t.Invalid ?
                new WorkshopItemUpdate(shipData.workshopPublishedFileID) :
                new WorkshopItemUpdate();

            workshopItem.ContentPath = tempUploadPath;
            //workshopItem.ContentPath = shipData.folderPath;
            workshopItem.Name = shipData.rootMeshObject.name;

            workshopItem.Description = "Custom Ship\nGame Version - " + GameManager.Current.GameVersion;
            workshopItem.IconPath = ShipIconFilePath;
            workshopItem.Tags = new List<string> { "Ship" };

            SteamWorkshopRenderController.Current.Render();
            RenderTexture renderTexture = SteamWorkshopRenderController.Current.renderCamera.targetTexture;

            var previousRenderTexture = RenderTexture.active;
            var saveTexture = new Texture2D(renderTexture.width, renderTexture.height);

            Directory.CreateDirectory(ShipIconFolderPath);

            RenderTexture.active = renderTexture;
            saveTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            saveTexture.Apply();
            File.WriteAllBytes(workshopItem.IconPath, saveTexture.EncodeToPNG());
            RenderTexture.active = previousRenderTexture;

            Destroy(saveTexture);
            SteamWorkshopMain.Instance.Upload(workshopItem, OnShipUploaded);
            GameDebug.Log("Upload Ship");
        }

        public void UpdateWorkshopUIActivation(bool enable, bool isOwned = false) {
            _workshopIDText.gameObject.SetActive(enable);
            _uploadButton.gameObject.SetActive(isOwned ? enable : false);
        }

        void OnShipUploaded(WorkshopItemUpdateEventArgs obj) {
            if(obj.IsError) {
                UIManager.Current.DoPopup("Upload Failed", "Ship was not uploaded: " + obj.ErrorMessage, null, (LapinerTools.uMyGUI.uMyGUI_PopupManager.BTN_OK, null));
                GameDebug.Log("Upload Failed");
            }
            else {
                UIManager.Current.DoPopup("Upload Completed", "Ship was uploaded successfully!", null, (LapinerTools.uMyGUI.uMyGUI_PopupManager.BTN_OK, null));
                GameDebug.Log("Uploaded Ship");

                var importSettings = ShipImporter.Current.localSettings[CurrentShipSelection];
                importSettings.workshopPublishedFileID = obj.Item.SteamNative.m_nPublishedFileId;

                var shipData = ShipImporter.Current.localShipData[CurrentShipSelection];
                shipData.workshopPublishedFileID = importSettings.workshopPublishedFileID;
                ShipImporter.Current.localShipData[CurrentShipSelection] = shipData;

                ShipImporter.Current.SaveSettings(CurrentShipSelection, importSettings);
            }

            //File.Delete(ShipIconFilePath);
        }

        void EnableShipSelectionMenuTemp(bool enable) {
            CanvasGroupComponent.interactable = enable;
            CanvasGroupComponent.alpha = enable ? 1 : .5f;

            if(enable)
                EventSystem.current.SetSelectedGameObject(GetComponentInChildren<Selectable>(false).gameObject);
        }

        public void ToggleHide(bool enable) {
            _shipOptionsRoot.gameObject.SetActive(enable);
            _shipRenderRoot.gameObject.SetActive(enable);
        }
    }
}