using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.UI;
using System.Collections;
using Rewired.UI.ControlMapper;
using System;
using Rewired.Data;

namespace Quadrablaze {
    public class UIControlOptions : UIOptionsTab {
        private const string playerPrefsBaseKey = "Input";

        //public UserDataStore userDataStore;

        public UIInputElement defaultInputElement;
        public ControlMapper controlMapper;

        public CanvasGroup optionsCanvasGroup;

        public Button controlMapperModifyButton;
        public Button controlMapperExitButton;
        public CanvasGroup controlMapperInputGroup;

        public GameObject controlMapperRemapFirstSelected;
        public GameObject controlMapperNormalFirstSelected;

        bool controlsCreated;

        Player RewiredPlayer { get; set; }

        public override void Initialize() {
            base.Initialize();

            RewiredPlayer = ReInput.players.GetPlayer(0);
        }

        public override void LoadPrefs() {
            
            //LoadAllMaps();
            //controlMapper.Open();
            ReInput.userDataStore.Load();
            controlMapper.Reset();
        }

        public override void SavePrefs() {
            //if(ReInput.userDataStore != null) ReInput.userDataStore.Save();
            //SaveAllMaps();
            ReInput.userDataStore.Save();
        }

        public override void SetToDefault() {
            foreach(var player in ReInput.players.Players) {
                player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
                player.controllers.maps.LoadDefaultMaps(ControllerType.Mouse);
                player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
            }

            controlMapper.Reset();
        }

        #region Load/Save

        private void LoadAllMaps() {
            // This example uses PlayerPrefs because its convenient, though not efficient, but you could use any data storage method you like.

            IList<Player> allPlayers = ReInput.players.AllPlayers;
            for(int i = 0; i < allPlayers.Count; i++) {
                Player player = allPlayers[i];

                // Load Input Behaviors - all players have an instance of each input behavior so it can be modified
                IList<InputBehavior> behaviors = ReInput.mapping.GetInputBehaviors(player.id); // get all behaviors from player
                for(int j = 0; j < behaviors.Count; j++) {
                    string xml = GetInputBehaviorXml(player, behaviors[j].id); // try to the behavior for this id
                    if(xml == null || xml == string.Empty) continue; // no data found for this behavior
                    behaviors[j].ImportXmlString(xml); // import the data into the behavior
                }

                // Load the maps first and make sure we have them to load before clearing

                // Load Keyboard Maps
                List<string> keyboardMaps = GetAllControllerMapsXml(player, true, ControllerType.Keyboard, ReInput.controllers.Keyboard);

                // Load Mouse Maps
                List<string> mouseMaps = GetAllControllerMapsXml(player, true, ControllerType.Mouse, ReInput.controllers.Mouse); // load mouse controller maps

                // Load Joystick Maps
                bool foundJoystickMaps = false;
                List<List<string>> joystickMaps = new List<List<string>>();
                foreach(Joystick joystick in player.controllers.Joysticks) {
                    List<string> maps = GetAllControllerMapsXml(player, true, ControllerType.Joystick, joystick);
                    joystickMaps.Add(maps);
                    if(maps.Count > 0) foundJoystickMaps = true;
                }

                // Now add the maps to the controller

                // Keyboard maps
                if(keyboardMaps.Count > 0) player.controllers.maps.ClearMaps(ControllerType.Keyboard, true); // clear only user-assignable maps, but only if we found something to load. Don't _really_ have to clear the maps as adding ones in the same cat/layout will just replace, but let's clear anyway.
                player.controllers.maps.AddMapsFromXml(ControllerType.Keyboard, 0, keyboardMaps); // add the maps to the player

                // Joystick maps
                if(foundJoystickMaps) player.controllers.maps.ClearMaps(ControllerType.Joystick, true); // clear only user-assignable maps, but only if we found something to load. Don't _really_ have to clear the maps as adding ones in the same cat/layout will just replace, but let's clear anyway.
                int count = 0;
                foreach(Joystick joystick in player.controllers.Joysticks) {
                    player.controllers.maps.AddMapsFromXml(ControllerType.Joystick, joystick.id, joystickMaps[count]); // add joystick controller maps to player
                    count++;
                }

                // Mouse Maps
                if(mouseMaps.Count > 0) player.controllers.maps.ClearMaps(ControllerType.Mouse, true); // clear only user-assignable maps, but only if we found something to load. Don't _really_ have to clear the maps as adding ones in the same cat/layout will just replace, but let's clear anyway.
                player.controllers.maps.AddMapsFromXml(ControllerType.Mouse, 0, mouseMaps); // add the maps to the player
            }

            // Load joystick calibration maps
            foreach(Joystick joystick in ReInput.controllers.Joysticks) {
                joystick.ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick)); // load joystick calibration map if any
            }
        }

        private void SaveAllMaps() {
            // This example uses PlayerPrefs because its convenient, though not efficient, but you could use any data storage method you like.

            IList<Player> allPlayers = ReInput.players.AllPlayers;
            for(int i = 0; i < allPlayers.Count; i++) {
                Player player = allPlayers[i];

                // Get all savable data from player
                PlayerSaveData playerData = player.GetSaveData(true);

                // Save Input Behaviors
                foreach(InputBehavior behavior in playerData.inputBehaviors) {
                    string key = GetInputBehaviorPlayerPrefsKey(player, behavior);
                    PlayerPrefs.SetString(key, behavior.ToXmlString()); // save the behavior to player prefs in XML format
                }

                // Save controller maps
                foreach(ControllerMapSaveData saveData in playerData.AllControllerMapSaveData) {
                    string key = GetControllerMapPlayerPrefsKey(player, saveData);
                    PlayerPrefs.SetString(key, saveData.map.ToXmlString()); // save the map to player prefs in XML format
                }
            }

            // Save joystick calibration maps
            foreach(Joystick joystick in ReInput.controllers.Joysticks) {
                JoystickCalibrationMapSaveData saveData = joystick.GetCalibrationMapSaveData();
                string key = GetJoystickCalibrationMapPlayerPrefsKey(saveData);
                PlayerPrefs.SetString(key, saveData.map.ToXmlString()); // save the map to player prefs in XML format
            }

            // Save changes to PlayerPrefs
            PlayerPrefs.Save();
        }

        private void LoadJoystickMaps(int joystickId) {
            // This example uses PlayerPrefs because its convenient, though not efficient, but you could use any data storage method you like.

            IList<Player> allPlayers = ReInput.players.AllPlayers;
            for(int i = 0; i < allPlayers.Count; i++) { // this controller may be owned by more than one player, so check all
                Player player = allPlayers[i];
                if(!player.controllers.ContainsController(ControllerType.Joystick, joystickId)) continue; // player does not have the joystick

                Joystick joystick = player.controllers.GetController<Joystick>(joystickId);
                if(joystick == null) continue;

                // Load the joystick maps first and make sure we have them to load before clearing
                List<string> xmlMaps = GetAllControllerMapsXml(player, true, ControllerType.Joystick, joystick);
                if(xmlMaps.Count == 0) continue;

                // Clear joystick maps first
                player.controllers.maps.ClearMaps(ControllerType.Joystick, true); // clear only user-assignable maps (technically you don't have to clear -- adding a map in same catId/layoutId will overwrite)

                // Load Joystick Maps
                player.controllers.maps.AddMapsFromXml(ControllerType.Joystick, joystickId, xmlMaps); // load joystick controller maps

                // Load joystick calibration map
                joystick.ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick)); // load joystick calibration map
            }
        }

        private void SaveJoystickMaps(int joystickId) {
            // This example uses PlayerPrefs because its convenient, though not efficient, but you could use any data storage method you like.

            IList<Player> allPlayers = ReInput.players.AllPlayers;
            for(int i = 0; i < allPlayers.Count; i++) { // this controller may be owned by more than one player, so check all
                string key;
                Player player = allPlayers[i];
                if(!player.controllers.ContainsController(ControllerType.Joystick, joystickId)) continue; // player does not have the joystick

                // Save controller maps
                JoystickMapSaveData[] saveData = player.controllers.maps.GetMapSaveData<JoystickMapSaveData>(joystickId, true);
                if(saveData != null) {
                    for(int j = 0; j < saveData.Length; j++) {
                        key = GetControllerMapPlayerPrefsKey(player, saveData[j]);
                        PlayerPrefs.SetString(key, saveData[j].map.ToXmlString()); // save the map to player prefs in XML format
                    }
                }

                // Save joystick calibration map
                Joystick joystick = player.controllers.GetController<Joystick>(joystickId);
                JoystickCalibrationMapSaveData calibrationSaveData = joystick.GetCalibrationMapSaveData();
                key = GetJoystickCalibrationMapPlayerPrefsKey(calibrationSaveData);
                PlayerPrefs.SetString(key, calibrationSaveData.map.ToXmlString()); // save the map to player prefs in XML format
            }

            // Save calibration maps
            IList<Joystick> joysticks = ReInput.controllers.Joysticks;
            for(int i = 0; i < joysticks.Count; i++) {
                JoystickCalibrationMapSaveData saveData = joysticks[i].GetCalibrationMapSaveData();
                string key = GetJoystickCalibrationMapPlayerPrefsKey(saveData);
                PlayerPrefs.SetString(key, saveData.map.ToXmlString()); // save the map to player prefs in XML format
            }
        }

        #region PlayerPrefs Methods

        /* NOTE ON PLAYER PREFS:
         * PlayerPrefs on Windows Standalone is saved in the registry. There is a bug in Regedit that makes any entry with a name equal to or greater than 255 characters
         * (243 + 12 unity appends) invisible in Regedit. Unity will still load the data fine, but if you are debugging and wondering why your data is not showing up in
         * Regedit, this is why. If you need to delete the values, either call PlayerPrefs.Clear or delete the key folder in Regedit -- Warning: both methods will
         * delete all player prefs including any ones you've created yourself or other plugins have created.
         */

        // WARNING: Do not use & symbol in keys. Linux cannot load them after the current session ends.

        private string GetBasePlayerPrefsKey(Player player) {
            string key = playerPrefsBaseKey;
            key += "|playerName=" + player.name; // make a key for this specific player, could use id, descriptive name, or a custom profile identifier of your choice
            return key;
        }

        private string GetControllerMapPlayerPrefsKey(Player player, ControllerMapSaveData saveData) {
            // Create a player prefs key like a web querystring so we can search for player prefs key when loading maps
            string key = GetBasePlayerPrefsKey(player);
            key += "|dataType=ControllerMap";
            key += "|controllerMapType=" + saveData.mapTypeString;
            key += "|categoryId=" + saveData.map.categoryId + "|" + "layoutId=" + saveData.map.layoutId;
            key += "|hardwareIdentifier=" + saveData.controllerHardwareIdentifier; // the hardware identifier string helps us identify maps for unknown hardware because it doesn't have a Guid
            if(saveData.mapType == typeof(JoystickMap)) { // store special info for joystick maps
                key += "|hardwareGuid=" + ((JoystickMapSaveData)saveData).joystickHardwareTypeGuid.ToString(); // the identifying GUID that determines which known joystick this is
            }
            return key;
        }

        private string GetControllerMapXml(Player player, ControllerType controllerType, int categoryId, int layoutId, Controller controller) {
            string key = GetBasePlayerPrefsKey(player);
            key += "|dataType=ControllerMap";
            key += "|controllerMapType=" + controller.mapTypeString;
            key += "|categoryId=" + categoryId + "|" + "layoutId=" + layoutId;
            key += "|hardwareIdentifier=" + controller.hardwareIdentifier; // the hardware identifier string helps us identify maps for unknown hardware because it doesn't have a Guid
            if(controllerType == ControllerType.Joystick) {
                Joystick joystick = (Joystick)controller;
                key += "|hardwareGuid=" + joystick.hardwareTypeGuid.ToString(); // the identifying GUID that determines which known joystick this is
            }

            if(!PlayerPrefs.HasKey(key)) return string.Empty; // key does not exist
            return PlayerPrefs.GetString(key); // return the data
        }

        private List<string> GetAllControllerMapsXml(Player player, bool userAssignableMapsOnly, ControllerType controllerType, Controller controller) {
            // Because player prefs does not allow us to search for partial keys, we have to check all possible category ids and layout ids to find the maps to load

            List<string> mapsXml = new List<string>();

            IList<InputMapCategory> categories = ReInput.mapping.MapCategories;
            for(int i = 0; i < categories.Count; i++) {
                InputMapCategory cat = categories[i];
                if(userAssignableMapsOnly && !cat.userAssignable) continue; // skip map because not user-assignable

                IList<InputLayout> layouts = ReInput.mapping.MapLayouts(controllerType);
                for(int j = 0; j < layouts.Count; j++) {
                    InputLayout layout = layouts[j];
                    string xml = GetControllerMapXml(player, controllerType, cat.id, layout.id, controller);
                    if(xml == string.Empty) continue;
                    mapsXml.Add(xml);
                }
            }

            return mapsXml;
        }

        private string GetJoystickCalibrationMapPlayerPrefsKey(JoystickCalibrationMapSaveData saveData) {
            // Create a player prefs key like a web querystring so we can search for player prefs key when loading maps
            string key = playerPrefsBaseKey;
            key += "|dataType=CalibrationMap";
            key += "|controllerType=" + saveData.controllerType.ToString();
            key += "|hardwareIdentifier=" + saveData.hardwareIdentifier; // the hardware identifier string helps us identify maps for unknown hardware because it doesn't have a Guid
            key += "|hardwareGuid=" + saveData.joystickHardwareTypeGuid.ToString();
            return key;
        }

        private string GetJoystickCalibrationMapXml(Joystick joystick) {
            string key = playerPrefsBaseKey;
            key += "|dataType=CalibrationMap";
            key += "|controllerType=" + joystick.type.ToString();
            key += "|hardwareIdentifier=" + joystick.hardwareIdentifier; // the hardware identifier string helps us identify maps for unknown hardware because it doesn't have a Guid
            key += "|hardwareGuid=" + joystick.hardwareTypeGuid.ToString();

            if(!PlayerPrefs.HasKey(key)) return string.Empty; // key does not exist
            return PlayerPrefs.GetString(key); // return the data
        }

        private string GetInputBehaviorPlayerPrefsKey(Player player, InputBehavior saveData) {
            // Create a player prefs key like a web querystring so we can search for player prefs key when loading maps
            string key = GetBasePlayerPrefsKey(player);
            key += "|dataType=InputBehavior";
            key += "|id=" + saveData.id;
            return key;
        }

        private string GetInputBehaviorXml(Player player, int id) {
            string key = GetBasePlayerPrefsKey(player);
            key += "|dataType=InputBehavior";
            key += "|id=" + id;

            if(!PlayerPrefs.HasKey(key)) { GameDebug.Log("Key Does Not Exist - " + key); return string.Empty; } // key does not exist
            return PlayerPrefs.GetString(key); // return the data
        }

        #endregion

        #endregion
    }
}